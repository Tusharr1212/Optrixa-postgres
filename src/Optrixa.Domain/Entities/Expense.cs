using System.Text.Json.Serialization;
using Optrixa.Domain.Common;

namespace Optrixa.Domain.Entities;

public class Expense : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int? SupplierId { get; set; }
    public bool IsPaid { get; set; } = true;
    public DateTime? PaidAt { get; set; }

    // Navigation — JsonIgnore on back-references
    public Category Category { get; set; } = null!;

    [JsonIgnore]
    public Supplier? Supplier { get; set; }
}