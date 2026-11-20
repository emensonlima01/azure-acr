using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ExchangeRateProviderOptions>(
    builder.Configuration.GetSection(ExchangeRateProviderOptions.SectionName));

builder.Services.AddHttpClient("exchange-rate-provider", (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<ExchangeRateProviderOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

var app = builder.Build();

app.MapGet("/exchange-rates/usd", async (IHttpClientFactory factory, IOptions<ExchangeRateProviderOptions> settings) =>
{
    var options = settings.Value;

    try
    {
        var client = factory.CreateClient("exchange-rate-provider");
        using var stream = await client.GetStreamAsync(options.Path);
        using var doc = await JsonDocument.ParseAsync(stream);
        var rate = doc.RootElement.GetProperty(options.ResponseKey);

        return Results.Ok(new
        {
            from = options.From,
            to = options.To,
            bid = decimal.Parse(rate.GetProperty("bid").GetString()!, CultureInfo.InvariantCulture),
            ask = decimal.Parse(rate.GetProperty("ask").GetString()!, CultureInfo.InvariantCulture),
            updatedAt = rate.GetProperty("create_date").GetString()
        });
    }
    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException)
    {
        return Results.Problem("Unable to retrieve the USD exchange rate.", statusCode: 502);
    }
});

app.Run();
