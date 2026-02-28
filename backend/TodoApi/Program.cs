using Scalar.AspNetCore;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Todo API";
        document.Info.Version = "v1";
        document.Info.Description = "Todo API on .NET 10";
        return Task.CompletedTask;
    });
});

// Register services
builder.Services.AddSingleton<ITodoService, TodoService>();
builder.Services.AddProblemDetails();

// Configure CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Todo API")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDarkModeToggle(true)
               .WithSearchHotKey("k")
               .WithDownloadButton(true);
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularApp");
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }