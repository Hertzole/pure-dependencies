using System;

namespace Hertzole.PureDependencies
{
	/// <summary>
	///     Marks a class as a service provider.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ServiceProviderAttribute : Attribute { }
}