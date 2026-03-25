namespace WexChallenge.Api.Dtos;

public record CreateTransactionRequest(string Description, DateTime TransactionDate, decimal Amount);

public record TransactionResponse(
    Guid Id,
    string Description,
    DateTime TransactionDate,
    decimal Amount);

public record ConvertedTransactionResponse(
    Guid Id,
    string Description,
    DateTime TransactionDate,
    decimal OriginalAmount,
    decimal ExchangeRate,
    decimal ConvertedAmount,
    string Currency);
