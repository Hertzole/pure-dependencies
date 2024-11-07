namespace Hertzole.CodeBuilder;

public readonly struct CodeBuilderOptions
{
	public static CodeBuilderOptions Default { get; } = new CodeBuilderOptions(
		appendGeneratedCodeAttribute: true,
		appendGlobalPrefix: false);
	
	public bool AppendGeneratedCodeAttribute { get; }
	public bool AppendGlobalPrefix { get; }

	public CodeBuilderOptions(bool appendGeneratedCodeAttribute = true, bool appendGlobalPrefix = false)
	{
		AppendGeneratedCodeAttribute = appendGeneratedCodeAttribute;
		AppendGlobalPrefix = appendGlobalPrefix;
	}

	public override string ToString()
	{
		return $"AppendGeneratedCodeAttribute: {AppendGeneratedCodeAttribute}, AppendGlobalPrefix: {AppendGlobalPrefix}";
	}
}