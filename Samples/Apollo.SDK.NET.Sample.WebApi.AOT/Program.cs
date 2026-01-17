using System.Text.Json.Serialization;

using Apollo.SDK.NET;
using Apollo.SDK.NET.Interfaces;

using Microsoft.AspNetCore.Http.HttpResults;

using Serilog;

var builder = WebApplication.CreateSlimBuilder(args);

//builder.Logging.AddConsole();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 注册 Apollo 客户端服务，请在配置 Logger 之后注册
builder.Services.AddApollo(new()
{
    TogglesPath = Path.Combine(Environment.CurrentDirectory, "toggles")
});

var app = builder.Build();

//var apolloClient = app.Services.GetService<IApolloClient>();

//var context = new ApolloContext("user_123")
//        .Set("city", "Beijing");

//var key = "smart_recommender_v2";

//bool? enabled = apolloClient?.IsToggleAllowed(key, context);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

Todo[] sampleTodos =
[
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
];

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos)
        .WithName("GetTodos");

todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound())
    .WithName("GetTodoById");

app.MapGet("/check/{key}", (string key, IApolloClient apollo) =>
{
    var context = new ApolloContext("user_123")
        .Set("city", "Beijing");

    var enabled = apollo.IsToggleAllowed(key, context);

    return Results.Ok();
});

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
