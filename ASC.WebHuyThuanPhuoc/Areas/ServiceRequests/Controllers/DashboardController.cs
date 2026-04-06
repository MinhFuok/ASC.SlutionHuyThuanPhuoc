using ASC.WebHuyThuanPhuoc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace ASC.WebHuyThuanPhuoc.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}