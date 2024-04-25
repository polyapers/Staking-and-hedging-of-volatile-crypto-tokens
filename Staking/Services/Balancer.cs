using System.Globalization;
using CryptoExchangeTools.Models.Binance.Futures.USDM;
using Staking.DataAccess.Data;

namespace Staking.Services;

public class Balancer : IBalancer
{
    private readonly IeplorerService eplorerService;

    private readonly IBinanceService binanceService;

    private readonly ILoggerService log;

    private readonly IConfiguration config;
    
    private const int Leverage = 2;

    public Balancer(IConfiguration config, IeplorerService eplorerService, IBinanceService binanceService, ILoggerService log)
    {
        this.config = config;
        this.eplorerService = eplorerService;
        this.binanceService = binanceService;
        this.log = log;
    }

    public async Task Run(AggregatedData data)
    {
        var stakeValuation = data.CurrentTokenPrice * data.CurrentlyStaked;
        var shortValuation = data.ShortPositionValue;

        var totalValue = data.CalculateTotalValue();
        
        var stakingTarget = totalValue * 2 / 3;
        var shortTarget = totalValue * 1 / 3;

        if (shortValuation > shortTarget)
             await RebalanceShortToStake(data);
        
        else if (stakeValuation > stakingTarget)
            await RebalanceStakeToShort(data);
    }

    private async Task RebalanceStakeToShort(AggregatedData data)
    {
        // TO DO
    }

    private async Task RebalanceShortToStake(AggregatedData data)
    {
        // TO DO
    }
}

