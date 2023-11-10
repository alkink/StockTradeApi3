using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StockTradeApi3.Models;

public partial class StockTransaction
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? StockId { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? Type { get; set; }

    [JsonIgnore]
    public virtual ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();

    [JsonIgnore]
    public virtual Stock? Stock { get; set; }

    [JsonIgnore]
    public virtual User? User { get; set; }
}
