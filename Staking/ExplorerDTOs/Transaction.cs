using Newtonsoft.Json;

namespace Staking.DTOs.eplorerExplorer;

public class Transaction
{
	[JsonProperty("hash")]
	public required string Hash { get; set; }

	[JsonProperty("block_hash")]
	public required string BlockHash { get; set; }

	[JsonProperty("block_number")]
	public long BlockNumber { get; set; }

	[JsonProperty("from")]
	public required string From { get; set; }

	[JsonProperty("to")]
	public required string To { get; set; }

	[JsonProperty("gas")]
	public required string Gas { get; set; }

	[JsonProperty("gas_price")]
	public required string GasPrice { get; set; }

	[JsonProperty("input")]
	public required string Input { get; set; }

	[JsonProperty("nonce")]
	public long Nonce { get; set; }

	[JsonProperty("tx_index")]
	public long TxIndex { get; set; }

	[JsonProperty("value")]
	public required string Value { get; set; }

	[JsonProperty("gas_used")]
	public required string GasUsed { get; set; }

	[JsonProperty("cumulative_gas_used")]
	public required string CumulativeGasUsed { get; set; }

	[JsonProperty("contract_address")]
	public required string ContractAddress { get; set; }

	[JsonProperty("status")]
	public long Status { get; set; }

	[JsonProperty("timestamp")]
	public long Timestamp { get; set; }
}