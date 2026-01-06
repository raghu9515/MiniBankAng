namespace MiniBank.Models;

public class Account : IAuditable
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public List<Transaction> Transactions { get; set; } = [];
}

public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
}
