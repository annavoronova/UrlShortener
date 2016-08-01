using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UrlShortener.Web.Controllers;
using Moq;
using UrlShortener.Business;
using UrlShortener.Entities;
using UrlShortener.Web.Models;

namespace UrlShortener.Tests.Controllers {
    [TestClass]
    public class UrlControllerTest {
        private Mock<IUrlManager> _urlManagerMock;
        //private Mock<HttpRequestBase> _requestMock;
        private Mock<HttpContextBase> _contextMock;

        [TestInitialize]
        public void SetUp()
        {
            _urlManagerMock = new Mock<IUrlManager>(MockBehavior.Strict);
            //_requestMock = new Mock<HttpRequestBase>(MockBehavior.Strict);
            _contextMock = new Mock<HttpContextBase>(MockBehavior.Strict);
        }

        [TestMethod]
        public void Index() {
            // Arrange
            UrlController controller = new UrlController(_urlManagerMock.Object);

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Model as Url);
        }

        [TestMethod]
        public void IndexWithParam() {
            // Arrange
            var url = new Url
            {
                LongUrl = "testUrl",
                ShortUrl = "000000"
            };
            var shortUrl = new ShortUrl
            {
                LongUrl = "testUrl",
                Segment = "000000"
            };
            var ip = "::1";

            _urlManagerMock.Setup(x => x.ShortenUrl(url.LongUrl, ip, "")).Returns(Task.FromResult(shortUrl));
            _contextMock.SetupGet(x => x.Request.UserHostAddress).Returns(ip);
            _contextMock.SetupGet(x => x.Request.Url).Returns(new Uri("http://localhost"));
            _contextMock.SetupGet(x => x.Request.ApplicationPath).Returns("/UrlShortener");
            var controller = new UrlController(_urlManagerMock.Object);
            controller.ControllerContext = new ControllerContext(_contextMock.Object, new RouteData(), controller);

            // Act
            Task<ActionResult> task = controller.Index(url);
            task.Wait();
            Assert.IsNotNull(task);
            var result = task.Result as ViewResult;
            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as Url;
            Assert.IsNotNull(model);
            Assert.AreEqual(url.LongUrl, model.LongUrl);
            Assert.IsTrue(model.ShortUrl.EndsWith(shortUrl.Segment));
        }

        [TestMethod]
        public void ListUrls() {
            // Arrange
            var ip = "::1";
            var urlList = new List<ShortUrl>
            {
                new ShortUrl{LongUrl = "test1", Segment = "000000", Added = DateTime.Today, Ip = ip, NumOfClicks = 1},
                new ShortUrl{LongUrl = "test2", Segment = "000001", Added = DateTime.Today, Ip = ip, NumOfClicks = 1}
            };
            _urlManagerMock.Setup(x => x.EnumUrls()).Returns(Task.FromResult(urlList));
            _contextMock.SetupGet(x => x.Request.Url).Returns(new Uri("http://localhost"));
            _contextMock.SetupGet(x => x.Request.ApplicationPath).Returns("/UrlShortener");
            var controller = new UrlController(_urlManagerMock.Object);
            controller.ControllerContext = new ControllerContext(_contextMock.Object, new RouteData(), controller);

            // Act
            Task<ActionResult> task = controller.List();
            task.Wait();
            Assert.IsNotNull(task);
            var result = task.Result as ViewResult;
            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as IEnumerable<Url>;
            Assert.IsNotNull(model);
            Assert.AreEqual(urlList.Count, model.Count());
            Assert.AreEqual(urlList.First().LongUrl, model.First().LongUrl);
        }
    }
}
