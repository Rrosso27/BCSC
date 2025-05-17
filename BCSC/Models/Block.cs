public class Block
{
    public int Index { get; set; }
    public string PreviousHash { get; set; }
    public string Hash { get; set; }
    public List<Transaction> Transactions { get; set; }
    public DateOnly Date { get; set; }      // Solo la fecha
    public TimeOnly Time { get; set; }      // Solo la hora

    public Block(int index, string previousHash, List<Transaction> transactions)
    {
        Index = index;
        PreviousHash = previousHash;
        Transactions = transactions;
        Date = DateOnly.FromDateTime(DateTime.Now);
        Time = TimeOnly.FromDateTime(DateTime.Now);
        Hash = CalculateHash();
    }

    public string CalculateHash()
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            string rawData = $"{Index}{PreviousHash}{Date:yyyy-MM-dd}{Time:HH:mm:ss.fffffff}{string.Join("", Transactions)}";
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }
    }
}