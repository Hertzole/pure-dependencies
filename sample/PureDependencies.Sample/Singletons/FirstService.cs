using System;
using System.Collections.Generic;

namespace PureDependencies.Sample;

public class FirstService : IDisposable, IBootService
{
	public FirstService(SecondService[] second, IReadOnlyList<IBootService> bootServices) { }

	public void Dispose()
	{
		
	}
}