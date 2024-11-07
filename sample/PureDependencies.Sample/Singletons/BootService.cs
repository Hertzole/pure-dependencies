using System;
using System.Threading.Tasks;

namespace PureDependencies.Sample;

public class BootService : IBootService, IAsyncDisposable
{
	public void Dispose()
	{
		// TODO release managed resources here
	}

	public async ValueTask DisposeAsync()
	{
		// TODO release managed resources here
	}
}