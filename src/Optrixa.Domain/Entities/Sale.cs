using System.Text.Json.Serialization;
using Optrixa.Domain.Common;
using Optrixa.Domain.Enums;

namespace Optrixa.Domain.Entities;

public class Sale : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxRate { get; set; } = 0;
    public decimal TaxAmount { get; set; }
    public decimal Discount { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    // Navigation
    public Customer? Customer { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}