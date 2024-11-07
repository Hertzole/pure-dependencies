using System;

namespace Hertzole.PureDependencies.Generator;

[Flags]
public enum DisposableFlags : byte
{
	None = 0,
	Disposable = 1,
	AsyncDisposable = 2,
}