namespace TicketManager.Api.Dtos;

public class ProjectTicketSummaryDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int OpenTickets { get; set; }
    public int HighPriorityOpenTickets { get; set; }
}