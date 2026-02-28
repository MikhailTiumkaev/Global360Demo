namespace TodoApi.Models;

public record class CreateTodoDto(
    string Title,
    string Description = "");