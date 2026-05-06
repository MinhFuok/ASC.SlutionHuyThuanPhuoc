using ASC.Business.Interfaces;
using ASC.Model.BaseTypes;
using ASC.Model.Models;
using ASC.Utilities;
using ASC.WebHuyThuanPhuoc.Areas.ServiceRequests.Models;
using ASC.WebHuyThuanPhuoc.Configuration;
using ASC.WebHuyThuanPhuoc.Controllers;
using ASC.WebHuyThuanPhuoc.Data;
using ASC.WebHuyThuanPhuoc.ServiceHub;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using ASC.WebHuyThuanPhuoc.Services;

namespace ASC.WebHuyThuanPhuoc.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IServiceRequestMessageOperations _serviceRequestMessageOperations;
        private readonly IOnlineUsersOperations _onlineUsersOperations;
        private readonly IHubContext<ServiceMessagesHub> _hubContext;
        private readonly IOptions<ApplicationSettings> _options;
        private readonly IMapper _mapper;
        private readonly IMasterDataCacheOperations _masterData;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ISmsSender _smsSender;
        public ServiceRequestController(
            IServiceRequestOperations operations,
            IServiceRequestMessageOperations messageOperations,
            IOnlineUsersOperations onlineUsersOperations,
            IHubContext<ServiceMessagesHub> hubContext,
            IOptions<ApplicationSettings> options,
            IMapper mapper,
            IMasterDataCacheOperations masterData,
            UserManager<IdentityUser> userManager,
            ISmsSender smsSender)
        {
            _serviceRequestOperations = operations;
            _serviceRequestMessageOperations = messageOperations;
            _onlineUsersOperations = onlineUsersOperations;
            _hubContext = hubContext;
            _options = options;
            _mapper = mapper;
            _masterData = masterData;
            _userManager = userManager;
            _smsSender = smsSender;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            var masterData = await _masterData.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();

            return View(new NewServiceRequestViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel request)
        {
            if (!ModelState.IsValid)
            {
                var masterData = await _masterData.GetMasterDataCacheAsync();
                ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
                ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();

                return View(request);
            }

            var currentUser = HttpContext.User.GetCurrentUserDetails();

            // Map the view model to model
            var serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(request);

            // Set RowKey, PartitionKey, RequestedDate, Status properties
            serviceRequest.PartitionKey = currentUser.Email;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.RequestedDate = request.RequestedDate;
            serviceRequest.Status = Status.New.ToString();
            serviceRequest.ServiceEngineer = string.Empty;

            // Audit fields
            serviceRequest.IsDeleted = false;
            serviceRequest.CreatedDate = DateTime.UtcNow;
            serviceRequest.UpdatedDate = DateTime.UtcNow;
            serviceRequest.CreatedBy = currentUser.Email;
            serviceRequest.UpdatedBy = currentUser.Email;

            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);

            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestDetails(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var serviceRequestDetails = await _serviceRequestOperations.GetServiceRequestByRowKey(id);

            if (serviceRequestDetails == null)
            {
                return NotFound();
            }

            var currentUser = HttpContext.User.GetCurrentUserDetails();

            // Access Check giống PDF
            if (HttpContext.User.IsInRole(Roles.Engineer.ToString())
                && serviceRequestDetails.ServiceEngineer != currentUser.Email)
            {
                throw new UnauthorizedAccessException();
            }

            if (HttpContext.User.IsInRole(Roles.User.ToString())
                && serviceRequestDetails.PartitionKey != currentUser.Email)
            {
                throw new UnauthorizedAccessException();
            }

            var masterData = await _masterData.GetMasterDataCacheAsync();

            ViewBag.VehicleTypes = masterData.Values
                .Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString())
                .ToList();

            ViewBag.VehicleNames = masterData.Values
                .Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString())
                .ToList();

            ViewBag.Status = Enum.GetValues(typeof(Status))
                .Cast<Status>()
                .Select(v => v.ToString())
                .ToList();

            ViewBag.ServiceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());

            return View(new ServiceRequestDetailViewModel
            {
                ServiceRequest = _mapper.Map<ServiceRequest, UpdateServiceRequestViewModel>(serviceRequestDetails),
                ServiceRequestAudit = new List<ServiceRequest>()
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServiceRequestDetails(
            [Bind(Prefix = "ServiceRequest")] UpdateServiceRequestViewModel serviceRequest)
        {
            var originalServiceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(serviceRequest.RowKey);

            if (originalServiceRequest == null)
            {
                return NotFound();
            }

            var oldStatus = originalServiceRequest.Status;
            var currentUser = HttpContext.User.GetCurrentUserDetails();

            originalServiceRequest.RequestedServices = serviceRequest.RequestedServices;
            originalServiceRequest.VehicleName = serviceRequest.VehicleName;
            originalServiceRequest.VehicleType = serviceRequest.VehicleType;
            // Update Status only if user role is either Admin or Engineer
            // Or Customer can update the status if it is only in PendingCustomerApproval.
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()) ||
                HttpContext.User.IsInRole(Roles.Engineer.ToString()) ||
                (HttpContext.User.IsInRole(Roles.User.ToString()) &&
                 originalServiceRequest.Status == Status.PendingCustomerApproval.ToString()))
            {
                originalServiceRequest.Status = serviceRequest.Status;
            }

            // Update Service Engineer field only if user role is Admin
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                originalServiceRequest.ServiceEngineer = serviceRequest.ServiceEngineer;
            }

            if (originalServiceRequest.Status == Status.Completed.ToString())
            {
                originalServiceRequest.CompletedDate = DateTime.UtcNow;
            }

            originalServiceRequest.UpdatedDate = DateTime.UtcNow;
            originalServiceRequest.UpdatedBy = currentUser.Email;

            await _serviceRequestOperations.UpdateServiceRequestAsync(originalServiceRequest);

            if (!string.Equals(oldStatus, originalServiceRequest.Status, StringComparison.OrdinalIgnoreCase))
            {
                await SendSmsAndWebNotificationsAsync(originalServiceRequest);
            }

            return RedirectToAction("ServiceRequestDetails", "ServiceRequest",
                new { Area = "ServiceRequests", id = serviceRequest.RowKey });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestMessages(string serviceRequestId)
        {
            if (string.IsNullOrWhiteSpace(serviceRequestId))
            {
                return Json(new List<ServiceRequestMessage>());
            }

            var messages = await _serviceRequestMessageOperations
                .GetServiceRequestMessagesAsync(serviceRequestId);

            return Json(messages.OrderByDescending(p => p.MessageDate));
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceRequestMessage(string serviceRequestId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(serviceRequestId) ||
                string.IsNullOrWhiteSpace(messageText))
            {
                return Json(new
                {
                    success = false,
                    error = "Missing serviceRequestId or messageText"
                });
            }

            var serviceRequestDetails = await _serviceRequestOperations
                .GetServiceRequestByRowKey(serviceRequestId);

            if (serviceRequestDetails == null)
            {
                return Json(new
                {
                    success = false,
                    error = "Service request not found"
                });
            }

            var currentUser = HttpContext.User.GetCurrentUserDetails();

            var message = new ServiceRequestMessage(serviceRequestId)
            {
                Message = messageText.Trim(),
                FromEmail = currentUser.Email,
                FromDisplayName = string.IsNullOrWhiteSpace(currentUser.Name)
                    ? currentUser.Email
                    : currentUser.Name,
                MessageDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = currentUser.Email,
                UpdatedBy = currentUser.Email,
                IsDeleted = false
            };

            await _serviceRequestMessageOperations.CreateServiceRequestMessageAsync(message);

            var users = new List<string>
    {
        serviceRequestDetails.PartitionKey,
        _options.Value.AdminEmail
    };

            if (!string.IsNullOrWhiteSpace(serviceRequestDetails.ServiceEngineer))
            {
                users.Add(serviceRequestDetails.ServiceEngineer);
            }

            await _hubContext.Clients
                .Users(users.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct())
                .SendAsync("publishMessage", message);

            return Json(new
            {
                success = true
            });
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> MarkOfflineUser(string serviceRequestId)
        {
            var currentUser = HttpContext.User.GetCurrentUserDetails();

            if (!string.IsNullOrWhiteSpace(currentUser.Email))
            {
                await _onlineUsersOperations.DeleteOnlineUserAsync(currentUser.Email);
            }

            if (!string.IsNullOrWhiteSpace(serviceRequestId))
            {
                await SendOnlineStatusAsync(serviceRequestId);
            }

            return Json(true);
        }

        private async Task SendSmsAndWebNotificationsAsync(ServiceRequest serviceRequest)
        {
            var customer = await _userManager.FindByEmailAsync(serviceRequest.PartitionKey);

            if (!string.IsNullOrWhiteSpace(customer?.PhoneNumber))
            {
                var phoneNumber = NormalizePhoneNumber(customer.PhoneNumber);

                await _smsSender.SendSmsAsync(
                    phoneNumber,
                    $"Service Request Status updated to {serviceRequest.Status}");
            }

            await _hubContext.Clients
                .User(serviceRequest.PartitionKey)
                .SendAsync("publishNotification", new
                {
                    status = serviceRequest.Status,
                    serviceRequestId = serviceRequest.RowKey
                });
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            var number = phoneNumber
                .Trim()
                .Replace(" ", "")
                .Replace("-", "");

            if (number.StartsWith("+"))
            {
                return number;
            }

            if (number.StartsWith("0"))
            {
                return "+84" + number.Substring(1);
            }

            return "+84" + number;
        }

        private async Task SendOnlineStatusAsync(string serviceRequestId)
        {
            var serviceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(serviceRequestId);

            if (serviceRequest == null)
            {
                return;
            }

            var customerEmail = serviceRequest.PartitionKey;
            var serviceEngineerEmail = serviceRequest.ServiceEngineer;
            var adminEmail = _options.Value.AdminEmail;

            var users = new List<string>();

            if (!string.IsNullOrWhiteSpace(customerEmail))
            {
                users.Add(customerEmail);
            }

            if (!string.IsNullOrWhiteSpace(serviceEngineerEmail))
            {
                users.Add(serviceEngineerEmail);
            }

            if (!string.IsNullOrWhiteSpace(adminEmail))
            {
                users.Add(adminEmail);
            }

            var status = new
            {
                isCu = await _onlineUsersOperations.GetOnlineUserAsync(customerEmail),
                isSe = await _onlineUsersOperations.GetOnlineUserAsync(serviceEngineerEmail),
                isAd = await _onlineUsersOperations.GetOnlineUserAsync(adminEmail)
            };

            await _hubContext.Clients
                .Users(users.Distinct())
                .SendAsync("online", status);
        }
    }
}