using System.Net;
using System.Net.Http.Json;
using WexChallenge.Api.Dtos;

namespace WexChallenge.Tests;

public class CardEndpointTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public CardEndpointTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCard_WithValidLimit_ReturnsCreated()
    {
        var request = new CreateCardRequest(5000m);

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var card = await response.Content.ReadFromJsonAsync<CardResponse>();
        Assert.NotNull(card);
        Assert.NotEqual(Guid.Empty, card.Id);
        Assert.Equal(5000m, card.CreditLimit);
    }

    [Fact]
    public async Task CreateCard_WithZeroLimit_ReturnsBadRequest()
    {
        var request = new CreateCardRequest(0m);

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCard_WithNegativeLimit_ReturnsBadRequest()
    {
        var request = new CreateCardRequest(-100m);

        var response = await _client.PostAsJsonAsync("/api/cards", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCard_ExistingCard_ReturnsOk()
    {
        // create a card first
        var createResponse = await _client.PostAsJsonAsync("/api/cards", new CreateCardRequest(1000m));
        var created = await createResponse.Content.ReadFromJsonAsync<CardResponse>();

        var response = await _client.GetAsync($"/api/cards/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var card = await response.Content.ReadFromJsonAsync<CardResponse>();
        Assert.Equal(created.Id, card!.Id);
        Assert.Equal(1000m, card.CreditLimit);
    }

    [Fact]
    public async Task GetCard_NonExistentCard_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/cards/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
