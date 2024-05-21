using Nethereum.Web3.Accounts;
using Staking.Functions;

namespace Staking.DataAccess.Models;

public class TransactionParameters
{
    public required ContractData ContractData { get; init; }
    public required string FunctionName { get; init; }
    public string? Data { get; set; }
    public object[]? Parameters { get; set; }
    public decimal Value { get; init; }

    public async Task<TransactionRecord> Send(Account account)
    {
        return await Blockchain.Transaction(account, this);
    }
}