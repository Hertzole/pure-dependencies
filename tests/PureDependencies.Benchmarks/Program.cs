using System.Reflection;
using BenchmarkDotNet.Running;

namespace PureDependencies.Benchmarks;

internal static class Program
{
	private static void Main(string[] args)
	{
		BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
	}
}