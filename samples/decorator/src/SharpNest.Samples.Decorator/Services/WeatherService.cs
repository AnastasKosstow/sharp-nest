namespace SharpNest.Samples.Decorator.Services;

internal class WeatherService : IWeatherService
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public void Run()
    {
        var result = Enumerable.Range(1, 3).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Temperature = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        foreach (var item in result)
        {
            Console.WriteLine($"Date: {item.Date}\nTemperature: {item.Temperature}\nDate: {item.Summary}\n");
        }
    }
}


public class WeatherForecast
{
    public DateOnly Date { get; set; }

    public int Temperature { get; set; }

    public string? Summary { get; set; }
}
