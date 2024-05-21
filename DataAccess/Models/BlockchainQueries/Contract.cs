using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;

namespace Staking.DataAccess.Models.BlockchainQueries;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

[Function("balanceOf", "uint256[]")]
public class BalanceOf : FunctionMessage
{
    public BalanceOf()
    {
    }

    [Parameter("address", "address", 1)]
    public string User { get; set; }

    public BalanceOf(string user)
    {
        User = user;
    }
}

[FunctionOutput]
public class BalanceOfAnswer : IFunctionOutputDTO
{
    [Parameter("uint256", "", 1)]
    public virtual BigInteger Balance { get; set; }
}

