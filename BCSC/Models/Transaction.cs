public class Transaction
{
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public decimal Amount { get; set; }
    public string? Email { get; set; }  

    public Transaction(string senderId, string receiverId, decimal amount, string? email = null)
    {
        SenderId = senderId;
        ReceiverId = receiverId;
        Amount = amount;
        Email = email;
    }

    public override string ToString()
    {
        return $"{SenderId}->{ReceiverId}:{Amount}:{Email}";
    }
}
