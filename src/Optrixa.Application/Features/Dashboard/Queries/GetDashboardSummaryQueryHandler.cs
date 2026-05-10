using MediatR;
using Optrixa.Application.Common;
using Optrixa.Domain.Interfaces;

namespace Optrixa.Application.Features.Dashboard.Queries;

public class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, ApiResponse<DashboardSummaryDto>>
{
    private readonly IUnitOfWork _uow;

    public GetDashboardSummaryQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<ApiResponse<DashboardSummaryDto>> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // ── Today ─────────────────────────────────────────────
        var todayStart = new DateTime(now.Year, now.Month, now.Day,
            0, 0, 0, DateTimeKind.Utc);
        var todayEnd = todayStart.AddDays(1);

        // ── Selected Period ───────────────────────────────────
        var periodStart = request.FromDate.HasValue
    ? new DateTime(
        request.FromDate.Value.Year,
        request.FromDate.Value.Month,
        request.FromDate.Value.Day,
        0, 0, 0, DateTimeKind.Utc)
    : new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

var periodEnd = request.ToDate.HasValue
    ? new DateTime(
        request.ToDate.Value.Year,
        request.ToDate.Value.Month,
        request.ToDate.Value.Day,
        23, 59, 59, DateTimeKind.Utc)
    : new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
        .AddMonths(1);

        // ── Revenue ───────────────────────────────────────────
        var revenueToday = await _uow.Sales
            .GetTotalRevenueAsync(todayStart, todayEnd);
        var revenuePeriod = await _uow.Sales
            .GetTotalRevenueAsync(periodStart, periodEnd);

        // ── Expenses ──────────────────────────────────────────
        var expensesToday = await _uow.Expenses
            .GetTotalExpensesAsync(todayStart, todayEnd);
        var expensesPeriod = await _uow.Expenses
            .GetTotalExpensesAsync(periodStart, periodEnd);

        // ── Products ──────────────────────────────────────────
        var allProducts = await _uow.Products.GetAllAsync();
        var lowStockProducts = await _uow.Products
            .GetLowStockProductsAsync();

        // ── Sales Counts ──────────────────────────────────────
        var salesToday = await _uow.Sales
            .GetByDateRangeAsync(todayStart, todayEnd);
        var salesPeriod = await _uow.Sales
            .GetByDateRangeAsync(periodStart, periodEnd);

        // ── Monthly Chart Data (last 6 months) ────────────────
        var monthlyRevenue = new List<MonthlyDataPoint>();
        var monthlyExpenses = new List<MonthlyDataPoint>();

        for (int i = 5; i >= 0; i--)
        {
            // Build dates explicitly to avoid PostgreSQL issues
            var monthStart = new DateTime(
                now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(-i);

            var monthEnd = new DateTime(
                monthStart.Year, monthStart.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(1);

            var monthName = monthStart.ToString("MMM yyyy");

            var rev = await _uow.Sales
                .GetTotalRevenueAsync(monthStart, monthEnd);
            var exp = await _uow.Expenses
                .GetTotalExpensesAsync(monthStart, monthEnd);

            monthlyRevenue.Add(new MonthlyDataPoint
            {
                Month = monthName,
                Amount = rev
            });

            monthlyExpenses.Add(new MonthlyDataPoint
            {
                Month = monthName,
                Amount = exp
            });
        }

        var summary = new DashboardSummaryDto
        {
            RevenueToday       = revenueToday,
            RevenueThisMonth   = revenuePeriod,
            ExpensesToday      = expensesToday,
            ExpensesThisMonth  = expensesPeriod,
            ProfitToday        = revenueToday - expensesToday,
            ProfitThisMonth    = revenuePeriod - expensesPeriod,
            TotalProducts      = allProducts.Count(),
            LowStockCount      = lowStockProducts.Count(),
            TotalSalesToday    = salesToday.Count(),
            TotalSalesThisMonth = salesPeriod.Count(),
            MonthlyRevenue     = monthlyRevenue,
            MonthlyExpenses    = monthlyExpenses,
            LowStockProducts   = lowStockProducts.Select(p => new LowStockProduct
            {
                Id                = p.Id,
                Name              = p.Name,
                SKU               = p.SKU,
                StockQuantity     = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold
            }).ToList()
        };

        return ApiResponse<DashboardSummaryDto>.Ok(summary);
    }
}