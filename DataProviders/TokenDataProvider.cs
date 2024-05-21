using Staking.Services;

namespace Staking.DataProviders;

public class TokenDataProvider : DataProviderBase
{
	private readonly AggregatedData data;

	private readonly IBinanceService binanceService;

	private readonly IeplorerService eplorerService;

	public TokenDataProvider(AggregatedData data, ServiceContainer serviceContainer)
	{
		this.data = data;
		binanceService = serviceContainer.BinanceService;
		eplorerService = serviceContainer.eplorerService;
    }

	protected sealed override async Task GatherData()
	{
		var tasks = new List<Task>
		{
			GetTokenPrice(),

			GetStakingData(),
			GetShortingData("ETHUSDT"),

			GetSpotBalances(),

			GetPositionInfo(),
			
			GetIncomeHistoryRecords(),
		};

		await Task.WhenAll(tasks);
	}

	private async Task GetStakingData()
	{
		data.CurrentlyStaked = await eplorerService.GetStakingAmount();
	}

	private async Task GetShortingData()
	{
		var futuresAccaountBalance = await binanceService.GetFuturesAccountBalanceUsdt();

		data.FuturesAccountBalance = futuresAccaountBalance.AvailableBalance;
		
		data.ShortPositionValue = futuresAccaountBalance.Balance + futuresAccaountBalance.CrossUnPnl;

		data.ShortUnPnl = futuresAccaountBalance.CrossUnPnl;
	}

	private async Task GetPositionInfo(string ticker)
	{
		data.ShortPositionInfo = await binanceService.GetShortPosition(ticker);
	}

	private async Task GetTokenPrice()
	{
		data.CurrentTokenPrice = await binanceService.GetTokenUsdtPrice();
	}

	private async Task GetSpotBalances()
	{
		data.SpotTokenBalance = await binanceService.GetSpotTokenBalance();
		data.SpotUsdtBalance = await binanceService.GetSpotUsdtBalance();
	}

	private async Task GetIncomeHistoryRecords()
	{
		data.IncomeHistoryRecords = await binanceService.GetIncomeHistory();
	}
}

