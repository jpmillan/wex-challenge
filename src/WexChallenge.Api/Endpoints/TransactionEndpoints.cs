using Microsoft.EntityFrameworkCore;
using WexChallenge.Api.Data;
using WexChallenge.Api.Dtos;
using WexChallenge.Api.Models;

namespace WexChallenge.Api.Endpoints;

public static class TransactionEndpoints
{
    public static void MapTransactionEndpoints(this WebApplication app)
    {
        app.MapPost("/api/cards/{cardId:guid}/transactions", async (
            Guid cardId,
            CreateTransactionRequest request,
            AppDbContext db) =>
        {
            var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId);
            if (card is null)
                return Results.NotFound("Card not found.");

            if (string.IsNullOrWhiteSpace(request.Description))
                return Results.BadRequest("Description is required.");

            if (request.Amount <= 0)
                return Results.BadRequest("Amount must be greater than zero.");

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                CardId = cardId,
                Description = request.Description.Trim(),
                TransactionDate = request.TransactionDate,
                Amount = request.Amount
            };

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            var response = new TransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.TransactionDate,
                transaction.Amount);

            return Results.Created(
                $"/api/cards/{cardId}/transactions/{transaction.Id}", response);
        });
    }
}
