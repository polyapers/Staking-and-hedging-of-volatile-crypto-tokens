using System.Diagnostics;

namespace Staking.DataProviders;

public abstract class DataProviderBase : IDataProvider
{
	private readonly string name;

	protected DataProviderBase()
	{
		name = GetType().Name;
	}

	public async Task Run()
	{
		var sw = new Stopwatch();
		
		sw.Start();

		await GatherData();
		
		sw.Stop();

		Console.WriteLine($"{name} Finished. Time: {sw.Elapsed.TotalMilliseconds} ms.");
	}

	protected abstract Task GatherData();
}