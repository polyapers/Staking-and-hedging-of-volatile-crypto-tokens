using Newtonsoft.Json;

namespace Staking.DTOs.eplorerExplorer;

public class SkyneteplorerTransaction
{
	[JsonProperty("hash")]
	public required string Hash { get; set; }

	[JsonProperty("from")]
	public required string From { get; set; }

	[JsonProperty("to")]
	public required string To { get; set; }
	
	[JsonProperty("input")]
	public required string Input { get; set; }

	[JsonProperty("status")]
	public long Status { get; set; }

	[JsonProperty("blockTime")]
	public long Timestamp { get; set; }
}

