using System;
using Xunit;
using System.Collections.Generic;
using System.Text;
using NetCoreTemp.WebApi.Models.View_Model;
using Microsoft.Extensions.Logging;
using NetCoreTemp.WebApi.Controllers;
using NetCoreTemp.WebApi.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Moq;
using System.Linq;
using NetCoreTemp.WebApi.Models.DatabaseContext;
using System.Security.Principal;
using NetCoreTemp.WebApi.Models.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace NetCoreTempXUnit.User
{
    public class TUserController
    {
        Mock<NetCoreTemp.WebApi.Models.DatabaseContext.AppDbContext> _mockContext;
        Mock<IServiceProvider> _mockServiceProvider;
        Mock<ILogger<UserController>> _mockLoggerUserController;
        Mock<ILogger<UserService>> _mockLoggerUserService;
        Mock<ILogger<UserRoleService>> _mockLoggerUserRoleService;
        Mock<ILogger<RoleService>> _mockLoggerRoleService;
        Mock<IConfiguration> _mockIConfg;
        UserController _controller;

        public TUserController()
        {
            _mockContext = new Mock<AppDbContext>(MockDbContextHelper.MockGetDbContextOptions());

            var dataUser = new List<NetCoreTemp.WebApi.Models.Entity.User>
            {
                new NetCoreTemp.WebApi.Models.Entity.User { ID = Guid.NewGuid(), Name = "BBB",Status= NetCoreTemp.WebApi.Models.EnumType.EnumRepo.UseStatusEnum.Enable },
                new NetCoreTemp.WebApi.Models.Entity.User { ID = Guid.NewGuid(), Name = "ZZZ",Status= NetCoreTemp.WebApi.Models.EnumType.EnumRepo.UseStatusEnum.Draft },
                new NetCoreTemp.WebApi.Models.Entity.User { ID = Guid.NewGuid(), Name = "AAA",Status= NetCoreTemp.WebApi.Models.EnumType.EnumRepo.UseStatusEnum.Disable },
            }.AsQueryable();

            var mockDbSetUser = MockDbContextHelper.MockGetQueryableDbSet<NetCoreTemp.WebApi.Models.Entity.User>(dataUser);

            _mockContext.Setup(c => c.User).Returns(mockDbSetUser.Object); 
            _mockContext.Setup(c => c.Set<NetCoreTemp.WebApi.Models.Entity.User>()).Returns(mockDbSetUser.Object);

            _mockServiceProvider = MockHelper.MockIServiceProvider();
            _mockServiceProvider.Setup(x => x.GetService(typeof(AppDbContext)))
                    .Returns(_mockContext.Object);

            _mockLoggerUserController = MockLoggerHelper.LoggerMock<UserController>();
            _mockLoggerUserService = MockLoggerHelper.LoggerMock<UserService>();
            _mockLoggerUserRoleService = MockLoggerHelper.LoggerMock<UserRoleService>();
            _mockLoggerRoleService = MockLoggerHelper.LoggerMock<RoleService>();
            
            _mockIConfg = new Mock<IConfiguration>();
            _mockIConfg.Setup(x => x["JWT:ServerSecret"]).Returns("ServerSecret");
            _mockIConfg.Setup(x => x["JWT:Issuer"]).Returns("Issuer");
            _mockIConfg.Setup(x => x["JWT:Audience"]).Returns("Audience");
        }

        [Fact]
        public async Task TGet()
        {
            var _userService = new Mock<UserService>(_mockLoggerUserService.Object, _mockServiceProvider.Object);
            var _userRoleService = new Mock<UserRoleService>(_mockLoggerUserRoleService.Object, _mockServiceProvider.Object);
            var _roleService = new Mock<RoleService>(_mockLoggerRoleService.Object, _mockServiceProvider.Object);

            var mockUserController = new Mock<UserController>(_mockLoggerUserController.Object, _userService.Object, _userRoleService.Object, _roleService.Object, _mockIConfg.Object);
            _controller = mockUserController.Object;
            var context =  MockHelper.MockFakeHttpContext(new NetCoreTemp.WebApi.Models.Entity.User { 
                ID = new Guid(), 
                Name = "admin",
                Email = "admin@admin.com",
                Roles = "admin", 
                Status = NetCoreTemp.WebApi.Models.EnumType.EnumRepo.UseStatusEnum.Enable 
            });
            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                 HttpContext = context.Object
            };

            string searhFilters = null;
            int page = 0;
            int limit = 10;

            var result = await _controller.Get(searhFilters, page, limit);

            Assert.Equal(1, result.RowsCount);
            var name = ((List<NetCoreTemp.WebApi.Models.AutoMapper.UserDto>)result.ArrData)[0]._Name;
            Assert.Equal("BBB", name);
        }
    }
}
