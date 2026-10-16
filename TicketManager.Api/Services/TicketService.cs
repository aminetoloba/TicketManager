using Microsoft.EntityFrameworkCore;
using TicketManager.Api.Data;
using TicketManager.Api.Dtos;
using TicketManager.Api.Models;

namespace TicketManager.Api.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;

    public TicketService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TicketDto>> GetAllAsync(int? projectId, TicketStatus? status)
    {
        var query = _context.Tickets.AsNoTracking();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();
    }

    public async Task<TicketDto?> GetByIdAsync(int id)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return ticket is null ? null : ToDto(ticket);
    }

    public async Task<TicketDto?> CreateAsync(CreateTicketDto dto)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == dto.ProjectId);
        if (!projectExists) return null;

        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            ProjectId = dto.ProjectId
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return ToDto(ticket);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTicketDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket is null) return false;

        ticket.Title = dto.Title;
        ticket.Description = dto.Description;
        ticket.Status = dto.Status;
        ticket.Priority = dto.Priority;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket is null) return false;

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();

        return true;
    }

    private static TicketDto ToDto(Ticket t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        CreatedAt = t.CreatedAt,
        ProjectId = t.ProjectId
    };
}