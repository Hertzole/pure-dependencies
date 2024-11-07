using Microsoft.CodeAnalysis;

namespace Hertzole.PureDependencies.Generator;

internal readonly record struct RequiredArrayTypes
{
	public TypeDefinition ArrayType { get; }
	public EquatableArray<TypeDefinition> Services { get; }

	public RequiredArrayTypes(TypeDefinition arrayType, EquatableArray<TypeDefinition> services)
	{
		ArrayType = arrayType;
		Services = services;
	}
}