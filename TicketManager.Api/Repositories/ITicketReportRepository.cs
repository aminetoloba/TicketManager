using TicketManager.Api.Dtos;

namespace TicketManager.Api.Repositories;

public interface ITicketReportRepository
{
    Task<List<ProjectTicketSummaryDto>> GetOpenTicketsByProjectAsync();
}