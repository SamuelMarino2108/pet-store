using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Serilog;
using Store.Api.ErrorHandling;
using Store.Application;
using Store.Infrastructure;
using Store.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, logger) => logger
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services));

// Enums travel as text in JSON, in query strings and in the OpenAPI document (which reads the HTTP JSON options).
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddDbContextCheck<StoreDbContext>("database");

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await app.Services.ApplyMigrationsAsync();
}

if (app.Configuration.GetValue<bool>("Database:SeedSampleData"))
{
    await app.Services.SeedSampleDataAsync();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

/// <summary>Entry point, exposed so integration tests can use WebApplicationFactory.</summary>
public partial class Program;
