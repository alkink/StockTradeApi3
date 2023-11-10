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
            int Stockid = (int)buy.StockId!;
            int quantity = (int)buy.Quantity!;

            string? userNameToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userNameToken))
            {
                User user = db1.Users.FirstOrDefault(x => x.Username == userNameToken);

                if (user != null && db1.Stocks.FirstOrDefault(x => x.Id == Stockid).IsActive == true &&
                    user.Balance >= quantity * stockData.Find(s => s.Id == Stockid).Buy &&
                    db1.Stocks.FirstOrDefault(x=> x.Id == Stockid).Quantity >= quantity )
                {
                    Stock stock = stockData.Find(s => s.Id == Stockid);
                    StockTransaction transaction = new StockTransaction { UserId = user.Id, Quantity = quantity, StockId = Stockid, Price = stock!.Buy, Type = "Buy", TransactionDate = DateTime.Now };
                    db1.StockTransactions.Add(transaction);
                    db1.SaveChanges();


                    if (db1.Portfolios.FirstOrDefault(x => x.StockId == Stockid) != null)
                    {
                        Portfolio portfolio = db1.Portfolios.FirstOrDefault(x => x.StockId == Stockid);
                        portfolio.StockQuantity += quantity;
                    }
                    else
                    {
                        Portfolio portfolio = new Portfolio { UserId = user.Id, StockId = Stockid, StockQuantity = quantity, StockTransaction = db1.StockTransactions.FirstOrDefault(x => x.Id == transaction.Id) };
                        db1.Portfolios.Add(portfolio);
                    }
                    db1.SaveChanges();
                    updateStock(stock, quantity, "Buy");
                    updateBuyBalance(stock, user, quantity);
                    db1.SaveChanges();



                    return Ok();
                }
                db1.SaveChanges();
            }
            return NotFound();
        }

        [HttpPost("Sell")]
        public IActionResult SellStock([FromBody] BuyTransaction sell)
        {
            int StockId = (int)sell.StockId!;
            int quantity = (int)sell.Quantity!;

            string? userNameToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userNameToken))
            {
                User user = db1.Users.FirstOrDefault(x => x.Username == userNameToken);

                if (user != null)
                {

                    Portfolio portfolio = db1.Portfolios.FirstOrDefault(x => x.StockId == StockId);
                    if (portfolio != null && db1.Stocks.FirstOrDefault(x=>x.Id == StockId).IsActive == true)
                    {
                        Stock stock = stockData.Find(s => s.Id == portfolio.StockId);
                        if ((portfolio.StockQuantity - quantity) > 0)
                        {
                            portfolio.StockQuantity -= quantity;
                            stock.Quantity += quantity;
                            SaveTransaction(stock, user, quantity, "Sell");
                        }
                        else if ((portfolio.StockQuantity - quantity) == 0)
                        {
                            SaveTransaction(stock, user, quantity, "Sell");
                            stock.Quantity += quantity;
                            db1.Remove(portfolio);

                        }
                        else
                        {
                            return NotFound("you dont have enough amount of stock ");
                        }
                        db1.SaveChanges();
                        updateStock(stock, quantity, "Sell");
                        updateSellBalance(stock, user, portfolio);
                    }
                    else
                    {
                        return NotFound("you don't have any of this stock ");
                    }

                    return Ok(portfolio);
                }
                else
                {
                    return NotFound("you don't have enough budget");
                }
            }
            return NotFound();
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

