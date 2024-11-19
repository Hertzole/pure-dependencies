using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hertzole.PureDependencies.Generator;

internal static partial class Extensions
{
	public static bool IsAttribute(this AttributeData attribute, in AttributeDefinition definition)
	{
		return definition.IsAttribute(attribute.AttributeClass);
	}

	public static bool IsAttribute(this AttributeSyntax syntax, in AttributeDefinition definition)
	{
		return definition.IsAttribute(syntax);
	}

	/// <summary>
	///     Tries to get a named argument from the attribute.
	/// </summary>
	/// <remarks>
	///     This method is case-sensitive.
	///     <para>
	///         Named arguments look like this: <c>[MyAttribute(Name = "Value")]</c>
	///     </para>
	/// </remarks>
	/// <param name="attribute">The attribute to get the argument from.</param>
	/// <param name="name">The name of the argument to get.</param>
	/// <param name="value">The output value, if found. Otherwise, it will be null.</param>
	/// <returns>True if the argument was found. Otherwise, false.</returns>
	public static bool TryGetNamedArgument(this AttributeData attribute, string name, out object? value)
	{
		if (attribute.NamedArguments.IsDefaultOrEmpty)
		{
			value = null;
			return false;
		}

		ReadOnlySpan<KeyValuePair<string, TypedConstant>> span = attribute.NamedArguments.AsSpan();

		for (int i = 0; i < span.Length; i++)
		{
			if (!span[i].Key.Equals(name, StringComparison.Ordinal))
			{
				continue;
			}

			value = span[i].Value.Value;
			return true;
		}

		value = null;
		return false;
	}
}