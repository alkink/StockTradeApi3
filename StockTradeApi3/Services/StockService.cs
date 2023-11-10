using System;
using HtmlAgilityPack;
using StockTradeApi3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using static System.Linq.Enumerable;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using StockTradeApi3.Services;
using System.Collections;
using System.Data.Entity;
using System.Linq;

namespace StockTradeApi3.Services
{
    public class StockService : IStockService
    {
        private readonly Db1Context db1;
        private List<Stock> stockData;
        private HtmlNodeCollection stockRows;
        List<StockHistory> displayhistory;
        StockHistoryLists stockhistorylist;
        public StockService(Db1Context db)
        {
            string url = "https://bigpara.hurriyet.com.tr/borsa/canli-borsa/";
            db1 = db;
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            stockhistorylist = new StockHistoryLists();

            displayhistory = new List<StockHistory>(3);
            stockData = new List<Stock>();

            // Specify the XPath for the elements you want to extract
            string stockXPath = "//*[@id=\"sortable\"]";

            stockRows = doc.DocumentNode.SelectNodes(stockXPath);
            if (stockRows != null)
            {
                foreach (int i in Range(1, 96))
                {
                    string symbol = doc.DocumentNode.SelectSingleNode($"//*[@id=\"sortable\"]/ul[{i}]/li[1]").InnerText.Trim().Replace("&nbsp;", "").Trim();
                    decimal buy = decimal.Parse(doc.DocumentNode.SelectSingleNode($"//*[@id=\"h_td_fiyat_id_{symbol}\"]").InnerText.Trim());
                    decimal sell = decimal.Parse(doc.DocumentNode.SelectSingleNode($"//*[@id=\"h_td_satis_id_{symbol}\"]").InnerText.Trim());
                    double volume = double.Parse(doc.DocumentNode.SelectSingleNode($"//*[@id=\"h_td_hacimlot_id_{symbol}\"]").InnerText.Trim());
                    int volume2 = Convert.ToInt32(volume);

                    Stock stock = db1.Stocks.Find(i);
                    if (stock == null)
                    {
                        Stock stock2 = new Stock
                        {
                            Id = i,
                            IsActive = true,
                            Name = symbol,
                            Buy = buy,
                            Sell = sell,
                            Quantity = 10000, // Assuming you want to set Quantity to 0 for new entities
                            CreatedAt = DateTime.Now
                            
                        };


                        db1.Stocks.Add(stock2);
                    }
                    else
                    {
                        stock.Buy = buy;
                        stock.Sell = sell;
                        UpdateLastUpdateDate(stock);
                        addStockHistory(stock);
                        stockhistorylist = UpdateStockHistory(stock);
                        var values = PushHistoryLists(stock, stockhistorylist);
                        displayhistory.Add(values.EightHourhistory);
                        displayhistory.Add(values.oneDayhistory);
                        displayhistory.Add(values.oneWeekhistory);

                    }

                    db1.SaveChanges();


                }
            }

        }


        public void SetIsActive(Stock stock)
        {
            if (stock != null)
            {
                db1.Stocks.FirstOrDefault(x => x.Id == stock.Id).IsActive = stock.IsActive;
                db1.SaveChanges();
            }
        }


        public List<Stock> GetStockData()
        {
            // StockController'daki GetStockData işlevini burada çağırabilirsiniz.
            stockData = db1.Stocks.ToList<Stock>();
            return stockData;
        }

        private bool IsWeekPassedSinceLastUpdate()
        {
            // Retrieve the last update date from your database or storage
            DateTime lastUpdateDate = GetLastUpdateDate();

            // Calculate the difference in days
            TimeSpan timeSinceLastUpdate = DateTime.Now - lastUpdateDate;

            // Check if a week has passed (7 days or more)
            return timeSinceLastUpdate.TotalDays >= 7;
        }

        private DateTime GetLastUpdateDate()
        {
            
                // Assuming you have a table or entity called "LastUpdateDate" that stores the last update date
                var lastUpdateEntity = db1.Stocks.FirstOrDefault();

                if (lastUpdateEntity != null)
                {
                    return lastUpdateEntity.CreatedAt.Value;
                }

                // If there is no record in the database, return a default date
                return DateTime.MinValue; // or any other suitable default date
            
        }

