using System.Net;
using System.Net.Http.Json;
using WexChallenge.Api.Dtos;

namespace WexChallenge.Tests;

public class ConvertedTransactionTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public ConvertedTransactionTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(CardResponse Card, TransactionResponse Transaction)> CreateCardAndTransaction()
    {
        var cardResponse = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest(10000m));
        var card = (await cardResponse.Content.ReadFromJsonAsync<CardResponse>())!;

        var txRequest = new CreateTransactionRequest(
            "Dinner at restaurant", new DateTime(2024, 10, 15), 50.00m);
        var txResponse = await _client.PostAsJsonAsync(
            $"/api/cards/{card.Id}/transactions", txRequest);
        var tx = (await txResponse.Content.ReadFromJsonAsync<TransactionResponse>())!;

        return (card, tx);
    }

    [Fact]
    public async Task GetTransaction_WithValidCurrency_ReturnsConvertedAmount()
    {
        var (_, tx) = await CreateCardAndTransaction();

        // set up a fake rate: 1 USD = 1.35 CAD
        _factory.FakeExchangeRates.SetRateForDate("Canada-Dollar", 1.35m, new DateTime(2024, 10, 10));

        var response = await _client.GetAsync(
            $"/api/transactions/{tx.Id}?currency=Canada-Dollar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ConvertedTransactionResponse>();
        Assert.NotNull(result);
        Assert.Equal(tx.Id, result.Id);
        Assert.Equal(50.00m, result.OriginalAmount);
        Assert.Equal(1.35m, result.ExchangeRate);
        Assert.Equal(67.50m, result.ConvertedAmount); // 50 * 1.35
        Assert.Equal("Canada-Dollar", result.Currency);
    }

    [Fact]
    public async Task GetTransaction_InPhilippinePeso_ReturnsConvertedAmount()
    {
        var (_, tx) = await CreateCardAndTransaction();

        _factory.FakeExchangeRates.SetRateForDate("Philippines-Peso", 58.91m, new DateTime(2024, 10, 12));

        var response = await _client.GetAsync(
            $"/api/transactions/{tx.Id}?currency=Philippines-Peso");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ConvertedTransactionResponse>();
        Assert.NotNull(result);
        Assert.Equal(50.00m, result.OriginalAmount);
        Assert.Equal(58.91m, result.ExchangeRate);
        Assert.Equal(2945.50m, result.ConvertedAmount); // 50 * 58.91
        Assert.Equal("Philippines-Peso", result.Currency);
    }

    [Fact]
    public async Task GetTransaction_InSingaporeDollar_ReturnsConvertedAmount()
    {
        var (_, tx) = await CreateCardAndTransaction();

        _factory.FakeExchangeRates.SetRateForDate("Singapore-Dollar", 1.29m, new DateTime(2024, 10, 14));

        var response = await _client.GetAsync(
            $"/api/transactions/{tx.Id}?currency=Singapore-Dollar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ConvertedTransactionResponse>();
        Assert.NotNull(result);
        Assert.Equal(50.00m, result.OriginalAmount);
        Assert.Equal(1.29m, result.ExchangeRate);
        Assert.Equal(64.50m, result.ConvertedAmount); // 50 * 1.29
        Assert.Equal("Singapore-Dollar", result.Currency);
    }

    [Fact]
    public async Task GetTransaction_NoRateAvailable_ReturnsBadRequest()
    {
        var (_, tx) = await CreateCardAndTransaction();

        // clear all rates so there's nothing to find
        _factory.FakeExchangeRates.Clear();

        var response = await _client.GetAsync(
            $"/api/transactions/{tx.Id}?currency=SomeFakeCurrency");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetTransaction_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/transactions/{Guid.NewGuid()}?currency=Canada-Dollar");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
