using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Hertzole.PureDependencies.Generator;

internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IEnumerable<T> where T : IEquatable<T>
{
	private readonly T[]? array;

	public int Length
	{
		get { return AsImmutableArray().Length; }
	}

	public EquatableArray(ImmutableArray<T> array)
	{
		this.array = Unsafe.As<ImmutableArray<T>, T[]?>(ref array);
	}

	public EquatableArray(T[] array)
	{
		this.array = array;
	}

	public ref readonly T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get { return ref AsImmutableArray().ItemRef(index); }
	}

	public override bool Equals(object? obj)
	{
		return obj is EquatableArray<T> other && Equals(other);
	}

	public bool Equals(EquatableArray<T> other)
	{
		return AsSpan().SequenceEqual(other.AsSpan());
	}

	public override int GetHashCode()
	{
		if (this.array is not T[] array)
		{
			return 0;
		}

		int hash = 0;
		unchecked
		{
			foreach (T item in array)
			{
				hash = (hash * 397) ^ item.GetHashCode();
			}
		}

		return hash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ImmutableArray<T> AsImmutableArray()
	{
		return Unsafe.As<T[]?, ImmutableArray<T>>(ref Unsafe.AsRef(in array));
	}

	public ReadOnlySpan<T> AsSpan()
	{
		return AsImmutableArray().AsSpan();
	}

	public ImmutableArray<T>.Enumerator GetEnumerator()
	{
		return AsImmutableArray().GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>) AsImmutableArray()).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable) AsImmutableArray()).GetEnumerator();
	}
}