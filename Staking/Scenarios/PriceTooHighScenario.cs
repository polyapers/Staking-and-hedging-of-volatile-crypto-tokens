using Staking.Services;

namespace Staking.Scenarios;

public class TokenTooHighScenario : ScenarioBase
{
    public TokenTooHighScenario(DataAnalyzer analyzator, List<Task> tasks, AggregatedData data, ServiceContainer serviceContainer) : base(analyzator, tasks, data, serviceContainer)
    {
    }

    protected sealed override async Task Execute()
    {
        await Balancer.Run(Data);
    }
}

