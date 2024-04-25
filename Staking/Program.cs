using System.Globalization;
using CryptoExchangeTools;
using Staking;
using Staking.DataAccess.Data;
using Staking.DataAccess.DbAccess;
using Staking.Scripts;
using Staking.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton<MonitorLoop>();
        services.AddSingleton<Application>();
        services.AddSingleton<ServiceContainer>();
        services.AddSingleton<IeplorerService, eplorerService>();
        services.AddSingleton<IBinanceService, BinanceService>();
        services.AddSingleton<IBalancer, Balancer>();
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IExplorerApiService, ExplorerApiService>();
        services.AddSingleton<ISqlDataAccess, SqlDataAccess>();
        services.AddSingleton<FillIncomeLog>();
    })
    .Build();

// await host.Services.GetRequiredService<FillIncomeLog>().Run();

try
{
    host.Services.GetRequiredService<MonitorLoop>().StartMonitorLoop();

    await host.RunAsync();
}
catch (RequestNotSuccessfulException ex)
{
    throw new Exception($"{ex.Endpoint}, {ex.StatusCode}, {ex.Response}");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    await host.Services.GetRequiredService<ILoggerService>().ReportError(ex);
    await host.Services.GetRequiredService<ILoggerService>().LogError(ex, "Program.cs");
}
