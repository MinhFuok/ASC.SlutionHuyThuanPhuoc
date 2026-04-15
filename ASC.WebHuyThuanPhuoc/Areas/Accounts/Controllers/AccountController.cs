using System.Security.Claims;
using ASC.Model.BaseTypes;
using ASC.WebHuyThuanPhuoc.Areas.Accounts.Models;
using ASC.WebHuyThuanPhuoc.Controllers;
using ASC.WebHuyThuanPhuoc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASC.WebHuyThuanPhuoc.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Authorize]
    public class AccountController : BaseController
    {
        private const string IsActiveClaimType = "IsActive";

        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Profile");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ServiceEngineers()
        {
            return View(await BuildServiceEngineerViewModelAsync(new ServiceEngineerRegistrationViewModel
            {
                IsActive = true
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel serviceEngineer)
        {
            var registration = serviceEngineer.Registration;

            if (!registration.IsEdit && string.IsNullOrWhiteSpace(registration.Password))
            {
                ModelState.AddModelError("Registration.Password", "The Password field is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildServiceEngineerViewModelAsync(registration));
            }

            if (registration.IsEdit)
            {
                var user = await _userManager.FindByEmailAsync(registration.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Registration.Email", "Service engineer account was not found.");
                    return View(await BuildServiceEngineerViewModelAsync(registration));
                }

                user.UserName = registration.UserName;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    AddIdentityErrors(updateResult);
                    return View(await BuildServiceEngineerViewModelAsync(registration));
                }

                if (!string.IsNullOrWhiteSpace(registration.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResult = await _userManager.ResetPasswordAsync(user, token, registration.Password);
                    if (!passwordResult.Succeeded)
                    {
                        AddIdentityErrors(passwordResult);
                        return View(await BuildServiceEngineerViewModelAsync(registration));
                    }
                }

                await AddOrReplaceClaimAsync(user, ClaimTypes.Email, registration.Email);
                await AddOrReplaceClaimAsync(user, IsActiveClaimType, registration.IsActive.ToString());
                await SendServiceEngineerEmailAsync(registration, isNewAccount: false);

                return RedirectToAction("ServiceEngineers");
            }

            var existingUser = await _userManager.FindByEmailAsync(registration.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Registration.Email", $"Email '{registration.Email}' is already taken.");
                return View(await BuildServiceEngineerViewModelAsync(registration));
            }

            var newUser = new IdentityUser
            {
                UserName = registration.UserName,
                Email = registration.Email,
                EmailConfirmed = true,
                LockoutEnabled = false
            };

            var createResult = await _userManager.CreateAsync(newUser, registration.Password!);
            if (!createResult.Succeeded)
            {
                AddIdentityErrors(createResult);
                return View(await BuildServiceEngineerViewModelAsync(registration));
            }

            await AddOrReplaceClaimAsync(newUser, ClaimTypes.Email, registration.Email);
            await AddOrReplaceClaimAsync(newUser, IsActiveClaimType, registration.IsActive.ToString());

            var roleResult = await _userManager.AddToRoleAsync(newUser, Roles.Engineer.ToString());
            if (!roleResult.Succeeded)
            {
                AddIdentityErrors(roleResult);
                return View(await BuildServiceEngineerViewModelAsync(registration));
            }

            await SendServiceEngineerEmailAsync(registration, isNewAccount: true);

            return RedirectToAction("ServiceEngineers");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            return View(await BuildCustomerViewModelAsync(new CustomerRegistrationViewModel()));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customers(CustomerViewModel customer)
        {
            var registration = customer.Registration;

            if (!registration.IsEdit)
            {
                ModelState.AddModelError("Registration.Email", "Select a customer account before saving.");
            }

            if (!ModelState.IsValid)
            {
                return View(await BuildCustomerViewModelAsync(registration));
            }

            var user = await _userManager.FindByEmailAsync(registration.Email);
            if (user == null)
            {
                ModelState.AddModelError("Registration.Email", "Customer account was not found.");
                return View(await BuildCustomerViewModelAsync(registration));
            }

            await AddOrReplaceClaimAsync(user, ClaimTypes.Email, registration.Email);
            await AddOrReplaceClaimAsync(user, IsActiveClaimType, registration.IsActive.ToString());

            if (registration.IsActive)
            {
                await _emailSender.SendEmailAsync(
                    registration.Email,
                    "Account Modified",
                    $"Your account has been activated. Email: {registration.Email}");
            }
            else
            {
                await _emailSender.SendEmailAsync(
                    registration.Email,
                    "Account Deactivated",
                    "Your account has been deactivated.");
            }

            return RedirectToAction("Customers");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            return View(new ProfileModel
            {
                UserName = user.UserName ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModel profile)
        {
            if (!ModelState.IsValid)
            {
                return View(profile);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            user.UserName = profile.UserName;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                AddIdentityErrors(result);
                return View(profile);
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
        }

        private async Task<ServiceEngineerViewModel> BuildServiceEngineerViewModelAsync(
            ServiceEngineerRegistrationViewModel registration)
        {
            var users = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());

            return new ServiceEngineerViewModel
            {
                ServiceEngineers = await ToAccountUsersAsync(users),
                Registration = registration
            };
        }

        private async Task<CustomerViewModel> BuildCustomerViewModelAsync(CustomerRegistrationViewModel registration)
        {
            var users = await _userManager.GetUsersInRoleAsync(Roles.User.ToString());

            return new CustomerViewModel
            {
                Customers = await ToAccountUsersAsync(users),
                Registration = registration
            };
        }

        private async Task<List<AccountUserViewModel>> ToAccountUsersAsync(IList<IdentityUser> users)
        {
            var accountUsers = new List<AccountUserViewModel>();

            foreach (var user in users.OrderBy(u => u.Email))
            {
                accountUsers.Add(new AccountUserViewModel
                {
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    IsActive = await IsActiveUserAsync(user)
                });
            }

            return accountUsers;
        }

        private async Task AddOrReplaceClaimAsync(IdentityUser user, string type, string value)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var claim in claims.Where(c => c.Type == type))
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            await _userManager.AddClaimAsync(user, new Claim(type, value));
        }

        private async Task<bool> IsActiveUserAsync(IdentityUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);
            var isActiveClaim = claims.FirstOrDefault(c => c.Type == IsActiveClaimType);

            return isActiveClaim == null ||
                bool.TryParse(isActiveClaim.Value, out var isActive) && isActive;
        }

        private async Task SendServiceEngineerEmailAsync(
            ServiceEngineerRegistrationViewModel registration,
            bool isNewAccount)
        {
            if (registration.IsActive)
            {
                var passwordText = string.IsNullOrWhiteSpace(registration.Password)
                    ? string.Empty
                    : $"<br/>Password: {registration.Password}";

                await _emailSender.SendEmailAsync(
                    registration.Email,
                    isNewAccount ? "Account Created" : "Account Modified",
                    $"Email: {registration.Email}<br/>User Name: {registration.UserName}{passwordText}");
            }
            else
            {
                await _emailSender.SendEmailAsync(
                    registration.Email,
                    "Account Deactivated",
                    "Your account has been deactivated.");
            }
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
