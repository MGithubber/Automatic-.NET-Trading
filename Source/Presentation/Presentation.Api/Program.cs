using System.Text.Json;

using AutomaticDotNETtrading.Application.Interfaces.Services;
using AutomaticDotNETtrading.Domain.Models;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Implementations;
using AutomaticDotNETtrading.Infrastructure.TradingStrategies.LuxAlgoAndPSAR.Models;

using Microsoft.Extensions.Configuration;

using Presentation.Api;


var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile("appsettings.Credentials.json");
builder.Configuration.AddJsonFile("appsettings.TradingParameters.json");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapEndpoints();

Task.WaitAll(app.Services.GetRequiredService<IPoolTradingService>().StartTradingAsync(),
             app.RunAsync());
