using ASC.WebHuyThuanPhuoc;
using ASC.WebHuyThuanPhuoc.Configuration;
using ASC.WebHuyThuanPhuoc.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ASC.Tests
{
    public class HomeControllerTests
    {
        [Fact]
        public void HomeController_Index_View_Test()
        {
            var controller = GetController();
            var result = controller.Index();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void HomeController_Index_NoModel_Test()
        {
            var controller = GetController();
            var result = controller.Index() as ViewResult;

            Assert.NotNull(result);
            Assert.NotNull(result.Model);
        }

        [Fact]
        public void HomeController_Index_Validation_Test()
        {
            var controller = GetController();
            var result = controller.Index();

            Assert.True(controller.ModelState.IsValid);
        }
        private HomeController GetController()
        {
            var mockOptions = new Mock<IOptions<ApplicationSettings>>();
            mockOptions.Setup(o => o.Value).Returns(new ApplicationSettings());

            var mockLogger = new Mock<ILogger<HomeController>>();

            //return new HomeController(mockLogger.Object, mockOptions.Object);
            var controller = new HomeController(mockLogger.Object, mockOptions.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new FakeSession();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            return controller;
        }

    }
}
//Áp dụng Test-Driven Development
//Đầu tiên viết unit test cho HomeController → test fail
//Sau đó viết code để pass test
//Khi thêm Session thì test bị fail do không có HttpContext
//Sẽ giải quyết bằng cách tạo FakeSession để mock Session
//Sau đó test pass lại hoàn toàn