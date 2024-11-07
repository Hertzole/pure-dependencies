using Hertzole.PureDependencies;

namespace PureDependencies.Sample;

[ServiceProvider]
[Singleton(typeof(FirstService))]
[Singleton(typeof(SecondService))]
[Singleton(typeof(BootService))]
[Singleton(typeof(RuntimeService))]
public partial class ServiceProvider
{
	
}

// [ServiceProvider]
// [Singleton(typeof(FirstService))]
public partial class ServiceProviderSecond
{
	
}