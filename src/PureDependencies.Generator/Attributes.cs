using System;
using System.Text;
using Hertzole.CodeBuilder;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hertzole.PureDependencies.Generator;

internal static class Attributes
{
	public static readonly AttributeDefinition ServiceProvider = new AttributeDefinition("ServiceProvider", NAMESPACE);

	public static readonly AttributeDefinition Singleton = new AttributeDefinition("Singleton", NAMESPACE);

	private const string NAMESPACE = "Hertzole.PureDependencies";
}

internal readonly record struct AttributeDefinition
{
	// Just the name of the attribute. For example, in [TestAttribute], this would be "Test".
	private readonly string name;
	// The full name of the attribute without the namespace. For example, in [TestAttribute], this would be "TestAttribute".
	private readonly string attributeName;
	// The full name of the attribute with the namespace. For example, in [TestAttribute], this would be "Hertzole.PureDependencies.TestAttribute".
	public readonly string FullName;

	public AttributeDefinition(string name, string nameSpace)
	{
		this.name = name;

		using (StringBuilderPool.Get(out StringBuilder sb))
		{
			sb.Append(nameSpace);
			sb.Append('.');
			sb.Append(name);

			attributeName = sb.ToString();

			sb.Append("Attribute");

			FullName = sb.ToString();
		}
	}

	public bool IsAttribute(INamedTypeSymbol? symbol)
	{
		if (symbol == null)
		{
			return false;
		}

		return symbol.Name.Equals(name, StringComparison.Ordinal) ||
		       symbol.Name.Equals(attributeName, StringComparison.Ordinal) ||
		       symbol.ToDisplayString().Equals(FullName, StringComparison.Ordinal);
	}

	public bool IsAttribute(AttributeSyntax syntax)
	{
		if(syntax.Name is not IdentifierNameSyntax identifier)
		{
			return false;
		}
		
		return identifier.Identifier.Text.Equals(name, StringComparison.Ordinal) ||
		       identifier.Identifier.Text.Equals(attributeName, StringComparison.Ordinal) ||
		       identifier.Identifier.Text.Equals(FullName, StringComparison.Ordinal);
	}
}