using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

//TODO: Custom equals and hashcode methods.
internal readonly record struct TypeToGenerate
{
	public INamedTypeSymbol TargetServiceProvider { get; }
	public TypeDefinition Type { get; }
	public string? Factory { get; }

	public TypeToGenerate(INamedTypeSymbol targetServiceProvider, TypeDefinition type, string? factory = null)
	{
		TargetServiceProvider = targetServiceProvider;
		Type = type;
		Factory = factory;
	}
}