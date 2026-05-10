using System.Text.Json.Serialization;
using Optrixa.Domain.Common;

namespace Optrixa.Domain.Entities;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal TotalPurchases { get; set; } = 0;

    [JsonIgnore]
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}