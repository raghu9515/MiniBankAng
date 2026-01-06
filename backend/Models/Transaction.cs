namespace MiniBank.Models;

public enum TransactionType
{
    Credit,
    Debit
}

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
}

public class TransferRequest
{
    public Guid DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
}
