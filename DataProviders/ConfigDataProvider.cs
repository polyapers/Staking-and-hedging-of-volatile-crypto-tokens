namespace Staking.DataProviders;

public class ConfigDataProvider : DataProviderBase
{
	private readonly IConfiguration config;
	
	private readonly AggregatedData data;

	public ConfigDataProvider(IConfiguration config, AggregatedData data)
	{
		this.config = config;
		this.data = data;
	}

	protected sealed override Task GatherData()
	{
		data.MaxValue = decimal.Parse(config.GetSection("BaseData:MaxValue").Value ?? throw new Exception("Base data is null."));
		
		data.BalancerThreshold = decimal.Parse(config.GetSection("Application:BalancerThreshold").Value ?? throw new Exception("Base data is null."));

		data.BalancerBasedOnTokenEnabled =
			bool.Parse(config.GetSection("Application:TokenBasedBalancerEnabled").Value ?? throw new Exception("TokenBasedBalancerEnabled setting is not set."));
		
		data.BalancerBasedOnTokenThreshold = 
			int.Parse(config.GetSection("Application:TokenBasedBalancerThreshold").Value ?? throw new Exception("TokenBasedBalancerThreshold setting is not set."));
		
		return Task.CompletedTask;
	}
}