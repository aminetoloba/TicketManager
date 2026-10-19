using TicketManager.Api.Dtos;
using TicketManager.Api.Models;
using TicketManager.Api.Services;
using TicketManager.Tests.Helpers;

namespace TicketManager.Tests.Services;

public class ProjectServiceTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        _service = new ProjectService(_db.Context);
    }

    public void Dispose() => _db.Dispose();

    private async Task<Project> SeedProjectAsync(string name = "Alpha")
    {
        var project = new Project { Name = name };
        _db.Context.Projects.Add(project);
        await _db.Context.SaveChangesAsync();
        return project;
    }

    [Fact]
    public async Task CreateAsync_ValidDto_PersistsAndReturnsDto()
    {
        var dto = new CreateProjectDto { Name = "Alpha", Description = "Desc" };

        var result = await _service.CreateAsync(dto);

        Assert.True(result.Id > 0);
        Assert.Equal("Alpha", result.Name);
        Assert.Single(_db.Context.Projects);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProjects()
    {
        await SeedProjectAsync("Alpha");
        await SeedProjectAsync("Beta");

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProject()
    {
        var project = await SeedProjectAsync();

        var result = await _service.GetByIdAsync(project.Id);

        Assert.NotNull(result);
        Assert.Equal("Alpha", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ChangesValuesAndReturnsTrue()
    {
        var project = await SeedProjectAsync();
        var dto = new UpdateProjectDto { Name = "Renamed", Description = "New" };

        var updated = await _service.UpdateAsync(project.Id, dto);

        Assert.True(updated);
        _db.Context.ChangeTracker.Clear();
        var saved = await _db.Context.Projects.FindAsync(project.Id);
        Assert.Equal("Renamed", saved!.Name);
        Assert.Equal("New", saved.Description);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsFalse()
    {
        var dto = new UpdateProjectDto { Name = "Renamed" };

        var updated = await _service.UpdateAsync(999, dto);

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_RemovesProjectAndReturnsTrue()
    {
        var project = await SeedProjectAsync();

        var deleted = await _service.DeleteAsync(project.Id);

        Assert.True(deleted);
        Assert.Empty(_db.Context.Projects);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ReturnsFalse()
    {
        var deleted = await _service.DeleteAsync(999);

        Assert.False(deleted);
    }
}