using System.Text.Json.Serialization;
using Optrixa.Domain.Common;

namespace Optrixa.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 10;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Category Category { get; set; } = null!;
    public Supplier? Supplier { get; set; }

    [JsonIgnore]
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    [JsonIgnore]
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}