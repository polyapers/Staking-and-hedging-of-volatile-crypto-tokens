using CryptoExchangeTools.Models.Binance.Futures.USDM;
using Staking.DataAccess.Data;
using Staking.DataProviders;
using Staking.Scenarios;
using Staking.Services;

namespace Staking;

public class Application
{
	private readonly ServiceContainer serviceContainer;

	public AggregatedData Data { get; }

	public DataAnalyzer Analyzer { get; }

	public List<IDataProvider> DataProviders { get; }

	public ScenarioMapper ScenarioMapper { get; set; }

    public readonly List<Task> ScenarioTasks = new();

	private readonly ILoggerService log;

	private readonly IExplorerService explorerService;

	private readonly IBinanceService binanceService;

	private readonly IConfiguration config;

	public Application(ServiceContainer serviceContainer, IConfiguration config, ILoggerService log, IeplorerService eplorerService, IBinanceService binanceService)
	{
		this.serviceContainer = serviceContainer;
		this.config = config;
		this.log = log;
		this.eplorerService = eplorerService;
		this.binanceService = binanceService;

		Data = new AggregatedData(config);
		Analyzer = new DataAnalyzer(Data, config);
		DataProviders = BuildDataProviders();
		ScenarioMapper = new ScenarioMapper(Analyzer, ScenarioTasks, Data, serviceContainer);

		ScenarioMapper.Map();

        foreach (var dataProvider in DataProviders)
        {
            dataProvider.Run().Wait();
        }
    }

	private List<IDataProvider> BuildDataProviders()
	{
		return new List<IDataProvider>
		{
			new ConfigDataProvider(config, Data),
			new TokenDataProvider(Data, serviceContainer),
			new StakingRewardsDataProvider(Data, serviceContainer),
			new ShortPriceDataProvider(Data, serviceContainer)
        };
    }

	public async Task DoWork()
	{
		try
		{
			Console.WriteLine($"{DateTime.Now} Starting cycle.");

			await ReStakeRewards();
			
			await StakeAllAvailableBalanceIfAny();
		

			var tasks = DataProviders.Select(dataProvider => dataProvider.Run());

			await Task.WhenAll(tasks);

			LogData();

			await CheckNetworkStatus();

			Console.WriteLine($"{DateTime.Now} Data Gathering completed.");

			Analyzer.Run();

			Console.WriteLine("Analyzed.");
		}
		catch (Exception ex)
		{
			await log.ReportError(ex);
			await log.LogError(ex, "Application.cs");
		}
	}

	private async Task ReStakeRewards()
	{
		await eplorerService.ReStakeAllPendingRewardsIfPossible(Data.CurrentTokenPrice);
	}
	
	private async Task ShortAllSpotBalanceIfAny()
	{
		if(!string.IsNullOrEmpty(Data.CurrentScenario))
			return;
		
		if (await lockStatusData.IsLockedByInvestment())
			return;

		await binanceService.ShortAllSpotBalanceIfAny(Data.CalculateTotalValue(), Data.MaxValue, Data.CurrentTokenPrice);
	}

	private async Task StakeAllAvailableBalanceIfAny()
	{
		if(!string.IsNullOrEmpty(Data.CurrentScenario))
			return;
		
		if (await lockStatusData.IsLockedByInvestment())
			return;
		
		await eplorerService.StakeAllAvailableToken();
	}

	private void LogData()
	{
		_ = new List<Task>()
		{
			LogPosition(),
			log.LogData(Data),
			LogIncomeHistory(),
			LogFundingHistory()
		};
	}

	private async Task LogPosition()
	{
		if(Data.ShortPositionInfo is null)
			return;

		await log.LogPositionInfo(Data.ShortPositionInfo);
	}

	private async Task LogIncomeHistory()
	{
		if(Data.IncomeHistoryRecords is null || !Data.IncomeHistoryRecords.Any())
			return;

		await log.LogIncomeHistory(Data.IncomeHistoryRecords);
	}

	private async Task LogFundingHistory()
	{
		if(Data.IncomeHistoryRecords is null || !Data.IncomeHistoryRecords.Any())
			return;

		var fundingRecords = Data.IncomeHistoryRecords.Where(x => x.IncomeType == IncomeType.FUNDING_FEE).ToArray();

		await log.LogFundingHistory(fundingRecords);
	}

	private async Task CheckNetworkStatus()
	{
		var withdrawInfo = await binanceService.WithdrawSuspended();

		if (withdrawInfo.Item1)
			throw new Exception(string.IsNullOrEmpty(withdrawInfo.Item2) ? "Withdrawal on RON is suspended!" : withdrawInfo.Item2);
			
		var depositInfo = await binanceService.DepositSuspended();

		if (depositInfo.Item1)
			throw new Exception(string.IsNullOrEmpty(depositInfo.Item2) ? "Deposit on RON is suspended!" : depositInfo.Item2);

	}
}

