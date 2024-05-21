namespace Staking.Services;

public interface IBalancer
{
    Task Run(AggregatedData data);
}