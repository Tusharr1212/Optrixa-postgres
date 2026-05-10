using System.Text.Json.Serialization;
using Optrixa.Domain.Common;

namespace Optrixa.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPurchased { get; set; } = 0;
    public decimal TotalPaid { get; set; } = 0;
    public decimal OutstandingBalance => TotalPurchased - TotalPaid;

    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();

    // Keep this one — we need it for the supplier page
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}