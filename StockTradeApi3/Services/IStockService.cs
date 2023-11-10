using System;
using StockTradeApi3.Models;

namespace StockTradeApi3.Services
{
	public interface IStockService
	{
        List<Stock> GetStockData();
        void SetIsActive(Stock stock);
        List<StockHistory> GetHistoryLists();
    }
}

