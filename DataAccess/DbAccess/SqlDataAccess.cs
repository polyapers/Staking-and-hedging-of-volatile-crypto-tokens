using System.Data;
using Dapper;
using MySql.Data.MySqlClient;

namespace Staking.DataAccess.DbAccess;

public class SqlDataAccess : ISqlDataAccess
{
    private readonly IConfiguration config;
    
    private const string ConnectonId = "DefaultConnection";

    public SqlDataAccess(IConfiguration config)
    {
        this.config = config;
    }

    public async Task<List<T>> LoadData<T, U>(string sql, U parameteres)
    {
        using IDbConnection connection = new MySqlConnection(config.GetConnectionString(ConnectonId));
        var result =  await connection.QueryAsync<T>(sql, parameteres, commandType: CommandType.Text);
        return result.ToList();
    }

    public async Task SaveData<T>(string sql, T parameteres)
    {
        using IDbConnection connection = new MySqlConnection(config.GetConnectionString(ConnectonId));
        await connection.ExecuteAsync(sql, parameteres, commandType: CommandType.Text);
    }

    public async Task<List<T>> LoadDataSp<T, U>(string storedProcedure, U parameteres)
    {
        using IDbConnection connection = new MySqlConnection(config.GetConnectionString(ConnectonId));
        var result = await connection.QueryAsync<T>(storedProcedure, parameteres, commandType: CommandType.StoredProcedure);
        return result.ToList();
    }

    public async Task SaveDataSp<T>(string storedProcedure, T parameteres)
    {
        using IDbConnection connection = new MySqlConnection(config.GetConnectionString(ConnectonId));
        await connection.ExecuteAsync(storedProcedure, parameteres, commandType: CommandType.StoredProcedure);
    }
}