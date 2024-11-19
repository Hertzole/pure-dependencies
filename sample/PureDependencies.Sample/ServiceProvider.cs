using Hertzole.PureDependencies;

namespace PureDependencies.Sample;

[ServiceProvider]
[Singleton(typeof(FirstService))]
[Singleton(typeof(SecondService))]
[Singleton(typeof(BootService))]
[Singleton(typeof(RuntimeService))]
[Singleton(typeof(ILogger), Factory = nameof(CreateLogger))]
public partial class ServiceProvider
{
	private static ILogger CreateLogger()
	{
		return new Logger();
	}
}

// [ServiceProvider]
// [Singleton(typeof(FirstService))]
public class ServiceProviderSecond { }