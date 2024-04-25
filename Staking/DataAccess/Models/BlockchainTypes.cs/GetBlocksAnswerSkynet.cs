using Newtonsoft.Json;

namespace Staking.DataAccess.Models.BlockchainTypes.cs;

public class GetBlocksAnswerSkynet
{
	[JsonProperty("result")]
	public required Result Result { get; set; }
}

public class Result
{
	[JsonProperty("items")]
	public required SkynetBlockInfo[] Items { get; set; }
}

public class SkynetBlockInfo
{
	[JsonProperty("number")]
	public required int Number { get; set; }
	
	[JsonProperty("timestamp")]
	public required int Timestamp { get; set; }
}