using Nethereum.Web3;

public class EthereumService
{
    private readonly string _endpoint;
    private readonly Web3 _web3;

    public EthereumService(IConfiguration config)
    {
        _endpoint = config.GetSection("INFURA")["ENDPOINT"];
        _web3 = new Web3(_endpoint);
    }

    public async Task<ulong> GetLatestBlockNumberAsync()
    {
        var blockNumber = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        return (ulong)blockNumber.Value;
    }

    public async Task<decimal> GetBalanceAsync(string address)
    {
        var balanceWei = await _web3.Eth.GetBalance.SendRequestAsync(address);
        return Web3.Convert.FromWei(balanceWei.Value);
    }
    public async Task<Nethereum.RPC.Eth.DTOs.Transaction> GetTransactionByHashAsync(string txHash)
    {
        return await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);
    }

    public async Task<Nethereum.RPC.Eth.DTOs.BlockWithTransactions> GetBlockByNumberAsync(ulong blockNumber)
    {
        var blockParameter = new Nethereum.RPC.Eth.DTOs.BlockParameter(blockNumber);
        return await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(blockParameter);
    }


    public async Task<string> SendEtherAsync(string privateKey, string toAddress, decimal amount)
    {
        var account = new Nethereum.Web3.Accounts.Account("ec5de8d38e0241b5975ee7a88fb95bd8");
        var web3 = new Web3(account, _endpoint);
        var txHash = await web3.Eth.GetEtherTransferService()
            .TransferEtherAndWaitForReceiptAsync(toAddress, amount);
        return txHash.TransactionHash;
    }





    // Puedes añadir más métodos para consultar balances, transacciones, etc.
}
