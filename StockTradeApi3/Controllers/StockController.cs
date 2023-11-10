using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using static System.Linq.Enumerable;
using StockTradeApi3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StockTradeApi3.Services;

namespace StockTradeApi3.Controllers
{
    [Authorize]
    [Route("api/stocks")]
    [ApiController]
    public class StocksController : IStockService
    {
        private List<Stock> stockData;
        private HtmlNodeCollection stockRows;
        private readonly IStockService _stockService;


        public StocksController(IStockService stockService)
        {
            _stockService = stockService;
            
        }

        [HttpGet]
        public List<Stock> GetStockData()
        {
            List<Stock> stockData = _stockService.GetStockData();
            return stockData;
        }

        [HttpPost]
        public void SetIsActive(Stock stock)
        {
            _stockService.SetIsActive(stock);
        }


        [HttpGet("GetHistoryLists")]
        public List<StockHistory> GetHistoryLists()
        {
            return _stockService.GetHistoryLists();

        }


    }

}
