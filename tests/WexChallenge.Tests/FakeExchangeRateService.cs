using WexChallenge.Api.Services;

namespace WexChallenge.Tests;

public class FakeExchangeRateService : IExchangeRateService
{
    private readonly Dictionary<string, ExchangeRate> _rates = new();
    private ExchangeRate? _latestRate;

    public void SetRateForDate(string currency, decimal rate, DateTime effectiveDate)
    {
        _rates[currency] = new ExchangeRate
        {
            Currency = currency,
            Rate = rate,
            EffectiveDate = effectiveDate
        };
    }

    public void SetLatestRate(string currency, decimal rate, DateTime effectiveDate)
    {
        _latestRate = new ExchangeRate
        {
            Currency = currency,
            Rate = rate,
            EffectiveDate = effectiveDate
        };
    }

    public void Clear()
    {
        _rates.Clear();
        _latestRate = null;
    }

    public Task<ExchangeRate?> GetRateForDate(string currency, DateTime transactionDate)
    {
        _rates.TryGetValue(currency, out var rate);
        return Task.FromResult(rate);
    }

    public Task<ExchangeRate?> GetLatestRate(string currency)
    {
        return Task.FromResult(_latestRate);
    }
}
