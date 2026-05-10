// Features/Dashboard/Queries/GetDashboardSummaryQuery.cs
using MediatR;
using Optrixa.Application.Common;

namespace Optrixa.Application.Features.Dashboard.Queries;

public record GetDashboardSummaryQuery(
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null)
    : IRequest<ApiResponse<DashboardSummaryDto>>;