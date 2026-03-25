using System.Net;
using System.Net.Http.Json;
using WexChallenge.Api.Dtos;

namespace WexChallenge.Tests;

public class BalanceEndpointTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebAppFactory _factory;

    public BalanceEndpointTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<CardResponse> CreateTestCard(decimal limit = 5000m)
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest(limit));
        return (await response.Content.ReadFromJsonAsync<CardResponse>())!;
    }

    private async Task AddTransaction(Guid cardId, decimal amount)
    {
        var request = new CreateTransactionRequest("Test purchase", DateTime.UtcNow, amount);
        await _client.PostAsJsonAsync($"/api/cards/{cardId}/transactions", request);
    }

    [Fact]
    public async Task GetBalance_NoTransactions_ReturnsFullLimit()
    {
        var card = await CreateTestCard(3000m);

        var response = await _client.GetAsync($"/api/cards/{card.Id}/balance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        Assert.NotNull(balance);
        Assert.Equal(3000m, balance.CreditLimit);
        Assert.Equal(0m, balance.TotalSpent);
        Assert.Equal(3000m, balance.AvailableBalance);
    }

    [Fact]
    public async Task GetBalance_WithTransactions_ReturnsCorrectBalance()
    {
        var card = await CreateTestCard(2000m);
        await AddTransaction(card.Id, 150m);
        await AddTransaction(card.Id, 350m);

        var response = await _client.GetAsync($"/api/cards/{card.Id}/balance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        Assert.Equal(2000m, balance!.CreditLimit);
        Assert.Equal(500m, balance.TotalSpent);
        Assert.Equal(1500m, balance.AvailableBalance);
    }

    [Fact]
    public async Task GetBalance_WithCurrencyConversion_ReturnsConvertedBalance()
    {
        var card = await CreateTestCard(1000m);
        await AddTransaction(card.Id, 200m);

        _factory.FakeExchangeRates.SetLatestRate("Euro Zone-Euro", 0.92m, DateTime.UtcNow);

        var response = await _client.GetAsync(
            $"/api/cards/{card.Id}/balance?currency=Euro Zone-Euro");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        Assert.Equal(800m, balance!.AvailableBalance);
        Assert.Equal("Euro Zone-Euro", balance.Currency);
        Assert.Equal(0.92m, balance.ExchangeRate);
        Assert.Equal(736.00m, balance.ConvertedBalance); // 800 * 0.92
    }

    [Fact]
    public async Task GetBalance_InvalidCurrency_ReturnsBadRequest()
    {
        var card = await CreateTestCard();

        _factory.FakeExchangeRates.Clear();

        var response = await _client.GetAsync(
            $"/api/cards/{card.Id}/balance?currency=FakeCurrency");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBalance_NonExistentCard_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/cards/{Guid.NewGuid()}/balance");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
