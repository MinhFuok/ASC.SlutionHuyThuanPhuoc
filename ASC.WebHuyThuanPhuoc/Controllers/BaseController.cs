using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASC.WebHuyThuanPhuoc.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
    }
}