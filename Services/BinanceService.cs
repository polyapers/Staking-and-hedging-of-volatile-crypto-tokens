using System.Globalization;
using System.Net;
using CryptoExchangeTools;
using CryptoExchangeTools.Models.Binance.Futures.USDM;
using Nethereum.Model;
using Staking.Utility;

namespace Staking.Services;

public class BinanceService : IBinanceService
{
    private readonly BinanceClient client;

    private readonly ILoggerService log;

    private readonly int baseLeverage;

    private const decimal MinRemovingAmountUsd = 2;

    public BinanceService(IConfiguration config, ILoggerService log)
    {
        this.log = log;

        var credentials = BuildCredentials(config);

        var proxy = BuildProxy(config);

        client = new BinanceClient(credentials["BinanceApiKey"], credentials["BinanceSecretKey"], proxy);

        baseLeverage = int.Parse(config.GetSection("BaseData:BaseLeverage").Value ?? throw new Exception("Base leverage is not set!"));

        client.OnMessage += (_, message) =>
        {
            log.LogAction($"[BINANCE] {message}");
        };
    }

    private void Log(string? text)
    {
#if DEBUG
        
        Console.WriteLine(text);
        
#endif
        
        if(string.IsNullOrEmpty(text))
            return;
            
        var tasks = new List<Task>
        {
            log.LogAction(text),
            log.LogTg(text),
        };
    }

    private static Dictionary<string, string> BuildCredentials(IConfiguration config)
    {
        var credentials = new Dictionary<string, string>();

        var cypheredApiKey = config.GetSection("Binance:ApiKey").Value
            ?? throw new Exception("ApiKey not set for Binance.");

        var cypheredSecretKey = config.GetSection("Binance:SecretKey").Value
            ?? throw new Exception("SecretKey not set for Binance.");
        
        var key = config.GetSection("Binance:Key").Value
                  ?? throw new Exception("Key not set for Binance.");

        credentials.Add("BinanceApiKey", AesTools.DecryptString(key, cypheredApiKey));
        credentials.Add("BinanceSecretKey", AesTools.DecryptString(key, cypheredSecretKey));

        return credentials;
    }

    private static WebProxy? BuildProxy(IConfiguration config)
    {
        var rawProxy = config.GetSection("Binance:Proxy").Value;

        if (string.IsNullOrEmpty(rawProxy))
            return null;

        return ProxyBuilder.Parse(rawProxy);
    }

    public async Task<PositionInformation> GetShortPosition(string symbol)
    {
        var positions = await client.Futures.USDM.Account.GetPositionInformationAsync(symbol);
        return positions.First();
    }

    private const decimal MaxOrderUsdt = 5000;

    /// <summary>
    /// Add spicified USD amount to position.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="amount">Amount in USD.</param>
    /// <returns></returns>
    public async Task AddToPosition(string symbol, decimal amount)
    {
        int limit = 100;
        
        while (amount > 1 && limit > 0)
        {
            var amountCapped = amount > MaxOrderUsdt ? MaxOrderUsdt : amount;

            await log.LogAction($"Adding {amountCapped} to position");
            
            await client.Futures.USDM.Account.ChangeLeverageAsync(symbol, baseLeverage);

            var currentPrice = await client.Futures.USDM.Market.GetMarkPriceAsync(symbol);

            var quantityInToken = amountCapped / currentPrice.MarkPrice;

            var flattenedQuantity = await FlattenFuturesOrderAmount(symbol, quantityInToken);

            var amountToAdd = flattenedQuantity * baseLeverage - 1;

            if(amountToAdd < 10)
            {
                await log.LogTg($"Token amount ({Math.Round(quantityInToken)}) is to low for futures order.");

                return;
            }

            try
            {
                await client.Futures.USDM.Account.NewOrderAsync(
                    symbol,
                    OrderSide.SELL,
                    OrderType.MARKET,
                    amountToAdd,
                    newOrderRespType: NewOrderRespType.RESULT);
            
                await log.LogTg($"Added {amountToAdd} Token to Short position.");
                await log.LogAction($"Added {amountToAdd} Token to Short position.");
            }
            catch (RequestNotSuccessfulException ex) when (ex.Response is not null && ex.Response.Contains("Margin is insufficient."))
            {
                await log.LogTg($"{ex.Response}");
                await log.LogAction($"{ex.Response}");
            }
            
            amount -= amountCapped;
            limit--;
        }
    }

