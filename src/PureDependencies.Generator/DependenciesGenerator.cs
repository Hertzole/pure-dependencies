using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Hertzole.CodeBuilder;
using Hertzole.UnityToolbox.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Hertzole.PureDependencies.Generator;

[Generator]
public sealed class DependenciesGenerator : IIncrementalGenerator
{
	private static readonly CodeStringBuilder<DependenciesGenerator> codeBuilder = new CodeStringBuilder<DependenciesGenerator>();

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<ImmutableArray<TypeToGenerate>> singletons =
			context.SyntaxProvider
			       .ForAttributeWithMetadataName(
				       Attributes.Singleton.FullName,
				       static (node, _) => node is ClassDeclarationSyntax,
				       GetTypeDefinition)
			       .Where(static data => data.Length > 0)
			       .SelectMany(static (data, _) => data)
			       .Collect();

		IncrementalValueProvider<ImmutableArray<KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>>>> requiredArrays =
			singletons.SelectMany(GetRequiredArrays).Collect();

		IncrementalValueProvider<ImmutableArray<ServiceProviderData>> serviceProviders =
			context.SyntaxProvider
			       .ForAttributeWithMetadataName(
				       Attributes.ServiceProvider.FullName,
				       static (node, _) => node is ClassDeclarationSyntax,
				       GetServiceProviderData)
			       .Where(static data => data != null)
			       .Select((data, _) => data!.Value)
			       .Collect();

		IncrementalValueProvider<((ImmutableArray<ServiceProviderData> Left, ImmutableArray<TypeToGenerate> Right) Left,
			ImmutableArray<KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>>> Right)> result =
			serviceProviders.Combine(singletons).Combine(requiredArrays);

