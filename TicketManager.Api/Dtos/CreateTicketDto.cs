using System.ComponentModel.DataAnnotations;
using TicketManager.Api.Models;

namespace TicketManager.Api.Dtos;

public class CreateTicketDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    [Range(1, int.MaxValue)]
    public int ProjectId { get; set; }
}