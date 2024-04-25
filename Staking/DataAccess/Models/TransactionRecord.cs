using System.Numerics;

namespace Staking.DataAccess.Models;

public class TransactionRecord
{
    public BigInteger TransactionStatus { get; set; }
    public string TransactionHash { get; set; }
    public long ChainId { get; set; }
    public decimal SpentGas { get; set; }

    public TransactionRecord(string transactionHash, long chainId, BigInteger transactionStatus, decimal spentGas)
    {
        TransactionHash = transactionHash;
        TransactionStatus = transactionStatus;
        ChainId = chainId;
        SpentGas = spentGas;
    }
}