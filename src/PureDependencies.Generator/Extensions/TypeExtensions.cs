using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

internal static partial class Extensions
{
	public static DisposableFlags GetDisposableFlags(this INamedTypeSymbol typeSymbol)
	{
		if (typeSymbol.Interfaces.Length == 0)
		{
			return DisposableFlags.None;
		}

		DisposableFlags flags = DisposableFlags.None;

		foreach (INamedTypeSymbol iface in typeSymbol.Interfaces)
		{
			if (iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::System.IDisposable", StringComparison.Ordinal))
			{
				flags |= DisposableFlags.Disposable;
			}

			if (iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Equals("global::System.IAsyncDisposable", StringComparison.Ordinal))
			{
				flags |= DisposableFlags.AsyncDisposable;
			}

			// There's nothing more to do.
			if (flags == (DisposableFlags.Disposable | DisposableFlags.AsyncDisposable))
			{
				break;
			}
		}

		return flags;
	}

	public static ImmutableArray<RequiredMemberData> GetConstructorParameters(this INamedTypeSymbol typeSymbol)
	{
		if (typeSymbol.Constructors.Length == 0)
		{
			return ImmutableArray<RequiredMemberData>.Empty;
		}

		foreach (IMethodSymbol constructor in typeSymbol.InstanceConstructors)
		{
			// Skip synthesized constructors
			if (constructor.IsImplicitlyDeclared)
			{
				continue;
			}

			if (constructor.Parameters.Length == 0)
			{
				return ImmutableArray<RequiredMemberData>.Empty;
			}

			ImmutableArray<RequiredMemberData>.Builder builder = ImmutableArray.CreateBuilder<RequiredMemberData>(constructor.Parameters.Length);

			foreach (IParameterSymbol parameter in constructor.Parameters)
			{
				builder.Add(new RequiredMemberData(parameter.Name, parameter.Type));
			}

			return builder.ToImmutable();
		}

		return ImmutableArray<RequiredMemberData>.Empty;
	}

	// public static ImmutableArray<RequiredArray> GetRequiredArrays(this IMethodSymbol constructor)
	// {
	// 	ImmutableArray<RequiredArray>.Builder builder = ImmutableArray.CreateBuilder<RequiredArray>();
	//
	// 	foreach (IParameterSymbol parameter in constructor.Parameters)
	// 	{
	// 		if (IsCollection(parameter.Type, out string? typeName, out string? fullName))
	// 		{
	// 			builder.Add(new RequiredArray(typeName!, fullName!));
	// 		}
	// 	}
	//
	// 	return builder.ToImmutable();
	// }

	public static bool IsCollection(this ITypeSymbol typeSymbol, out INamedTypeSymbol? collectionType)
	{
		collectionType = null;

		if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol && arrayTypeSymbol.ElementType is INamedTypeSymbol namedSymbol)
		{
			collectionType = namedSymbol;
			return true;
		}

		if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
		{
			foreach (INamedTypeSymbol? iface in namedTypeSymbol.AllInterfaces)
			{
				// For some reason it sometimes reports as IEnumerable even though it has a generic.
				// If so we need to get the generic type from the typeSymbol itself instead of the interface.
				// ¯\_(ツ)_/¯
				if (iface.OriginalDefinition.SpecialType == SpecialType.System_Collections_IEnumerable)
				{
					collectionType = namedTypeSymbol.TypeArguments[0] as INamedTypeSymbol;
				}
				else if (iface.OriginalDefinition.SpecialType == SpecialType.System_Collections_IEnumerable ||
				         iface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_ICollection_T ||
				         iface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IReadOnlyList_T ||
				         iface.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IReadOnlyCollection_T)
				{
					collectionType = iface.TypeArguments[0] as INamedTypeSymbol;
				}

				if (collectionType != null)
				{
					return true;
				}
			}
		}

		return false;
	}

	public static string? GetNamespace(this ISymbol typeSymbol, SymbolDisplayFormat? displayFormat = null)
	{
		if (typeSymbol.ContainingNamespace == null || typeSymbol.ContainingNamespace.IsGlobalNamespace)
		{
			return null;
		}

		return displayFormat == null ? typeSymbol.ContainingNamespace.ToString() : typeSymbol.ContainingNamespace.ToDisplayString(displayFormat);
	}

	public static ImmutableArray<INamedTypeSymbol> GetAllBaseTypesAndInterfaces(this INamedTypeSymbol typeSymbol)
	{
		ImmutableArray<INamedTypeSymbol>.Builder builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

		INamedTypeSymbol? current = typeSymbol;
		while (current != null)
		{
			builder.Add(current);
			current = current.BaseType;
		}

		builder.AddRange(typeSymbol.AllInterfaces);

		return builder.ToImmutable();
	}
}