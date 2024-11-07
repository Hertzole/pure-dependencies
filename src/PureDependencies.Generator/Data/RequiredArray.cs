using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

internal readonly record struct RequiredArray
{
	public INamedTypeSymbol TypeSymbol { get; }
	public TypeName TypeName { get; }

	public RequiredArray(INamedTypeSymbol typeSymbol)
	{
		TypeSymbol = typeSymbol;
		TypeName = new TypeName(typeSymbol);
	}

	public bool Equals(RequiredArray other)
	{
		return SymbolEqualityComparer.Default.Equals(TypeSymbol, other.TypeSymbol);
	}

	public override int GetHashCode()
	{
		return SymbolEqualityComparer.Default.GetHashCode(TypeSymbol);
	}
}