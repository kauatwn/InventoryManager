using InventoryManager.API.Extensions;
using InventoryManager.Application.Extensions;
using InventoryManager.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new()
        {
            Title = "Inventory Manager API",
            Version = "v1",
            Description = "A robust Web API for product inventory management built with Clean Architecture and DDD principles."
        };

        return Task.CompletedTask;
    });
});

builder.Services.AddApiExceptionHandlers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Inventory Manager API"));

    await app.CreateDatabaseAsync();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
