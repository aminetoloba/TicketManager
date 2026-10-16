using TicketManager.Api.Dtos;
using TicketManager.Api.Models;

namespace TicketManager.Api.Services;

public interface ITicketService
{
    Task<List<TicketDto>> GetAllAsync(int? projectId, TicketStatus? status);
    Task<TicketDto?> GetByIdAsync(int id);
    Task<TicketDto?> CreateAsync(CreateTicketDto dto);
    Task<bool> UpdateAsync(int id, UpdateTicketDto dto);
    Task<bool> DeleteAsync(int id);
}