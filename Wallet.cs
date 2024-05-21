using Nethereum.Web3.Accounts;
using Staking.DataAccess.Models;
using Staking.DataAccess.Models.BlockchainQueries;
using Staking.Functions;

namespace Staking;

public class Wallet
{
    private readonly Dictionary<string, TokenData> tokens = new();

    private readonly Account account;

    public string WalletAddress { get; }

    public Wallet(IReadOnlyDictionary<string, string> credentials)
	{
        account = new Account(credentials["Pkey"]);

        WalletAddress = credentials["WalletAddress"];

        var eplorerNetwork = new Network
        {
            ChainId = 1,
            Name = "network",
            NativeToken = "eth",
            Rpc = ""
        };
    }

    public async Task<TransactionRecord> Transfer(string token, string address, decimal amount)
    {
        var hexAmount = await Converter.MultiplyByDecimals(tokens[token], amount);

        var transaction = new TransactionParameters()
        {
            ContractData = tokens[token],
            FunctionName = "transfer",
            Value = 0,
            Parameters = new object[]
            {
                address,
                hexAmount
            }
        };

        return await transaction.Send(account);
    }

    public async Task<decimal> GetTokenBalance()
    {
        var queryHandler = Converter.GetWeb3Service(tokens["Token"].Network.Rpc).Eth.GetContractQueryHandler<BalanceOf>();

        var call = new BalanceOf(account.Address);

        var answer = await queryHandler.QueryDeserializingToObjectAsync<BalanceOfAnswer>(call, tokens["Token"].ContractAddress);

        return await Converter.DivideByDecimals(tokens["Token"], answer.Balance);
    }
}

