using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BlockchainController : ControllerBase
{
    private readonly EthereumService _ethereumService;

    public BlockchainController(EthereumService ethereumService)
    {
        _ethereumService = ethereumService;
    }

    [HttpGet("latest-block")]
    public async Task<IActionResult> GetLatestBlock()
    {
        var blockNumber = await _ethereumService.GetLatestBlockNumberAsync();
        return Ok(new { BlockNumber = blockNumber });
    }
    /// <summary>
    /// Obtiene la cadena de bloques completa con todas las transacciones registradas.
    /// </summary>
    [HttpGet("blockchain")]
    public IActionResult GetBlockchain()
    {
        var blockchain = TokenSystem.GetBlockchain();
        return Ok(blockchain.Chain);
    }

    /// <summary>
    /// Valida la integridad de la blockchain.
    /// </summary>
    [HttpGet("validate-blockchain")]
    public IActionResult ValidateBlockchain()
    {
        var isValid = TokenSystem.GetBlockchain().IsValid();
        return Ok(new { IsValid = isValid });
    }

    [HttpGet("transactions/{userId}")]
    public IActionResult GetTransactionHistory(string userId)
    {
        var blockchain = TokenSystem.GetBlockchain();
        var transactions = blockchain.Chain
            .SelectMany(block => block.Transactions)
            .Where(tx => tx.SenderId == userId || tx.ReceiverId == userId)
            .ToList();

        return Ok(transactions);
    }

    [HttpGet("statistics")]
    public IActionResult GetSystemStatistics()
    {
        var blockchain = TokenSystem.GetBlockchain();
        var totalTransactions = blockchain.Chain.Sum(block => block.Transactions.Count);
        var activeUsers = TokenSystem.GetActiveUsers().Count;
        var tokenValue = TokenSystem.GetTokenValue();

        return Ok(new
        {
            TotalTransactions = totalTransactions,
            ActiveUsers = activeUsers,
            TokenValue = tokenValue
        });
    }


}

