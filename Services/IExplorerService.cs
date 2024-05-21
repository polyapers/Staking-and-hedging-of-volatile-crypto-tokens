using System.Numerics;

namespace Staking.Services;

public interface IeplorerService
{
    Task Claim();
    Task Stake(decimal amount);
    Task UnStake(decimal amount);
    Task UnstakeAll();
    Task<decimal> GetStakingAmount();
    Task<decimal> GetPendingRewards();
    Task<string> TransferToken(string address, decimal amount);
    Task<string> UnstakeTokenAndSendToCex(string address, decimal amount);
    string GetStakingAddress();
    Task<decimal> StakeAllAvailableToken();
    Task ReStakeAllPendingRewards();
    Task ReStakeAllPendingRewardsIfPossible(decimal price);
    Task<decimal> GetBlockTimeSeconds();
    Task<long> GetLatestBlockNumber();
    Task<BigInteger> GetLatestBlockNumberHex();
    Task<decimal> CalculateApr();
    Task ApproveStaking();
}