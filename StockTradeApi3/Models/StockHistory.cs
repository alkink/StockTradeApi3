using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StockTradeApi3.Models;

public partial class StockHistory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? Buy { get; set; }

    public decimal? Sell { get; set; }

    [JsonIgnore]
    public DateTime CreatedAt { get; set; }
    [JsonIgnore]
    public int? StockId { get; set; }
    [JsonIgnore]
    public virtual Stock? Stock { get; set; }
}
