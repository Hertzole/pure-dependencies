using System;

namespace Hertzole.PureDependencies
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class SingletonAttribute : Attribute
	{
		public string Factory { get; set; }
		
		public SingletonAttribute(Type type) { }
	}
}