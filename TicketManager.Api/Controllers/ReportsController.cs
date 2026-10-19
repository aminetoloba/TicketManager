using Microsoft.AspNetCore.Mvc;
using TicketManager.Api.Dtos;
using TicketManager.Api.Repositories;

namespace TicketManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ITicketReportRepository _repository;

    public ReportsController(ITicketReportRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("open-tickets-by-project")]
    public async Task<ActionResult<List<ProjectTicketSummaryDto>>> GetOpenTicketsByProject()
        => Ok(await _repository.GetOpenTicketsByProjectAsync());
}