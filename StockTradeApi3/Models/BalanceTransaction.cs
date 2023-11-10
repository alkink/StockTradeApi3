using System;
using System.Collections.Generic;

namespace StockTradeApi3.Models;

public partial class BalanceTransaction
{
    public int BalanceTransactionId { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? TransactionDate { get; set; }

    public virtual User? User { get; set; }
}
