using System;
using System.Collections.Generic;
using System.Text;

namespace Hertzole.CodeBuilder;

public class MethodScope : ICodeScope, ICanHaveAttributes
{
	private readonly CodeStringBuilder codeBuilder;
	private readonly TypeScope typeScope;
	private readonly string methodName;
	private string returnType;

	private readonly StringBuilder contentBuilder;
	private readonly List<AttributeBuilder> attributes;
	private readonly List<(string type, string name)> parameters;

	private bool shouldIndentWrite;

	private MethodAccessor accessor;
	private TypeAttributes typeAttributes;

	public int Indent
	{
		get { return typeScope.Indent; }
		set { typeScope.Indent = value; }
	}

	internal int ExtraIndent { get; set; }

	internal MethodScope(CodeStringBuilder codeBuilder, TypeScope typeScope, string methodName, string returnType)
	{
		this.codeBuilder = codeBuilder;
		this.typeScope = typeScope;
		this.methodName = methodName;
		this.returnType = returnType;

		//TODO: Get from pool
		contentBuilder = new StringBuilder(256);
		attributes = new List<AttributeBuilder>();
		parameters = new List<(string type, string name)>();

		accessor = MethodAccessor.None;

		shouldIndentWrite = true;
	}

	internal const string METHOD_PREFIX = "%%METHOD%%";

	public AttributeBuilder AddAttribute(AttributeBuilder attribute)
	{
		attributes.Add(attribute);

		return attribute;
	}

	public void AddParameter(string parameterType, string parameterName)
	{
		parameters.Add((parameterType, parameterName));
	}

	public IndentScope WithIndent(int indentAmount = 1, bool addBrackets = false)
	{
		return new IndentScope(this, indentAmount, addBrackets);
	}

	public MethodScope WithAccessor(MethodAccessor accessor)
	{
		this.accessor = accessor;
		return this;
	}

	public MethodScope WithReturnType(string returnType)
	{
		this.returnType = returnType;
		return this;
	}

	public MethodScope Partial()
	{
		typeAttributes |= TypeAttributes.Partial;

		return this;
	}

	public MethodScope Sealed()
	{
		typeAttributes |= TypeAttributes.Sealed;

		return this;
	}

	public MethodScope Abstract()
	{
		typeAttributes |= TypeAttributes.Abstract;

		return this;
	}

	public MethodScope ReadOnly()
	{
		typeAttributes |= TypeAttributes.ReadOnly;

		return this;
	}

	public MethodScope Virtual()
	{
		typeAttributes |= TypeAttributes.Virtual;

		return this;
	}

	public MethodScope Override()
	{
		typeAttributes |= TypeAttributes.Override;

		return this;
	}

	public MethodScope Static()
	{
		typeAttributes |= TypeAttributes.Static;

		return this;
	}

	public MethodScope Ref()
	{
		typeAttributes |= TypeAttributes.Ref;

		return this;
	}

	public MethodScope Async()
	{
		typeAttributes |= TypeAttributes.Async;

		return this;
	}

	public void AppendLine(string value)
	{
		WriteIndentIfNeeded();

		contentBuilder.AppendLine(value);
		shouldIndentWrite = true;
	}

	public void AppendLine()
	{
		contentBuilder.AppendLine();
		shouldIndentWrite = true;
	}

	public void AppendType(string typeName)
	{
		WriteIndentIfNeeded();

		if (codeBuilder.Options.AppendGlobalPrefix)
		{
			contentBuilder.Append("global::");
		}

		contentBuilder.Append(typeName);
	}

	public void Append(ushort value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(uint value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(char value, int repeatCount)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value, repeatCount);
	}

	public void Append(char[] value, int startIndex, int charCount)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value, startIndex, charCount);
	}

	public void Append(string value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(string value, int startIndex, int count)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value, startIndex, count);
	}

	public void Append(float value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(ulong value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(sbyte value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(bool value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(byte value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(char value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(decimal value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(double value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(char[] value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(short value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(int value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(long value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	public void Append(object value)
	{
		WriteIndentIfNeeded();

		contentBuilder.Append(value);
	}

	private void WriteIndentIfNeeded()
	{
		if (shouldIndentWrite)
		{
			contentBuilder.AppendRepeating(METHOD_PREFIX, Indent + 2 + ExtraIndent);
			contentBuilder.AppendIndent(1);
			shouldIndentWrite = false;
		}
	}

	public void Dispose()
	{
		try
		{
			//TODO: Get from pool
			StringBuilder finalBuilder = new StringBuilder();

			BuildHeader(finalBuilder);

			if (!HasContent())
			{
				finalBuilder.Append(" { }");
			}
			else
			{
				BuildContent(finalBuilder);
			}

			typeScope.contentBuilder.AppendLine(finalBuilder.ToString());
			//TODO: Pool items
		}

		catch (Exception e)
		{
			typeScope.contentBuilder.AppendLine("// Error: " + e.Message);
		}
	}

	private void BuildHeader(StringBuilder builder)
	{
		// If the last written thing was a method, add a new line to create spacing between methods.
		if (IsLastWrittenAMethod(typeScope.contentBuilder, Indent + 3))
		{
			builder.AppendLine();
		}

		for (int i = 0; i < attributes.Count; i++)
		{
			builder.AppendRepeating(METHOD_PREFIX, 2);
			attributes[i].BuildAttribute(builder, codeBuilder.Options.AppendGlobalPrefix);
			builder.AppendLine();
		}

		builder.AppendRepeating(METHOD_PREFIX, 2);
		builder.AppendMethodAccessor(accessor);
		builder.AppendTypeAttributes(typeAttributes);
		builder.Append(returnType);
		builder.Append(' ');
		builder.Append(methodName);

		if (parameters.Count > 0)
		{
			builder.Append('(');

			for (int i = 0; i < parameters.Count; i++)
			{
				if (i > 0)
				{
					builder.Append(", ");
				}

				builder.Append(parameters[i].type);
				builder.Append(' ');
				builder.Append(parameters[i].name);
			}
			
			builder.Append(')');
		}
		else
		{
			builder.Append("()");
		}
	}

	private void BuildContent(StringBuilder builder)
	{
		builder.AppendLine();

		builder.AppendRepeating(METHOD_PREFIX, 2);
		builder.AppendLine('{');

		builder.Append(contentBuilder);
		builder.AppendLine();

		builder.AppendRepeating(METHOD_PREFIX, 2);
		builder.Append('}');
	}

	private bool HasContent()
	{
		return contentBuilder.Length > 0;
	}

	private static bool IsLastWrittenAMethod(StringBuilder builder, int indent)
	{
		if (builder.Length == 0)
		{
			return false;
		}

		for (int i = 0; i < indent; i++)
		{
			if (builder[builder.Length - i - 1] == '}')
			{
				return true;
			}
		}

		return false;
	}
}