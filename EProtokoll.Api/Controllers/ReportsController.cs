using EProtokoll.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EProtokoll.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports)
    {
        _reports = reports;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var report = await _reports.GetSummaryAsync(from, to);
        return Ok(report);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> Overdue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var report = await _reports.GetOverdueAsync(from, to);
        return Ok(report);
    }

    [HttpGet("by-user")]
    public async Task<IActionResult> ByUser([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var report = await _reports.GetByUserAsync(from, to);
        return Ok(report);
    }

    [HttpGet("tracking")]
    public async Task<IActionResult> Tracking([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var report = await _reports.GetTrackingAsync(from, to);
        return Ok(report);
    }

    [HttpGet("by-priority")]
    public async Task<IActionResult> ByPriority([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var report = await _reports.GetByPriorityAsync(from, to);
        return Ok(report);
    }

    [HttpGet("by-status")]
    public async Task<IActionResult> ByStatus([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var report = await _reports.GetByStatusAsync(from, to);
        return Ok(report);
    }

    [HttpGet("by-department")]
    public async Task<IActionResult> ByDepartment([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var report = await _reports.GetByDepartmentAsync(from, to);
        return Ok(report);
    }
}
