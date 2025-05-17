using System.IO;
using System.Text.Json;
using LiteDB;

public static class NodeManager
{
    private const int NodeCount = 9;
    private static readonly string NodeDirectory = "Nodes";
    private const string DatabasePath = "Nodes/nodes.db";

    /// <summary>
    /// Inicializa los nodos creando archivos JSON y guardando en LiteDB para cada nodo si no existen.
    /// </summary>
    public static void InitializeNodes()
    {
        // Asegurar directorio
        if (!Directory.Exists(NodeDirectory))
        {
            Directory.CreateDirectory(NodeDirectory);
        }

        // Inicializar base de datos
        using (var db = new LiteDatabase(DatabasePath))
        {
            var col = db.GetCollection<BlockchainDocument>("blockchains");
            col.EnsureIndex(x => x.NodeId);

            for (int i = 1; i <= NodeCount; i++)
            {
                string jsonFile = Path.Combine(NodeDirectory, $"Node{i}.json");
                var existing = col.FindOne(x => x.NodeId == i);

                if (!File.Exists(jsonFile))
                {
                    var blockchain = new Blockchain();
                    SaveBlockchainToFile(blockchain, jsonFile);
                    InsertOrUpdateInDatabase(col, i, blockchain);
                }
                else if (existing == null)
                {
                    // Si existe el archivo pero no el registro, cargar y guardar en DB
                    var blockchain = LoadBlockchainFromFile(jsonFile);
                    InsertOrUpdateInDatabase(col, i, blockchain);
                }
            }
        }
    }

    public static Blockchain LoadBlockchainFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return System.Text.Json.JsonSerializer.Deserialize<Blockchain>(json);
    }

    public static void SaveBlockchainToFile(Blockchain blockchain, string path)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(blockchain, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static Blockchain LoadBlockchainFromDatabase(int nodeId)
    {
        using (var db = new LiteDatabase(DatabasePath))
        {
            var col = db.GetCollection<BlockchainDocument>("blockchains");
            var doc = col.FindOne(x => x.NodeId == nodeId);
            return doc != null
                ? System.Text.Json.JsonSerializer.Deserialize<Blockchain>(doc.JsonData)
                : new Blockchain();
        }
    }

    public static void SaveBlockchainToDatabase(Blockchain blockchain, int nodeId)
    {
        using (var db = new LiteDatabase(DatabasePath))
        {
            var col = db.GetCollection<BlockchainDocument>("blockchains");
            InsertOrUpdateInDatabase(col, nodeId, blockchain);
        }
    }

    private static void InsertOrUpdateInDatabase(ILiteCollection<BlockchainDocument> col, int nodeId, Blockchain chain)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(chain, new JsonSerializerOptions { WriteIndented = true });
        var doc = new BlockchainDocument { NodeId = nodeId, JsonData = json };
        col.Upsert(doc);
    }

    /// <summary>
    /// Sincroniza la blockchain seleccionando la más larga y válida de los nodos (de base de datos).
    /// </summary>
    public static Blockchain SynchronizeBlockchain()
    {
        Blockchain best = null;
        using (var db = new LiteDatabase(DatabasePath))
        {
            var col = db.GetCollection<BlockchainDocument>("blockchains");
            foreach (var doc in col.FindAll())
            {
                var chain = System.Text.Json.JsonSerializer.Deserialize<Blockchain>(doc.JsonData);
                if (chain.IsValid() && (best == null || chain.Chain.Count > best.Chain.Count))
                {
                    best = chain;
                }
            }
        }

        return best ?? new Blockchain();
    }

    /// <summary>
    /// Propaga un nuevo bloque a todos los nodos (archivo y base).
    /// </summary>
    public static void PropagateBlock(Block newBlock)
    {
        using (var db = new LiteDatabase(DatabasePath))
        {
            var col = db.GetCollection<BlockchainDocument>("blockchains");

            for (int i = 1; i <= NodeCount; i++)
            {
                string file = Path.Combine(NodeDirectory, $"Node{i}.json");
                var chain = LoadBlockchainFromFile(file);
                chain.AddBlock(newBlock.Transactions);

                SaveBlockchainToFile(chain, file);
                InsertOrUpdateInDatabase(col, i, chain);
            }
        }
    }
}