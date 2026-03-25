namespace WexChallenge.Api.Dtos;

public record BalanceResponse(
    Guid CardId,
    decimal CreditLimit,
    decimal TotalSpent,
    decimal AvailableBalance,
    string? Currency,
    decimal? ExchangeRate,
    decimal? ConvertedBalance);
