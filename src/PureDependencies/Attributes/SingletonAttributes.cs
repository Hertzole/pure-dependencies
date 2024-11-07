using System;

namespace Hertzole.PureDependencies
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class SingletonAttribute : Attribute
	{
		public SingletonAttribute(Type type) { }
	}
}