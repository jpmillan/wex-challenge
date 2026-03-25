namespace WexChallenge.Api.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }

    public Card Card { get; set; } = null!;
}
