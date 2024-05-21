using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Staking.DataAccess.Models.BlockchainQueries;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#region Get Staking Amount

[Function("getStakingAmount", "uint256[]")]
public class GetStakingAmount : FunctionMessage
{
    public GetStakingAmount()
    {
    }

    public GetStakingAmount(string user)
    {
        User = user;
    }

    [Parameter("address", "_user", 1)]
    public string User { get; set; }
}

[FunctionOutput]
public class GetStakingAmountAnswer : IFunctionOutputDTO
{
    [Parameter("uint256", "", 1)]
    public virtual BigInteger Amount { get; set; }
}

#endregion Get Staking Amount

#region Get Pending Rewards

[Function("getPendingRewards", "uint256[]")]
public class GetPendingRewards : FunctionMessage
{
    public GetPendingRewards()
    {
    }

    public GetPendingRewards(string user)
    {
        User = user;
    }

    [Parameter("address", "_user", 1)]
    public string User { get; set; }
}

[FunctionOutput]
public class GetPendingRewardsAnswer : IFunctionOutputDTO
{
    [Parameter("uint256", "", 1)]
    public virtual BigInteger Amount { get; set; }
}

#endregion Get Pending Rewards

#region Get Staking Total

[Function("getStakingTotal", "uint256[]")]
public class GetStakingTotal : FunctionMessage
{
}

[FunctionOutput]
public class GetStakingTotalAnswer : IFunctionOutputDTO
{
    [Parameter("uint256", "", 1)]
    public virtual BigInteger Amount { get; set; }
}

#endregion Get Staking Total
