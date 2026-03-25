namespace WexChallenge.Api.Dtos;

public record CreateTransactionRequest(string Description, DateTime TransactionDate, decimal Amount);

public record TransactionResponse(
    Guid Id,
    string Description,
    DateTime TransactionDate,
    decimal Amount);
