using ASC.Model.BaseTypes;
using ASC.WebHuyThuanPhuoc.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ASC.Web.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<ApplicationSettings> options)
        {
            var roles = options.Value.Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var role in roles)
            {
                var trimmedRole = role.Trim();

                if (!await roleManager.RoleExistsAsync(trimmedRole))
                {
                    IdentityRole storageRole = new IdentityRole
                    {
                        Name = trimmedRole
                    };

                    IdentityResult roleResult = await roleManager.CreateAsync(storageRole);

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(string.Join(" | ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            var admin = await userManager.FindByEmailAsync(options.Value.AdminEmail);
            if (admin == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = options.Value.AdminName,
                    Email = options.Value.AdminEmail,
                    EmailConfirmed = true
                };

                IdentityResult result = await userManager.CreateAsync(user, options.Value.AdminPassword);

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(" | ", result.Errors.Select(e => e.Description)));
                }

                await userManager.AddClaimAsync(user,
                    new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.AdminEmail));
                await userManager.AddClaimAsync(user,
                    new System.Security.Claims.Claim("IsActive", "True"));

                await userManager.AddToRoleAsync(user, Roles.Admin.ToString());
            }

            var engineer = await userManager.FindByEmailAsync(options.Value.EngineerEmail);
            if (engineer == null)
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = options.Value.EngineerName,
                    Email = options.Value.EngineerEmail,
                    EmailConfirmed = true,
                    LockoutEnabled = false
                };

                IdentityResult result = await userManager.CreateAsync(user, options.Value.EngineerPassword);

                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(" | ", result.Errors.Select(e => e.Description)));
                }

                await userManager.AddClaimAsync(user,
                    new System.Security.Claims.Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", options.Value.EngineerEmail));
                await userManager.AddClaimAsync(user,
                    new System.Security.Claims.Claim("IsActive", "True"));

                await userManager.AddToRoleAsync(user, Roles.Engineer.ToString());
            }
        }
    }
}