    /// <summary>
    /// Remove spicified USD amount from position.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="amount">Amount in USD.</param>
    /// <returns></returns>
    public async Task<decimal> RemoveFromPosition(string symbol, decimal amount)
    {
        int limit = 100;
        var result = 0m;

        while (amount > 1 && limit > 0)
        {
            var amountCapped = amount > MaxOrderUsdt ? MaxOrderUsdt : amount;
            
            await log.LogAction($"Removing {amountCapped} to position");
            
            await client.Futures.USDM.Account.ChangeLeverageAsync(symbol, baseLeverage);

            var currentPrice = await client.Futures.USDM.Market.GetMarkPriceAsync(symbol);

            var quantityInToken = amountCapped / currentPrice.MarkPrice;

            var flattenedQuantity = await FlattenFuturesOrderAmount(symbol, quantityInToken);

            if(flattenedQuantity < 10)
            {
                await log.LogTg($"Token amount ({Math.Round(quantityInToken)}) is to low for futures order.");

                return result;
            }

            try
            {
                var order = await client.Futures.USDM.Account.NewOrderAsync(
                    symbol,
                    OrderSide.BUY,
                    OrderType.MARKET,
                    flattenedQuantity,
                    newOrderRespType: NewOrderRespType.RESULT);
                
                result += order.CumQuote / baseLeverage;
            
                await log.LogTg($"Removed {order.CumQuote / baseLeverage} USDT from Short position.");
                await log.LogAction($"Removed {order.CumQuote / baseLeverage} USDT from Short position.");
            }
            catch (RequestNotSuccessfulException ex) when (ex.Response is not null && ex.Response.Contains("Margin is insufficient."))
            {
                await log.LogTg($"{ex.Response}");
                await log.LogAction($"{ex.Response}");
            }
            
            amount -= amountCapped;
            limit--;
        }

        return result;
    }

    public async Task<string> GetDepositAddress(string asset, string network)
    {
        return (await client.Wallet.GetDepositAddressAsync(asset, network)).Address;
    }

    public async Task WaitForReceive(string hash)
    {
        await client.Wallet.WaitForReceiveAsync(hash);
    }

    private async Task<decimal> FlattenOrderAmount(string symbol, decimal amount)
    {
        var stepSize = await client.Market.GetTradeStepSizeAsync(symbol);

        amount -= 2 * stepSize;

        var decimals = Math.Log10((double)stepSize);

        var multiplyer = (decimal)Math.Pow(10, -decimals);

        return Math.Floor(amount * multiplyer) / multiplyer;
    }

    private async Task<decimal> FlattenFuturesOrderAmount(string symbol, decimal amount)
    {
        var stepSize = await client.Futures.USDM.Market.GetTradeStepSizeAsync(symbol);

        var decimals = Math.Log10((double)stepSize);

        var multiplyer = (decimal)Math.Pow(10, -decimals);

        return Math.Floor(amount * multiplyer) / multiplyer;
    }

    /// <summary>
    /// Swap exact Token amount on spot market with market order.
    /// </summary>
    /// <param name="TokenAmount">Amount to swap in Token.</param>
    /// <returns>Resulting USDT amount.</returns>
    public async Task<decimal> SwapTokenToUsd(decimal TokenAmount)
    {
        var flattenedAmount = await FlattenOrderAmount("TokenUSDT" ,TokenAmount);

        var order = await client.Trade.NewOrderAsync("TokenUSDT", CryptoExchangeTools.Models.Binance.OrderSide.SELL, CryptoExchangeTools.Models.Binance.OrderType.MARKET, quantity: flattenedAmount);

        await log.LogTg($"Swapped {TokenAmount} Token to {order.CummulativeQuoteQty} usdt.");

        return order.CummulativeQuoteQty;
    }

