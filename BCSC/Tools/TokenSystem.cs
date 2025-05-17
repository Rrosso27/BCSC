using System.Transactions;

public static class TokenSystem
{
    private const int TotalTokens = 50000;
    private const decimal MaxValue = 1000m; // Valor máximo de los tokens
    private static decimal AvailableTokens = TotalTokens;
    private static readonly Dictionary<string, User> Users = new();
    private static Blockchain Blockchain = new Blockchain();

    /// <summary>
    /// Establece la blockchain sincronizada desde los nodos.
    /// </summary>
    public static void SetBlockchain(Blockchain blockchain)
    {
        Blockchain = blockchain;

        // Sincronizar los usuarios con la blockchain
        SynchronizeUsersFromBlockchain();
    }

    /// <summary>
    /// Obtiene la blockchain actual.
    /// </summary>
    public static Blockchain GetBlockchain()
    {
        return Blockchain;
    }

    /// <summary>
    /// Inicializa el sistema de tokens, restableciendo los tokens disponibles, los usuarios y la blockchain.
    /// </summary>
    public static void InitializeSystem()
    {
        AvailableTokens = TotalTokens;
        Users.Clear();
        Blockchain.Chain.Clear();
        Blockchain.Chain.Add(Blockchain.GetLatestBlock() ?? Blockchain.CreateGenesisBlock());

        // Sincronizar los usuarios con la blockchain
        SynchronizeUsersFromBlockchain();
    }

    public static void SellTokens(string userId, decimal amount)
    {
        if (!Users.ContainsKey(userId))
            throw new Exception("User does not exist.");

        if (!IsVerifiedUser(userId))
            throw new Exception("User is not verified.");

        var user = Users[userId];

        if (user.Balance < amount)
            throw new Exception("User does not have enough tokens.");

        user.Balance -= amount;
        AvailableTokens += amount;

        // Registrar la venta en la blockchain (de usuario a SYSTEM)
        var transaction = new Transaction(userId, "SYSTEM", amount, user.Email);
        Blockchain.AddBlock(new List<Transaction> { transaction });

        // Propagar el bloque a los nodos
        var newBlock = Blockchain.GetLatestBlock();
        NodeManager.PropagateBlock(newBlock);
    }


    public static List<string> GetActiveUsers()
    {
        return Users.Where(u => u.Value.Balance > 0).Select(u => u.Key).ToList();
    }

    /// <summary>
    /// Calcula el valor actual de los tokens basado en la oferta y la demanda.
    /// </summary>
    public static decimal GetTokenValue()
    {
        return 1 + (MaxValue - 1) * (1 - (AvailableTokens / TotalTokens));
    }

    /// <summary>
    /// Agrega un nuevo usuario al sistema.
    /// </summary>
    public static void AddUser(string userId, string email)
    {
        if (!Users.ContainsKey(userId))
        {
            Users[userId] = new User(userId, email);
        }
    }

    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    public static User? GetUser(string userId)
    {
        return Users.TryGetValue(userId, out var user) ? user : null;
    }

    private static bool IsVerifiedUser(string userId)
    {
        var user = TokenSystem.GetUser(userId);
        //return true;
        return user != null && user.IsVerified;
    }

    /// <summary>
    /// Transfiere una cantidad específica de tokens de un usuario a otro.
    /// </summary>
    public static void TransferTokens(string senderId, string receiverId, decimal amount)
    {
        if (!Users.ContainsKey(senderId) || !Users.ContainsKey(receiverId))
            throw new Exception("Sender or receiver does not exist.");

        // Solo permitir si ambos están verificados
        if (!IsVerifiedUser(senderId) || !IsVerifiedUser(receiverId))
            throw new Exception("Sender or receiver is not verified.");

        var sender = Users[senderId];
        var receiver = Users[receiverId];

        if (sender.Balance < amount)
            throw new Exception("Sender does not have enough tokens.");

        sender.Balance -= amount;
        receiver.Balance += amount;

        // Registrar la transacción en la blockchain con el email del receptor
        var transaction = new Transaction(senderId, receiverId, amount, receiver.Email);
        Blockchain.AddBlock(new List<Transaction> { transaction });

        // Propagar el bloque a los nodos
        var newBlock = Blockchain.GetLatestBlock();
        NodeManager.PropagateBlock(newBlock);
    }

    /// <summary>
    /// Distribuye una cantidad inicial de tokens a un usuario.
    /// </summary>
    public static void DistributeInitialTokens(string userId, decimal amount)
    {
        if (AvailableTokens < amount)
            throw new Exception("Not enough tokens available.");

        if (!Users.ContainsKey(userId))
            throw new Exception("User does not exist. Please register first.");

        // Solo permitir si el usuario está verificado
        if (!IsVerifiedUser(userId))
            throw new Exception("User is not verified.");

        Users[userId].Balance += amount;
        AvailableTokens -= amount;

        // Registrar la distribución en la blockchain con email
        var transaction = new Transaction("SYSTEM", userId, amount, Users[userId].Email);
        Blockchain.AddBlock(new List<Transaction> { transaction });
    }

    /// <summary>
    /// Obtiene un resumen del sistema de tokens, incluyendo los tokens disponibles, distribuidos y los balances de los usuarios.
    /// </summary>
    public static object GetTokenSummary()
    {
        var userBalances = Users.ToDictionary(u => u.Key, u => u.Value.Balance);
        return new
        {
            AvailableTokens,
            DistributedTokens = TotalTokens - AvailableTokens,
            UserBalances = userBalances
        };
    }

    /// <summary>
    /// Sincroniza los usuarios y sus balances basándose en las transacciones de la blockchain.
    /// </summary>
    public static void SynchronizeUsersFromBlockchain()
    {
        Users.Clear();
        decimal totalDistributedTokens = 0;

        foreach (var block in Blockchain.Chain)
        {
            foreach (var transaction in block.Transactions)
            {
                // Remitente (no SYSTEM)
                if (!string.IsNullOrEmpty(transaction.SenderId) && transaction.SenderId != "SYSTEM")
                {
                    if (!Users.ContainsKey(transaction.SenderId))
                    {
                        // Busca el email en la transacción, si existe
                        Users[transaction.SenderId] = new User(transaction.SenderId, transaction.Email ?? "");
                    }
                    Users[transaction.SenderId].Balance -= transaction.Amount;
                    Users[transaction.SenderId].IsVerified = true;

                }

                // Receptor
                if (!Users.ContainsKey(transaction.ReceiverId))
                {
                    Users[transaction.ReceiverId] = new User(transaction.ReceiverId, transaction.Email ?? "");
                }
                // Si el usuario ya existe y la transacción trae un email diferente y no vacío, actualiza el email
                else if (!string.IsNullOrEmpty(transaction.Email) && Users[transaction.ReceiverId].Email != transaction.Email)
                {
                    Users[transaction.ReceiverId].Email = transaction.Email;
                }

                Users[transaction.ReceiverId].Balance += transaction.Amount;
                Users[transaction.ReceiverId].IsVerified = true;
                if (transaction.SenderId == "SYSTEM")
                {
                    totalDistributedTokens += transaction.Amount;
                }
            }
        }

        AvailableTokens = TotalTokens - totalDistributedTokens;
    }
}
