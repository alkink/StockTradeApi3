using System;
namespace StockTradeApi3.Models
{
	public class StockHistoryLists
	{
        public List<StockHistory>? EightHourHistoryList { get; set; }
        public List<StockHistory>? OneDayHistoryList { get; set; }
        public List<StockHistory>? OneWeekHistoryList { get; set; }
    }
}

