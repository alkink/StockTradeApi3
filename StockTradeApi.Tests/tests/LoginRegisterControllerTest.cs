using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using StockTradeApi3.Controllers;
using StockTradeApi3.Models;
using Xunit;
using StockTradeApi3.Services;
using System.Reflection;

namespace StockTradeApiTests.tests
{

	public class LoginRegisterControllerTest
	{
        private readonly Db1Context dbContext;
        private LoginRegisterController controller;
        private readonly JwtSettings jwtSetting;

        public LoginRegisterControllerTest()
        {
            var options = new DbContextOptionsBuilder<Db1Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            dbContext = new Db1Context(options);


            var user = new User { Username = "alkinkadmin", Password = "123123", Role = "admin" };
            dbContext.Users.Add(user);
            dbContext.SaveChanges();


            // Mock JwtSettings using Moq
            jwtSetting = new JwtSettings
            {
                Key = "FJDSFJDSJDFSDJFDJSALDKFDSJFDKFDSGKDFSKDFS",
                Issuer = "https://localhost:7179/",
                Audience = "https://localhost:7179/"
            };

            var jwtOptions = Options.Create(jwtSetting);
            var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            jwtSettingsMock.Setup(x => x.Value).Returns(jwtSetting);

            controller = new LoginRegisterController(jwtSettingsMock.Object, dbContext);
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsOkResultWithToken()
        {
            // Arrange
            var validUser = new User { Username = "alkinkadmin", Password = "123123" };

            // Act
            var result = controller.login(validUser) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var token = result.Value as string;
            Assert.NotNull(token);
        }

        [Fact]
        public void Login_InvalidCredentials_ReturnsNotFoundResult()
        {
            // Arrange
            var invalidUser = new User { Username = "invalidUser", Password = "invalidPassword" };

            // Act
            var result = controller.login(invalidUser) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("user couldn't find", result.Value);
        }

        [Fact]
        public void Register_ValidUser_ReturnsCreatedResultWithUser()
        {
            // Arrange
            var newUser = new User { Username = "newUser", Password = "newPassword" };

            // Act
            var result = controller.Add(newUser) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var createdUser = result.Value as User;
            Assert.NotNull(createdUser);
            Assert.Equal(newUser.Username, createdUser.Username);
        }

        [Fact]
        public void Register_UserWithExistingUsername_ReturnsConflictResult()
        {
            // Arrange
            var existingUser = new User { Username = "alkinkadmin", Password = "123123", Role = "admin" };
            dbContext.Users.Add(existingUser);
            dbContext.SaveChanges();

            // Act
            var result = controller.Add(existingUser) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(409, result.StatusCode); 
        }
    }
}

