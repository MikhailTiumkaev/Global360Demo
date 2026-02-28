using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests.Controllers;

public class TodoControllerTests
{
    private readonly Mock<ITodoService> _serviceMock;
    private readonly Mock<ILogger<TodoController>> _loggerMock;
    private readonly TodoController _controller;
    private readonly CancellationToken _cancellationToken;

    public TodoControllerTests()
    {
        _serviceMock = new Mock<ITodoService>();
        _loggerMock = new Mock<ILogger<TodoController>>();
        _controller = new TodoController(_serviceMock.Object);
        _cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task GetTodos_ShouldReturnOkWithTodos()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            new() { Id = 1, Title = "Todo 1", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Todo 2", CreatedAt = DateTime.UtcNow }
        };
        
        _serviceMock.Setup(s => s.GetAllTodosAsync(_cancellationToken))
            .ReturnsAsync(todos);

        // Act
        var result = await _controller.GetTodos(_cancellationToken);

        // Assert
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(todos);
    }

    [Fact]
    public async Task GetTodo_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var todo = new TodoItem { Id = 1, Title = "Test Todo", CreatedAt = DateTime.UtcNow };
        _serviceMock.Setup(s => s.GetTodoByIdAsync(1, _cancellationToken))
            .ReturnsAsync(todo);

        // Act
        var result = await _controller.GetTodo(1, _cancellationToken);

        // Assert
        result.Result.Should().BeOfType<Ok<TodoItem>>();
        var okResult = result.Result as Ok<TodoItem>;
        okResult!.Value.Should().Be(todo);
    }

    [Fact]
    public async Task GetTodo_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _serviceMock.Setup(s => s.GetTodoByIdAsync(999, _cancellationToken))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _controller.GetTodo(999, _cancellationToken);

        // Assert
        result.Result.Should().BeOfType<NotFound<string>>();
        var notFoundResult = result.Result as NotFound<string>;
        notFoundResult!.Value.Should().Contain("999");
    }

    [Fact]
    public async Task CreateTodo_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var createDto = new CreateTodoDto(Title: "New Todo");
        var createdTodo = new TodoItem { Id = 1, Title = "New Todo", CreatedAt = DateTime.UtcNow };
        
        _serviceMock.Setup(s => s.CreateTodoAsync(createDto, _cancellationToken))
            .ReturnsAsync(createdTodo);

        // Act
        var result = await _controller.CreateTodo(createDto, _cancellationToken);

        // Assert
        result.Result.Should().BeOfType<CreatedAtRoute<TodoItem>>();
        var createdResult = result.Result as CreatedAtRoute<TodoItem>;
        createdResult!.Value.Should().Be(createdTodo);
        createdResult.RouteName.Should().Be(nameof(TodoController.GetTodo));
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(1);
    }

    [Fact]
    public async Task CreateTodo_WithEmptyTitle_ShouldReturnBadRequest()
    {
        // Arrange
        var createDto = new CreateTodoDto(Title: "");

        // Act
        var result = await _controller.CreateTodo(createDto, _cancellationToken);

        // Assert
        result.Result.Should().BeOfType<BadRequest<string>>();
        var badRequestResult = result.Result as BadRequest<string>;
        badRequestResult!.Value.Should().Be("Title is required");
    }

    [Fact]
    public async Task DeleteTodo_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        _serviceMock.Setup(s => s.DeleteTodoAsync(1, _cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTodo(1, _cancellationToken);

        // Assert
        result.Result.Should().BeOfType<NoContent>();
    }
}