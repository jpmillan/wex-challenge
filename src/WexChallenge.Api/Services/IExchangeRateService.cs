namespace WexChallenge.Api.Services;

public class ExchangeRate
{
    public string Currency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; }
}

public interface IExchangeRateService
{
    /// <summary>
    /// Gets the exchange rate for a currency on or before the given date, within the prior 6 months.
    /// Returns null if no rate is available in that window.
    /// </summary>
    Task<ExchangeRate?> GetRateForDate(string currency, DateTime transactionDate);

    /// <summary>
    /// Gets the latest available exchange rate for a given currency.
    /// </summary>
    Task<ExchangeRate?> GetLatestRate(string currency);
}
