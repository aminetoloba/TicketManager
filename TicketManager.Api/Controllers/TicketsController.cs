using Microsoft.AspNetCore.Mvc;
using TicketManager.Api.Dtos;
using TicketManager.Api.Models;
using TicketManager.Api.Services;

namespace TicketManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _service;

    public TicketsController(ITicketService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<TicketDto>>> GetAll(
        [FromQuery] int? projectId, [FromQuery] TicketStatus? status)
        => Ok(await _service.GetAllAsync(projectId, status));

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketDto>> GetById(int id)
    {
        var ticket = await _service.GetByIdAsync(id);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketDto dto)
    {
        var created = await _service.CreateAsync(dto);
        if (created is null)
            return BadRequest(new { error = $"Project {dto.ProjectId} does not exist." });

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTicketDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}