using Staking.DataAccess.Data;
using Staking.Services;

namespace Staking.Scenarios;

public abstract class ScenarioBase
{
    private readonly List<Task> tasks;

    private readonly string name;

    protected readonly AggregatedData Data;

    protected IExplorerService ExplorerService { get; }

    protected IBinanceService BinanceService { get; }
    
    protected ILockStatusData LockStatusData { get; }

    private readonly object scenarioStartLockObject = new();

    protected readonly IBalancer Balancer;

    protected readonly ILoggerService Log;

    protected ScenarioBase(DataAnalyzer analyzator, List<Task> tasks, AggregatedData data, ServiceContainer serviceContainer)
    {
        this.tasks = tasks;
        Data = data;
        name = GetType().Name;

        ExplorerService = serviceContainer.ExplorerService;
        Balancer = serviceContainer.Balancer;
        Log = serviceContainer.Log;
        BinanceService = serviceContainer.BinanceService;
        LockStatusData = serviceContainer.LockStatusData;
    }

    public async Task Run()
    {
        List<Task> temp;
        
        lock (scenarioStartLockObject)
        {
            if (tasks.Any(x => x.Status == TaskStatus.WaitingForActivation))
            {
                Console.WriteLine($"One Scenario is already running! {Data.CurrentScenario}");

                return;
            }
            
            Data.CurrentScenario = $"{name} - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            tasks.Add(Execute());
        }
        
        try
        {
            temp = new List<Task>
            {
                LockStatusData.StartNewScenario(),
                Log.LogTg($"Running scenario {name}.")
            };
            
            await Task.WhenAll(tasks);

            temp.Add(Log.LogTg($"Scenario completed {name}."));
        }
        catch (Exception ex)
        {
            await Log.ReportError(ex);
            await Log.LogError(ex, $"Scenario {name}.");
            await Log.LogAction("Resetting scenario.");
        }
        finally
        {
            lock (scenarioStartLockObject)
            {
                Data.CurrentScenario = null;
            }
            
            await LockStatusData.FinishScenario();
        }
    }

    protected abstract Task Execute();
}

