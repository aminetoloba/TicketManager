using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketManager.Api.Controllers;
using TicketManager.Api.Dtos;
using TicketManager.Api.Models;
using TicketManager.Api.Services;

namespace TicketManager.Tests.Controllers;

public class TicketsControllerTests
{
    private readonly Mock<ITicketService> _service = new();
    private readonly TicketsController _controller;

    public TicketsControllerTests()
    {
        _controller = new TicketsController(_service.Object);
    }

    [Fact]
    public async Task GetAll_PassesFiltersToService()
    {
        _service.Setup(s => s.GetAllAsync(1, TicketStatus.Open))
                .ReturnsAsync(new List<TicketDto>());

        var result = await _controller.GetAll(1, TicketStatus.Open);

        Assert.IsType<OkObjectResult>(result.Result);
        _service.Verify(s => s.GetAllAsync(1, TicketStatus.Open), Times.Once);
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        _service.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(new TicketDto { Id = 1, Title = "Bug" });

        var result = await _controller.GetById(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("Bug", Assert.IsType<TicketDto>(ok.Value).Title);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        _service.Setup(s => s.GetByIdAsync(999))
                .ReturnsAsync((TicketDto?)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ExistingProject_ReturnsCreatedAtAction()
    {
        var dto = new CreateTicketDto { Title = "Bug", ProjectId = 1 };
        _service.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync(new TicketDto { Id = 7, Title = "Bug", ProjectId = 1 });

        var result = await _controller.Create(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TicketsController.GetById), created.ActionName);
        Assert.Equal(7, created.RouteValues!["id"]);
    }

    [Fact]
    public async Task Create_UnknownProject_ReturnsBadRequest()
    {
        var dto = new CreateTicketDto { Title = "Orphelin", ProjectId = 999 };
        _service.Setup(s => s.CreateAsync(dto))
                .ReturnsAsync((TicketDto?)null);

        var result = await _controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ExistingId_ReturnsNoContent()
    {
        var dto = new UpdateTicketDto { Title = "X" };
        _service.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsNotFound()
    {
        var dto = new UpdateTicketDto { Title = "X" };
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