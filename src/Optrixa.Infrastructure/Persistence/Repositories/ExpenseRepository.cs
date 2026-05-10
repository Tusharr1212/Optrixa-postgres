using Microsoft.EntityFrameworkCore;
using Optrixa.Domain.Entities;
using Optrixa.Domain.Interfaces;

namespace Optrixa.Infrastructure.Persistence.Repositories;

public class ExpenseRepository : Repository<Expense>, IExpenseRepository
{
    public ExpenseRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Expense>> GetByDateRangeAsync(
    DateTime from, DateTime to)
{
    var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
    var utcTo   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);

    return await _dbSet
        .Where(x => x.ExpenseDate >= utcFrom && x.ExpenseDate <= utcTo)
        .Include(x => x.Category)
        .OrderByDescending(x => x.ExpenseDate)
        .ToListAsync();
}

public async Task<decimal> GetTotalExpensesAsync(
    DateTime from, DateTime to)
{
    var utcFrom = DateTime.SpecifyKind(from, DateTimeKind.Utc);
    var utcTo   = DateTime.SpecifyKind(to,   DateTimeKind.Utc);

    return await _dbSet
        .Where(x => x.ExpenseDate >= utcFrom && x.ExpenseDate <= utcTo)
        .SumAsync(x => (decimal?)x.Amount) ?? 0;
}

public async Task<IEnumerable<object>> GetMonthlySummaryAsync(int year)
{
    return await _dbSet
        .Where(x => x.ExpenseDate.Year == year)
        .GroupBy(x => x.ExpenseDate.Month)
        .Select(g => new
        {
            Month = g.Key,
            Total = g.Sum(x => x.Amount),
            Count = g.Count()
        })
        .Cast<object>()
        .ToListAsync();
}
}