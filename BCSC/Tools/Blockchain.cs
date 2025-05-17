using System.Text.Json.Serialization;

public class Blockchain
{
    [JsonInclude]
    public List<Block> Chain { get; private set; }

    public Blockchain()
    {
        Chain = new List<Block> { CreateGenesisBlock() };
    }

    public Block CreateGenesisBlock()
    {
        return new Block(0, "0", new List<Transaction>());
    }

    public Block? GetLatestBlock()
    {
        return Chain.Count > 0 ? Chain[^1] : null;
    }

    public void AddBlock(List<Transaction> transactions)
    {
        Block latestBlock = GetLatestBlock();
        Block newBlock = new Block(latestBlock.Index + 1, latestBlock.Hash, transactions);
        Chain.Add(newBlock);
    }

    public bool IsValid()
    {
        for (int i = 1; i < Chain.Count; i++)
        {
            Block currentBlock = Chain[i];
            Block previousBlock = Chain[i - 1];

            if (currentBlock.Hash != currentBlock.CalculateHash() ||
                currentBlock.PreviousHash != previousBlock.Hash)
            {
                return false;
            }
        }
        return true;
    }
}
