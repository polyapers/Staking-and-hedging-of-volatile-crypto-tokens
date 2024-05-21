using CryptoExchangeTools.Models.Binance.Futures.USDM;

namespace Staking.Services;

public interface ILoggerService
{
    Task LogTg(string message);
    Task LogData(AggregatedData data);
    Task LogAction(string text);
    Task ReportError(Exception ex);
    Task LogReStake(decimal rewards, decimal price);
    Task LogError(Exception ex, string provider);
    Task LogPositionInfo(PositionInformation position);
    Task LogIncomeHistory(IncomeHistoryRecord[] records);
    Task LogFundingHistory(IncomeHistoryRecord[] records);
}