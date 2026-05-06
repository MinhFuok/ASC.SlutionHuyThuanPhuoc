using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ASC.WebHuyThuanPhuoc.ServiceHub
{
    public class EmailBasedUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.Email)?.Value
                ?? connection.User?.Identity?.Name;
        }
    }
}