using Staking.DataAccess.Models.BlockchainTypes.cs;

namespace Staking.Services;

public interface ITokenExplorerApiService
{
    Task<List<SkynetBlockInfo>> GetLatestBlocks(int size = 5);
    Task<decimal> GetLatestBlocksMiningTime(int size = 100);
    Task<long> GetLatestBlockNumber();
}