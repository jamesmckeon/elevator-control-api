using ElevatorApi.Api;
using ElevatorApi.Api.Config;
using ElevatorApi.Api.Dal;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IValidateOptions<ElevatorSettings>, ElevatorSettingsValidator>();
builder.Services.AddOptions<ElevatorSettings>()
    .Bind(builder.Configuration.GetSection("ElevatorSettings"))
    .ValidateOnStart();

builder.Services.AddTransient<ICarService, CarService>();
builder.Services.AddSingleton<ICarRepository, CarRepository>();

var app = builder.Build();
app.MapControllers();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();