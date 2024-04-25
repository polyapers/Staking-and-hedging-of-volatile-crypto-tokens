using Newtonsoft.Json;

namespace Staking.DTOs.eplorerExplorer;

public class SkynetSearchTransactionsResponse
{
	[JsonProperty("result")]
	public required SkyneteplorerTransactionData Result { get; set; }
}

public class SkyneteplorerTransactionData
{
	[JsonProperty("items")]
	public required List<SkyneteplorerTransaction> Items { get; set; }
	
	[JsonProperty("paging")]
	public required PagingData Paging { get; set; }
}

public class PagingData
{
	[JsonProperty("total")]
	public int Total { get; set; }
}