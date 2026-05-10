using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optrixa.Application.Common;
using Optrixa.Application.Features.Dashboard.Queries;

namespace Optrixa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        // Explicitly convert to UTC DateTime
        DateTime? utcFrom = fromDate.HasValue
            ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc)
            : null;

        DateTime? utcTo = toDate.HasValue
            ? DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc)
            : null;

        var result = await _mediator.Send(
            new GetDashboardSummaryQuery(utcFrom, utcTo));
        return Ok(result);
    }

    [HttpGet("daily/{year:int}/{month:int}")]
    public async Task<IActionResult> GetDailyBreakdown(
        int year, int month)
    {
        if (month < 1 || month > 12)
            return BadRequest("Invalid month.");

        var result = await _mediator.Send(
            new GetDailyBreakdownQuery(year, month));
        return Ok(result);
    }
}