using Staking.Services;

namespace Staking.DataProviders;

public class StakingRewardsDataProvider : DataProviderBase
{
    private readonly AggregatedData data;

    private readonly IExplorerService explorerService;

    public StakingRewardsDataProvider(AggregatedData data, ServiceContainer serviceContainer)
    {
        this.data = data;
        explorerService = serviceContainer.ExplorerService;
    }

    protected sealed override async Task GatherData()
    {
        await CalculateApr();
    }

    public async Task CalculateApr()
    {
        try
        {
            data.StakeApr = await explorerService.CalculateApr();
        }
        catch (Exception)
        {
            data.StakeApr = 0.3501m;
        }
    }
}

