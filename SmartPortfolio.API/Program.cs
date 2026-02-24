using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using Serilog;
using SmartPortfolio.API.Extensions;
using SmartPortfolio.API.Infrastructure;
using SmartPortfolio.Domain.Entities;
using SmartPortfolio.Domain.Interfaces;
using SmartPortfolio.Infrastructure.Services.CurrencyConverter;
using SmartPortfolio.Infrastructure.Services.StockPrice;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Login configuration
builder.Host.UseSerilog((context, configuration) =>
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console());

// --- 2. Services Registers
builder.Services.AddCustomOpenApi();
builder.Services.AddCustomDbContext(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
var nbpUrl = builder.Configuration["NbpSettings:ApiUrl"]; 
builder.Services.AddHttpClient<ICurrencyConverter, NbpCurrencyConverter>(client =>
{
    if (!string.IsNullOrEmpty(nbpUrl))
    {
        client.BaseAddress = new Uri(nbpUrl);
    }
});
var finnhubBaseUrl = builder.Configuration["FinnhubSettings:ApiUrl"];
builder.Services.AddHttpClient<IStockPricingService, FinnhubStockPricingService>(client =>
{
    if (!string.IsNullOrEmpty(finnhubBaseUrl))
    {
        client.BaseAddress = new Uri(finnhubBaseUrl);
    }
});
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// WEB Infrastructure
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// --- 3. HTML pipeline configuration
app.ApplyMigrations();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }