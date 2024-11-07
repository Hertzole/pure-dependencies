using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace PureDependencies.Benchmarks;

[MemoryDiagnoser]
public class StartupTime
{
	[Benchmark]
	public Singleton Microsoft()
	{
		ServiceCollection serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton<Singleton>();
		using ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
		return serviceProvider.GetRequiredService<Singleton>();
	}

	[Benchmark]
	public Singleton Jab()
	{
		using StartupContainer_Jab serviceProvider = new StartupContainer_Jab();
		return serviceProvider.GetService<Singleton>();
	}

	[Benchmark]
	public Singleton PureDependencies()
	{
		using StartupContainer_PureDependencies serviceProvider = new StartupContainer_PureDependencies();
		return serviceProvider.GetService<Singleton>();
	}
}