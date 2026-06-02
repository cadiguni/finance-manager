using FinTrack.Application.Forecast;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Controllers;

[ApiController]
[Route("api/forecast")]
public sealed class ForecastController : ControllerBase
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IForecastService _forecastService;

    public ForecastController(IForecastService forecastService)
    {
        _forecastService = forecastService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ForecastMonthDto>>> GetNextMonths(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int months = 6,
        CancellationToken cancellationToken = default)
    {
        if (month is < 1 or > 12)
        {
            return BadRequest(new { message = "Month must be between 1 and 12." });
        }

        var forecast = await _forecastService.GetNextMonthsAsync(
            DemoUserId,
            year,
            month,
            months,
            cancellationToken);

        return Ok(forecast);
    }
}
