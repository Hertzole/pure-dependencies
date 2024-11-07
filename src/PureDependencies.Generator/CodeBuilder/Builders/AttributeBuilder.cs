using System.Text;

namespace Hertzole.CodeBuilder;

public readonly record struct AttributeBuilder
{
	private readonly string attributeName;
	private readonly string? values;

	internal AttributeBuilder(string attributeName) : this(attributeName, string.Empty) { }

	public AttributeBuilder(string attributeName, string value)
	{
		this.attributeName = attributeName;
		values = value;
	}

	public AttributeBuilder(string attributeName, string value1, string value2) : this(attributeName, $"{value1}, {value2}") { }

	public AttributeBuilder(string attributeName, string value1, string value2, string value3) : this(attributeName, $"{value1}, {value2}, {value3}") { }

	public AttributeBuilder(string attributeName, params string[] values) : this(attributeName, string.Join(", ", values)) { }

	internal void BuildAttribute(StringBuilder builder, bool appendGlobalPrefix)
	{
		builder.Append("[");

		if (appendGlobalPrefix && !attributeName.StartsWith("global::"))
		{
			builder.Append("global::");
		}

		builder.Append(attributeName);

		if (!string.IsNullOrWhiteSpace(values))
		{
			builder.Append("(");
			builder.Append(values);
			builder.Append(")");
		}

		builder.Append("]");
	}
}