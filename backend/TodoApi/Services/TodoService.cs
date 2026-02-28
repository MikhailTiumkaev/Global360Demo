using System.Collections.Concurrent;
using TodoApi.Models;

namespace TodoApi.Services;

public sealed class TodoService(ILogger<TodoService> _logger) : ITodoService
{
    private readonly ConcurrentDictionary<int, TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<IEnumerable<TodoItem>> GetAllTodosAsync(CancellationToken cancellationToken = default)
    {
        var todos = _todos.Values
            .OrderBy(t => t.CreatedAt)
            .AsEnumerable();
        
        return Task.FromResult(todos);
    }

    public Task<TodoItem?> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _todos.TryGetValue(id, out var todo);
        return Task.FromResult(todo);
    }

    public Task<TodoItem> CreateTodoAsync(CreateTodoDto todoDto, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(todoDto.Title, nameof(todoDto.Title));

        var todo = new TodoItem
        {
            Id = _nextId++,
            Title = todoDto.Title,
            Description = todoDto.Description,
            CreatedAt = DateTime.UtcNow
        };

        if (!_todos.TryAdd(todo.Id, todo))
        {
            throw new InvalidOperationException($"Failed to create todo with title: {todoDto.Title}");
        }

        _logger.LogInformation("Created todo with ID: {Id}, Title: {Title}", todo.Id, todo.Title);
        return Task.FromResult(todo);
    }

    public Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default)
    {
        var removed = _todos.TryRemove(id, out _);
        
        if (removed)
        {
            _logger.LogInformation("Deleted todo with ID: {Id}", id);
        }
        
        return Task.FromResult(removed);
    }
}