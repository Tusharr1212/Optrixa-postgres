using System.Text.Json.Serialization;
using Optrixa.Domain.Common;

namespace Optrixa.Domain.Entities;

public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation
    [JsonIgnore]
    public Sale Sale { get; set; } = null!;

    public Product Product { get; set; } = null!;
}