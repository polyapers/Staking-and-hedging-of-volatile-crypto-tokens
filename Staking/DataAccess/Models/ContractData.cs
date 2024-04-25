namespace Staking.DataAccess.Models;

public class ContractData
{
    public required string Name { get; set; }
    public required string ContractAddress { get; set; }
    public required string Abi { get; set; }
    public required Network Network { get; set; }
}

public class TokenData : ContractData
{
}

public class ServiceData : ContractData
{
}