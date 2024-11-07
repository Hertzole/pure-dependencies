namespace Hertzole.CodeBuilder;

public interface ITypeScope<out T> : ICodeScope, ICanHaveAttributes, ICanHaveFields, ICanHaveMethods, ICanHaveInterfaces<T>
{
	T WithNamespace(string typeNamespace);

	T WithAccessor(TypeAccessor accessor);

	T Partial();
}