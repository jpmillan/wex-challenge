using System.Net;
using System.Net.Http.Json;
using WexChallenge.Api.Dtos;

namespace WexChallenge.Tests;

public class TransactionEndpointTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public TransactionEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<CardResponse> CreateTestCard(decimal limit = 5000m)
    {
        var response = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest(limit));
        return (await response.Content.ReadFromJsonAsync<CardResponse>())!;
    }

    [Fact]
    public async Task CreateTransaction_ValidData_ReturnsCreated()
    {
        var card = await CreateTestCard();
        var request = new CreateTransactionRequest(
            "Coffee shop", DateTime.UtcNow.AddDays(-1), 4.50m);

        var response = await _client.PostAsJsonAsync(
            $"/api/cards/{card.Id}/transactions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var tx = await response.Content.ReadFromJsonAsync<TransactionResponse>();
        Assert.NotNull(tx);
        Assert.Equal("Coffee shop", tx.Description);
        Assert.Equal(4.50m, tx.Amount);
    }

    [Fact]
    public async Task CreateTransaction_NonExistentCard_ReturnsNotFound()
    {
        var request = new CreateTransactionRequest(
            "Groceries", DateTime.UtcNow, 55.00m);

        var response = await _client.PostAsJsonAsync(
            $"/api/cards/{Guid.NewGuid()}/transactions", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_EmptyDescription_ReturnsBadRequest()
    {
        var card = await CreateTestCard();
        var request = new CreateTransactionRequest("", DateTime.UtcNow, 10m);

        var response = await _client.PostAsJsonAsync(
            $"/api/cards/{card.Id}/transactions", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ZeroAmount_ReturnsBadRequest()
    {
        var card = await CreateTestCard();
        var request = new CreateTransactionRequest("Test", DateTime.UtcNow, 0m);

        var response = await _client.PostAsJsonAsync(
            $"/api/cards/{card.Id}/transactions", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
