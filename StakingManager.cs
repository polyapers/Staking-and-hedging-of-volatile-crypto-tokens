using System.Numerics;
using Nethereum.Web3;
using Staking.DataAccess.Models;
using Staking.DataAccess.Models.BlockchainQueries;
using Staking.Functions;

namespace Staking;

public class StakingManager
{
    private readonly ServiceData contract;

    private readonly Network network;

    public StakingManager(Network eplorerNetwork)
    {
        network = eplorerNetwork;

        contract = new ServiceData();
    }

    #region Read Contract

    public async Task<BigInteger> GetLatestBlockRewards(string poolAddress, BigInteger blockNumber)
    {
        var queryHandler = Converter.GetWeb3Service(network.Rpc).Eth.GetContractQueryHandler<GetBlockReward>();

        var call = new GetBlockReward(poolAddress, blockNumber);

        var answer = await queryHandler.QueryDeserializingToObjectAsync<GetBlockRewardAnswer>(call, contract.ContractAddress);

        return answer.Reward;
    }

    #endregion Read Contract
}
