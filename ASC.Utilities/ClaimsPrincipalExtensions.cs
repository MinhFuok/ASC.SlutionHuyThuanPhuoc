using System.Security.Claims;

namespace ASC.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        public static CurrentUser GetCurrentUserDetails(this ClaimsPrincipal principal)
        {
            var isActiveValue = principal.Claims
                .Where(c => c.Type == "IsActive")
                .Select(c => c.Value)
                .SingleOrDefault();

            var isActive = false;
            if (!string.IsNullOrWhiteSpace(isActiveValue))
            {
                bool.TryParse(isActiveValue, out isActive);
            }

            return new CurrentUser
            {
                Name = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? string.Empty,
                Email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty,
                IsActive = isActive,
                Roles = principal.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToArray()
            };
        }
    }
}
