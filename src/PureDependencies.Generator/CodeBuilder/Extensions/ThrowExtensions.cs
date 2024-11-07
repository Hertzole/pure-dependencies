using System;

namespace Hertzole.CodeBuilder;

internal static class ThrowExtensions
{
	public static void ThrowIfNullOrWhitespace(this string value, string paramName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Value cannot be null or whitespace.", paramName);
		}
	}

	public static void ThrowIfNullOrEmpty<T>(this T[] array, string paramName)
	{
		if (array == null || array.Length == 0)
		{
			throw new ArgumentException("Array cannot be null or empty.", paramName);
		}
	}
}