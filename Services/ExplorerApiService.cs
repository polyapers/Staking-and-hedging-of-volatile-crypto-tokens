using Newtonsoft.Json;
using RestSharp;
using Staking.DataAccess.Models.BlockchainTypes.cs;
using Staking.DTOs.eplorerExplorer;
using Staking.Utility;

namespace Staking.Services;

public class TokenExplorerApiService : ITokenExplorerApiService
{
    public TokenExplorerApiService()
    {
    }

    /// <summary>
    /// Gets latest blocks from explorer.
    /// </summary>
    /// <returns></returns>
    public async Task<List<SkynetBlockInfo>> GetLatestBlocks(int size = 5)
    {
        var client = new RestClient("");

        var result = new List<SkynetBlockInfo>();

        int attemptLimit = 10;
        int from = 0;

        while (result.Count < size && attemptLimit > 0)
        {
            var request = new RestRequest("blocks");

            // request.AddParameter("size", size);
            // request.AddParameter("from", from);
            
            request.AddParameter("limit", size);
            request.AddParameter("offset", from);

            var response = await client.GetAsync(request);

            if (string.IsNullOrEmpty(response.Content))
                throw new Exception("Content was null");

            var data = JsonConvert.DeserializeObject<GetBlocksAnswerSkynet>(response.Content)
                       ?? throw new Exception("Data was null after deserialization.");
            
            result.AddRange(data.Result.Items.ToList());

            int dataSize = data.Result.Items.Length;

            from += dataSize;
            
            attemptLimit--;
        }
        
        return result;
    }

    public async Task<decimal> GetLatestBlocksMiningTime(int size = 100)
    {
        var blocks = await GetLatestBlocks(size);

        decimal diff = 0;

        for (int i = size - 2; i >= 0; i--)
        {
            diff += blocks[i].Timestamp - blocks[i + 1].Timestamp;
        }

        return diff / (size - 1);
    }

    public async Task<long> GetLatestBlockNumber()
    {
        var blocks = await GetLatestBlocks(1);

        return blocks.First().Number;
    }

    public static async Task<List<eplorerTransaction>> GetTransactions(string address)
    {
        var client = new RestClient("");

        var result = new List<eplorerTransaction>();

        int offset = 0;
        int total = 0;

        do
        {
            var request = new RestRequest($"explorer/txs/{address}");
            request.AddHeader("X-API-KEY", "");

            if (offset > 0)
            {
                request.AddHeader("from", offset);
                await Task.Delay(1000);
            }

            var response = await client.GetAsync(request);
            var transactions = response.Deserialize<GetTransactionsResposne>();
            
            result.AddRange(transactions.Results);

            total = transactions.Total;
            offset += 100;

        } while (result.Count < total);


        return result;
    }

    private static async Task<List<SkyneteplorerTransaction>> GetTransactionsSkynetOpenApi(string address)
    {
        var client = new RestClient("");

        var result = new List<SkyneteplorerTransaction>();

        int offset = 0;
        int total = 0;
        const int limit = 25; 

        do
        {
            var request = new RestRequest("txs/search", Method.Post);
            // request.AddHeader("X-API-KEY", "");

            var body = new TxsSearchRequestBody(address, offset, limit);
            request.AddJsonBody(body);

            // if (offset > 0)
            // {
            //     request.AddHeader("from", offset);
            //     await Task.Delay(1000);
            // }

            var response = await client.PostAsync(request);
            var transactions = response.Deserialize<SkynetSearchTransactionsResponse>();
            
            result.AddRange(transactions.Result.Items);
            
            if(ClaimTransactionIsPresent(result))
                break;
            
            total = transactions.Result.Paging.Total;

            offset += limit;
        } while (result.Count < total);


        return result;
    }

    private static bool ClaimTransactionIsPresent(IEnumerable<SkyneteplorerTransaction> transactions)
    {
        return transactions.Any(
            x => string.Equals(x.To, "", StringComparison.InvariantCultureIgnoreCase) 
                 && string.Equals(x.Input, "", StringComparison.InvariantCultureIgnoreCase)
                 && x.Status == 1);
    }

    public static async Task<DateTime> GetLatestClaimTransactionTime(string address)
    {
        var transactions = await GetTransactionsSkynetOpenApi(address);
        // var transactions = await GetTransactions(address);

        var claimTransactions = transactions.Where(x =>
            string.Equals(x.To, "", StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(x.Input, "", StringComparison.InvariantCultureIgnoreCase)
            && x.Status == 1).ToList();

        var latest = claimTransactions.MaxBy(x => x.Timestamp);
        
        if(latest is null)
            return DateTime.Now.AddYears(-1);

        return DateTimeUtils.UnixTimeStampToLocalDateTime(latest.Timestamp);
    }
}

