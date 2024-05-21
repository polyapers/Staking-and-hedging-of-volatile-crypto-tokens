using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Staking.DataAccess.Models.BlockchainQueries;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

[Function("getBlockReward", "uint256[]")]
public class GetBlockReward : FunctionMessage
{
	public GetBlockReward()
	{
	}

    public GetBlockReward(string pool, BigInteger blockNumber)
    {
        Pool = pool;
        BlockNumber = blockNumber;
    }

    [Parameter("address", "_pool", 1)]
    public string Pool { get; set; }

    [Parameter("uint256", "_blockNumber", 2)]
    public BigInteger BlockNumber { get; set; }
}

[FunctionOutput]
public class GetBlockRewardAnswer : IFunctionOutputDTO
{
    [Parameter("uint256", "", 1)]
    public virtual BigInteger Reward { get; set; }
}