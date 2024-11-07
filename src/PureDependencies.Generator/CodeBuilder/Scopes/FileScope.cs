using System.Collections.Generic;
using System.Text;

namespace Hertzole.CodeBuilder
{
	//TODO: Pool this
	public sealed class FileScope : ICodeScope
	{
		private readonly StringBuilder sb;
		private readonly CodeStringBuilder codeBuilder;
		private readonly string? fileScopeNamespace;

		private readonly List<string> usings;
		private readonly List<string> contents;

		private int indent;
		
		private bool nullableEnabled;

		internal int Indent
		{
			get { return indent; }
			set
			{
				indent = value < 0 ? 0 : value;
			}
		}

		internal FileScope(StringBuilder sb, CodeStringBuilder codeBuilder) : this(sb, codeBuilder, string.Empty) { }

		private FileScope(StringBuilder sb, CodeStringBuilder codeBuilder, string? fileScopeNamespace)
		{
			this.sb = sb;
			this.codeBuilder = codeBuilder;
			this.fileScopeNamespace = fileScopeNamespace;

			//TODO: Get from pool
			usings = new List<string>(16);
			contents = new List<string>(16);
		}

		public void Dispose()
		{
			//TODO: Return string builder to pool
			//TODO: Return list to pool
		}

		public void AddUsing(string usingNamespace)
		{
			usingNamespace.ThrowIfNullOrWhitespace(nameof(usingNamespace));

			usings.Add(usingNamespace);
		}

		public FileScope WithFileScopedNamespace(string namespaceName)
		{
			namespaceName.ThrowIfNullOrWhitespace(nameof(namespaceName));

			return new FileScope(sb, codeBuilder, namespaceName);
		}

		public ClassScope AddClass(string className)
		{
			className.ThrowIfNullOrWhitespace(nameof(className));

			ClassScope scope = ClassScope.Create(codeBuilder, this, className);
			
			if(codeBuilder.Options.AppendGeneratedCodeAttribute)
			{
				scope.AddAttribute(codeBuilder.GeneratedCodeAttribute);
			}

			return scope;
		}
		
		public StructScope AddStruct(string structName)
		{
			structName.ThrowIfNullOrWhitespace(nameof(structName));
			
			StructScope scope = new StructScope(codeBuilder, this, structName);
			
			if(codeBuilder.Options.AppendGeneratedCodeAttribute)
			{
				scope.AddAttribute(codeBuilder.GeneratedCodeAttribute);
			}

			return scope;
		}

		public FileScope EnableNullable()
		{
			this.nullableEnabled = true;
			return this;
		}
		
		internal void AddContent(string content)
		{
			contents.Add(content);
		}

		public override string ToString()
		{
			if (nullableEnabled)
			{
				sb.AppendLine("#nullable enable");
				sb.AppendLine();
			}
			
			for (int i = 0; i < usings.Count; i++)
			{
				sb.Append("using ");
				sb.Append(usings[i]);
				sb.Append(';');
				sb.AppendLine();
			}

			if (usings.Count > 0)
			{
				sb.AppendLine();
			}

			if (!string.IsNullOrWhiteSpace(fileScopeNamespace))
			{
				sb.Append("namespace ");
				sb.Append(fileScopeNamespace);
				sb.Append(';');
				sb.AppendLine();
				sb.AppendLine();
			}

			if (contents.Count > 0)
			{
				for (int i = 0; i < contents.Count; i++)
				{
					if(i < contents.Count - 1)
					{
						sb.AppendLine(contents[i]);
						sb.AppendLine();
					}
					else
					{
						sb.Append(contents[i]);
					}
				}
			}

			return sb.ToString();
		}
	}
}