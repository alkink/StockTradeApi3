using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using StockTradeApi3.Controllers;
using StockTradeApi3.Models;
using StockTradeApiTests.tests;

namespace StockTradeApiTests
{

    [TestFixture]
    public class UnitTest1
    {
        private Db1Context GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<Db1Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new Db1Context(options);

            if (databaseContext.Users.Count() <= 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    databaseContext.Users.Add(new User { Username = "user123", Password = "1234" });
                }
                databaseContext.SaveChanges();
            }
            return databaseContext;
        }

        [Test]
        public void GetUser_ReturnsListOfUsers()
        {
            var dbContext = new DbContext(TestData);
            var controller = new UserController(dbContext);

            // Act
            var result = controller.GetUser();

            // Assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<List<User>>(result);
            var returnedUsers = result as List<User>;
            Assert.AreEqual(10, returnedUsers.Count); // Change to 10 to match the data added in the context
        }


    }

}
