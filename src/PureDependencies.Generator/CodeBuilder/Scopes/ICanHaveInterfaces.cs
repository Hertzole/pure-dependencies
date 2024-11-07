namespace Hertzole.CodeBuilder;

public interface ICanHaveInterfaces<out T>
{
	T WithInterface(string interfaceName);
}