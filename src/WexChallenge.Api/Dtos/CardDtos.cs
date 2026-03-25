namespace WexChallenge.Api.Dtos;

public record CreateCardRequest(decimal CreditLimit);

public record CardResponse(Guid Id, decimal CreditLimit, DateTime CreatedAt);
