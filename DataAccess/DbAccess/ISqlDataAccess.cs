namespace Staking.DataAccess.DbAccess;

public interface ISqlDataAccess
{
	Task<List<T>> LoadData<T, U>(string sql, U parameteres);
	Task SaveData<T>(string sql, T parameteres);
	Task<List<T>> LoadDataSp<T, U>(string storedProcedure, U parameteres);
	Task SaveDataSp<T>(string storedProcedure, T parameteres);
}