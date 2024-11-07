namespace Hertzole.CodeBuilder;

public sealed class ClassScope : ITypeScope<ClassScope>
{
	internal TypeScope typeScope = null!;

	private static readonly ObjectPool<ClassScope> classScopePool = new ObjectPool<ClassScope>(static () => new ClassScope(), null, null);

	internal static ClassScope Create(CodeStringBuilder codeBuilder, FileScope fileScope, string className)
	{
		ClassScope classScope = classScopePool.Get();

		classScope.typeScope = TypeScope.Create(codeBuilder, fileScope, className, DeclarationType.Class);

		return classScope;
	}

	public FieldBuilder AddField(string fieldName, string fieldType)
	{
		return typeScope.AddField(fieldName, fieldType);
	}

	public MethodScope AddMethod(string methodName, string returnType = "void")
	{
		return typeScope.AddMethod(methodName, returnType);
	}

	public AttributeBuilder AddAttribute(AttributeBuilder attribute)
	{
		return typeScope.AddAttribute(attribute);
	}

	public ClassScope WithNamespace(string? typeNamespace)
	{
		typeScope.WithNamespace(typeNamespace);
		return this;
	}

	public ClassScope WithAccessor(TypeAccessor accessor)
	{
		typeScope.WithAccessor(accessor);
		return this;
	}

	public ClassScope WithInterface(string interfaceName)
	{
		typeScope.WithInterface(interfaceName);
		return this;
	}

	public ClassScope Partial()
	{
		typeScope.Partial();
		return this;
	}

	public ClassScope Sealed()
	{
		typeScope.Sealed();
		return this;
	}

	public ClassScope Abstract()
	{
		typeScope.Abstract();
		return this;
	}

	public void Dispose()
	{
		typeScope.Dispose();

		classScopePool.Return(this);
	}
}