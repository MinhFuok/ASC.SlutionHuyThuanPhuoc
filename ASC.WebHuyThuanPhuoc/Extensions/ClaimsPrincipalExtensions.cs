using ASC.Utilities;
using System.Security.Claims;

namespace ASC.WebHuyThuanPhuoc.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public static string? GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal?.FindFirstValue(ClaimTypes.Email);
        }

        public static string? GetUserName(this ClaimsPrincipal principal)
        {
            return principal?.Identity?.Name;
        }

        public static string? GetUserRole(this ClaimsPrincipal principal)
        {
            var roleClaim = principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            return roleClaim?.Value;
        }

        public static CurrentUser GetCurrentUser(this ClaimsPrincipal principal)
        {
            return new CurrentUser
            {
                UserId = principal.GetUserId(),
                UserName = principal.GetUserName(),
                Email = principal.GetUserEmail(),
                Role = principal.GetUserRole(),
                IsAuthenticated = principal?.Identity?.IsAuthenticated ?? false
            };
        }
    }
}
