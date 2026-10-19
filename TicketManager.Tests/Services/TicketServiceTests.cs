using TicketManager.Api.Dtos;
using TicketManager.Api.Models;
using TicketManager.Api.Services;
using TicketManager.Tests.Helpers;

namespace TicketManager.Tests.Services;

public class TicketServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly TicketService _service;

    public TicketServiceTests()
    {
        _service = new TicketService(_db.Context);
    }

    public void Dispose() => _db.Dispose();

    private async Task<Project> SeedProjectAsync(string name = "Alpha")
    {
        var project = new Project { Name = name };
        _db.Context.Projects.Add(project);
        await _db.Context.SaveChangesAsync();
        return project;
    }

    private async Task<Ticket> SeedTicketAsync(
        int projectId,
        string title = "Ticket",
        TicketStatus status = TicketStatus.Open,
        DateTime? createdAt = null)
    {
        var ticket = new Ticket
        {
            Title = title,
            ProjectId = projectId,
            Status = status,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
        _db.Context.Tickets.Add(ticket);
        await _db.Context.SaveChangesAsync();
        return ticket;
    }

    // --- Create ---

    [Fact]
    public async Task CreateAsync_ExistingProject_PersistsTicketWithOpenStatus()
    {
        var project = await SeedProjectAsync();
        var dto = new CreateTicketDto
        {
            Title = "Bug login",
            Priority = TicketPriority.High,
            ProjectId = project.Id
        };

        var result = await _service.CreateAsync(dto);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal(TicketStatus.Open, result.Status);
        Assert.Equal(TicketPriority.High, result.Priority);
        Assert.Single(_db.Context.Tickets);
    }

    [Fact]
    public async Task CreateAsync_UnknownProject_ReturnsNullAndPersistsNothing()
    {
        var dto = new CreateTicketDto { Title = "Orphelin", ProjectId = 999 };

        var result = await _service.CreateAsync(dto);

        Assert.Null(result);
        Assert.Empty(_db.Context.Tickets);
    }

    // --- Read ---

    [Fact]
    public async Task GetAllAsync_NoFilter_ReturnsAllTickets()
    {
        var project = await SeedProjectAsync();
        await SeedTicketAsync(project.Id, "A");
        await SeedTicketAsync(project.Id, "B");

        var result = await _service.GetAllAsync(null, null);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_FilterByProject_ReturnsOnlyThatProjectTickets()
    {
        var alpha = await SeedProjectAsync("Alpha");
        var beta = await SeedProjectAsync("Beta");
        await SeedTicketAsync(alpha.Id, "A1");
        await SeedTicketAsync(alpha.Id, "A2");
        await SeedTicketAsync(beta.Id, "B1");

        var result = await _service.GetAllAsync(alpha.Id, null);

        Assert.Equal(2, result.Count);
        Assert.All(result, t => Assert.Equal(alpha.Id, t.ProjectId));
    }

    [Fact]
    public async Task GetAllAsync_FilterByStatus_ReturnsOnlyMatchingTickets()
    {
        var project = await SeedProjectAsync();
        await SeedTicketAsync(project.Id, "Open 1", TicketStatus.Open);
        await SeedTicketAsync(project.Id, "Done 1", TicketStatus.Done);

        var result = await _service.GetAllAsync(null, TicketStatus.Done);

        var ticket = Assert.Single(result);
        Assert.Equal("Done 1", ticket.Title);
    }

    [Fact]
    public async Task GetAllAsync_ProjectAndStatusFilters_AreCombined()
    {
        var alpha = await SeedProjectAsync("Alpha");
        var beta = await SeedProjectAsync("Beta");
        await SeedTicketAsync(alpha.Id, "A open", TicketStatus.Open);
        await SeedTicketAsync(alpha.Id, "A done", TicketStatus.Done);
        await SeedTicketAsync(beta.Id, "B open", TicketStatus.Open);

        var result = await _service.GetAllAsync(alpha.Id, TicketStatus.Open);

        var ticket = Assert.Single(result);
        Assert.Equal("A open", ticket.Title);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsNewestFirst()
    {
        var project = await SeedProjectAsync();
        await SeedTicketAsync(project.Id, "Old", createdAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        await SeedTicketAsync(project.Id, "New", createdAt: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc));

        var result = await _service.GetAllAsync(null, null);

        Assert.Equal("New", result[0].Title);
        Assert.Equal("Old", result[1].Title);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsTicket()
    {
        var project = await SeedProjectAsync();
        var ticket = await SeedTicketAsync(project.Id, "Mon ticket");

        var result = await _service.GetByIdAsync(ticket.Id);

        Assert.NotNull(result);
        Assert.Equal("Mon ticket", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    // --- Update ---

    [Fact]
    public async Task UpdateAsync_ExistingId_ChangesStatusAndReturnsTrue()
    {
        var project = await SeedProjectAsync();
        var ticket = await SeedTicketAsync(project.Id);
        var dto = new UpdateTicketDto
        {
            Title = "Titre modifié",
            Status = TicketStatus.InProgress,
            Priority = TicketPriority.High
        };

        var updated = await _service.UpdateAsync(ticket.Id, dto);

        Assert.True(updated);
        _db.Context.ChangeTracker.Clear();
        var saved = await _db.Context.Tickets.FindAsync(ticket.Id);
        Assert.Equal("Titre modifié", saved!.Title);
        Assert.Equal(TicketStatus.InProgress, saved.Status);
        Assert.Equal(TicketPriority.High, saved.Priority);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotChangeProject()
    {
        var project = await SeedProjectAsync();
        var ticket = await SeedTicketAsync(project.Id);
        var dto = new UpdateTicketDto { Title = "X", Status = TicketStatus.Done };

        await _service.UpdateAsync(ticket.Id, dto);

        _db.Context.ChangeTracker.Clear();
        var saved = await _db.Context.Tickets.FindAsync(ticket.Id);
        Assert.Equal(project.Id, saved!.ProjectId);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsFalse()
    {
        var dto = new UpdateTicketDto { Title = "X" };

        var updated = await _service.UpdateAsync(999, dto);

        Assert.False(updated);
    }

    // --- Delete ---

    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesTicketAndReturnsTrue()
    {
        var project = await SeedProjectAsync();
        var ticket = await SeedTicketAsync(project.Id);

        var deleted = await _service.DeleteAsync(ticket.Id);

        Assert.True(deleted);
        Assert.Empty(_db.Context.Tickets);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        var deleted = await _service.DeleteAsync(999);

        Assert.False(deleted);
    }

    // --- Règle de la base ---

    [Fact]
    public async Task DeletingProject_CascadesToItsTickets()
    {
        var project = await SeedProjectAsync();
        await SeedTicketAsync(project.Id, "A");
        await SeedTicketAsync(project.Id, "B");

        _db.Context.Projects.Remove(project);
        await _db.Context.SaveChangesAsync();

        Assert.Empty(_db.Context.Tickets);
    }
}