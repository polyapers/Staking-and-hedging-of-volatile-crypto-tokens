using System.Numerics;

namespace Staking.DataAccess.Models;

public class Network
{
    public required string Name { get; set; }
    public required BigInteger ChainId { get; set; }
    public required string Rpc { get; set; }
    public string? NativeToken { get; set; }
}
