﻿using System.Text;

namespace Hertzole.CodeBuilder;

public static class StringBuilderPool
{
	private static readonly ObjectPool<StringBuilder> pool = new ObjectPool<StringBuilder>(() => new StringBuilder(1024), null, x => x.Clear());
	
	public static StringBuilder Get()
	{
		return pool.Get();
	}
	
	public static PoolHandle<StringBuilder> Get(out StringBuilder item)
	{
		return pool.Get(out item);
	}
	
	public static void Return(StringBuilder item)
	{
		pool.Return(item);
	}
}