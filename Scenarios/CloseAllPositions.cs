using Staking.Services;

namespace Staking.Scenarios;

public class CloseAllPositions : ScenarioBase
{
	public CloseAllPositions(DataAnalyzer analyzator, List<Task> tasks, AggregatedData data, ServiceContainer serviceContainer) : base(analyzator, tasks, data, serviceContainer)
    {
    }

    protected sealed override async Task Execute()
    {
        //await eplorerService.UnstakeAll();

        //await log.LogTg("Closed entire staking position.");

        //var removedShort = await binanceService.CloseEntireShortingPosition();

        //await log.LogTg($"Closed entire short position ({Math.Round(removedShort, 2)} Token).");
    }
}