    /// <summary>
    /// Swap exact usdt amount on spot market with market order.
    /// </summary>
    /// <param name="usdAmount"></param>
    /// <returns>Resulting Token amount.</returns>
    public async Task<decimal> SwapUsdToToken(decimal usdAmount)
    {
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        
        var price = (await client.Futures.USDM.Market.GetMarkPriceAsync("TokenUSDT")).MarkPrice;

        var quantityInToken = usdAmount / price;

        var flattenedAmount = await FlattenOrderAmount("TokenUSDT", quantityInToken);

        for(int i = 0; i < 50; i ++)
        {
            if (flattenedAmount <= 0)
            {
                await log.LogAction("flattenedAmount is less than zero, canceling order.");
                await log.LogTg("flattenedAmount is less than zero, canceling order.");
                return 0;
            }
            
            try
            {
                var order = await client.Trade.NewOrderAsync("TokenUSDT", CryptoExchangeTools.Models.Binance.OrderSide.BUY, CryptoExchangeTools.Models.Binance.OrderType.MARKET, quantity: flattenedAmount);

                await log.LogTg($"Swapped {Math.Round(flattenedAmount * price, 2)} USDT to {Math.Round(order.ExecutedQty, 2)} Token.");

                return order.ExecutedQty;
            }
            catch (RequestNotSuccessfulException ex)
            {
                await log.LogAction($"[BINANCE] [ERROR]: {ex.Response}");
                
                Log(ex.Response);

                flattenedAmount *= 0.99m;

                flattenedAmount = await FlattenOrderAmount("TokenUSDT", flattenedAmount);
            }
        }

        throw new Exception("Can not place an order to swap Usdt to Token after 50 attempts!");
    }

    public async Task TransferUsdtFromSpotToFutures(decimal amount)
    {
        await client.FuturesTransfer.NewFutureAccountTransferAsync("USDT", amount, CryptoExchangeTools.Models.Binance.TransferType.SpotToUsdm);
    }

    public async Task TransferUsdtFromFuturesToSpot(decimal amount)
    {
        await client.FuturesTransfer.NewFutureAccountTransferAsync("USDT", amount, CryptoExchangeTools.Models.Binance.TransferType.UsdmToSpot);
    }

    public async Task ReceiveTokenAdAddToPosition(string hash, decimal receiveAmount)
    {
        await WaitForReceive(hash)
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "WaitForReceive");

        await Task.Delay(2_000);

        await SwapTokenToUsd(receiveAmount);

        var currentTokenPrice = await GetTokenUsdtPrice();

        // Emulation of order result. Why not using order result? Because on large amounts (or when market is crazy) it returns like 0.01% executed quantity 
        var resultUsdt = receiveAmount * currentTokenPrice;

        var spotUsdtBalance = await GetSpotUsdtBalance();

        resultUsdt = Math.Min(resultUsdt, spotUsdtBalance);
        
        await log.LogAction($"Received {receiveAmount} Token, Swapped to: {resultUsdt} USD.");
        
        if(resultUsdt <= 5)
            return;

        await TransferUsdtFromSpotToFutures(resultUsdt);

        await Task.Delay(1000);

        var futuresAccountBalance = (await GetFuturesAccountBalanceUsdt()).AvailableBalance;

        await log.LogAction($"Transfered to futues, futures balance is {futuresAccountBalance}");
        
        if(futuresAccountBalance <= 10)
            return;

        await AddToPosition("TokenUSDT", futuresAccountBalance);

