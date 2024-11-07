using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

internal readonly record struct RequiredMemberData
{
	private readonly INamedTypeSymbol? collectionType;

	public string ParameterName { get; }
	public ITypeSymbol TypeSymbol { get; }
	public TypeName TypeName { get; }

	public RequiredMemberData(string parameterName, ITypeSymbol typeSymbol)
	{
		ParameterName = parameterName;
		TypeSymbol = typeSymbol;
		typeSymbol.IsCollection(out collectionType);

		string typeName;
		string fullTypeName;

		if (collectionType != null)
		{
			typeName = collectionType.Name;
			fullTypeName = collectionType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}
		else
		{
			typeName = typeSymbol.Name;
			fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}

		TypeName = new TypeName(typeName!, typeSymbol.GetNamespace(), fullTypeName!);
	}

	public bool TryGetCollection(out INamedTypeSymbol? type)
	{
		if (collectionType == null)
		{
			type = null;
			return false;
		}

		type = collectionType;
		return true;
	}
}