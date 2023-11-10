using System;
using System.Collections.Generic;

namespace StockTradeApi3.Models;

public partial class CommissionRate
{
    public int RateId { get; set; }

    public decimal AdminCommissionRate { get; set; }
}
