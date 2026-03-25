using Microsoft.EntityFrameworkCore;
using WexChallenge.Api.Data;
using WexChallenge.Api.Dtos;
using WexChallenge.Api.Models;
using WexChallenge.Api.Services;

namespace WexChallenge.Api.Endpoints;

public static class CardEndpoints
{
    public static void MapCardEndpoints(this WebApplication app)
    {
        app.MapPost("/api/cards", async (CreateCardRequest request, AppDbContext db) =>
        {
            if (request.CreditLimit <= 0)
                return Results.BadRequest("Credit limit must be greater than zero.");

            var card = new Card
            {
                Id = Guid.NewGuid(),
                CreditLimit = request.CreditLimit,
                CreatedAt = DateTime.UtcNow
            };

            db.Cards.Add(card);
            await db.SaveChangesAsync();

            var response = new CardResponse(card.Id, card.CreditLimit, card.CreatedAt);
            return Results.Created($"/api/cards/{card.Id}", response);
        });

        app.MapGet("/api/cards/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == id);
            if (card is null)
                return Results.NotFound();

            return Results.Ok(new CardResponse(card.Id, card.CreditLimit, card.CreatedAt));
        });

        app.MapGet("/api/cards/{id:guid}/balance", async (
            Guid id,
            string? currency,
            AppDbContext db,
            IExchangeRateService exchangeRateService) =>
        {
            var card = await db.Cards
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (card is null)
                return Results.NotFound("Card not found.");

            var totalSpent = card.Transactions.Sum(t => t.Amount);
            var availableBalance = card.CreditLimit - totalSpent;

            // if no currency requested, return balance in USD
            if (string.IsNullOrWhiteSpace(currency))
            {
                return Results.Ok(new BalanceResponse(
                    card.Id, card.CreditLimit, totalSpent, availableBalance,
                    null, null, null));
            }

            var rate = await exchangeRateService.GetLatestRate(currency);
            if (rate is null)
            {
                return Results.BadRequest(
                    $"No exchange rate available for currency: {currency}");
            }

            var convertedBalance = Math.Round(availableBalance * rate.Rate, 2);

            return Results.Ok(new BalanceResponse(
                card.Id, card.CreditLimit, totalSpent, availableBalance,
                currency, rate.Rate, convertedBalance));
        });
    }
}
