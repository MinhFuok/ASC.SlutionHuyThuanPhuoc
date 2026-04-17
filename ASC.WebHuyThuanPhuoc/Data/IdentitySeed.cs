using ASC.Model.BaseTypes;
using ASC.WebHuyThuanPhuoc.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

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

                admin = user;
            }

            await EnsureClaimAsync(userManager, admin, ClaimTypes.Email, options.Value.AdminEmail);
            await EnsureClaimAsync(userManager, admin, "IsActive", bool.TrueString);
            await EnsureRoleAsync(userManager, admin, Roles.Admin.ToString());

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

                engineer = user;
            }

            await EnsureClaimAsync(userManager, engineer, ClaimTypes.Email, options.Value.EngineerEmail);
            await EnsureClaimAsync(userManager, engineer, "IsActive", bool.TrueString);
            await EnsureRoleAsync(userManager, engineer, Roles.Engineer.ToString());
        }

        private static async Task EnsureClaimAsync(
            UserManager<IdentityUser> userManager,
            IdentityUser user,
            string type,
            string value)
        {
            var claims = await userManager.GetClaimsAsync(user);
            var existingClaims = claims.Where(c => c.Type == type).ToList();

            foreach (var claim in existingClaims.Where(c => c.Value != value))
            {
                await userManager.RemoveClaimAsync(user, claim);
            }

            if (existingClaims.Any(c => c.Value == value))
            {
                return;
            }

            var result = await userManager.AddClaimAsync(user, new Claim(type, value));
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(" | ", result.Errors.Select(e => e.Description)));
            }
        }

        private static async Task EnsureRoleAsync(
            UserManager<IdentityUser> userManager,
            IdentityUser user,
            string role)
        {
            if (await userManager.IsInRoleAsync(user, role))
            {
                return;
            }

            var result = await userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(" | ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
