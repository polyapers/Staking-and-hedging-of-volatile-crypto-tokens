using Staking.Services;

namespace Staking.DataProviders;

public class ShortPriceDataProvider : DataProviderBase
{
    private readonly AggregatedData data;

    private readonly IBinanceService binanceService;

    private const long FundingRateDurationHours = 8;

    public ShortPriceDataProvider(AggregatedData data, ServiceContainer serviceContainer)
    {
        this.data = data;
        binanceService = serviceContainer.BinanceService;
    }

    protected sealed override async Task GatherData()
    {
        await GetShortFees("ETHUSDT");
    }

    private async Task GetShortFees(string ticker)
    {
        var fundingRate = await binanceService.GetShortInteresRates(ticker);

        var feesPerHour = fundingRate / FundingRateDurationHours;

        data.ShortFeesYearly = feesPerHour * 24 * 365;
    }
}

