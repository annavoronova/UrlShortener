using System.Data.Common.CommandTrees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UrlShortener.Business;
using UrlShortener.Data;
using UrlShortener.Entities;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace UrlShortener.Tests.Managers {
    [TestClass]
    public class UrlManagerTest
    {
        private Mock<IShortenerContext> _ctxMock;
        private Mock<DbSet<ShortUrl>> _dbSetUrlMock;
        [TestInitialize]
        public void SetUp() {
            _ctxMock = new Mock<IShortenerContext>(MockBehavior.Strict);
            _dbSetUrlMock = new Mock<DbSet<ShortUrl>>();
        }

        [TestMethod]
        public void EnumUrls() {
            // Arrange
            _ctxMock.Setup(x => x.ShortUrls).Returns(It.IsAny<DbSet<ShortUrl>>());
            IUrlManager mgr = new UrlManager(_ctxMock.Object);
            // Act
            var task = mgr.EnumUrls();
            task.Wait();
            // Assert
            var urls = task.Result;
            Assert.IsNotNull(urls);
        }
    }
}