		context.RegisterImplementationSourceOutput(result,
			(productionContext, tuple) => { Execute(in productionContext, in tuple.Left.Left, in tuple.Left.Right, in tuple.Right); });
	}

	private static ImmutableArray<TypeToGenerate> GetTypeDefinition(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
	{
		try
		{
			if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
			{
				return ImmutableArray<TypeToGenerate>.Empty;
			}

			cancellationToken.ThrowIfCancellationRequested();

			ImmutableArray<TypeToGenerate>.Builder builder = ImmutableArray.CreateBuilder<TypeToGenerate>(context.Attributes.Length);

			foreach (AttributeData attribute in context.Attributes)
			{
				cancellationToken.ThrowIfCancellationRequested();

				string? factory = null;
				if (attribute.TryGetNamedArgument("Factory", out object? factoryValue))
				{
					factory = factoryValue as string;
				}

				if (attribute.ConstructorArguments.Length == 1 &&
				    attribute.ConstructorArguments[0].Value is INamedTypeSymbol typeSymbol)
				{
					builder.Add(new TypeToGenerate(classSymbol, new TypeDefinition(typeSymbol), factory));
				}
			}

			return builder.ToImmutable();
		}

		catch (Exception e)
		{
			Log.LogError($"Error when getting type definition: {e}");
			return ImmutableArray<TypeToGenerate>.Empty;
		}
	}

	private static IEnumerable<KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>>> GetRequiredArrays(ImmutableArray<TypeToGenerate> array,
		CancellationToken token)
	{
		ImmutableArray<RequiredArray>.Builder builder = ImmutableArray.CreateBuilder<RequiredArray>();
		ImmutableDictionary<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>.Builder>.Builder dictionary =
			ImmutableDictionary.CreateBuilder<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>.Builder>(SymbolEqualityComparer.Default);

		foreach (TypeToGenerate type in array)
		{
			token.ThrowIfCancellationRequested();

			foreach (RequiredMemberData requiredMember in type.Type.RequiredMembers)
			{
				token.ThrowIfCancellationRequested();

				if (requiredMember.TryGetCollection(out INamedTypeSymbol? collectionType) && !builder.Contains(new RequiredArray(collectionType)))
				{
					builder.Add(new RequiredArray(collectionType));
					Log.LogInfo("Added required array: " + requiredMember.TypeName.MinimalName);
				}
			}
		}

		ImmutableArray<RequiredArray> requiredArrays = builder.ToImmutable();

		Log.LogInfo("Required arrays: " + requiredArrays.Length);

		foreach (TypeToGenerate type in array)
		{
			ImmutableArray<INamedTypeSymbol> allBaseTypes = type.Type.TypeSymbol.GetAllBaseTypesAndInterfaces();
			foreach (INamedTypeSymbol baseType in allBaseTypes)
			{
				RequiredArray requiredArray = new RequiredArray(baseType);
				Log.LogInfo("Checking if required array: " + requiredArray.TypeName.MinimalName);

				if (requiredArrays.Contains(requiredArray))
				{
					Log.LogInfo($"Type {type.Type.Name.MinimalName} is required by array {requiredArray.TypeName.MinimalName}");

					if (dictionary.TryGetValue(requiredArray.TypeSymbol, out ImmutableArray<INamedTypeSymbol>.Builder? services))
					{
						services.Add(type.Type.TypeSymbol);
					}
					else
					{
						ImmutableArray<INamedTypeSymbol>.Builder typesBuilder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
						typesBuilder.Add(type.Type.TypeSymbol);
						dictionary.Add(requiredArray.TypeSymbol, typesBuilder);
					}
				}
			}
		}

		ImmutableDictionary<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>> dic =
			dictionary
				.ToImmutableDictionary<KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>.Builder>, INamedTypeSymbol,
					ImmutableArray<INamedTypeSymbol>>(pair => pair.Key, pair => pair.Value.ToImmutable(), SymbolEqualityComparer.Default);

		return dic;
	}

	private static ServiceProviderData? GetServiceProviderData(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
	{
		if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
		{
			return null;
		}

		cancellationToken.ThrowIfCancellationRequested();

		return new ServiceProviderData(classSymbol, classSymbol.IsSealed);
	}

	private static void Execute(in SourceProductionContext productionContext,
		in ImmutableArray<ServiceProviderData> serviceProviders,
		in ImmutableArray<TypeToGenerate> types,
		in ImmutableArray<KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>>> requiredArrays)
	{
		try
		{
			foreach (ServiceProviderData serviceProvider in serviceProviders)
			{
				productionContext.CancellationToken.ThrowIfCancellationRequested();

				ImmutableArray<TypeToGenerate> typesToGenerate = GetTypesForServiceProvider(in serviceProvider, in types);

				WriteServiceProvider(in productionContext, in serviceProvider, in typesToGenerate, in requiredArrays);
			}
		}
		catch (Exception e)
		{
			Log.LogError(e.ToString());
		}
	}

	private static void WriteServiceProvider(in SourceProductionContext productionContext,
		in ServiceProviderData data,
		in ImmutableArray<TypeToGenerate> typesToGenerate,
		in ImmutableArray<KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>>> requiredArrays)
	{
		productionContext.CancellationToken.ThrowIfCancellationRequested();

		using FileScope file = codeBuilder.NewFile().EnableNullable();
		using (ClassScope type = file.AddClass(data.TypeName.MinimalName)
		                             .Partial()
		                             .WithNamespace(data.TypeName.Namespace)
		                             .WithInterface("global::System.IServiceProvider"))
		{
			if (!data.IsSealed)
			{
				type.Sealed();
			}

			type.AddField("syncRoot", "object")
			    .WithAccessor(FieldAccessor.Private)
			    .WithDefaultValue("new object()")
			    .ReadOnly()
			    .Dispose();

			DisposableFlags disposableFlags = GetDisposableFlags(typesToGenerate);

			using (MethodScope getService = type.AddMethod("GetService<T>", "T").WithAccessor(MethodAccessor.Public))
			{
				getService.Append(
					"return this is global::Hertzole.PureDependencies.IServiceProvider<T> provider ? provider.GetService() : throw new global::System.InvalidOperationException();");
			}

			if ((disposableFlags & DisposableFlags.Disposable) != 0 && (disposableFlags & DisposableFlags.AsyncDisposable) != 0)
			{
				WriteBothDisposeMethods(typesToGenerate, type);
			}
			else if ((disposableFlags & DisposableFlags.AsyncDisposable) != 0)
			{
				WriteAsyncDisposeMethod(typesToGenerate, type);
			}
			else if ((disposableFlags & DisposableFlags.Disposable) != 0)
			{
				WriteDisposeMethod(typesToGenerate, type);
			}

			WriteServiceProviderGetService(in productionContext, in typesToGenerate, type);

			foreach (TypeToGenerate toGenerate in typesToGenerate)
			{
				//TODO: Check type and write the correct method
				WriteSingleton(in productionContext, in toGenerate, type);
			}

			foreach (KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>> requiredArray in requiredArrays)
			{
				WriteRequiredArray(in productionContext, in requiredArray, type);
			}
		}

		productionContext.AddSource(GetHintName(data), file.ToString());
	}

	private static void WriteServiceProviderGetService(in SourceProductionContext context, in ImmutableArray<TypeToGenerate> types, ClassScope scope)
	{
		context.CancellationToken.ThrowIfCancellationRequested();
		
		using PoolHandle<StringBuilder> stringBuilderScope = StringBuilderPool.Get(out StringBuilder? sb);

		using (MethodScope getService = scope.AddMethod("GetService", "object?").WithAccessor(MethodAccessor.Public))
		{
			getService.AddParameter("global::System.Type", "serviceType");
			
			getService.AppendLine("return serviceType switch");
			
			using (getService.WithIndent(1, true))
			{
				foreach (TypeToGenerate type in types)
				{
					getService.Append("_ when serviceType == typeof(");
					getService.Append(type.Type.Name.FullyQualifiedName);
					getService.Append(") => ((global::Hertzole.PureDependencies.IServiceProvider<");
					getService.Append(type.Type.Name.FullyQualifiedName);
					getService.AppendLine(">) this).GetService(),");
				}
				
				getService.AppendLine("_ => null");
			}

			getService.Append(';');
		}
	}

	private static void WriteSingleton(in SourceProductionContext context, in TypeToGenerate type, ClassScope typeScope)
	{
		context.CancellationToken.ThrowIfCancellationRequested();

		using PoolHandle<StringBuilder> stringBuilderScope = StringBuilderPool.Get(out StringBuilder? sb);

		sb.Clear();
		sb.Append(type.Type.Name.FullyQualifiedName);
		sb.Append('?');

		string fieldType = sb.ToString();

		typeScope.AddField(type.Type.Name.MinimalName, fieldType).WithAccessor(FieldAccessor.Private).WithDefaultValue("null").Dispose();

		sb.Clear();
		sb.Append("global::Hertzole.PureDependencies.IServiceProvider<");
		sb.Append(type.Type.Name.FullyQualifiedName);
		sb.Append(">");

		string interfaceName = sb.ToString();

		typeScope.WithInterface(interfaceName);

		sb.Clear();
		sb.Append(interfaceName);
		sb.Append(".GetService");

		string methodName = sb.ToString();

		using (MethodScope getService = typeScope.AddMethod(methodName, type.Type.Name.FullyQualifiedName))
		{
			getService.Append("if (");
			getService.Append(type.Type.Name.MinimalName);
			getService.AppendLine(" == null)");

			using (getService.WithIndent(1, true))
			{
				foreach (RequiredMemberData requiredMember in type.Type.RequiredMembers)
				{
					bool isCollection = requiredMember.TryGetCollection(out _);

					getService.Append(requiredMember.TypeName.FullyQualifiedName);

					if (isCollection)
					{
						getService.Append("[]");
					}

					getService.Append(' ');
					getService.Append(requiredMember.ParameterName);
					getService.Append(" = ((global::Hertzole.PureDependencies.IServiceProvider<");
					getService.Append(requiredMember.TypeName.FullyQualifiedName);

					if (isCollection)
					{
						getService.Append("[]");
					}

					getService.AppendLine(">) this).GetService();");
				}

				if (type.Type.RequiredMembers.Length > 0)
				{
					getService.AppendLine();
				}

				getService.AppendLine("lock (syncRoot)");

				using (getService.WithIndent(1, true))
				{
					getService.Append(type.Type.Name.MinimalName);
					getService.Append(" = ");

					// Write a normal 'new' instance.
					if (string.IsNullOrEmpty(type.Factory))
					{
						getService.Append("new ");
						getService.Append(type.Type.Name.FullyQualifiedName);
						getService.Append('(');

						for (int i = 0; i < type.Type.RequiredMembers.Length; i++)
						{
							if (i > 0)
							{
								getService.Append(", ");
							}

							getService.Append(type.Type.RequiredMembers[i].ParameterName);
						}

						getService.AppendLine(");");
					}
					else // Use the provided factory method
					{
						getService.Append(type.Factory!);
						getService.AppendLine("();");
					}
				}
			}

			getService.AppendLine();
			getService.Append("return ");
			getService.Append(type.Type.Name.MinimalName);
			getService.Append(';');
		}
	}

	private static void WriteRequiredArray(in SourceProductionContext productionContext,
		in KeyValuePair<INamedTypeSymbol, ImmutableArray<INamedTypeSymbol>> requiredArray,
		ClassScope type)
	{
		productionContext.CancellationToken.ThrowIfCancellationRequested();

		using PoolHandle<StringBuilder> stringBuilderScope = StringBuilderPool.Get(out StringBuilder? sb);

		string fullName = requiredArray.Key.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		sb.Append("global::Hertzole.PureDependencies.IServiceProvider<");
		sb.Append(fullName);
		sb.Append("[]>");

		string interfaceName = sb.ToString();

		type.WithInterface(interfaceName);

		sb.Clear();
		sb.Append(requiredArray.Key.Name);
		sb.Append("Array");

		string fieldName = sb.ToString();

		sb.Clear();
		sb.Append(fullName);
		sb.Append("[]?");

		type.AddField(fieldName, sb.ToString()).WithDefaultValue("null");

		sb.Clear();
		sb.Append(interfaceName);
		sb.Append(".GetService");

		string methodName = sb.ToString();

		using (MethodScope getService = type.AddMethod(methodName, fullName + "[]"))
		{
			getService.Append("if (");
			getService.Append(fieldName);
			getService.AppendLine(" == null)");

			using (getService.WithIndent(1, true))
			{
				getService.AppendLine("lock (syncRoot)");

				using (getService.WithIndent(1, true))
				{
					getService.Append(fieldName);
					getService.Append(" = new ");
					getService.Append(fullName);
					getService.AppendLine("[]");
					getService.AppendLine("{");

					using (getService.WithIndent())
					{
						for (int i = 0; i < requiredArray.Value.Length; i++)
						{
							if (i > 0)
							{
								getService.AppendLine(",");
							}

							sb.Clear();
							sb.Append("((global::Hertzole.PureDependencies.IServiceProvider<");
							sb.Append(requiredArray.Value[i].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
							sb.Append(">) this).GetService()");

							getService.Append(sb.ToString());
						}
					}

					getService.AppendLine();
					getService.AppendLine("};");
				}
			}

			getService.AppendLine();
			getService.Append("return ");
			getService.Append(fieldName);
			getService.Append(';');
		}
	}

	private static ImmutableArray<TypeToGenerate> GetTypesForServiceProvider(in ServiceProviderData serviceProvider, in ImmutableArray<TypeToGenerate> types)
	{
		ImmutableArray<TypeToGenerate>.Builder builder = ImmutableArray.CreateBuilder<TypeToGenerate>(types.Length);

		foreach (TypeToGenerate type in types)
		{
			if (SymbolEqualityComparer.Default.Equals(type.TargetServiceProvider, serviceProvider.Symbol))
			{
				builder.Add(type);
			}
		}

		return builder.ToImmutable();
	}

	private static DisposableFlags GetDisposableFlags(in ImmutableArray<TypeToGenerate> typesToGenerate)
	{
		DisposableFlags flags = DisposableFlags.None;

		if (typesToGenerate.Length == 0)
		{
			return DisposableFlags.None;
		}

		foreach (DisposableFlags disposableFlags in typesToGenerate.Select(x => x.Type.DisposableFlags))
		{
			if ((disposableFlags & DisposableFlags.Disposable) != 0)
			{
				flags |= DisposableFlags.Disposable;
			}

			if ((disposableFlags & DisposableFlags.AsyncDisposable) != 0)
			{
				flags |= DisposableFlags.AsyncDisposable;
			}

			if (flags == (DisposableFlags.Disposable | DisposableFlags.AsyncDisposable))
			{
				break;
			}
		}

		return flags;
	}

	private static void WriteDisposeMethod(in ImmutableArray<TypeToGenerate> types, ClassScope typeScope)
	{
		WriteDisposeMethodHelper(
			"global::System.IDisposable",
			"Dispose",
			null,
			types,
			DisposableFlags.Disposable,
			typeScope,
			static (method, typeName) =>
			{
				method.Append(typeName);
				method.Append("?.Dispose();");
			});
	}

	private static void WriteAsyncDisposeMethod(in ImmutableArray<TypeToGenerate> types, ClassScope typeScope)
	{
		WriteDisposeMethodHelper(
			"global::System.IAsyncDisposable",
			"DisposeAsync",
			"global::System.Threading.Tasks.ValueTask",
			types,
			DisposableFlags.AsyncDisposable,
			typeScope,
			static (method, typeName) =>
			{
				method.Append("if (");
				method.Append(typeName);
				method.AppendLine(" != null)");

				using (method.WithIndent(1, true))
				{
					method.Append("await ");
					method.Append(typeName);
					method.AppendLine(".DisposeAsync();");
				}
			},
			static method => method.Async());
	}

	private static void WriteBothDisposeMethods(in ImmutableArray<TypeToGenerate> types, ClassScope typeScope)
	{
		typeScope.WithInterface("global::System.IDisposable");
		typeScope.WithInterface("global::System.IAsyncDisposable");

		using (MethodScope dispose = typeScope.AddMethod("Dispose").WithAccessor(MethodAccessor.Public))
		{
			dispose.AppendLine("Dispose(true);");
			dispose.Append("global::System.GC.SuppressFinalize(this);");
		}

		using (MethodScope disposeAsync =
		       typeScope.AddMethod("DisposeAsync", "global::System.Threading.Tasks.ValueTask").WithAccessor(MethodAccessor.Public).Async())
		{
			disposeAsync.AppendLine("await DisposeAsyncCore();");
			disposeAsync.AppendLine();
			disposeAsync.AppendLine("Dispose(false);");
			disposeAsync.Append("global::System.GC.SuppressFinalize(this);");
		}

		using (MethodScope disposeCore = typeScope.AddMethod("Dispose").WithAccessor(MethodAccessor.Private))
		{
			disposeCore.AddParameter("bool", "disposing");

			disposeCore.AppendLine("if (disposing)");

			using (disposeCore.WithIndent(1, true))
			{
				foreach (TypeToGenerate type in types)
				{
					if ((type.Type.DisposableFlags & DisposableFlags.Disposable) != 0)
					{
						disposeCore.Append(type.Type.Name.MinimalName);
						disposeCore.AppendLine("?.Dispose();");
						disposeCore.Append(type.Type.Name.MinimalName);
						disposeCore.AppendLine(" = null;");
					}
					else if ((type.Type.DisposableFlags & DisposableFlags.AsyncDisposable) != 0)
					{
						disposeCore.Append("if (");
						disposeCore.Append(type.Type.Name.MinimalName);
						disposeCore.Append(" is global::System.IDisposable ");
						disposeCore.Append(type.Type.Name.MinimalName);
						disposeCore.AppendLine("_Disposable)");

						using (disposeCore.WithIndent(1, true))
						{
							disposeCore.Append(type.Type.Name.MinimalName);
							disposeCore.AppendLine("_Disposable.Dispose();");
							disposeCore.Append(type.Type.Name.MinimalName);
							disposeCore.AppendLine(" = null;");
						}
					}
				}
			}
		}

		using (MethodScope disposeAsyncCore = typeScope.AddMethod("DisposeAsyncCore", "global::System.Threading.Tasks.ValueTask")
		                                               .WithAccessor(MethodAccessor.Private).Async())
		{
			int writeCount = 0;
			foreach (TypeToGenerate type in types)
			{
				if (writeCount > 0)
				{
					disposeAsyncCore.AppendLine();
				}

				if ((type.Type.DisposableFlags & DisposableFlags.AsyncDisposable) != 0)
				{
					disposeAsyncCore.Append("if (");
					disposeAsyncCore.Append(type.Type.Name.MinimalName);
					disposeAsyncCore.AppendLine(" != null)");

					using (disposeAsyncCore.WithIndent(1, true))
					{
						disposeAsyncCore.Append("await ");
						disposeAsyncCore.Append(type.Type.Name.MinimalName);
						disposeAsyncCore.AppendLine(".DisposeAsync();");

						disposeAsyncCore.Append(type.Type.Name.MinimalName);
						disposeAsyncCore.AppendLine(" = null;");
					}
				}
				else if ((type.Type.DisposableFlags & DisposableFlags.Disposable) != 0)
				{
					disposeAsyncCore.Append("if (");
					disposeAsyncCore.Append(type.Type.Name.MinimalName);
					disposeAsyncCore.Append(" is global::System.IAsyncDisposable ");
					disposeAsyncCore.Append(type.Type.Name.MinimalName);
					disposeAsyncCore.AppendLine("_AsyncDisposable)");

					using (disposeAsyncCore.WithIndent(1, true))
					{
						disposeAsyncCore.Append("await ");
						disposeAsyncCore.Append(type.Type.Name.MinimalName);
						disposeAsyncCore.AppendLine("_AsyncDisposable.DisposeAsync();");
					}

					disposeAsyncCore.AppendLine();
					disposeAsyncCore.AppendLine("else");

					using (disposeAsyncCore.WithIndent(1, true))
					{
						disposeAsyncCore.Append(type.Type.Name.MinimalName);
						disposeAsyncCore.AppendLine("?.Dispose();");
					}

					disposeAsyncCore.AppendLine();
					disposeAsyncCore.Append(type.Type.Name.MinimalName);
					disposeAsyncCore.AppendLine(" = null;");
				}

				writeCount++;
			}
		}
	}

	private static void WriteDisposeMethodHelper(string interfaceName,
		string methodName,
		string? returnType,
		in ImmutableArray<TypeToGenerate> types,
		DisposableFlags flagToCheck,
		ClassScope typeScope,
		Action<MethodScope, string> writeDispose,
		Action<MethodScope>? setupMethod = null)
	{
		typeScope.WithInterface(interfaceName);

		using (MethodScope method = typeScope.AddMethod(methodName).WithAccessor(MethodAccessor.Public))
		{
			setupMethod?.Invoke(method);

			if (!string.IsNullOrEmpty(returnType))
			{
				method.WithReturnType(returnType!);
			}

			int writeCount = 0;
			for (int i = 0; i < types.Length; i++)
			{
				TypeDefinition type = types[i].Type;
				if ((type.DisposableFlags & flagToCheck) == 0)
				{
					continue;
				}

				if (writeCount > 0)
				{
					method.AppendLine();
				}

				writeDispose.Invoke(method, type.Name.MinimalName);
				writeCount++;
			}
		}
	}

	private static string GetHintName(in ServiceProviderData data)
	{
		using (StringBuilderPool.Get(out StringBuilder? sb))
		{
			sb.Append(data.TypeName.MinimalName);
			sb.Append(".PureDependencies.g.cs");

			return sb.ToString();
		}
	}
}