using System;

namespace Hertzole.PureDependencies
{
	/// <summary>
	///     Adds a singleton to the service provider.
	///     <remarks>
	///         This attribute must be used on a class that is marked with the <see cref="ServiceProviderAttribute" />.
	///     </remarks>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class SingletonAttribute : Attribute
	{
		/// <summary>
		///     Optional factory method to create the singleton.
		/// </summary>
		public string Factory { get; set; }

		/// <summary>
		///     Creates a new instance of the <see cref="SingletonAttribute" />.
		/// </summary>
		/// <param name="type">The type of the singleton.</param>
		public SingletonAttribute(Type type) { }
	}
}