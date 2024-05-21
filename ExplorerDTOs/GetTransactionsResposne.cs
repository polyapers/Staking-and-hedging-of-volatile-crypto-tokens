using Newtonsoft.Json;

namespace Staking.DTOs.eplorerExplorer;

public class GetTransactionsResposne
{
	[JsonProperty("results")]
	public required List<eplorerTransaction> Results { get; set; }
	
	[JsonProperty("total")]
	public int Total { get; set; }
}