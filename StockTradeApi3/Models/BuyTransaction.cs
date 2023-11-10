using System;
using System.Collections.Generic;

namespace StockTradeApi3.Models;

public partial class BuyTransaction
{
    public int? StockId { get; set; }

    public int? Quantity { get; set; }

}
