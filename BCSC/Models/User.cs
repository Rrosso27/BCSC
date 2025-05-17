public class User
{
    public string Id { get; set; }
    public decimal Balance { get; set; }
    public string Email { get; set; }
    public bool IsVerified { get; set; } // Nuevo campo
    public string? VerificationCode { get; set; }

    public User(string id, string email)
    {
        Id = id;
        Email = email;
        Balance = 0;
        IsVerified = false;
    }
}
