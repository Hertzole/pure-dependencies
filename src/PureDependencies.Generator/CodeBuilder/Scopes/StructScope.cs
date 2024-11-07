namespace Hertzole.CodeBuilder;

public readonly record struct StructScope : ITypeScope<StructScope>
{
	private readonly TypeScope typeScope;

	internal StructScope(CodeStringBuilder codeBuilder, FileScope fileScope, string structName) : this()
	{
		// typeScope = new TypeScope(codeBuilder, fileScope, structName, DeclarationType.Struct);
	}

	private StructScope(TypeScope typeScope)
	{
		this.typeScope = typeScope;
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

	public StructScope WithNamespace(string typeNamespace)
	{
		TypeScope newTypeScope = typeScope.WithNamespace(typeNamespace);
		return new StructScope(newTypeScope);
	}

	public StructScope WithAccessor(TypeAccessor accessor)
	{
		TypeScope newTypeScope = typeScope.WithAccessor(accessor);
		return new StructScope(newTypeScope);
	}
	
	public StructScope WithInterface(string interfaceName)
	{
		throw new System.NotImplementedException();
	}

	public StructScope Partial()
	{
		TypeScope newTypeScope = typeScope.Partial();
		return new StructScope(newTypeScope);
	}

	public StructScope ReadOnly()
	{
		TypeScope newTypeScope = typeScope.ReadOnly();
		return new StructScope(newTypeScope);
	}

	public void Dispose()
	{
		typeScope.Dispose();
	}
}