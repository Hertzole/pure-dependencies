namespace Hertzole.CodeBuilder;

public interface ICanHaveFields
{
	FieldBuilder AddField(string fieldName, string fieldType);
}