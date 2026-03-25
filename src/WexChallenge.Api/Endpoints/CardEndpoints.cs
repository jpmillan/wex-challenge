using Microsoft.EntityFrameworkCore;
using WexChallenge.Api.Data;
using WexChallenge.Api.Dtos;
using WexChallenge.Api.Models;

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
    }
}
