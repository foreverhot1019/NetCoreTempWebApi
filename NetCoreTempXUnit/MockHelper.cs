using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NetCoreTemp.WebApi.Models.BaseModel;
using NetCoreTemp.WebApi.Models.DatabaseContext;
using NetCoreTemp.WebApi.Services.Base;

namespace NetCoreTempXUnit
{
    public static class MockHelper
    {
        public static Mock<IServiceProvider> MockIServiceProvider()
        {
            var _mockServiceProvider = new Mock<IServiceProvider>();
            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory.Setup(x => x.CreateScope())
                    .Returns(serviceScope.Object);

            _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                    .Returns(serviceScopeFactory.Object);

            return _mockServiceProvider;
        }

        public static Mock<HttpContext> MockFakeHttpContext(NetCoreTemp.WebApi.Models.Entity.User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim("Email", user.Email),
                new Claim(ClaimTypes.Role, user.Roles)
            };
            var identity = new ClaimsIdentity(claims, "MockXUnitTest");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);
            mockPrincipal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(m => m.User).Returns(claimsPrincipal);

            //var context = new Mock<HttpContextBase>();
            //var request = new Mock<HttpRequestBase>();
            //var response = new Mock<HttpResponseBase>();
            //var session = new Mock<HttpSessionStateBase>();
            //var server = new Mock<HttpServerUtilityBase>();
            //var user = new Mock<IPrincipal>();
            //var identity = new Mock<IIdentity>();

            //request.Expect(req => req.ApplicationPath).Returns("~/");
            //request.Expect(req => req.AppRelativeCurrentExecutionFilePath).Returns("~/");
            //request.Expect(req => req.PathInfo).Returns(string.Empty);
            //response.Expect(res => res.ApplyAppPathModifier(It.IsAny<string>()))
            //    .Returns((string virtualPath) => virtualPath);
            //user.Expect(usr => usr.Identity).Returns(identity.Object);
            //identity.ExpectGet(ident => ident.IsAuthenticated).Returns(true);

            //context.Expect(ctx => ctx.Request).Returns(request.Object);
            //context.Expect(ctx => ctx.Response).Returns(response.Object);
            //context.Expect(ctx => ctx.Session).Returns(session.Object);
            //context.Expect(ctx => ctx.Server).Returns(server.Object);
            //context.Expect(ctx => ctx.User).Returns(user.Object);

            return mockHttpContext;
        }

    }

    public static class MockDbContextHelper
    {
        public static DbContextOptions<AppDbContext> MockGetDbContextOptions(string databaseName = "InMemoryXUnitTestDb")
        {
            var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                                .UseInMemoryDatabase(databaseName: databaseName)
                                .Options;
            return dbOptions;
        }

        public static Mock<TDbContext> MockGetDbContext<TDbContext>(string databaseName = "InMemoryXUnitTestDb") where TDbContext : DbContext
        {
            var dbOptions = MockGetDbContextOptions(databaseName);
            var mockDbContext = new Mock<TDbContext>(MockDbContextHelper.MockGetDbContextOptions());
            return mockDbContext;
        }

        public static Mock<DbSet<T>> MockGetQueryableDbSet<T>(params T[] sourceList) where T : class, IEntity_
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.Setup(m => m.AsQueryable()).Returns(sourceList.AsQueryable());
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return dbSet;
        }

        public static Mock<DbSet<T>> MockGetQueryableDbSet<T>(IQueryable<T> queryable) where T : class, IEntity_
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.Setup(m => m.AsQueryable()).Returns(queryable);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            return mockDbSet;
        }
    }

    public static class MockLoggerHelper
    {
        public static Mock<ILogger<T>> LoggerMock<T>() where T : class
        {
            return new Mock<ILogger<T>>();
        }

        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, string containMessage, Times times)
        {
            loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains(containMessage)),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times);
        }

        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, LogLevel level, Times times)
        {
            loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            times);
        }
    }
}
