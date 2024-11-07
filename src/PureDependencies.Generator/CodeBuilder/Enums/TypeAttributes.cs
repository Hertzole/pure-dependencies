using System;

namespace Hertzole.CodeBuilder
{
	[Flags]
	internal enum TypeAttributes : ushort
	{
		None = 0,
		Partial = 1,
		Sealed = 2,
		Abstract = 4,
		ReadOnly = 8,
		Virtual = 16,
		Override = 32,
		Static = 64,
		Const = 128,
		Unsafe = 256,
		Extern = 512,
		Ref = 1024,
		Async = 2048
	}
}