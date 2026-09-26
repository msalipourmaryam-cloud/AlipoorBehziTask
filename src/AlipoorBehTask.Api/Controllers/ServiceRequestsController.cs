using System.ComponentModel.DataAnnotations;
using AlipoorBehTask.Application.Commands;
using AlipoorBehTask.Application.Queries;
using AlipoorBehTask.Application.Tools;
using AlipoorBehTask.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AlipoorBehTask.Api.Controllers;

[ApiController]
[Route("api/service-requests")]
public sealed class ServiceRequestsController(
    ICommandHandler<CreateServiceRequestCommand, ServiceRequestResponse> createServiceRequest,
    IQueryHandler<GetServiceRequestQuery, ServiceRequestResponse> getServiceRequest,
    IQueryHandler<GetPriorityQueueQuery, QueuePage> getPriorityQueue,
    ICommandHandler<ChangeServiceRequestStatusCommand> changeStatus) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ServiceRequestResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestResponse>> Create(
        CreateServiceRequestInput input,
        CancellationToken cancellationToken)
    {
        var response = await createServiceRequest.HandleAsync(new CreateServiceRequestCommand(input), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ServiceRequestResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServiceRequestResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await getServiceRequest.HandleAsync(new GetServiceRequestQuery(id), cancellationToken));
    }

    [HttpGet("queue")]
    [ProducesResponseType<QueuePage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<QueuePage>> GetQueue(
        [FromQuery, Range(1, int.MaxValue)] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        [FromQuery] ServiceType? serviceType = null,
        [FromQuery] RequestStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "priority",
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        return Ok(await getPriorityQueue.HandleAsync(
            new GetPriorityQueueQuery(page, pageSize, serviceType, status, search, sortBy, sortDirection),
            cancellationToken));
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        ChangeRequestStatusInput input,
        CancellationToken cancellationToken)
    {
        await changeStatus.HandleAsync(new ChangeServiceRequestStatusCommand(id, input), cancellationToken);
        return NoContent();
    }
}