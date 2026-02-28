using TodoApi.Models;
using TodoApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TodoController(ITodoService todoService) : ControllerBase
{
    private readonly ITodoService _todoService = todoService;

    [HttpGet]
    [ProducesResponseType<IEnumerable<TodoItem>>(StatusCodes.Status200OK)]
    [EndpointSummary("Get all todos")]
    [EndpointDescription("Returns all todo items ordered by priority and due date")]
    public async Task<Ok<IEnumerable<TodoItem>>> GetTodos(CancellationToken cancellationToken)
    {
        var todos = await _todoService.GetAllTodosAsync(cancellationToken);
        return TypedResults.Ok(todos);
    }

    [HttpGet("{id}", Name = "GetTodo")]
    [ProducesResponseType<TodoItem>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<TodoItem>, NotFound<string>>> GetTodo(int id, CancellationToken cancellationToken)
    {
        var todo = await _todoService.GetTodoByIdAsync(id, cancellationToken);
        
        return todo is not null 
            ? TypedResults.Ok(todo) 
            : TypedResults.NotFound($"Todo with id {id} not found");
    }

    [HttpPost]
    [ProducesResponseType<TodoItem>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<Results<CreatedAtRoute<TodoItem>, BadRequest<string>>> CreateTodo(
        CreateTodoDto todoDto, 
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(todoDto.Title))
        {
            return TypedResults.BadRequest("Title is required");
        }

        try
        {
            var todo = await _todoService.CreateTodoAsync(todoDto, cancellationToken);
            
            return TypedResults.CreatedAtRoute(
                todo, 
                nameof(GetTodo), 
                new { id = todo.Id });
        }
        catch (ArgumentException ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<Results<NoContent, NotFound<string>>> DeleteTodo(int id, CancellationToken cancellationToken)
    {
        var deleted = await _todoService.DeleteTodoAsync(id, cancellationToken);
        
        return deleted 
            ? TypedResults.NoContent() 
            : TypedResults.NotFound($"Todo with id {id} not found");
    }
}