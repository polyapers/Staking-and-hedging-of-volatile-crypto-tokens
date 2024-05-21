using Staking.Services;

namespace Staking.Scenarios;

public class ScenarioMapper
{
    private readonly DataAnalyzer analyzer;

    private readonly AggregatedData data;

	public List<Task> Tasks { get; set; }

	public ScenarioMapper(DataAnalyzer analyzer, List<Task> tasks, AggregatedData data, ServiceContainer serviceContainer)
	{
		this.analyzer = analyzer;
        this.data = data;
		Tasks = tasks;

		TokenTooHighScenario = new TokenTooHighScenario(analyzer, tasks, data, serviceContainer);
        TokenTooLowScenario = new TokenTooLowScenario(analyzer, tasks, data, serviceContainer);
        PositionsDisbalancedScenario = new PositionsDisbalancedScenario(analyzer, tasks, data, serviceContainer);
        CloseAllPositions = new CloseAllPositions(analyzer, tasks, data, serviceContainer);
        ReOpenAllPositions = new ReOpenAllPositions(analyzer, tasks, data, serviceContainer);
        TokenStakedAndShortDisbalanced = new TokenStakedAndShortDisbalanced(this.analyzer, tasks, data, serviceContainer);
        ShortToStakeRebalanceRequestedScenario = new ShortToStakeRebalanceRequestedScenario(this.analyzer, tasks, data, serviceContainer);
    }

    public TokenTooHighScenario TokenTooHighScenario { get; init; }

    public TokenTooLowScenario TokenTooLowScenario { get; init; }

    public PositionsDisbalancedScenario PositionsDisbalancedScenario { get; init; }

    public CloseAllPositions CloseAllPositions { get; init; }

    public ReOpenAllPositions ReOpenAllPositions { get; init; }
    
    public TokenStakedAndShortDisbalanced TokenStakedAndShortDisbalanced { get; init; }
    
    public ShortToStakeRebalanceRequestedScenario ShortToStakeRebalanceRequestedScenario { get; init; }

    public void Map()
	{
        analyzer.OnTokenPriceTooHigh += (_, price) =>
        {
            Console.WriteLine("Price too high! new price: " + price);

            if (data.DisabledUntillRestored)
                return;

            Tasks.Add(TokenTooHighScenario.Run());
        };

        analyzer.OnTokenPriceTooLow += (_, price) =>
        {
            if (data.DisabledUntillRestored)
                return;

            Console.WriteLine("Price too low! new price: " + price);
            Tasks.Add(TokenTooLowScenario.Run());
        };

        analyzer.OnPositionsDisbalanced += (_, _) =>
        {
            if (data.DisabledUntillRestored)
                return;

            Console.WriteLine("Positions Disbalanced!");
            Tasks.Add(PositionsDisbalancedScenario.Run());
        };

        analyzer.OnShortTooExpensive += (_, e) =>
        {
            if (data.DisabledUntillRestored)
                return;

            Console.WriteLine($"Short Too Expensive! ShortFees: {Math.Round(e.Item1, 2)}, stake apr: {Math.Round(e.Item2, 2)}");
            Tasks.Add(CloseAllPositions.Run());
        };

        analyzer.OnShortBackToNormal += (_, e) =>
        {
            if (!data.DisabledUntillRestored)
                return;

            Console.WriteLine($"Short Back To Normal! ShortFees: {Math.Round(e.Item1, 2)}, stake apr: {Math.Round(e.Item2, 2)}");
            Tasks.Add(ReOpenAllPositions.Run());
        };

        analyzer.OnTokenShortAndStakedDisbalanced += (_, _) =>
        {
            if (data.DisabledUntillRestored)
                return;

            Console.WriteLine("Positions in Token Disbalanced!");
            Tasks.Add(TokenStakedAndShortDisbalanced.Run());
        };
    }

    public void StartShortToStakeRebalanceRequestedScenario()
    {
        Tasks.Add(ShortToStakeRebalanceRequestedScenario.Run());
    }
}

