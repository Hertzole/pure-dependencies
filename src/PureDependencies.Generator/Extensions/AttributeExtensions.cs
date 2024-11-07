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
}