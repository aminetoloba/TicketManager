using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketManager.Api.Controllers;
using TicketManager.Api.Dtos;
using TicketManager.Api.Services;

namespace TicketManager.Tests.Controllers;

public class ProjectsControllerTests
{
    private readonly Mock<IProjectService> _service = new();
    private readonly ProjectsController _controller;

    public ProjectsControllerTests()
    {
        _controller = new ProjectsController(_service.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithProjects()
    {
        _service.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<ProjectDto> { new() { Id = 1, Name = "Alpha" } });

        var result = await _controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var projects = Assert.IsType<List<ProjectDto>>(ok.Value);
        Assert.Single(projects);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        _service.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(new ProjectDto { Id = 1, Name = "Alpha" });

        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Alpha", Assert.IsType<ProjectDto>(ok.Value).Name);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(999))
                .ReturnsAsync((ProjectDto?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtActionWithNewId()
    {
        var dto = new CreateProjectDto { Name = "Alpha" };
        _service.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(new ProjectDto { Id = 5, Name = "Alpha" });

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProjectsController.GetById), created.ActionName);
        Assert.Equal(5, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsNoContent()
    {
        var dto = new UpdateProjectDto { Name = "X" };
        _service.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsNotFound()
    {
        var dto = new UpdateProjectDto { Name = "X" };
        _service.Setup(s => s.UpdateAsync(999, dto)).ReturnsAsync(false);

        var result = await _controller.Update(999, dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        _service.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        _service.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}