using ASC.Business.Interfaces;
using ASC.Utilities;
using ASC.WebHuyThuanPhuoc.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace ASC.WebHuyThuanPhuoc.ServiceHub
{
    [Authorize]
    public class ServiceMessagesHub : Hub
    {
        private readonly IOnlineUsersOperations _onlineUsersOperations;
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IOptions<ApplicationSettings> _options;

        public ServiceMessagesHub(
            IOnlineUsersOperations onlineUsersOperations,
            IServiceRequestOperations serviceRequestOperations,
            IOptions<ApplicationSettings> options)
        {
            _onlineUsersOperations = onlineUsersOperations;
            _serviceRequestOperations = serviceRequestOperations;
            _options = options;
        }

        public override async Task OnConnectedAsync()
        {
            var currentUser = Context.User?.GetCurrentUserDetails();
            var serviceRequestId = Context.GetHttpContext()?.Request.Query["serviceRequestId"].ToString();

            if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.Email))
            {
                await _onlineUsersOperations.CreateOnlineUserAsync(currentUser.Email);
            }

            if (!string.IsNullOrWhiteSpace(serviceRequestId))
            {
                await NotifyOnlineUsersAsync(serviceRequestId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var currentUser = Context.User?.GetCurrentUserDetails();
            var serviceRequestId = Context.GetHttpContext()?.Request.Query["serviceRequestId"].ToString();

            if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.Email))
            {
                await _onlineUsersOperations.DeleteOnlineUserAsync(currentUser.Email);
            }

            if (!string.IsNullOrWhiteSpace(serviceRequestId))
            {
                await NotifyOnlineUsersAsync(serviceRequestId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task NotifyOnlineUsersAsync(string serviceRequestId)
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

            await Clients.Users(users.Distinct()).SendAsync("online", status);
        }
    }
}