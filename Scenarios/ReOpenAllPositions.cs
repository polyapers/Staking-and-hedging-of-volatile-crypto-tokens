using Staking.Services;

namespace Staking.Scenarios;

public class ReOpenAllPositions : ScenarioBase
{
    public ReOpenAllPositions(DataAnalyzer analyzator, List<Task> tasks, AggregatedData data, ServiceContainer serviceContainer) : base(analyzator, tasks, data, serviceContainer)
    {
    }

    protected sealed override async Task Execute()
    {
        //var stakedToken = await eplorerService.StakeAllAvailableToken();

        //await log.LogTg($"Staked {Math.Round(stakedToken, 2)} Token.");

        //await binanceService.ShortAllAvailableBalance();
    }
}
