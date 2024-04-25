using Staking.Services;

namespace Staking.Scenarios;

public class TokenStakedAndShortDisbalanced : ScenarioBase
{
	public TokenStakedAndShortDisbalanced(DataAnalyzer analyzator, List<Task> tasks, AggregatedData data, ServiceContainer serviceContainer) : base(analyzator, tasks, data, serviceContainer)
	{
	}

	protected sealed override async Task Execute()
	{
		await Balancer.Run(Data);
	}
}