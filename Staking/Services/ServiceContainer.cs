using Staking.DataAccess.Data;

namespace Staking.Services;

public class ServiceContainer
{
	public IeplorerService eplorerService { get; }

	public IBalancer Balancer { get; }

	public IBinanceService BinanceService { get; }

	public ILoggerService Log { get; }
	
	public ILockStatusData LockStatusData { get; }
	
	public IRebalanceOrderData RebalanceOrderData { get; }

	public ServiceContainer(IeplorerService eplorerService, IBalancer balacer, IBinanceService binanceService, ILoggerService log, ILockStatusData lockStatusData, IRebalanceOrderData rebalanceOrderData)
	{
		eplorerService = eplorerService;
		Balancer = balacer;
		BinanceService = binanceService;
		Log = log;
		LockStatusData = lockStatusData;
		RebalanceOrderData = rebalanceOrderData;
	}
}

