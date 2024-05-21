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

    private const int ForcedTokenDecimals = 6;
    
    private const int Leverage = 2;

    public Balancer(IConfiguration config, IeplorerService eplorerService, IBinanceService binanceService, ILoggerService log)
    {
        this.config = config;
        this.eplorerService = eplorerService;
        this.binanceService = binanceService;
        this.log = log;
    }

    private void Log(string text)
    {
        #if DEBUG
        
        Console.WriteLine(text);
        
        #endif
            
        var tasks = new List<Task>
        {
            log.LogAction(text),
            log.LogTg(text),
        };
    }

    public async Task Run(AggregatedData data)
    {
        var stakeValuation = data.CurrentTokenPrice * data.CurrentlyStaked;
        var shortValuation = data.ShortPositionValue;

        var totalValue = data.CalculateTotalValue();
        
        var stakingTarget = totalValue * 2 / 3;
        var shortTarget = totalValue * 1 / 3;
        
        var text =
            $"Running Balancer\nShorting position: {Math.Round(shortValuation, 2)} usd.\nStaking position: {Math.Round(stakeValuation, 2)} usd.";

        await log.LogTg(text);
        await log.LogAction(text);

        if (shortValuation > shortTarget)
             await RebalanceShortToStake(data);
        
        else if (stakeValuation > stakingTarget)
            await RebalanceStakeToShort(data);
    }

    private async Task RebalanceStakeToShort(AggregatedData data)
    {
        var totalValue = data.CalculateTotalValue();
        
        var stakingTarget = totalValue * 2 / 3;
        
        var usdToRemove = data.CurrentlyStaked * data.CurrentTokenPrice - stakingTarget;
        
        var TokenToRemove = usdToRemove / data.CurrentTokenPrice;

        var text = $"Reabalncing Stake To Short, amount: {Math.Round(usdToRemove, 2)} usdt.";

        await log.LogTg(text);
        await log.LogAction(text);

        var flattenedToken = FlattenToken(TokenToRemove);

        var depositAddress = await binanceService.GetDepositAddress("Token", "RON");

        var hash = await eplorerService.UnstakeTokenAndSendToCex(depositAddress, flattenedToken);

        await binanceService.ReceiveTokenAdAddToPosition(hash, flattenedToken);

        text = "Removed From staking and added to balance.";
        
        await log.LogTg(text);
        await log.LogAction(text);
    }

    private static decimal FlattenToken(decimal amount)
    {
        decimal multiplyer = (decimal)Math.Pow(10, ForcedTokenDecimals);

        return Math.Floor(amount * multiplyer) / multiplyer;
    }

    private async Task RebalanceShortToStake(AggregatedData data)
    {
        var totalValue = data.CalculateTotalValue();
        
        var shortTarget = totalValue * 1 / 3;
        
        var usdToTransferFromFutures = data.ShortPositionValue - shortTarget;

        var text = $"Reabalncing Short To Stake, amount: {Math.Round(usdToTransferFromFutures, 2)} usdt.";

        await log.LogTg(text);
        await log.LogAction(text);

        var stakingAddress = eplorerService.GetStakingAddress();
        
        var futuresAccountBalanceObject = await binanceService.GetFuturesAccountBalanceUsdt();
        var currentAccountBalance = futuresAccountBalanceObject.AvailableBalance;
        
        var usdToRemove = usdToTransferFromFutures - currentAccountBalance;

        await binanceService.RemoveUsdFromPositionAndSendToken(stakingAddress, usdToRemove, usdToTransferFromFutures);

        await eplorerService.StakeAllAvailableToken();

        text = "Removed From short and added to staking.";

        await log.LogTg(text);
        await log.LogAction(text);
    }
}

