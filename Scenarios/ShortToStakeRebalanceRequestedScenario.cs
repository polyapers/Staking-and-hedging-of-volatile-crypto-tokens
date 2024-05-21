using Staking.DataAccess.Data;
using Staking.Services;

namespace Staking.Scenarios;

public class ShortToStakeRebalanceRequestedScenario: ScenarioBase
{
	private readonly IRebalanceOrderData rebalanceOrderData;
	
	public ShortToStakeRebalanceRequestedScenario(DataAnalyzer analyzator, List<Task> tasks, AggregatedData data, ServiceContainer serviceContainer) : base(analyzator, tasks, data, serviceContainer)
	{
		rebalanceOrderData = serviceContainer.RebalanceOrderData;
	}

	protected sealed override async Task Execute()
	{
		await Balancer.Run(Data);
	}
}