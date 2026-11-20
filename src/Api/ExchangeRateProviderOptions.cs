public class ExchangeRateProviderOptions
{
    public const string SectionName = "ExchangeRateProvider";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 10;
    public string Path { get; set; } = string.Empty;
    public string ResponseKey { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}
