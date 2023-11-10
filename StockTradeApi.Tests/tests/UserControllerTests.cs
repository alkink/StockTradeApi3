using System;
using System.Collections.Generic;
using System.Data.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockTradeApi3.Controllers;
using StockTradeApi3.Models;
using Xunit;

public class UserControllerTests
{
    private readonly Db1Context dbcontext;
    UserController controller;
    public UserControllerTests()
    {
        var options = new DbContextOptionsBuilder<Db1Context>()
           .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
           .Options;
        dbcontext = new Db1Context(options);
        controller = new UserController(dbcontext);
    }

 [Fact]
    public async void GetUser_ReturnsListOfUsers()
    {
        if (dbcontext.Users.Count() <= 0)
        {
            for (int i = 0; i < 10; i++)
            {
                dbcontext.Users.Add(new User { Username = "user123", Password = "1234" });
            }
            dbcontext.SaveChanges();
        }
        var username = "user123";

        // Act
        var result = controller.GetUser();

        // Assert
        Assert.NotNull(result);
        var okResult = Assert.IsType<List<User>>(result);
        var returnedUsers = Assert.IsType<List<User>>(okResult);
        Assert.Equal(10, returnedUsers.Count);
    }

    [Fact]
    public void Add_AddsNewUser()
    {

        var newUser = new User { Username = "User3" };

        // Act
        var result = controller.Add(newUser) as User;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newUser, dbcontext.Users.Find(result.Id));
    }
    public void Add_ReturnsBadRequest_ForInvalidModel()
    {
        // Arrange
        var newUser = new User(); // Invalid user with missing properties

        // Act
        var result = controller.Add(newUser);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }



    [Fact]
    public void EditUser_ReturnsNotFoundForNonExistingUser()
    {
        // Arrange
        var nonExistingUser = new User { Id = 999, Username = "NonExistentUser" };

        // Act
        var result = controller.EditUser(nonExistingUser);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeleteUser_DeletesUser()
    {
        // Arrange
        var userToDelete = new User { Username = "UserToDelete" };
        dbcontext.Users.Add(userToDelete);
        dbcontext.SaveChanges();


        // Act
        var result = controller.DeleteUser(userToDelete.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userToDelete.Id, result.Id);
        Assert.Null(dbcontext.Users.Find(userToDelete.Id));
    }

}


