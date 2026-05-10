using Microsoft.EntityFrameworkCore;
using Optrixa.Domain.Entities;
using Optrixa.Domain.Interfaces;

namespace Optrixa.Infrastructure.Persistence.Repositories;

public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Sale>> GetByDateRangeAsync(
    DateTime from, DateTime to)
{
    var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
    var utcTo   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);

    return await _dbSet
        .Include(x => x.Items)
            .ThenInclude(x => x.Product)
        .Include(x => x.Customer)
        .Where(x => x.SaleDate >= utcFrom && x.SaleDate <= utcTo)
        .OrderByDescending(x => x.SaleDate)
        .ToListAsync();
}

public async Task<decimal> GetTotalRevenueAsync(
    DateTime from, DateTime to)
{
    var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
    var utcTo   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);

    return await _dbSet
        .Where(x => x.SaleDate >= utcFrom && x.SaleDate <= utcTo)
        .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;
}

public async Task<string> GenerateInvoiceNumberAsync()
{
    var count = await _dbSet.CountAsync();
    return $"INV-{DateTime.UtcNow.Year}-{(count + 1):D5}";
}

public async Task<Sale?> GetWithItemsAsync(int id) =>
    await _dbSet
        .Include(x => x.Items)
            .ThenInclude(x => x.Product)
        .Include(x => x.Customer)
        .FirstOrDefaultAsync(x => x.Id == id);

}