using Newtonsoft.Json.Linq;
using System.Globalization;
using CryptoExchangeTools.Models.Binance.Futures.USDM;

namespace Staking;

public class AggregatedData
{
	private readonly IConfiguration config;

	public decimal CurrentTokenPrice { get; set; }

	public decimal BaseTokenPrice { get; set; }

	public decimal BaseTokenStakingAmount { get; set; }

    public decimal BaseTokenShortingAmount { get; set; }

    public decimal TokenPriceThreshold { get; set; }

	public decimal CurrentlyStaked { get; set; }

    public decimal ShortPositionValue { get; set; }
    
    public decimal ShortUnPnl { get; set; }

	public bool DisabledUntillRestored { get; set; }

	public decimal ShortFeesYearly { get; set; }

	public decimal StakeApr { get; set; }

	public string? CurrentScenario { get; set; }
	
	public decimal MaxValue { get; set; }
	
	public decimal BalancerThreshold { get; set; }
	
	public decimal FuturesAccountBalance { get; set; }
	
	public decimal SpotTokenBalance { get; set; }
	
	public decimal SpotUsdtBalance { get; set; }
	
	public PositionInformation? ShortPositionInfo { get; set; }
	
	public IncomeHistoryRecord[]? IncomeHistoryRecords { get; set; }

	public bool BalancerBasedOnTokenEnabled { get; set; }
	
	public int BalancerBasedOnTokenThreshold { get; set; }

    public AggregatedData(IConfiguration config)
	{
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
        this.config = config;

		var fileContents = File.ReadAllText("InitialData.json");
		dynamic json = JObject.Parse(fileContents);

		BaseTokenPrice = json["BaseTokenPrice"];

		BaseTokenStakingAmount = decimal.Parse(config.GetSection("BaseData:BaseTokenStakingAmount").Value ?? throw new Exception("Base data is null."));

        BaseTokenShortingAmount = decimal.Parse(config.GetSection("BaseData:BaseTokenShortingAmount").Value ?? throw new Exception("Base data is null."));

		DisabledUntillRestored = false;

        SetDefaults();
    }

    public decimal CalculateProfitability()
    {
	    return StakeApr * (2m / 3m) - ShortFeesYearly * (2m / 3m);
    }

    public decimal CalculateTotalValue()
    {
	    return CurrentlyStaked * CurrentTokenPrice + ShortPositionValue + SpotTokenBalance * CurrentTokenPrice + SpotUsdtBalance;
    }

	private void SetDefaults()
	{
		CurrentTokenPrice = BaseTokenPrice;
		TokenPriceThreshold = 0.1m;
		BalancerBasedOnTokenEnabled = false;
		BalancerBasedOnTokenThreshold = 10;
	}

	public decimal GetStakeValuation()
	{
		return CurrentTokenPrice * CurrentlyStaked;
	}
}

