using System;
using System.Collections.Generic;

namespace StockTradeApi3.Models;

public partial class SellTransaction
{
    public int? PortfolioId { get; set; }

    public int? Quantity { get; set; }

}
