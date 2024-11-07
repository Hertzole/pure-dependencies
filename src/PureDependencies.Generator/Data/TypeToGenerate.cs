using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

//TODO: Custom equals and hashcode methods.
internal readonly record struct TypeToGenerate
{
	public INamedTypeSymbol TargetServiceProvider { get; }
	public TypeDefinition Type { get; }

	public TypeToGenerate(INamedTypeSymbol targetServiceProvider, TypeDefinition type)
	{
		TargetServiceProvider = targetServiceProvider;
		Type = type;
	}
}