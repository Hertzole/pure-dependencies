using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

//TODO: Custom equals and hashcode methods.
internal readonly record struct TypeDefinition
{
	public TypeName Name { get; }
	public INamedTypeSymbol TypeSymbol { get; }

	public readonly DisposableFlags DisposableFlags;

	public readonly EquatableArray<RequiredMemberData> RequiredMembers;

	public TypeDefinition(INamedTypeSymbol typeSymbol)
	{
		Name = new TypeName(typeSymbol);
		TypeSymbol = typeSymbol;
		DisposableFlags = typeSymbol.GetDisposableFlags();
		RequiredMembers = new EquatableArray<RequiredMemberData>(typeSymbol.GetConstructorParameters());
	}
}