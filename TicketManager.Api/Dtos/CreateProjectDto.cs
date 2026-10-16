using System.ComponentModel.DataAnnotations;

namespace TicketManager.Api.Dtos;

public class CreateProjectDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}