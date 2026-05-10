using System.Text.Json.Serialization;
using Optrixa.Domain.Common;

namespace Optrixa.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;

    // JsonIgnore prevents circular reference when serializing
    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();

    [JsonIgnore]
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}