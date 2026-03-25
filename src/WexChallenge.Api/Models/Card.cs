namespace WexChallenge.Api.Models;

public class Card
{
    public Guid Id { get; set; }
    public decimal CreditLimit { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<Transaction> Transactions { get; set; } = new();
}
