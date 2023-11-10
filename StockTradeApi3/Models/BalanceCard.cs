using System;
using System.Collections.Generic;

namespace StockTradeApi3.Models;

public partial class BalanceCard
{
    public int CardId { get; set; }

    public decimal BalanceAmount { get; set; }

    public bool IsUsed { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
