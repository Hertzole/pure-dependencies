using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

internal readonly record struct ServiceProviderData
{
	public INamedTypeSymbol Symbol { get; }
	public TypeName TypeName { get; }
	public bool IsSealed { get; }

	public ServiceProviderData(INamedTypeSymbol symbol, bool isSealed)
	{
		Symbol = symbol;
		TypeName = new TypeName(symbol);
		IsSealed = isSealed;
	}

	// Because the symbol needs to be compared with the SymbolEqualityComparer we need to override the default Equals method. 
	public bool Equals(ServiceProviderData other)
	{
		return SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol) && TypeName.Equals(other.TypeName) && IsSealed == other.IsSealed;
	}

	public override int GetHashCode()
	{
		return SymbolEqualityComparer.Default.GetHashCode(Symbol) * -1521134295 + TypeName.GetHashCode() * -1521134295 + IsSealed.GetHashCode();
	}
}