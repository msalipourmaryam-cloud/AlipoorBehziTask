using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Application.Tools;
using Microsoft.AspNetCore.Mvc;

namespace AlipoorBehTask.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController(
    IQueryHandler<GetRequestsSummaryQuery, RequestsSummary> getRequestsSummary) : ControllerBase
{
    [HttpGet("requests-summary")]
    [ProducesResponseType<RequestsSummary>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RequestsSummary>> GetRequestsSummary(CancellationToken cancellationToken)
    {
        return Ok(await getRequestsSummary.HandleAsync(new GetRequestsSummaryQuery(), cancellationToken));
    }
}