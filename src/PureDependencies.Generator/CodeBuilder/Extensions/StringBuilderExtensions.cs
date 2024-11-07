using System;
using System.Collections.Generic;
using System.Text;

namespace Hertzole.CodeBuilder;

internal static class StringBuilderExtensions
{
	public static void AppendTypeAccessor(this StringBuilder builder, TypeAccessor accessor)
	{
		switch (accessor)
		{
			case TypeAccessor.Public:
				builder.Append("public ");
				break;
			case TypeAccessor.Internal:
				builder.Append("internal ");
				break;
			case TypeAccessor.File:
				builder.Append("file ");
				break;
		}
	}

	public static void AppendTypeAttributes(this StringBuilder builder, TypeAttributes attributes)
	{
		if ((attributes & TypeAttributes.Sealed) != 0)
		{
			builder.Append("sealed ");
		}
		else if ((attributes & TypeAttributes.Abstract) != 0)
		{
			builder.Append("abstract ");
		}

		if ((attributes & TypeAttributes.ReadOnly) != 0)
		{
			builder.Append("readonly ");
		}

		if ((attributes & TypeAttributes.Static) != 0)
		{
			builder.Append("static ");
		}
		
		if ((attributes & TypeAttributes.Partial) != 0)
		{
			builder.Append("partial ");
		}
		
		if ((attributes & TypeAttributes.Const) != 0)
		{
			builder.Append("const ");
		}
		
		if ((attributes & TypeAttributes.Unsafe) != 0)
		{
			builder.Append("unsafe ");
		}

		if ((attributes & TypeAttributes.Override) != 0)
		{
			builder.Append("override ");
		}
		
		if ((attributes & TypeAttributes.Virtual) != 0)
		{
			builder.Append("virtual ");
		}
		
		if ((attributes & TypeAttributes.Extern) != 0)
		{
			builder.Append("extern ");
		}
		
		if ((attributes & TypeAttributes.Ref) != 0)
		{
			builder.Append("ref ");
		}

		if ((attributes & TypeAttributes.Async) != 0)
		{
			builder.Append("async ");
		}
	}

	public static void AppendTypeDeclaration(this StringBuilder builder, DeclarationType declarationType)
	{
		switch (declarationType)
		{
			case DeclarationType.Class:
				builder.Append("class ");
				break;
			case DeclarationType.Struct:
				builder.Append("struct ");
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(declarationType), declarationType, null);
		}
	}

	public static void AppendFieldAccessor(this StringBuilder builder, FieldAccessor accessor)
	{
		switch (accessor)
		{
			case FieldAccessor.Public:
				builder.Append("public ");
				break;
			case FieldAccessor.Private:
				builder.Append("private ");
				break;
			case FieldAccessor.Internal:
				builder.Append("internal ");
				break;
			case FieldAccessor.Protected:
				builder.Append("protected ");
				break;
			case FieldAccessor.File:
				builder.Append("file ");
				break;
		}
	}

	public static void AppendMethodAccessor(this StringBuilder builder, in MethodAccessor accessor)
	{
		switch (accessor)
		{
			case MethodAccessor.Public:
				builder.Append("public ");
				break;
			case MethodAccessor.Private:
				builder.Append("private ");
				break;
			case MethodAccessor.Protected:
				builder.Append("protected ");
				break;
			case MethodAccessor.Internal:
				builder.Append("internal ");
				break;
		}
	}

	public static void AppendIndent(this StringBuilder builder, int indent)
	{
		builder.Append('\t', indent);
	}
	
	public static StringBuilder AppendRepeating(this StringBuilder builder, string value, int count = 1)
	{
		for (int i = 0; i < count; i++)
		{
			builder.Append(value);
		}
		
		return builder;
	}

	public static void AppendFields(this StringBuilder builder, List<FieldBuilder> fields, int indent)
	{
		if (fields.Count == 0)
		{
			return;
		}

		for (int i = 0; i < fields.Count; i++)
		{
			fields[i].BuildField(builder, indent);
			builder.AppendLine();
		}
	}

	public static void AppendLine(this StringBuilder builder, char value)
	{
		builder.Append(value);
		builder.Append('\n');
	}
}