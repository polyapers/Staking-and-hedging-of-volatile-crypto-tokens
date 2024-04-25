using System.Text;
using CryptoExchangeTools.Models.Binance.Futures.USDM;
using CryptoExchangeTools.Utility;
using Newtonsoft.Json;
using RestSharp;
using Staking.DataAccess.DbAccess;
// ReSharper disable RedundantAnonymousTypePropertyName

namespace Staking.Services;

public class LoggerService : ILoggerService
{
    private const string Token = "";

    private const string Url = $"https://api.telegram.org/bot{Token}";

    private readonly ISqlDataAccess db;

    private readonly IConfiguration config;

    public LoggerService(ISqlDataAccess db, IConfiguration config)
    {
        this.db = db;
        this.config = config;
    }

    public async Task LogTg(string message)
    {
        using var client = new RestClient(Url);
        
        var chatId = config.GetSection("Application:ReportChatId").Value 
                      ?? throw new Exception("Report Chat id not set.");

        var request = new RestRequest("sendMessage");
        request.AddParameter("chat_id", chatId);
        request.AddParameter("text", message);

        await client.ExecuteGetAsync(request);
    }

    public async Task ReportError(Exception ex)
    {
        using var client = new RestClient($"https://api.telegram.org/bot{Token}");

        var errorChatId = config.GetSection("Application:ErrorChatId").Value 
                          ?? throw new Exception("errorChatId is null.");

        var request = new RestRequest("sendMessage");
        request.AddParameter("chat_id", errorChatId);
        request.AddParameter("text", ex.Message);

        await client.ExecuteGetAsync(request);

        Console.WriteLine(ex.Message);
    }

    public async Task LogData(AggregatedData data)
    {
        const string sql = "INSERT INTO AggregatedData (TokenPrice, StakingPositionValue, ShortPositionValue, TokenStaked, TokenShorted, ShortFeesYearly, StakeApr, ShortUnPnl, TotalValue, ScenarioInProgress, Profitability, CurrentScenario, FuturesAccountBalance, SpotTokenBalance, SpotUsdtBalance) VALUES(@TokenPrice, @StakingPositionValue, @ShortPositionValue, @TokenStaked, @TokenShorted, @ShortFeesYearly, @StakeApr, @ShortUnPnl, @TotalValue, @ScenarioInProgress, @Profitability, @CurrentScenario, @FuturesAccountBalance, @SpotTokenBalance, @SpotUsdtBalance)";

        await db.SaveData<dynamic>(sql, new
        {
            TokenPrice = data.CurrentTokenPrice, 
            StakingPositionValue = data.CurrentlyStaked * data.CurrentTokenPrice,
            ShortPositionValue = data.ShortPositionValue,
            TokenStaked = data.CurrentlyStaked,
            TokenShorted = -data.ShortPositionInfo?.PositionAmt ?? -1,
            ShortFeesYearly = data.ShortFeesYearly,
            StakeApr = data.StakeApr,
            ShortUnPnl = data.ShortUnPnl,
            TotalValue = data.CalculateTotalValue(),
            ScenarioInProgress = !string.IsNullOrEmpty(data.CurrentScenario),
            Profitability = data.CalculateProfitability(),
            CurrentScenario = data.CurrentScenario,
            FuturesAccountBalance = data.FuturesAccountBalance,
            SpotTokenBalance = data.SpotTokenBalance,
            SpotUsdtBalance = data.SpotUsdtBalance
        });
    }

    public async Task LogAction(string text)
    {
        const string sql = "INSERT INTO ActionLog (`Text`) VALUES(@text)";

        await db.SaveData<dynamic>(sql, new { text });
        
        Console.WriteLine(text);
    }

    public async Task LogReStake(decimal rewards, decimal price)
    {
        const string sql = "INSERT INTO ReStakeLog (`Amount`, `Price`, `UsdValue`) VALUES(@amount, @price, @usdValue)";

        await db.SaveData<dynamic>(sql, new { amount = rewards, price = price, usdValue = rewards * price });
    }

    public async Task LogError(Exception ex, string provider)
    {
        const string sql = "INSERT INTO ErrorLog (`Message`, `StackTrace`, `FullError`) VALUES(@Message, @StackTrace, @FullError)";

        await db.SaveData<dynamic>(sql, new { ex.Message, ex.StackTrace, FullError = JsonConvert.SerializeObject(ex)});
    }

    public async Task LogPositionInfo(PositionInformation position)
    {
        const string sql = "INSERT INTO PositionInfo (EntryPrice, IsAutoAddMargin, IsolatedMargin, IsolatedWallet, Leverage, LiquidationPrice, MarginType, MarkPrice, MaxNotionalValue, Notional, PositionAmt, Symbol, UnRealizedProfit, UpdateTime) VALUES(@EntryPrice, @IsAutoAddMargin, @IsolatedMargin, @IsolatedWallet, @Leverage, @LiquidationPrice, @MarginType, @MarkPrice, @MaxNotionalValue, @Notional, @PositionAmt, @Symbol, @UnRealizedProfit, @UpdateTime)";

        await db.SaveData(sql, position);
    }

    public async Task LogIncomeHistory(IncomeHistoryRecord[] records)
    {
        var dbTime = (await db.LoadData<DateTime, dynamic>("SELECT NOW()", new { })).Single();

        var timeOffSet = dbTime - DateTime.Now;
        
        var sql = new StringBuilder("INSERT IGNORE INTO IncomeLog (Time, DbTime, Symbol, IncomeType, Income, Asset, Info, TranId, TradeId) VALUES ");

        for (int i = 0; i < records.Length; i++)
        {
            var r = records[i];

            var time = TimeUtils.UnixTimeStampToDateTime(r.Time, UnixTimeStampFormat.Milliseconds);
            
            sql.Append($"(\"{time.ToString("yyyy-MM-dd HH:mm:ss")}\",\"{(time + timeOffSet).ToString("yyyy-MM-dd HH:mm:ss")}\",\"{r.Symbol}\",\"{r.IncomeType}\",{r.Income},\"{r.Asset}\",\"{r.Info}\",\"{r.TranId}\",\"{r.TradeId?.ToString()}\")");

            if (i < records.Length - 1)
                sql.Append(',');
        }

        await db.SaveData<dynamic>(sql.ToString(), new { });
    }
    
    public async Task LogFundingHistory(IncomeHistoryRecord[] records)
    {
        if(!records.Any())
            return;
        
        var dbTime = (await db.LoadData<DateTime, dynamic>("SELECT NOW()", new { })).Single();

        var timeOffSet = dbTime - DateTime.Now;
        
        var sql = new StringBuilder("INSERT IGNORE INTO FundingLog (Time, DbTime, Symbol, IncomeType, Income, Asset, Info, TranId, TradeId) VALUES ");

        for (int i = 0; i < records.Length; i++)
        {
            var r = records[i];

            var time = TimeUtils.UnixTimeStampToDateTime(r.Time, UnixTimeStampFormat.Milliseconds);
            
            sql.Append($"(\"{time.ToString("yyyy-MM-dd HH:mm:ss")}\",\"{(time + timeOffSet).ToString("yyyy-MM-dd HH:mm:ss")}\",\"{r.Symbol}\",\"{r.IncomeType}\",{r.Income},\"{r.Asset}\",\"{r.Info}\",\"{r.TranId}\",\"{r.TradeId?.ToString()}\")");

            if (i < records.Length - 1)
                sql.Append(',');
        }

        await db.SaveData<dynamic>(sql.ToString(), new { });
    }
}

