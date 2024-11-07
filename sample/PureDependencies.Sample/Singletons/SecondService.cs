using System;
using System.Threading.Tasks;

namespace PureDependencies.Sample;

public class SecondService : IAsyncDisposable
{
	public SecondService(IRuntimeService[] runtimeServices) { }

	public ValueTask DisposeAsync()
	{
		return default;
	}
}