        private void UpdateLastUpdateDate(Stock stock)
        {
            // Implement this method to update the last update date in your storage to the current date and time.
            if (IsWeekPassedSinceLastUpdate())
            {
                stock.CreatedAt = DateTime.Now;
            }
        }

        private StockHistoryLists UpdateStockHistory(Stock stock)
        {
            StockHistoryLists historyLists = new StockHistoryLists();
            historyLists.EightHourHistoryList = new List<StockHistory>();
            historyLists.OneDayHistoryList = new List<StockHistory>();
            historyLists.OneWeekHistoryList = new List<StockHistory>();
            List<StockHistory> histories1 = db1.StockHistories.Where(p => p.StockId == stock.Id).ToList();
            foreach (StockHistory histories in histories1)
            {

                TimeSpan lastupdate = DateTime.Now - histories.CreatedAt;
                if (lastupdate.TotalHours >= 8)
                {
                    historyLists.EightHourHistoryList.Add(histories);
                }else if (lastupdate.TotalDays >= 1)
                {
                    historyLists.OneDayHistoryList.Add(histories);
                }
                else if (lastupdate.TotalDays >= 7)
                {
                    historyLists.OneWeekHistoryList.Add(histories);
                }
                
            }
            return historyLists;
        }
        private void addStockHistory(Stock stock)
        {
            StockHistory newstockHistory = new StockHistory
            {
                Buy = stock.Buy,
                Sell = stock.Sell,
                Name = stock.Name,
                StockId = stock.Id,
                CreatedAt = DateTime.Now
            };
            db1.StockHistories.Add(newstockHistory);
            db1.SaveChanges();
        }

        public (StockHistory EightHourhistory, StockHistory oneDayhistory, StockHistory oneWeekhistory) PushHistoryLists(Stock stock, StockHistoryLists stockhistorylist)
        {
            List<StockHistory> displayhistory = new List<StockHistory>();
            StockHistory EightHourhistory = new StockHistory { StockId = stock.Id, Name = stock.Name, Buy = 0, Sell = 0 };
            StockHistory oneDayhistory = new StockHistory { StockId = stock.Id, Name = stock.Name, Buy = 0, Sell = 0 };
            StockHistory oneWeekhistory = new StockHistory { StockId = stock.Id, Name = stock.Name, Buy = 0, Sell = 0 };

            if (stockhistorylist.EightHourHistoryList!=null) {
                foreach (StockHistory EightHourHistory in stockhistorylist.EightHourHistoryList)
                {
                    EightHourhistory.Buy += EightHourHistory.Buy;
                    EightHourhistory.Sell += EightHourHistory.Sell;
                }
                EightHourhistory.Buy /= stockhistorylist.EightHourHistoryList.Count;
                EightHourhistory.Sell /= stockhistorylist.EightHourHistoryList.Count;
            }


            if (stockhistorylist.OneDayHistoryList != null && stockhistorylist.OneDayHistoryList.Count != 0)
            {
                foreach (StockHistory OneDayHistoryList in stockhistorylist.OneDayHistoryList)
                {
                    oneDayhistory.Buy += OneDayHistoryList.Buy;
                    oneDayhistory.Sell += OneDayHistoryList.Sell;
                }
                oneDayhistory.Buy /= stockhistorylist.OneDayHistoryList.Count;
                oneDayhistory.Sell /= stockhistorylist.OneDayHistoryList.Count;
            }


            if (stockhistorylist.OneWeekHistoryList != null && stockhistorylist.OneWeekHistoryList.Count != 0)
            {
                foreach (StockHistory OneWeekHistoryList in stockhistorylist.OneWeekHistoryList)
                {
                    oneWeekhistory.Buy += OneWeekHistoryList.Buy;
                    oneWeekhistory.Sell += OneWeekHistoryList.Sell;
                }
                oneWeekhistory.Buy /= stockhistorylist.OneWeekHistoryList.Count;
                oneWeekhistory.Sell /= stockhistorylist.OneWeekHistoryList.Count;

            }
            return (EightHourhistory, oneDayhistory, oneWeekhistory);


        }

        public List<StockHistory> GetHistoryLists()
        {
            return displayhistory;
        }
    }
}


