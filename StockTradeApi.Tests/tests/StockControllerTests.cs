using System;
using Microsoft.EntityFrameworkCore;
using StockTradeApi3.Controllers;
using StockTradeApi3.Models;
using StockTradeApi3.Services;

namespace StockTradeApiTests.tests
{
    public class StockControllerTests
    {
        private readonly Db1Context dbContext;
        private StocksController controller;
        private IStockService service;

        public StockControllerTests()
        {

            var options = new DbContextOptionsBuilder<Db1Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            dbContext = new Db1Context(options);
            service = new StockService(dbContext);

            // Make sure to pass the IStockService implementation to the controller
            controller = new StocksController(service);
        }

        [Fact]
        public void GetStockData_ReturnsStockData()
        {
            // Arrange: Insert some test data into the in-memory database
            dbContext.Stocks.Add(new Stock {Name= "stock123",IsActive = true});
            dbContext.SaveChanges();

            // Act: Call the GetStockData method
            var result = controller.GetStockData();

            // Assert: Check if the result is as expected
            Assert.NotEmpty(result);
        }

        [Fact]
        public void SetIsActive_SetsStockToActive()
        {
            // Arrange: Insert a test stock into the in-memory database
            var testStock = new Stock { Id = 1, Name = "stock123" ,IsActive = true} ;
            dbContext.Stocks.Add(testStock);
            dbContext.SaveChanges();

            // Act: Call the SetIsActive method with the test stock
            controller.SetIsActive(testStock);

            // Assert: Check if the stock is set to active
            Assert.True(testStock.IsActive);
        }
        

        [Fact]
        public void GetStockData_ReturnsEmptyListWhenNoStocks()
        {
            // Act: Call the GetStockData method with no data in the database
            var result = controller.GetStockData();

            // Assert: Check if the result is an empty list
            Assert.Empty(result);
        }

        [Fact]
        public void SetIsActive_DoesNotChangeInactiveStock()
        {
            // Arrange: Insert a test stock with IsActive=false into the in-memory database
            var testStock = new Stock { IsActive = false, /* Initialize other stock properties */ };
            dbContext.Stocks.Add(testStock);
            dbContext.SaveChanges();

            // Act: Call the SetIsActive method with the test stock
            controller.SetIsActive(testStock);

            // Assert: Check if the stock is still inactive
            Assert.False(testStock.IsActive);
        }
    }
}



