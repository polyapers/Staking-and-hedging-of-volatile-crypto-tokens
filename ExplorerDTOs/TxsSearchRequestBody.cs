using Newtonsoft.Json;

namespace Staking.DTOs.eplorerExplorer;

public class TxsSearchRequestBody
{
	[JsonProperty("address")]
	public AddressData Address { get; set; }
	
	[JsonProperty("paging")]
	public PagingDataInput Paging { get; set; }

	public TxsSearchRequestBody(string address, int offset, int limit)
	{
		Address = new AddressData()
		{
			RelateTo = address
		};

		Paging = new PagingDataInput()
		{
			Offset = offset,
			Limit = limit
		};
	}
}

public class AddressData
{
	[JsonProperty("relateTo")]
	public required string RelateTo { get; set; }
}

public class PagingDataInput
{
	[JsonProperty("offset")]
	public required int Offset { get; set; }
	
	[JsonProperty("limit")]
	public required int Limit { get; set; }
}