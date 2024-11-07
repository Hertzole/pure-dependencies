using System;
using System.Collections.Generic;
using System.Text;

namespace Hertzole.CodeBuilder;

internal sealed class TypeScope : ITypeScope<TypeScope>
{
	private CodeStringBuilder codeBuilder;
	private FileScope fileScope;
	private string typeName;
	private DeclarationType declarationType;

	internal StringBuilder contentBuilder;

	private List<FieldBuilder> fields;
	private List<AttributeBuilder> attributes;
	internal List<string> methods;

	private List<string> interfaces;

	private string? typeNamespace;
	private TypeAccessor typeAccessor;
	private TypeAttributes typeAttributes;

	private static readonly ObjectPool<TypeScope> typeScopePool = new ObjectPool<TypeScope>(static () => new TypeScope(), null, null);

	internal int Indent
	{
		get { return fileScope.Indent; }
		set { fileScope.Indent = value; }
	}

	private bool HasInheritors
	{
		get { return interfaces.Count > 0; }
	}

	internal static TypeScope Create(CodeStringBuilder codeBuilder, FileScope fileScope, string typeName, DeclarationType declarationType)
	{
		TypeScope typeScope = typeScopePool.Get();

		typeScope.codeBuilder = codeBuilder;
		typeScope.fileScope = fileScope;
		typeScope.typeName = typeName;
		typeScope.declarationType = declarationType;

		typeScope.contentBuilder = StringBuilderPool.Get();

		typeScope.fields = ListPool<FieldBuilder>.Get();
		typeScope.attributes = ListPool<AttributeBuilder>.Get();
		typeScope.methods = ListPool<string>.Get();

		typeScope.interfaces = ListPool<string>.Get();

		return typeScope;
	}

	public FieldBuilder AddField(string fieldName, string fieldType)
	{
		FieldBuilder field = new FieldBuilder(codeBuilder, fields, fieldName, fieldType);
		if (codeBuilder.Options.AppendGeneratedCodeAttribute)
		{
			field.AddAttribute(codeBuilder.GeneratedCodeAttribute);
		}

		return field;
	}

	public MethodScope AddMethod(string methodName, string returnType)
	{
		MethodScope method = new MethodScope(codeBuilder, this, methodName, returnType);
		if (codeBuilder.Options.AppendGeneratedCodeAttribute)
		{
			method.AddAttribute(codeBuilder.GeneratedCodeAttribute);
		}

		return method;
	}

	public AttributeBuilder AddAttribute(AttributeBuilder attribute)
	{
		attributes.Add(attribute);

		return attribute;
	}

	public TypeScope WithNamespace(string? nameSpace)
	{
		this.typeNamespace = nameSpace;
		return this;
	}

	public TypeScope WithAccessor(TypeAccessor accessor)
	{
		typeAccessor = accessor;
		return this;
	}

	public TypeScope WithInterface(string interfaceName)
	{
		interfaces.Add(interfaceName);
		return this;
	}

	public TypeScope Partial()
	{
		// If it's already partial, just return this.
		if ((typeAttributes & TypeAttributes.Partial) != 0)
		{
			return this;
		}

		typeAttributes |= TypeAttributes.Partial;
		return this;
	}

	public TypeScope Sealed()
	{
		// If it's already sealed, just return this.
		if ((typeAttributes & TypeAttributes.Sealed) != 0)
		{
			return this;
		}

		// If it's abstract, we can't seal it.
		if ((typeAttributes & TypeAttributes.Abstract) != 0)
		{
			throw new ArgumentException("Cannot have a class that is both sealed and abstract.");
		}

		typeAttributes |= TypeAttributes.Sealed;
		return this;
	}

	public TypeScope Abstract()
	{
		// If it's already abstract, just return this.
		if ((typeAttributes & TypeAttributes.Abstract) != 0)
		{
			return this;
		}

		// If it's sealed, we can't make it abstract.
		if ((typeAttributes & TypeAttributes.Sealed) != 0)
		{
			throw new ArgumentException("Cannot have a class that is both sealed and abstract.");
		}

		typeAttributes |= TypeAttributes.Abstract;
		return this;
	}

	public TypeScope ReadOnly()
	{
		// If it's already readonly, just return this.
		if ((typeAttributes & TypeAttributes.ReadOnly) != 0)
		{
			return this;
		}

		typeAttributes |= TypeAttributes.ReadOnly;
		return this;
	}

	public void Dispose()
	{
		using PoolHandle<StringBuilder> finalBuilderScope = StringBuilderPool.Get(out StringBuilder finalBuilder);

		bool hasNamespace = !string.IsNullOrWhiteSpace(typeNamespace);

		if (hasNamespace)
		{
			finalBuilder.Append("namespace ");
			finalBuilder.AppendLine(typeNamespace);
			finalBuilder.AppendLine('{');
			fileScope.Indent++;
		}

		BuildHeader(finalBuilder);

		if (!HasContent())
		{
			finalBuilder.Append(" { }");
		}
		else
		{
			BuildContent(finalBuilder);
		}

		if (hasNamespace)
		{
			finalBuilder.AppendLine();
			fileScope.Indent--;
			finalBuilder.Append('}');
		}

		fileScope.AddContent(finalBuilder.ToString());

		fileScope.Indent--;

		ReturnDisposables();
		
		typeScopePool.Return(this);
	}

	private void ReturnDisposables()
	{
		StringBuilderPool.Return(contentBuilder);
		ListPool<FieldBuilder>.Return(fields);
		ListPool<AttributeBuilder>.Return(attributes);
		ListPool<string>.Return(methods);
		ListPool<string>.Return(interfaces);
	}

	private void BuildHeader(StringBuilder builder)
	{
		for (int i = 0; i < attributes.Count; i++)
		{
			builder.AppendIndent(fileScope.Indent);
			attributes[i].BuildAttribute(builder, codeBuilder.Options.AppendGlobalPrefix);
			builder.AppendLine();
		}

		builder.AppendIndent(fileScope.Indent);
		builder.AppendTypeAccessor(typeAccessor);
		builder.AppendTypeAttributes(typeAttributes);
		builder.AppendTypeDeclaration(declarationType);

		builder.Append(typeName);

		BuildInheritors(builder);
	}

	private void BuildInheritors(StringBuilder builder)
	{
		if (!HasInheritors)
		{
			return;
		}

		builder.Append(" : ");
		int inheritorIndex = 0;

		for (int i = 0; i < interfaces.Count; i++)
		{
			if (inheritorIndex > 0)
			{
				builder.AppendLine();
				builder.AppendIndent(fileScope.Indent + 1);
			}

			builder.Append(interfaces[i]);

			if (i < interfaces.Count - 1)
			{
				builder.Append(", ");
			}

			inheritorIndex++;
		}
	}

	private void BuildContent(StringBuilder builder)
	{
		builder.AppendLine();
		builder.AppendIndent(Indent);
		builder.AppendLine("{");

		Indent++;
		builder.AppendFields(fields, Indent);
		Indent--;

		contentBuilder.Replace(MethodScope.METHOD_PREFIX, new string('\t', Indent));
		
		builder.Append(contentBuilder);

		builder.AppendIndent(Indent);
		builder.Append('}');
	}

	private bool HasContent()
	{
		if (contentBuilder.Length > 0)
		{
			return true;
		}

		if (fields.Count > 0)
		{
			return true;
		}

		return false;
	}
}