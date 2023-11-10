using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTradeApi3.Controllers;
using StockTradeApi3.Models;
using StockTradeApi3.Services;
using System;
using System.Linq;
using System.Security.Claims;
using Xunit;

public class StockTransactionControllerTests
{
    private readonly DbContextOptions<Db1Context> dbContextOptions;
    private readonly IStockService stockService;
    private readonly Db1Context dbContext;
    private StockTransactionController controller;

    public StockTransactionControllerTests()
    {
        dbContextOptions = new DbContextOptionsBuilder<Db1Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        dbContext = new Db1Context(dbContextOptions);
        stockService = new StockService(dbContext);
        controller = new StockTransactionController(stockService, dbContext);

        dbContext.CommissionRates.Add(new CommissionRate { AdminCommissionRate = (decimal)0.05 });
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "testuser")
        };

        var identity = new ClaimsIdentity(claims, "test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public void GetPortfolio_WithValidUser_ReturnsOkResult()
    {
        // Arrange
        var user = new User { Username = "testuser" };

        dbContext.Users.Add(user);
        var portfolio = new Portfolio { UserId = user.Id };
        dbContext.Portfolios.Add(portfolio);
        dbContext.SaveChanges();

        // Act
        var result = controller.GetPortfolio();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var portfolios = Assert.IsType<List<Portfolio>>(okResult.Value);
        Assert.Single(portfolios);
    }

    [Fact]
    public void GetPortfolio_WithInvalidUser_ReturnsNotFound()
    {

        // Act
        var result = controller.GetPortfolio();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void BuyStock_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock { Buy = 50, IsActive = true, Quantity = 100 };
        dbContext.CommissionRates.Add(new CommissionRate { AdminCommissionRate = (decimal)0.05 });
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.SaveChanges();

        var buyTransaction = new BuyTransaction { StockId = 1, Quantity = 5 };

        // Act
        var result = controller.BuyStock(buyTransaction);

        // Assert
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public void BuyStock_InsufficientBalance_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 100 };
        var stock = new Stock { Buy = 50, IsActive = true, Quantity = 100 };
        dbContext.CommissionRates.Add(new CommissionRate { AdminCommissionRate = (decimal)0.05 });
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.SaveChanges();

        var buyTransaction = new BuyTransaction { StockId = 1, Quantity = 5 };

        // Act
        var result = controller.BuyStock(buyTransaction);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void BuyStock_InactiveStock_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock { Buy = 50, IsActive = false, Quantity = 100 };
        dbContext.CommissionRates.Add(new CommissionRate { AdminCommissionRate = (decimal)0.05 });
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.SaveChanges();

        var buyTransaction = new BuyTransaction { StockId = 0, Quantity = 5 };

        // Act
        var result = controller.BuyStock(buyTransaction);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void BuyStock_InsufficientStockQuantity_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock { Buy = 50, IsActive = true, Quantity = 10 };
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.SaveChanges();

        var buyTransaction = new BuyTransaction { StockId = 1, Quantity = 15 };

        // Act
        var result = controller.BuyStock(buyTransaction);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void SellStock_WithValidParameters_ReturnsOkResult()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock { Sell = 60, IsActive = true, Quantity = 100 };
        var portfolio = new Portfolio { UserId = user.Id, StockId = 1, StockQuantity = 10 };
        dbContext.CommissionRates.Add(new CommissionRate { AdminCommissionRate = (decimal)0.05 });

        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.Portfolios.Add(portfolio);
        dbContext.SaveChanges();

        var sellTransaction = new BuyTransaction { StockId = 1, Quantity = 5 };

        // Act
        var result = controller.SellStock(sellTransaction);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void SellStock_InsufficientPortfolioQuantity_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock {  Sell = 60, IsActive = true, Quantity = 100 };
        var portfolio = new Portfolio { UserId = user.Id, StockId = 1, StockQuantity = 5 };
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.Portfolios.Add(portfolio);
        dbContext.SaveChanges();

        var sellTransaction = new BuyTransaction { StockId = 1, Quantity = 10 };

        // Act
        var result = controller.SellStock(sellTransaction);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void SellStock_InactiveStock_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock { Sell = 60, IsActive = false, Quantity = 100 };
        var portfolio = new Portfolio { UserId = user.Id, StockId = 0, StockQuantity = 10 };
        dbContext.CommissionRates.Add(new CommissionRate { AdminCommissionRate = (decimal)0.05 });
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.Portfolios.Add(portfolio);
        dbContext.SaveChanges();

        var sellTransaction = new BuyTransaction { StockId = 1, Quantity = 5 };

        // Act
        var result = controller.SellStock(sellTransaction);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void SellStock_InvalidUser_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock {  Sell = 60, IsActive = true, Quantity = 100 };
        var portfolio = new Portfolio { UserId = user.Id, StockId = 1, StockQuantity = 10 };
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.Portfolios.Add(portfolio);
        dbContext.SaveChanges();

        var sellTransaction = new BuyTransaction { StockId = 1, Quantity = 5 };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = null } 
        };

        // Act
        var result = controller.SellStock(sellTransaction);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }


    //






    [Fact]
    public void SellStock_ZeroQuantity_ReturnsBadRequest()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock {  Sell = 60, IsActive = true, Quantity = 100 };
        var portfolio = new Portfolio { UserId = user.Id, StockId = 1, StockQuantity = 10 };
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.Portfolios.Add(portfolio);
        dbContext.SaveChanges();

        var sellTransaction = new BuyTransaction { StockId = 0, Quantity = 0 };

        // Act
        var result = controller.SellStock(sellTransaction);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetStockData_ReturnsListOfStocks()
    {
        // Arrange

        // Act
        var result = controller.GetStockData();

        // Assert
        var stockList = Assert.IsType<List<Stock>>(result);
        Assert.NotEmpty(stockList);
    }

    [Fact]
    public void BuyStock_WithInactiveStock_ReturnsNotFound()
    {
        // Arrange
        var user = new User { Username = "testuser", Balance = 1000 };
        var stock = new Stock { Buy = 50, IsActive = false, Quantity = 100 };
        dbContext.Users.Add(user);
        dbContext.Stocks.Add(stock);
        dbContext.SaveChanges();

        var buyTransaction = new BuyTransaction { StockId = 0, Quantity = 5 };

        // Act
        var result = controller.BuyStock(buyTransaction);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void ListTransactions_WithInvalidUser_ReturnsNotFound()
    {
        // Arrange
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = null } // Simulate an invalid user context.
        };

        // Act
        var result = controller.ListTransactions();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void ListTransactions_WithValidUser_ReturnsList()
    {
        // Arrange
        var user = new User { Username = "testuser" };
        dbContext.Users.Add(user);
        dbContext.SaveChanges(); // Save changes to get the generated Id
        int count = 10;

        for (int i = 0; i<count; i++) 
        {
            dbContext.StockTransactions.Add(new StockTransaction { UserId = user.Id });
        }


        dbContext.SaveChanges();

        // Act
        var result = controller.ListTransactions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var transactionList = Assert.IsType<List<StockTransaction>>(okResult.Value);
        Assert.Equal(count, transactionList.Count);
    }


}
