namespace Hertzole.CodeBuilder;

public enum TypeAccessor
{
	/// <summary>
	///     Won't append any accessor. This results in an internal type.
	/// </summary>
	None = 0,
	/// <summary>
	///     Will append 'public' to the type.
	/// </summary>
	Public = 1,
	/// <summary>
	///     Will append 'internal' to the type.
	/// </summary>
	Internal = 2,
	/// <summary>
	///     Will append 'protected' to the type.
	/// </summary>
	File = 3
}