namespace Hertzole.CodeBuilder;

public interface ICanHaveMethods
{
	MethodScope AddMethod(string methodName, string returnType = "void");
}