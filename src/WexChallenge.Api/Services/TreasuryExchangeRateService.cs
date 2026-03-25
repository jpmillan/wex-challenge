using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace WexChallenge.Api.Services;

public class TreasuryExchangeRateService : IExchangeRateService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.fiscaldata.treasury.gov/services/api/fiscal_service/v1/accounting/od/rates_of_exchange";

    public TreasuryExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExchangeRate?> GetRateForDate(string currency, DateTime transactionDate)
    {
        // look for rates within 6 months before the transaction date
        var sixMonthsBefore = transactionDate.AddMonths(-6);
        var fromDate = sixMonthsBefore.ToString("yyyy-MM-dd");
        var toDate = transactionDate.ToString("yyyy-MM-dd");

        var url = $"{BaseUrl}?fields=currency,exchange_rate,record_date" +
                  $"&filter=currency:eq:{currency},record_date:gte:{fromDate},record_date:lte:{toDate}" +
                  "&sort=-record_date" +
                  "&page[size]=1";

        var result = await _httpClient.GetFromJsonAsync<TreasuryApiResponse>(url);

        if (result?.Data is null || result.Data.Count == 0)
            return null;

        var entry = result.Data[0];
        return new ExchangeRate(
            entry.Currency,
            decimal.Parse(entry.ExchangeRate, CultureInfo.InvariantCulture),
            DateTime.Parse(entry.RecordDate, CultureInfo.InvariantCulture));
    }

    public async Task<ExchangeRate?> GetLatestRate(string currency)
    {
        var url = $"{BaseUrl}?fields=currency,exchange_rate,record_date" +
                  $"&filter=currency:eq:{currency}" +
                  "&sort=-record_date" +
                  "&page[size]=1";

        var result = await _httpClient.GetFromJsonAsync<TreasuryApiResponse>(url);

        if (result?.Data is null || result.Data.Count == 0)
            return null;

        var entry = result.Data[0];
        return new ExchangeRate(
            entry.Currency,
            decimal.Parse(entry.ExchangeRate, CultureInfo.InvariantCulture),
            DateTime.Parse(entry.RecordDate, CultureInfo.InvariantCulture));
    }
}

public class TreasuryApiResponse
{
    [JsonPropertyName("data")]
    public List<TreasuryRateEntry> Data { get; set; } = new();
}

public class TreasuryRateEntry
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("exchange_rate")]
    public string ExchangeRate { get; set; } = string.Empty;

    [JsonPropertyName("record_date")]
    public string RecordDate { get; set; } = string.Empty;
}
