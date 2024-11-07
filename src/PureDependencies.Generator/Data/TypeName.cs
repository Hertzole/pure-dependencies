using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

internal readonly record struct TypeName
{
	public string MinimalName { get; }
	public string? Namespace { get; }
	public string FullyQualifiedName { get; }

	public TypeName(string minimalName, string? ns, string fullyQualifiedName)
	{
		MinimalName = minimalName;
		Namespace = ns;
		FullyQualifiedName = fullyQualifiedName;
	}

	public TypeName(INamedTypeSymbol typeSymbol)
	{
		MinimalName = typeSymbol.Name;
		Namespace = typeSymbol.GetNamespace();
		FullyQualifiedName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}
}