        await log.LogAction("Added to position.");
    }
    
    public async Task<decimal> RemoveUsdFromPositionAndSendToken(string address, decimal usdToRemove, decimal usdToSend)
    {
        if (usdToSend <= 10)
            return 0;
        
        var price = await client.Futures.USDM.Market.GetMarkPriceAsync("TokenUSDT");

        var f = await GetFuturesAccountBalanceUsdt();

        var p = await GetShortPosition("TokenUSDT");

        var r = f.Balance + f.CrossUnPnl + p.Notional / 2; 

        var usdToSendFromFutures = usdToSend;

        if (MinRemovingAmountUsd > usdToRemove / price.MarkPrice)
            await log.LogAction($"Can't remove from short. {usdToRemove / price.MarkPrice} is less than minimum {MinRemovingAmountUsd}");
        
        else
        {
            var resultinUsd = await RemoveFromPosition("TokenUSDT", usdToRemove * baseLeverage)
                .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "RemoveFromPosition");

            usdToSendFromFutures = usdToSend - usdToRemove + resultinUsd;
        }
        
        var spotUsdtBalanceBefore = await GetSpotUsdtBalance();
        var futuresAccountBalanceObject = await GetFuturesAccountBalanceUsdt();
        var futuresAccountBalance = futuresAccountBalanceObject.AvailableBalance;

        usdToSendFromFutures = Math.Min(futuresAccountBalance - 10, usdToSendFromFutures);

        if (usdToSendFromFutures > 10)
        {
            var transferedUsd = await TransferUsdBalanceFromFuturesToSpot(usdToSendFromFutures);

            if (transferedUsd > 0)
            {
                decimal newSpotUsdtBalance;

                int limit = 100;

                do
                {
                    newSpotUsdtBalance = await GetSpotUsdtBalance();
                    limit--;
                    await Task.Delay(300);
                } while (newSpotUsdtBalance <= spotUsdtBalanceBefore && limit > 0);

                await SwapUsdToToken(Math.Min(usdToSend, newSpotUsdtBalance))
                    .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "SwapUsdToToken");
            }
        }
        
        var fees = await client.GetWithdrawalFeeAsync("Token", "RON")
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "GetWithdrawalFeeAsync");

        var TokenBalance = await GetSpotTokenBalance();
        
        price = await client.Futures.USDM.Market.GetMarkPriceAsync("TokenUSDT");

        var TokenAmount = usdToSend / price.MarkPrice;

        var toWithdraw = Math.Min(TokenAmount, TokenBalance) - fees - 1;

        toWithdraw = await FlattenWithdrawalAmount(toWithdraw);

        if (toWithdraw < 1)
            return 0;

        await client.WithdrawAsync("Token", toWithdraw, address, "RON")
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "WithdrawAsync");

        return toWithdraw - fees;
    }

    public async Task WithdrawTokenFromSpotAndStake(string address, decimal TokenAmount)
    {
        var fees = await client.GetWithdrawalFeeAsync("Token", "RON");
            
        var TokenBalance = await GetSpotTokenBalance();
        
        var toWithdraw = Math.Min(TokenAmount, TokenBalance) - fees - 1;

        toWithdraw = await FlattenWithdrawalAmount(toWithdraw);

        if (toWithdraw < 1)
            return;

        await client.WithdrawAsync("Token", toWithdraw, address, "RON");
    }

    private async Task<decimal> FlattenWithdrawalAmount(decimal amount)
    {
        var precision = await client.QueryWithdrawalPrecisionAsync("Token", "RON");
        
        var decimals = Math.Pow(10, precision);

        var multipliedFloor = Math.Floor((double)amount * decimals);

        return (decimal)(multipliedFloor / decimals);
    }

    public async Task<decimal> GetTokenUsdtPrice()
    {
        var price = await client.Futures.USDM.Market.GetMarkPriceAsync("TokenUSDT");

        return price.MarkPrice;
    }

    public async Task<decimal> CloseEntireShortingPosition()
    {
        var position = await GetShortPosition("TokenUSDT")
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "GetShortPosition");

        if (position.Notional < 1)
            return 0;

        var usdToRemove = -position.Notional;

        var price = await client.Futures.USDM.Market.GetMarkPriceAsync("TokenUSDT")
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "GetMarkPriceAsync");

        if (MinRemovingAmountUsd > usdToRemove / price.MarkPrice)
            throw new Exception($"Can't remove from short. {usdToRemove / price.MarkPrice} is less than minimum {MinRemovingAmountUsd}");

        return await RemoveFromPosition("TokenUSDT", usdToRemove)
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "RemoveFromPosition");
    }

    public async Task ShortAllAvailableBalance()
    {
        var usdtBalance = await client.Futures.USDM.Account.GetFuturesAcountAssetBalanceAsync("USDT")
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "GetFuturesAcountAssetBalanceAsync");

        await AddToPosition("TokenUSDT", usdtBalance.AvailableBalance)
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "AddToPosition");
    }

    public async Task<decimal> GetShortInteresRates(string symbol)
    {
        var data = await client.Futures.USDM.Market.GetMarkPriceAsync(symbol)
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "GetMarkPriceAsync");

        return -data.LastFundingRate;
    }

    public async Task<FuturesAccaountBalance> GetFuturesAccountBalanceUsdt()
    {
       return await client.Futures.USDM.Account.GetFuturesAcountAssetBalanceAsync("USDT")
           .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "GetFuturesAcountAssetBalanceAsync");
    }

    public async Task<decimal> GetSpotTokenBalance()
    {
        return await client.GetBalanceAsync("Token")
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "GetBalanceAsync");
    }
    
    public async Task<decimal> GetSpotUsdtBalance()
    {
        return await client.GetBalanceAsync("USDT")
            .DoAsyncWithRetries(5, TimeSpan.FromSeconds(5), "GetBalanceAsync");
    }

    public async Task<IncomeHistoryRecord[]> GetIncomeHistory()
    {
        return await client.Futures.USDM.Account.GetIncomeHistoryAsync("TokenUSDT");
    }
    
    public async Task<IncomeHistoryRecord[]> GetIncomeHistory(long startTimeTimeStamp)
    {
        return await client.Futures.USDM.Account.GetIncomeHistoryAsync("TokenUSDT", startTime: startTimeTimeStamp);
    }

    private async Task<decimal> TransferUsdBalanceFromFuturesToSpot(decimal maxAmount)
    {
        for (int i = 0; i < 25; i++)
        {
            try
            {
                var spotBalanceBefore = await GetSpotUsdtBalance();
                
                var futuresAccountBalanceObject = await GetFuturesAccountBalanceUsdt();
                var currentAccountBalance = futuresAccountBalanceObject.AvailableBalance;
                
                var amountToWithdraw = Math.Min(maxAmount, currentAccountBalance);
                
                if (currentAccountBalance > 10)
                {
                    await TransferUsdtFromFuturesToSpot(currentAccountBalance);
                    await ConfirmReceivingTransferFromFuturesToSpt(spotBalanceBefore);
                }
                Log($"Transfered {amountToWithdraw} USD Fom futures to spot.");

                return amountToWithdraw;
            }
            catch (RequestNotSuccessfulException ex)
            {
                Log(ex.Response);
            }
        }
        
        throw new Exception("Was not able to transfer balance from futures to spot.");
    }

    public async Task<decimal> TransferAllUsdBalanceFromFuturesToSpot()
    {
        for (int i = 0; i < 25; i++)
        {
            try
            {
                var spotBalanceBefore = await GetSpotUsdtBalance();
                
                var futuresAccountBalanceObject = await GetFuturesAccountBalanceUsdt();
                var currentAccountBalance = futuresAccountBalanceObject.AvailableBalance;

                if (currentAccountBalance > 10)
                {
                    await TransferUsdtFromFuturesToSpot(currentAccountBalance);
                    await ConfirmReceivingTransferFromFuturesToSpt(spotBalanceBefore);
                }
                
                Log($"Transfered {currentAccountBalance} USD Fom futures to spot.");

                return currentAccountBalance;
            }
            catch (RequestNotSuccessfulException ex)
            {
                Log(ex.Response);
            }
        }
        
        throw new Exception("Was not able to transfer balance from futures to spot.");
    }

    public async Task<decimal> ShortCappedAmount(decimal usdMaxAmount)
    {
        for (int i = 0; i < 25; i++)
        {
            try
            {
                var futuresAccountBalanceObject = await GetFuturesAccountBalanceUsdt();
                var currentAccountBalance = futuresAccountBalanceObject.AvailableBalance;

                var amountToAddToPosition = Math.Min(usdMaxAmount, currentAccountBalance);
            
                if(currentAccountBalance > 1)
                    await AddToPosition("TokenUSDT", amountToAddToPosition);

                Log($"Added {currentAccountBalance} USD to position.");

                return amountToAddToPosition;
            }
            catch (RequestNotSuccessfulException ex)
            {
                Log(ex.Response);
            }
        }
        
        throw new Exception("Was not able to add to position.");
    }

    public async Task ConfirmReceivingTransferFromFuturesToSpt(decimal spotUsdtBalanceBefore)
    {
        decimal newSpotUsdtBalance;
        int limit = 100;

        do
        {
            newSpotUsdtBalance = await GetSpotUsdtBalance();
            limit--;
            await Task.Delay(300);
        } while (newSpotUsdtBalance <= spotUsdtBalanceBefore && limit > 100);

        Log($"Received {newSpotUsdtBalance - spotUsdtBalanceBefore} USDT from futures to spot.");
    }

    public async Task<decimal> GetSupposedAvailableBalance()
    {
        var f = await GetFuturesAccountBalanceUsdt();

        var p = await GetShortPosition("TokenUSDT");

       return -(f.Balance + f.CrossUnPnl + p.Notional / 2); 
    }
    
    public async Task ShortAllSpotBalanceIfAny(decimal currentValue, decimal maxValuation, decimal currentTokenPrice)
    {
        var spotTokenBalance = await GetSpotTokenBalance();
        var spotUsdtBalance = await GetSpotUsdtBalance();

        var spotBalanceValuation = spotTokenBalance * currentTokenPrice + spotUsdtBalance;

        if (spotBalanceValuation < 5 *currentTokenPrice)
            return;
        
        if(currentValue >= maxValuation)
            return;

        var toShortUsd = Math.Min(maxValuation - currentValue, spotBalanceValuation);
		
        if(toShortUsd < 20)
            return;
        
        await log.LogAction($"Spot balance is significant. Shorting {Math.Round(toShortUsd, 2)} USD.");

        if (spotTokenBalance > 1)
            await SwapTokenToUsd(spotTokenBalance);
        
        spotUsdtBalance = await GetSpotUsdtBalance();

        toShortUsd = Math.Min(toShortUsd, spotUsdtBalance);

        await TransferUsdtFromSpotToFutures(toShortUsd);

        await ShortAllAvailableBalance();
    }

    public async Task<(bool, string?)> WithdrawSuspended()
    {
        var info = await client.Wallet.GetCoinInformationAsync("Token");

        var networkInfo = info.NetworkList.Single(x => x.NetworkName == "RON");
        
        return (!networkInfo.WithdrawEnable, networkInfo.WithdrawDesc);
    }
    
    public async Task<(bool, string?)> DepositSuspended()
    {
        var info = await client.Wallet.GetCoinInformationAsync("Token");

        var networkInfo = info.NetworkList.Single(x => x.NetworkName == "RON");
        
        return (!networkInfo.DepositEnable, networkInfo.DepositDesc);
    }

    public async Task<List<KlineData>> GetKlineData(string ticker, KlineInterval interval, int limit)
    {
        return await client.Futures.USDM.Market.GetKlineCandlestickDataAsync(ticker, interval, limit: limit);
    }

    public async Task<List<KlineData>> GetLastTokenUsdtKlineData(KlineInterval interval, int limit)
    {
        return await GetKlineData("TokenUSDT", interval, limit);
    }

    public async Task<List<decimal>> GetLastTokenUsdtKlineChanges(KlineInterval interval, int limit)
    {
        var data = await GetLastTokenUsdtKlineData(interval, limit + 2);

        var result = new List<decimal>();

        for (int i = 1; i < data.Count - 1; i++)
        {
            var change = (data[i].Close - data[i + 1].Close) / data[i + 1].Close;
            result.Add(change);
        }

        await log.LogAction(
            $"TokenUSDT Kline[{interval}] changes: {string.Join(',', result.Select(x => Math.Round(x, 4)))} ");

        return result;
    }

    public async Task<decimal> GetMaxPriceChangesInTokenUsdt(KlineInterval interval, int limit)
    {
        var changes = await GetLastTokenUsdtKlineChanges(interval, limit);

        return changes.Max(Math.Abs);
    }
}

