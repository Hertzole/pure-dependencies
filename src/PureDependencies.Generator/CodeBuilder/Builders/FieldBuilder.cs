using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Hertzole.CodeBuilder;

public readonly record struct FieldBuilder : IDisposable, ICanHaveAttributes
{
	private readonly CodeStringBuilder codeBuilder;
	private readonly List<FieldBuilder> parentFields;
	private readonly int index;
	private readonly string fieldName;
	private readonly string fieldType;
	public readonly List<AttributeBuilder> attributes;

	private FieldAccessor Accessor { get; init; }
	private string? DefaultValue { get; init; }
	private TypeAttributes TypeAttributes { get; init; }

	internal FieldBuilder(CodeStringBuilder codeBuilder, List<FieldBuilder> parentFields, string fieldName, string fieldType)
	{
		this.codeBuilder = codeBuilder;
		this.parentFields = parentFields;
		this.fieldName = fieldName;
		this.fieldType = fieldType;

		//TODO: Get from pool
		attributes = new List<AttributeBuilder>();

		index = parentFields.Count;
		parentFields.Add(this);
	}

	public FieldBuilder WithAccessor(FieldAccessor accessor)
	{
		FieldBuilder newField = this with { Accessor = accessor };
		parentFields[index] = newField;
		return newField;
	}

	public FieldBuilder WithDefaultValue(string value)
	{
		value.ThrowIfNullOrWhitespace(nameof(value));
		FieldBuilder newField = this with { DefaultValue = value };
		parentFields[index] = newField;
		return newField;
	}

	public FieldBuilder ReadOnly()
	{
		if ((this.TypeAttributes & TypeAttributes.ReadOnly) != 0)
		{
			return this;
		}
		
		var newTypeAttributes = TypeAttributes | TypeAttributes.ReadOnly;
		var newField = this with { TypeAttributes = newTypeAttributes };
		parentFields[index] = newField;
		return newField;
	}
	
	public AttributeBuilder AddAttribute(AttributeBuilder attribute)
	{
		attributes.Add(attribute);

		parentFields[index] = this;

		return attribute;
	}

	internal void BuildField(StringBuilder builder, int indent)
	{
		BuildAttributes(builder, indent, codeBuilder.Options.AppendGlobalPrefix, attributes);

		builder.AppendIndent(indent);
		builder.AppendFieldAccessor(Accessor);
		builder.AppendTypeAttributes(TypeAttributes);
		builder.Append(fieldType);
		builder.Append(' ');
		builder.Append(fieldName);

		if (!string.IsNullOrWhiteSpace(DefaultValue))
		{
			builder.Append(" = ");
			builder.Append(DefaultValue);
		}

		builder.Append(';');
	}

	private static void BuildAttributes(StringBuilder builder, int indent, bool appendGlobalPrefix, List<AttributeBuilder> attributes)
	{
		if (attributes.Count == 0)
		{
			return;
		}

		for (int i = 0; i < attributes.Count; i++)
		{
			builder.AppendIndent(indent);
			attributes[i].BuildAttribute(builder, appendGlobalPrefix);
			builder.AppendLine();
		}
	}

	public void Dispose()
	{
		//TODO: Return to pool
	}
}