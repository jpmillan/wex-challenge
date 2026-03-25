namespace WexChallenge.Api.Services;

public record ExchangeRate(string Currency, decimal Rate, DateTime EffectiveDate);

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
