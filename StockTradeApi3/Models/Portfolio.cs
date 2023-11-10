using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StockTradeApi3.Models;

public partial class Portfolio
{
    public int PortfolioId { get; set; }

    public int? UserId { get; set; }

    public int StockQuantity { get; set; }

    public decimal? BuyValue { get; set; }

    public int? StockId { get; set; }

    public int? StockTransactionId { get; set; }

    [JsonIgnore]
    public virtual Stock? Stock { get; set; }

    [JsonIgnore]
    public virtual StockTransaction? StockTransaction { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
