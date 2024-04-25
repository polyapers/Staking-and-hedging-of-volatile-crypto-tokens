using CryptoExchangeTools.Models.Binance.Futures.USDM;

namespace Staking.Services;

public interface IBinanceService
{
    Task AddToPosition(string symbol, decimal amount);
    Task<PositionInformation> GetShortPosition(string symbol);
    Task<string> GetDepositAddress(string asset, string network);
    Task WaitForReceive(string hash);
    Task<decimal> SwapTokenToUsd(decimal TokenAmount);
    Task TransferUsdtFromSpotToFutures(decimal amount);
    Task ReceiveTokenAdAddToPosition(string hash, decimal receiveAmount);
    Task<decimal> RemoveFromPosition(string symbol, decimal amount);
    Task TransferUsdtFromFuturesToSpot(decimal amount);
    Task<decimal> SwapUsdToToken(decimal usdAmount);
    Task<decimal> RemoveUsdFromPositionAndSendToken(string address, decimal usdToRemove, decimal usdToSend);
    Task WithdrawTokenFromSpotAndStake(string address, decimal TokenAmount);
    Task<decimal> GetTokenUsdtPrice();
    Task<decimal> CloseEntireShortingPosition();
    Task ShortAllAvailableBalance();
    Task<decimal> GetShortInteresRates(string symbol);
    Task<FuturesAccaountBalance> GetFuturesAccountBalanceUsdt();
    Task<decimal> GetSpotTokenBalance();
    Task<decimal> GetSpotUsdtBalance();
    Task<IncomeHistoryRecord[]> GetIncomeHistory();
    Task<IncomeHistoryRecord[]> GetIncomeHistory(long startTimeTimeStamp);
    Task<decimal> TransferAllUsdBalanceFromFuturesToSpot();
    Task<decimal> ShortCappedAmount(decimal usdMaxAmount);
    Task<decimal> GetSupposedAvailableBalance();
    Task ShortAllSpotBalanceIfAny(decimal currentValue, decimal maxValuation, decimal currentTokenPrice);
    Task<(bool, string?)> WithdrawSuspended();
    Task<(bool, string?)> DepositSuspended();
    Task<decimal> GetMaxPriceChangesInTokenUsdt(KlineInterval interval, int limit);
}