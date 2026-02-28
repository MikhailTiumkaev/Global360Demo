using System.Text.Json.Serialization;

namespace TodoApi.Models;

public class TodoItem
{
    public required int Id { get; init; }
    public required string Title { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;    
}