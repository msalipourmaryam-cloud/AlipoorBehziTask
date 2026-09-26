using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Application.Tools;
using Microsoft.AspNetCore.Mvc;

namespace AlipoorBehTask.Api.Controllers;

[ApiController]
[Route("api/beneficiaries")]
public sealed class BeneficiariesController(
    ICommandHandler<CreateBeneficiaryCommand, BeneficiaryResponse> createBeneficiary,
    IQueryHandler<GetBeneficiaryQuery, BeneficiaryResponse> getBeneficiary,
    IQueryHandler<GetBeneficiariesQuery, PagedResult<BeneficiaryResponse>> getBeneficiaries) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<BeneficiaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BeneficiaryResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "nationalId",
        [FromQuery] string sortDirection = "asc",
        CancellationToken cancellationToken = default)
    {
        return Ok(await getBeneficiaries.HandleAsync(
            new GetBeneficiariesQuery(page, pageSize, search, sortBy, sortDirection),
            cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType<BeneficiaryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BeneficiaryResponse>> Create(
        CreateBeneficiaryInput input,
        CancellationToken cancellationToken)
    {
        var response = await createBeneficiary.HandleAsync(new CreateBeneficiaryCommand(input), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<BeneficiaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BeneficiaryResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await getBeneficiary.HandleAsync(new GetBeneficiaryQuery(id), cancellationToken));
    }
}