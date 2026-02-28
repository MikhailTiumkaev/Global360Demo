using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TodoApi.Models;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests.Services;

public class TodoServiceTests
{
    private readonly Mock<ILogger<TodoService>> _loggerMock;
    private readonly TodoService _service;
    private readonly CancellationToken _cancellationToken;

    public TodoServiceTests()
    {
        _loggerMock = new Mock<ILogger<TodoService>>();
        _service = new TodoService(_loggerMock.Object);
        _cancellationToken = CancellationToken.None;
    }

    [Fact]
    public async Task CreateTodo_WithValidData_ShouldAddTodoAndReturnIt()
    {
        // Arrange
        var createDto = new CreateTodoDto(
            Title: "Test Todo",
            Description: "Test Description"
        );

        // Act
        var result = await _service.CreateTodoAsync(createDto, _cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be(createDto.Title);
        result.Description.Should().Be(createDto.Description);        
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task CreateTodo_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var createDto = new CreateTodoDto(Title: "");

        // Act
        var act = () => _service.CreateTodoAsync(createDto, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Title*");
    }

    [Fact]
    public async Task GetAllTodos_ShouldReturnAllTodos()
    {
        // Arrange
        await _service.CreateTodoAsync(new CreateTodoDto(Title: "Todo 1"), _cancellationToken);
        await _service.CreateTodoAsync(new CreateTodoDto(Title: "Todo 2"), _cancellationToken);
        await _service.CreateTodoAsync(new CreateTodoDto(Title: "Todo 3"), _cancellationToken);

        // Act
        var todos = await _service.GetAllTodosAsync(_cancellationToken);

        // Assert
        todos.Should().HaveCount(3);
    }

    [Fact]
    public async Task DeleteTodo_WithValidId_ShouldRemoveTodo()
    {
        // Arrange
        var created = await _service.CreateTodoAsync(
            new CreateTodoDto(Title: "To Delete"), 
            _cancellationToken);

        // Act
        var deleted = await _service.DeleteTodoAsync(created.Id, _cancellationToken);
        var getResult = await _service.GetTodoByIdAsync(created.Id, _cancellationToken);

        // Assert
        deleted.Should().BeTrue();
        getResult.Should().BeNull();
    }
}