namespace Hertzole.CodeBuilder;

public readonly record struct IndentScope : ICodeScope
{
	private readonly int indentAmount;
	private readonly MethodScope methodScope;
	private readonly bool addBrackets;
	
	internal IndentScope(MethodScope methodScope, int indentAmount, bool addBrackets)
	{
		this.methodScope = methodScope;
		this.indentAmount = indentAmount;
		this.addBrackets = addBrackets;

		if (addBrackets)
		{
			methodScope.AppendLine("{");
		}

		methodScope.ExtraIndent += indentAmount;
	}

	public void Dispose()
	{
		methodScope.ExtraIndent -= indentAmount;
		
		if (addBrackets)
		{
			if(methodScope.ExtraIndent > 0)
			{
				methodScope.AppendLine("}");
			}
			else
			{
				methodScope.Append("}");
			}
		}
	}
}