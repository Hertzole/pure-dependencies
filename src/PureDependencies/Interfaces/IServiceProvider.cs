namespace Hertzole.PureDependencies
{
	/// <summary>
	///     Interface for a service provider for a specific type.
	/// </summary>
	/// <typeparam name="T">The type of service to provide.</typeparam>
	public interface IServiceProvider<out T>
	{
		/// <summary>
		///     Gets the service of type <typeparamref name="T" />.
		/// </summary>
		/// <returns>The service of type <typeparamref name="T" />.</returns>
		T GetService();
	}
}