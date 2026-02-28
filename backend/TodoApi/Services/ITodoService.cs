using TodoApi.Models;

namespace TodoApi.Services;

public interface ITodoService
{
    Task<IEnumerable<TodoItem>> GetAllTodosAsync(CancellationToken cancellationToken = default);
    Task<TodoItem?> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoItem> CreateTodoAsync(CreateTodoDto todoDto, CancellationToken cancellationToken = default);    
    Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default);    
}