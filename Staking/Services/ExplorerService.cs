using System.Numerics;
using Staking.Functions;
using Staking.Utility;

namespace Staking.Services;

public class eplorerService : IeplorerService
{
    private readonly TokenStaking staking;

    private readonly eplorerWallet wallet;

    private readonly ITokenExplorerApiService explorer;

    private readonly ILoggerService loggerService;

    public eplorerService(IConfiguration config, ITokenExplorerApiService explorer, ILoggerService loggerService)
    {
        var credentials = BuildCredentials(config);

        staking = new TokenStaking(credentials);
        wallet = new eplorerWallet(credentials);

        this.explorer = explorer;
        this.loggerService = loggerService;
    }

    private static Dictionary<string, string> BuildCredentials(IConfiguration config)
    {
        var credentials = new Dictionary<string, string>();

        var cypheredPkey = config.GetSection("eplorer:Pkey").Value
            ?? throw new Exception("Pkey not set for eplorer.");

        var eplorerAddress = config.GetSection("eplorer:Address").Value
            ?? throw new Exception("Address not set for eplorer.");
        
        var key = config.GetSection("eplorer:Key").Value
                  ?? throw new Exception("Address not set for eplorer.");

        credentials.Add("eplorerPkey", AesTools.DecryptString(key, cypheredPkey));
        credentials.Add("eplorerWalletAddress", eplorerAddress);

        return credentials;
    }

    public async Task Stake(decimal amount)
    {
        await staking.Stake(amount);
    }

    public async Task UnStake(decimal amount)
    {
        await staking.Unstake(amount);
    }

    public async Task Claim()
    {
        await staking.Claim();
    }

    public async Task UnstakeAll()
    {
        await staking.UnStakeAll();
    }

    public async Task<decimal> GetStakingAmount()
    {
        return await staking.GetStakingAmount();
    }

    public async Task<decimal> GetPendingRewards()
    {
        return await staking.GetPendingRewards();
    }

    public async Task<string> TransferToken(string address, decimal amount)
    {
        var transaction = await wallet.Transfer("Token", address, amount);

        return transaction.TransactionHash;
    }

    public async Task<string> UnstakeTokenAndSendToCex(string address, decimal amount)
    {
        await UnStake(amount);

        await Task.Delay(10_000);

        return await TransferToken(address, amount);
    }

    public string GetStakingAddress()
    {
        return wallet.WalletAddress;
    }

    public async Task<decimal> StakeAllAvailableToken()
    {
        var availableToken = await wallet.GetTokenBalance();
        
        if(availableToken > 0)
            await Stake(availableToken);

        return availableToken;
    }

    public async Task ReStakeAllPendingRewards()
    {
        await staking.ReStake();
    }

    public async Task ReStakeAllPendingRewardsIfPossible(decimal price)
    {
        try
        {
            var lastClaim = await TokenExplorerApiService.GetLatestClaimTransactionTime(wallet.WalletAddress);

            if (DateTime.Now - lastClaim <= TimeSpan.FromDays(1))
                return;

            var rewards = await GetPendingRewards();
        
            await staking.ReStake();

            await loggerService.LogReStake(rewards, price);
        }
        catch (Exception ex)
        {
            await loggerService.LogTg("Can't restake!");
            await loggerService.LogError(ex, "pplication");
        }
    }

    public async Task<decimal> GetBlockTimeSeconds()
    {
        return await explorer.GetLatestBlocksMiningTime();
    }

    public async Task<long> GetLatestBlockNumber()
    {
        return await explorer.GetLatestBlockNumber();
    }

    public async Task<BigInteger> GetLatestBlockNumberHex()
    {
        var number = await GetLatestBlockNumber();

        return new BigInteger(number);
    }

    public async Task<decimal> CalculateApr()
    {
        var latestBlock = await GetLatestBlockNumberHex();

        var blockRewardsRaw = await staking.GetLatestBlockRewards(latestBlock);

        var blockRewards = await Converter.DivideByDecimals(staking.Token, blockRewardsRaw);

        var blockTime = await GetBlockTimeSeconds();

        var rewardsPerSecond = blockRewards / blockTime;

        var returnsPerYear = rewardsPerSecond * 86400 * 365;

        var totalStakingRaw = await staking.GetStakingTotal();

        var totalStaking = await Converter.DivideByDecimals(staking.Token, totalStakingRaw);

        return returnsPerYear / totalStaking;
    }

    public async Task ApproveStaking()
    {
        await staking.ApproveStaking();
    }
}

