using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTradeApi3.Models;
using StockTradeApi3.Controllers;
using StockTradeApi3.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Runtime.ConstrainedExecution;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace StockTradeApi3.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [Consumes("application/json")]
    public class StockTransactionController : ControllerBase
    {
        private List<Stock> stockData;
        private readonly Db1Context db1;
        private readonly IStockService _stockService;
        public StockTransactionController(IStockService stockService, Db1Context db)
        {

            db1 = db;
            _stockService = stockService;
            stockData = _stockService.GetStockData();
        }



        [HttpGet("ListPortfolios")]
        public IActionResult GetPortfolio()
        {
            string? userNameToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userNameToken))
            {
                User user = db1.Users.FirstOrDefault(x => x.Username == userNameToken);

                if (user != null)
                {
                    List<Portfolio> portfolios = db1.Portfolios.Where(p => p.UserId == user.Id).ToList();


                    return Ok(portfolios);
                }
            }

            // Kullanıcı bulunamazsa veya kimlik bilgisi eksikse uygun bir yanıt döndürün.
            return NotFound();
        }

        [HttpGet("GetStock")]
        public List<Stock> GetStockData()
        {
            List<Stock> stockData = _stockService.GetStockData();
            return stockData;
        }




        [HttpPost("Buy")]
        public IActionResult BuyStock([FromBody] BuyTransaction buy)
        {
            int stockId = (int)buy.StockId!;
            int quantity = (int)buy.Quantity!;

            if (quantity <= 0 || stockId < 0)
            {
                return BadRequest();
            }

            string? userNameToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userNameToken))
            {
                return NotFound();
            }

            User user = db1.Users.FirstOrDefault(x => x.Username == userNameToken);
            Stock stock = stockData.Find(s => s.Id == stockId);

            bool stockIsActive = db1.Stocks.FirstOrDefault(x => x.Id == stockId)?.IsActive == true;
            bool enoughBalance = user?.Balance >= quantity * (stock?.Buy ?? 0);
            bool enoughQuantity = db1.Stocks.FirstOrDefault(x => x.Id == stockId)?.Quantity >= quantity;

            if (user == null || stock == null || !stockIsActive || !enoughBalance || !enoughQuantity)
            {
                db1.SaveChanges();
                return NotFound();
            }

            StockTransaction transaction = new StockTransaction
            {
                UserId = user.Id,
                Quantity = quantity,
                StockId = stockId,
                Price = stock.Buy,
                Type = "Buy",
                TransactionDate = DateTime.Now
            };

            db1.StockTransactions.Add(transaction);
            db1.SaveChanges();

            Portfolio portfolio = db1.Portfolios.FirstOrDefault(x => x.StockId == stockId);
            if (portfolio != null)
            {
                portfolio.StockQuantity += quantity;
            }
            else
            {
                Portfolio newPortfolio = new Portfolio
                {
                    UserId = user.Id,
                    StockId = stockId,
                    StockQuantity = quantity,
                    StockTransaction = db1.StockTransactions.FirstOrDefault(x => x.Id == transaction.Id)
                };
                db1.Portfolios.Add(newPortfolio);
            }

            db1.SaveChanges();
            updateStock(stock, quantity, "Buy");
            updateBuyBalance(stock, user, quantity);

            return Ok();
        }


        [HttpPost("Sell")]
        public IActionResult SellStock([FromBody] BuyTransaction sell)
        {
            int stockId = (int)sell.StockId!;
            int quantity = (int)sell.Quantity!;

            if (quantity <= 0 || stockId < 0)
            {
                return BadRequest();
            }

            string? userNameToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userNameToken))
            {
                return NotFound();
            }

            User user = db1.Users.FirstOrDefault(x => x.Username == userNameToken);
            if (user == null)
            {
                return NotFound("You don't have enough budget.");
            }

            Portfolio portfolio = db1.Portfolios.FirstOrDefault(x => x.StockId == stockId);
            Stock stock = stockData.Find(s => s.Id == portfolio?.StockId);

            if (portfolio == null || stock == null || (db1.Stocks.FirstOrDefault(x => x.Id == stockId)?.IsActive != true))
            {
                return NotFound("You don't have any of this stock.");
            }

            if (portfolio.StockQuantity < quantity)
            {
                return NotFound("You don't have enough amount of stock.");
            }

            portfolio.StockQuantity -= quantity;
            stock.Quantity += quantity;

            SaveTransaction(stock, user, quantity, "Sell");

            if (portfolio.StockQuantity == 0)
            {
                db1.Remove(portfolio);
            }

            db1.SaveChanges();
            updateStock(stock, quantity, "Sell");
            updateSellBalance(stock, user, portfolio);

            return Ok(portfolio);
        }


        public void SaveTransaction(Stock stock, User user, int quantity, string buyOrSell)
        {

            StockTransaction transaction = new StockTransaction
            {
                UserId = user.Id,
                Quantity = quantity,
                StockId = stock.Id,
                Price = stock!.Sell,
                Type = buyOrSell,
                TransactionDate = DateTime.Now
            };
            db1.StockTransactions.Add(transaction);
            db1.SaveChanges();


        }


        public void updateStock(Stock stock, int quantity, string buyOrSell)
        {
            Stock stockdb = db1.Stocks.FirstOrDefault(x => x.Id == stock.Id)!;
            if (stockdb != null)
            {
                if (buyOrSell == "Sell")
                {
                    stockdb.Quantity += quantity;
                }
                else if (buyOrSell == "Buy")
                {
                    stockdb.Quantity -= quantity;
                }
            }
            db1.SaveChanges();

        }
        public void updateBuyBalance(Stock stock, User user, int quantity)
        {
            User userdb = db1.Users.FirstOrDefault(x => x.Id == user.Id)!;
            if (userdb != null)
            {
                userdb.Balance -= ((stock.Buy * quantity)* db1.CommissionRates.FirstOrDefault().AdminCommissionRate);
                db1.SaveChanges();
            }


        }
        public void updateSellBalance(Stock stock, User user, Portfolio portfolio)
        {
            User userdb = db1.Users.FirstOrDefault(x => x.Id == user.Id)!;
            if (userdb != null)
            {
                userdb.Balance += ((stock.Sell * portfolio.StockQuantity * db1.CommissionRates.FirstOrDefault().AdminCommissionRate));
                db1.SaveChanges();
            }
        }

        [HttpGet("ListTransactions")]
        public IActionResult ListTransactions()
        {
            string? userNameToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userNameToken))
            {
                User user = db1.Users.FirstOrDefault(x => x.Username == userNameToken)!;

                if (user != null)
                {
                    List<StockTransaction> transactions = db1.StockTransactions.Where(p => p.UserId == user.Id).ToList();
                    return Ok(transactions);
                }
            }

            // Eğer kullanıcı bulunamazsa veya diğer koşullar sağlanmazsa, NotFound sonucunu döndürün.
            return NotFound();
        }

    }
}

