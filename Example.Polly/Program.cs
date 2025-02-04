using Example.Polly;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Polly;

var builder = WebApplication.CreateBuilder(args);

const string TestClient = "TestClient";

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(TestClient)
    .AddPolicyHandler(AsyncTimingPolicy<HttpResponseMessage>.Create(PublishTiming));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/do-thing", async () =>
{
    var client = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient(TestClient);
    string[] urls =
    [
        "https://www.google.com/",
        "https://www.google.co.uk/",
        "https://www.bbc.co.uk/"
    ];

    foreach (var url in urls)
    {
        var context = new Context {["url"] = url};
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.SetPolicyExecutionContext(context);

        var result = await client.SendAsync(request, CancellationToken.None);
    }
    await Task.CompletedTask;
})
.WithName("DoThing")
.WithOpenApi();

app.Run();

static Task PublishTiming(TimeSpan executionDuration, Context context)
{
    object url = "[unknown]";
    if (context?.TryGetValue("url", out url) ?? false)
    {
        Console.WriteLine($"Took {executionDuration.TotalSeconds:0.###} seconds retrieving {url}");
    }
    return Task.CompletedTask;
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}