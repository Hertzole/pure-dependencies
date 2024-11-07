namespace Hertzole.CodeBuilder;

public static class ScopeExtensions
{
	public static AttributeBuilder AddAttribute<T>(this T scope, string attributeName) where T : ICanHaveAttributes
	{
		attributeName.ThrowIfNullOrWhitespace(nameof(attributeName));
		
		return scope.AddAttribute(new AttributeBuilder(attributeName));
	}

	public static AttributeBuilder AddAttribute<T>(this T scope, string attributeName, string value) where T : ICanHaveAttributes
	{
		attributeName.ThrowIfNullOrWhitespace(nameof(attributeName));
		value.ThrowIfNullOrWhitespace(nameof(value));
		
		return scope.AddAttribute(new AttributeBuilder(attributeName, value));
	}

	public static AttributeBuilder AddAttribute<T>(this T scope, string attributeName, string value1, string value2) where T : ICanHaveAttributes
	{
		 attributeName.ThrowIfNullOrWhitespace(nameof(attributeName));
		  value1.ThrowIfNullOrWhitespace(nameof(value1));
		   value2.ThrowIfNullOrWhitespace(nameof(value2));
		
		return scope.AddAttribute(new AttributeBuilder(attributeName, value1, value2));
	}

	public static AttributeBuilder AddAttribute<T>(this T scope, string attributeName, string value1, string value2, string value3) where T : ICanHaveAttributes
	{
		attributeName.ThrowIfNullOrWhitespace(nameof(attributeName));
		value1.ThrowIfNullOrWhitespace(nameof(value1));
		value2.ThrowIfNullOrWhitespace(nameof(value2));
		value3.ThrowIfNullOrWhitespace(nameof(value3));
		
		return scope.AddAttribute(new AttributeBuilder(attributeName, value1, value2, value3));
	}
	
	public static AttributeBuilder AddAttribute<T>(this T scope, string attributeName, params string[] values) where T : ICanHaveAttributes
	{
		attributeName.ThrowIfNullOrWhitespace(nameof(attributeName));
		values.ThrowIfNullOrEmpty(nameof(values));
		
		return scope.AddAttribute(new AttributeBuilder(attributeName, values));
	}
}