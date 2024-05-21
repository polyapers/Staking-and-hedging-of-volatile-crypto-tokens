using System.Numerics;
using Nethereum.Web3.Accounts;
using Staking.DataAccess.Models;
using Staking.DataAccess.Models.BlockchainQueries;
using Staking.Functions;

namespace Staking;

public class TokenStaking
{
    public StakingManager StakingManager { get; }

    public ServiceData TokenStakingPool { get; }

    public TokenData Token { get; }

    private readonly Network eplorerNetwork;

    private readonly Account account;

    public TokenStaking(IReadOnlyDictionary<string, string> credentials)
    {
        account = new Account(credentials["walletPkey"]);

        eplorerNetwork = new Network()
        {
            ChainId = 1,
            Name = "network",
            NativeToken = "eth",
            Rpc = ""
        };

        StakingManager = new StakingManager(eplorerNetwork);
    }

    #region Write

    public async Task Stake(decimal amount)
    {
        var transaction = await BuildTransaction(Write.stake, amount);

        await transaction.Send(account);
    }

    public async Task Unstake(decimal amount)
    {
        var transaction = await BuildTransaction(Write.unstake, amount);

        await transaction.Send(account);
    }

    public async Task Claim()
    {
        var transaction = await BuildTransaction(Write.claimPendingRewards);

        await transaction.Send(account);
    }

    public async Task ReStake()
    {
        var transaction = await BuildTransaction(Write.restakeRewards);

        await transaction.Send(account);
    }

    public async Task UnStakeAll()
    {
        var amount = await GetStakingAmount();

        var transaction = await BuildTransaction(Write.unstake, amount);

        await transaction.Send(account);
    }

    private async Task<TransactionParameters> BuildTransaction(Write method, decimal amount = 0)
    {
        var hexAmount = await Converter.MultiplyByDecimals(Token, amount);

        var transaction =  new TransactionParameters()
        {
            ContractData = TokenStakingPool,
            FunctionName = method.ToString(),
            Value = 0,
            Parameters = Array.Empty<object>()
        };

        if (method == Write.stake || method == Write.unstake)
        {
            transaction.Parameters = new object[]
            {
                hexAmount
            };
        }

        return transaction;
    }

    private enum Write
    {
        stake,
        unstake,
        claimPendingRewards,
        restakeRewards,
        unstakeAll,
        emergencyUnstake
    }

    #endregion Write

    #region Read

    public async Task<decimal> GetStakingAmount()
    {
        var queryHandler = Converter.GetWeb3Service(eplorerNetwork.Rpc).Eth.GetContractQueryHandler<GetStakingAmount>();

        var call = new GetStakingAmount(account.Address);

        var answer =  await queryHandler.QueryDeserializingToObjectAsync<GetStakingAmountAnswer>(call, TokenStakingPool.ContractAddress);

        return await Converter.DivideByDecimals(Token, answer.Amount);
    }

    public async Task<decimal> GetPendingRewards()
    {
        var queryHandler = Converter.GetWeb3Service(eplorerNetwork.Rpc).Eth.GetContractQueryHandler<GetPendingRewards>();

        var call = new GetPendingRewards(account.Address);

        var answer = await queryHandler.QueryDeserializingToObjectAsync<GetPendingRewardsAnswer>(call, TokenStakingPool.ContractAddress);

        return await Converter.DivideByDecimals(Token, answer.Amount);
    }

    public async Task<BigInteger> GetStakingTotal()
    {
        var queryHandler = Converter.GetWeb3Service(eplorerNetwork.Rpc).Eth.GetContractQueryHandler<GetStakingTotal>();

        var call = new GetStakingTotal();

        var answer = await queryHandler.QueryDeserializingToObjectAsync<GetStakingTotalAnswer>(call, TokenStakingPool.ContractAddress);

        return answer.Amount;
    }

    #endregion Read

    #region Derived Methods

    public async Task<BigInteger> GetLatestBlockRewards(BigInteger blockNumberHex)
    {
        return await StakingManager.GetLatestBlockRewards(TokenStakingPool.ContractAddress, blockNumberHex);
    }

    public async Task ApproveStaking()
    {
        await Blockchain.ApproveWithMyAddress(account, Token, TokenStakingPool);
    }

    #endregion Derived Methods
}
