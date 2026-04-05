using ASC.WebHuyThuanPhuoc.Models;
using ASC.WebHuyThuanPhuoc.Configuration;        
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;               
using System.Diagnostics;

namespace ASC.WebHuyThuanPhuoc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IOptions<ApplicationSettings> _settings;
        public HomeController(ILogger<HomeController> logger, IOptions<ApplicationSettings> settings)
        {
            _logger = logger;
            _settings = settings;
        }
        public IActionResult Index()
        {
            ViewBag.Title = _settings.Value.ApplicationTitle;
            HttpContext.Session.SetString("msg", "hello");
            var value = HttpContext.Session.GetString("msg");
            var model = new object();
            return View(model);
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
