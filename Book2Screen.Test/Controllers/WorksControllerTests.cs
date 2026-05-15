// <copyright file="WorksControllerTests.cs" company="Team 17">
// Copyright (c) Team 17. All rights reserved.
// </copyright>

namespace Book2Screen.Tests.Controllers;

using Book2Screen.API__Web_.Controllers;
using Book2Screen.Application.DTOs;
using Book2Screen.Application.Filters;
using Book2Screen.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

/// <summary>
/// Тести для WorksController.
/// </summary>
public class WorksControllerTests
{
    private readonly Mock<IWorkService> workServiceMock;
    private readonly WorksController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorksControllerTests"/> class.
    /// </summary>
    public WorksControllerTests()
    {
        this.workServiceMock = new Mock<IWorkService>();
        this.controller = new WorksController(this.workServiceMock.Object);
    }

    /// <summary>
    /// Перевіряє, що GetWorks повертає Ok з результатом із сервісу.
    /// </summary>
    [Fact]
    public async Task GetWorks_ShouldReturnOkWithWorks()
    {
        // Arrange
        var works = new List<BookScreenItemDto> { new BookScreenItemDto { Title = "Test" } };
        this.workServiceMock.Setup(s => s.GetAllWorksAsync(It.IsAny<WorkFilter>())).ReturnsAsync(works);

        // Act
        var result = await this.controller.GetWorks(new WorkFilter());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedWorks = Assert.IsAssignableFrom<IEnumerable<BookScreenItemDto>>(okResult.Value);
        Assert.Single(returnedWorks);
    }

    /// <summary>
    /// Перевіряє, що GetWorkById повертає Ok, якщо твір знайдено.
    /// </summary>
    [Fact]
    public async Task GetWorkById_ShouldReturnOk_WhenWorkExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var work = new BookScreenItemDto { Id = id, Title = "Found" };
        this.workServiceMock.Setup(s => s.GetWorkByIdAsync(id)).ReturnsAsync(work);

        // Act
        var result = await this.controller.GetWorkById(id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedWork = Assert.IsType<BookScreenItemDto>(okResult.Value);
        Assert.Equal(id, returnedWork.Id);
    }

    /// <summary>
    /// Перевіряє, що GetWorkById повертає NotFound, якщо твір не знайдено.
    /// </summary>
    [Fact]
    public async Task GetWorkById_ShouldReturnNotFound_WhenWorkDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        this.workServiceMock.Setup(s => s.GetWorkByIdAsync(id)).ReturnsAsync((BookScreenItemDto?)null);

        // Act
        var result = await this.controller.GetWorkById(id);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
