using Newtonsoft.Json;
using Staking.Utility;

namespace Staking.DataAccess.Models.BlockchainTypes.cs;

public class GetBlocksAnswer
{
    [JsonProperty("results")]
    public required List<BlockInfo> Results { get; set; }

    [JsonProperty("total")]
    public long Total { get; set; }
}

public class BlockInfo
{
    [JsonProperty("number")]
    public long Number { get; set; }

    [JsonProperty("hash")]
    public required string Hash { get; set; }

    [JsonProperty("parent_hash")]
    public required string ParentHash { get; set; }

    [JsonProperty("nonce")]
    [JsonConverter(typeof(HexStringToUlongConverter))]
    public ulong Nonce { get; set; }

    [JsonProperty("transaction_root")]
    public required string TransactionRoot { get; set; }

    [JsonProperty("state_root")]
    public required string StateRoot { get; set; }

    [JsonProperty("receipts_root")]
    public required string ReceiptsRoot { get; set; }

    [JsonProperty("miner")]
    public required string Miner { get; set; }

    [JsonProperty("difficulty")]
    [JsonConverter(typeof(StringToLongConverter))]
    public long Difficulty { get; set; }

    [JsonProperty("extra_data")]
    public string? ExtraData { get; set; }

    [JsonProperty("size")]
    public long Size { get; set; }

    [JsonProperty("gas_limit")]
    [JsonConverter(typeof(StringToLongConverter))]
    public long GasLimit { get; set; }

    [JsonProperty("gas_used")]
    [JsonConverter(typeof(StringToLongConverter))]
    public long GasUsed { get; set; }

    [JsonProperty("fee_earned")]
    public string? FeeEarned { get; set; }

    [JsonProperty("transactions")]
    public long Transactions { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
}

