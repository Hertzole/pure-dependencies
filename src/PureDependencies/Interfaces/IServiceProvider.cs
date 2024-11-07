namespace Hertzole.PureDependencies
{
	public interface IServiceProvider<out T>
	{
		T GetService();
	}
}