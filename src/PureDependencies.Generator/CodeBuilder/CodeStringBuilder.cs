using System;
using System.Text;

namespace Hertzole.CodeBuilder
{
	/// <summary>
	///     The main class for building code strings.
	/// </summary>
	public abstract class CodeStringBuilder
	{
		internal string Version { get; }
		internal string GeneratorName { get; }
		internal CodeBuilderOptions Options { get; }

		public AttributeBuilder GeneratedCodeAttribute { get; }

		public CodeStringBuilder(string generatorName, string version) : this(CodeBuilderOptions.Default, generatorName, version) { }

		public CodeStringBuilder(CodeBuilderOptions options, string generatorName, string version)
		{
			Options = options;
			GeneratorName = generatorName;
			Version = version;

			GeneratedCodeAttribute = new AttributeBuilder("global::System.CodeDom.Compiler.GeneratedCode", $"\"{generatorName}\", \"{version}\"");
		}

		public const string DEFAULT_VERSION = "1.0.0.0";

		public FileScope NewFile()
		{
			//TODO: Pool string builder
			return new FileScope(new StringBuilder(), this);
		}
	}

	/// <summary>
	///     The main class for building code strings.
	/// </summary>
	/// <typeparam name="T">The type of the generator. Used with the optional [GeneratedCode] attribute.</typeparam>
	public sealed class CodeStringBuilder<T> : CodeStringBuilder
	{
		public CodeStringBuilder(string? version = null) : this(CodeBuilderOptions.Default, version) { }

		public CodeStringBuilder(CodeBuilderOptions options, string? version = null) : base(options, typeof(T).FullName!, GetVersion(version)) { }

		private static string GetVersion(string? version)
		{
			if (!string.IsNullOrWhiteSpace(version))
			{
				return version!;
			}

			Version? assemblyVersion = typeof(T).Assembly.GetName().Version;
			return assemblyVersion != null ? assemblyVersion.ToString() : DEFAULT_VERSION;
		}
	}
}