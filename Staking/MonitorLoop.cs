namespace Staking;

public class MonitorLoop
{
    private const int Default = 30;
    private readonly int periodic;
    private readonly Application application;

    public MonitorLoop(IConfiguration config, Application application)
    {
        var periodic = config.GetSection("Application:periodic")?.Value;
        if (!int.TryParse(periodic, out this.periodic))
        {
            this.periodic = Default;
        }
        this.application = application;
    }

    public void StartMonitorLoop()
    {
        Task.Run(async () => await MonitorAsync());
    }

    private async ValueTask MonitorAsync()
    {
        await RunTask();
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(periodic));
        while (await timer.WaitForNextTickAsync())
        {
            await RunTask();
        }
    }

    private async ValueTask RunTask()
    {
        try
        {
            await Task.Run(application.DoWork);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

