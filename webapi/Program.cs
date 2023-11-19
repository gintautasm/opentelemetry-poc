using System.Diagnostics;
using Microsoft.Data.SqlClient;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Activity.DefaultIdFormat = ActivityIdFormat.W3C;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<KafkaConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();

var connectionString = System.Environment.GetEnvironmentVariable("DB_CONNECTION");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", WeatherForecast)
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Logger.LogInformation("starting the app ...");
app.Run();

async Task ExecuteSql(string sql)
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    using var command = new SqlCommand(sql, connection);
    using var reader = await command.ExecuteReaderAsync();
}

async Task<WeatherForecast[]> WeatherForecast(ILogger<Program> logger, string? name)
{
    logger.LogInformation("executing SQL");
    await ExecuteSql("SELECT 1");
    name ??= "My-name-is";
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

        logger.LogInformation("say hello to {name} ...", name);
    return forecast;
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


