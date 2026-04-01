using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Waher.Runtime.Inventory.Generator
{
	[Generator]
	public sealed class WaherInventorySourceGenerator : ISourceGenerator
	{
		private static readonly DiagnosticDescriptor TypesLoaderInAotRule = new DiagnosticDescriptor(
			"WaherAOT001",
			"TypesLoader is trim-unsafe",
			"TypesLoader.Initialize uses runtime assembly scanning and remains a compatibility-only path in trimmed/AOT builds.",
			"Waher.AOT",
			DiagnosticSeverity.Warning,
			true);

		private static readonly DiagnosticDescriptor HiddenImplementationRule = new DiagnosticDescriptor(
			"WaherAOT002",
			"Non-public implementation is not auto-registered",
			"Type '{0}' is non-public and will not be auto-registered unless it is marked with [WaherInventoryInclude].",
			"Waher.AOT",
			DiagnosticSeverity.Info,
			true);

		public void Initialize(GeneratorInitializationContext context)
		{
		}

		public void Execute(GeneratorExecutionContext context)
		{
			Compilation compilation = context.Compilation;
			INamedTypeSymbol metadataSymbol = compilation.GetTypeByMetadataName("Waher.Runtime.Inventory.GeneratedTypeMetadata");
			if (metadataSymbol is null)
				return;

			INamedTypeSymbol includeAttributeSymbol = compilation.GetTypeByMetadataName("Waher.Runtime.Inventory.WaherInventoryIncludeAttribute");
			INamedTypeSymbol aotModelAttributeSymbol = compilation.GetTypeByMetadataName("Waher.Runtime.Inventory.WaherAotModelAttribute");
			INamedTypeSymbol typeAliasAttributeSymbol = compilation.GetTypeByMetadataName("Waher.Runtime.Inventory.TypeAliasAttribute");
			INamedTypeSymbol moduleDependencyAttributeSymbol = compilation.GetTypeByMetadataName("Waher.Runtime.Inventory.ModuleDependencyAttribute");
			INamedTypeSymbol singletonAttributeSymbol = compilation.GetTypeByMetadataName("Waher.Runtime.Inventory.SingletonAttribute");
			INamedTypeSymbol defaultImplementationAttributeSymbol = compilation.GetTypeByMetadataName("Waher.Runtime.Inventory.DefaultImplementationAttribute");
			INamedTypeSymbol inventoryTypeSymbol = compilation.GetTypeByMetadataName("Waher.Runtime.Inventory.Types");
			if (inventoryTypeSymbol is null)
				return;

			if (IsTrimmedOrAot(context))
				ReportTypesLoaderWarnings(context, compilation);

			List<TypeRegistration> registrations = new List<TypeRegistration>();

			foreach (INamedTypeSymbol type in GetAllTypes(compilation.Assembly.GlobalNamespace))
			{
				if (type.TypeKind == TypeKind.Delegate || type.IsGenericType)
					continue;

				bool include = IsExported(type) || HasAttribute(type, includeAttributeSymbol);
				if (!include)
				{
					if (!type.IsImplicitlyDeclared &&
						type.TypeKind == TypeKind.Class &&
						type.AllInterfaces.Length > 0 &&
						type.Locations.Length > 0)
					{
						context.ReportDiagnostic(Diagnostic.Create(HiddenImplementationRule, type.Locations[0], type.ToDisplayString()));
					}

					continue;
				}

				registrations.Add(CreateRegistration(type, aotModelAttributeSymbol, typeAliasAttributeSymbol,
					moduleDependencyAttributeSymbol, singletonAttributeSymbol, defaultImplementationAttributeSymbol));
			}

			if (registrations.Count == 0)
				return;

			string assemblyName = SanitizeIdentifier(compilation.AssemblyName);
			string registrarName = "__WaherGeneratedInventoryRegistrar_" + assemblyName;
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("using System;");
			sb.AppendLine("using Waher.Runtime.Inventory;");
			sb.AppendLine("[assembly: global::Waher.Runtime.Inventory.AssemblyInventoryRegistrarAttribute(typeof(global::Waher.Runtime.Inventory.Generated.");
			sb.Append(registrarName);
			sb.AppendLine("))]");
			sb.AppendLine("namespace Waher.Runtime.Inventory.Generated");
			sb.AppendLine("{");
			sb.Append("	internal static class ");
			sb.Append(registrarName);
			sb.AppendLine();
			sb.AppendLine("	{");
			sb.AppendLine("		private static bool registered = false;");
			sb.AppendLine();
			sb.AppendLine("		public static void Register()");
			sb.AppendLine("		{");
			sb.AppendLine("			if (registered)");
			sb.AppendLine("				return;");
			sb.AppendLine();
			sb.AppendLine("			registered = true;");
			sb.AppendLine();

			int constructorIndex = 0;
			int memberIndex = 0;
			foreach (TypeRegistration registration in registrations)
			{
				sb.AppendLine("			Types.RegisterGeneratedMetadata(new GeneratedTypeMetadata(");
				sb.Append("				typeof(");
				sb.Append(registration.TypeExpression);
				sb.AppendLine("),");
				sb.Append("				Namespace: ");
				sb.Append(ToLiteral(registration.Namespace));
				sb.AppendLine(",");
				sb.Append("				BaseType: ");
				sb.Append(ToTypeValue(registration.BaseTypeExpression));
				sb.AppendLine(",");
				sb.Append("				ImplementedInterfaces: ");
				sb.Append(BuildTypeArray(registration.ImplementedInterfaces));
				sb.AppendLine(",");
				sb.Append("				TypeAliases: ");
				sb.Append(BuildStringArray(registration.TypeAliases));
				sb.AppendLine(",");
				sb.AppendLine("				TypeAttributes: null,");
				sb.Append("				Members: ");
				sb.Append(BuildMembers(registration.Members, ref memberIndex));
				sb.AppendLine(",");
				sb.Append("				Constructors: ");
				sb.Append(BuildConstructors(registration.Constructors, ref constructorIndex));
				sb.AppendLine(",");
				sb.Append("				ModuleDependencies: ");
				sb.Append(BuildStringArray(registration.ModuleDependencies));
				sb.AppendLine(",");
				sb.Append("				HasSingletonAttribute: ");
				sb.Append(registration.HasSingleton ? "true" : "false");
				sb.AppendLine(",");
				sb.Append("				DefaultImplementationType: ");
				sb.Append(ToTypeValue(registration.DefaultImplementationExpression));
				sb.AppendLine("));");
				sb.AppendLine();
			}

			sb.AppendLine("		}");
			sb.AppendLine();

			constructorIndex = 0;
			foreach (TypeRegistration registration in registrations)
			{
				foreach (ConstructorRegistration constructor in registration.Constructors)
				{
					sb.Append("		private static object CreateCtor");
					sb.Append(constructorIndex++);
					sb.AppendLine("(object[] args)");
					sb.AppendLine("		{");
					sb.Append("			return new ");
					sb.Append(registration.TypeExpression);
					sb.Append("(");
					for (int i = 0; i < constructor.ParameterTypes.Count; i++)
					{
						if (i > 0)
							sb.Append(", ");

						sb.Append("(");
						sb.Append(constructor.ParameterTypes[i]);
						sb.Append(")args[");
						sb.Append(i.ToString(CultureInfo.InvariantCulture));
						sb.Append("]");
					}
					sb.AppendLine(");");
					sb.AppendLine("		}");
					sb.AppendLine();
				}
			}

			memberIndex = 0;
			foreach (TypeRegistration registration in registrations)
			{
				foreach (MemberRegistration member in registration.Members)
				{
					sb.Append("		private static object GetMember");
					sb.Append(memberIndex);
					sb.Append("(object instance) => ((");
					sb.Append(registration.TypeExpression);
					sb.Append(")instance).");
					sb.Append(member.Name);
					sb.AppendLine(";");

					if (member.CanWrite)
					{
						sb.Append("		private static void SetMember");
						sb.Append(memberIndex);
						sb.Append("(object instance, object value) => ((");
						sb.Append(registration.TypeExpression);
						sb.Append(")instance).");
						sb.Append(member.Name);
						sb.Append(" = (");
						sb.Append(member.MemberTypeExpression);
						sb.AppendLine(")value;");
					}

					sb.AppendLine();
					memberIndex++;
				}
			}

			sb.AppendLine("	}");
			sb.AppendLine("}");

			context.AddSource(registrarName + ".g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
		}

		private static void ReportTypesLoaderWarnings(GeneratorExecutionContext context, Compilation compilation)
		{
			foreach (SyntaxTree tree in compilation.SyntaxTrees)
			{
				SemanticModel model = compilation.GetSemanticModel(tree);
				IEnumerable<SyntaxNode> nodes = tree.GetRoot().DescendantNodes();

				foreach (SyntaxNode node in nodes)
				{
					if (!(model.GetSymbolInfo(node).Symbol is IMethodSymbol method))
						continue;

					if (method.Name == "Initialize" &&
						method.ContainingType?.ToDisplayString() == "Waher.Runtime.Inventory.Loader.TypesLoader")
					{
						context.ReportDiagnostic(Diagnostic.Create(TypesLoaderInAotRule, node.GetLocation()));
					}
				}
			}
		}

		private static bool IsTrimmedOrAot(GeneratorExecutionContext context)
		{
			if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.PublishAot", out string publishAot) &&
				bool.TryParse(publishAot, out bool isAot) && isAot)
			{
				return true;
			}

			if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.PublishTrimmed", out string publishTrimmed) &&
				bool.TryParse(publishTrimmed, out bool isTrimmed) && isTrimmed)
			{
				return true;
			}

			return false;
		}

		private static List<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
		{
			List<INamedTypeSymbol> result = new List<INamedTypeSymbol>();

			foreach (INamespaceSymbol childNamespace in ns.GetNamespaceMembers())
				result.AddRange(GetAllTypes(childNamespace));

			foreach (INamedTypeSymbol type in ns.GetTypeMembers())
				AddType(type, result);

			return result;
		}

		private static void AddType(INamedTypeSymbol type, List<INamedTypeSymbol> result)
		{
			result.Add(type);

			foreach (INamedTypeSymbol nested in type.GetTypeMembers())
				AddType(nested, result);
		}

		private static TypeRegistration CreateRegistration(INamedTypeSymbol type, INamedTypeSymbol aotModelAttributeSymbol,
			INamedTypeSymbol typeAliasAttributeSymbol, INamedTypeSymbol moduleDependencyAttributeSymbol,
			INamedTypeSymbol singletonAttributeSymbol, INamedTypeSymbol defaultImplementationAttributeSymbol)
		{
			List<MemberRegistration> members = new List<MemberRegistration>();
			if (HasAttribute(type, aotModelAttributeSymbol))
			{
				foreach (ISymbol member in type.GetMembers())
				{
					if (member.IsStatic)
						continue;

					switch (member)
					{
						case IFieldSymbol field when !field.IsImplicitlyDeclared && !field.IsConst:
							if (!CanAccess(field.DeclaredAccessibility))
								continue;

							members.Add(new MemberRegistration(field.Name, GetTypeExpression(field.Type), true, !field.IsReadOnly,
								field.DeclaredAccessibility == Accessibility.Public, true));
							break;

						case IPropertySymbol property:
							if (property.IsIndexer || property.IsStatic)
								continue;

							bool canRead = !(property.GetMethod is null) && CanAccess(property.GetMethod.DeclaredAccessibility);
							bool canWrite = !(property.SetMethod is null) && CanAccess(property.SetMethod.DeclaredAccessibility);
							bool isPublic = (!(property.GetMethod is null) && property.GetMethod.DeclaredAccessibility == Accessibility.Public) ||
								(!(property.SetMethod is null) && property.SetMethod.DeclaredAccessibility == Accessibility.Public);

							if (!canRead && !canWrite)
								continue;

							members.Add(new MemberRegistration(property.Name, GetTypeExpression(property.Type), canRead, canWrite, isPublic, false));
							break;
					}
				}
			}

			List<ConstructorRegistration> constructors = new List<ConstructorRegistration>();
			if ((type.TypeKind == TypeKind.Class && !type.IsAbstract) || type.TypeKind == TypeKind.Struct)
			{
				foreach (IMethodSymbol ctor in type.InstanceConstructors)
				{
					if (ctor.IsImplicitlyDeclared)
						continue;
					if (!CanAccess(ctor.DeclaredAccessibility))
						continue;

					constructors.Add(new ConstructorRegistration(
						ctor.DeclaredAccessibility == Accessibility.Public,
						ctor.Parameters.Select(x => GetTypeExpression(x.Type)).ToList()));
				}
			}

			List<string> moduleDependencies = new List<string>();
			foreach (AttributeData attr in type.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, moduleDependencyAttributeSymbol))
				{
					if (attr.ConstructorArguments.Length == 1)
					{
						TypedConstant argument = attr.ConstructorArguments[0];
						if (argument.Kind == TypedConstantKind.Type && argument.Value is ITypeSymbol dependencyType)
							moduleDependencies.Add(GetRuntimeTypeName(dependencyType));
						else if (argument.Value is string dependencyName && !string.IsNullOrEmpty(dependencyName))
							moduleDependencies.Add(dependencyName);
					}
				}
			}

			string defaultImplementationExpression = null;
			AttributeData defaultImplementationAttribute = type.GetAttributes().FirstOrDefault(attr =>
				SymbolEqualityComparer.Default.Equals(attr.AttributeClass, defaultImplementationAttributeSymbol));
			if (!(defaultImplementationAttribute is null) &&
				defaultImplementationAttribute.ConstructorArguments.Length == 1 &&
				defaultImplementationAttribute.ConstructorArguments[0].Kind == TypedConstantKind.Type &&
				defaultImplementationAttribute.ConstructorArguments[0].Value is ITypeSymbol defaultImplementation)
			{
				defaultImplementationExpression = GetTypeExpression(defaultImplementation);
			}

			return new TypeRegistration(
				GetTypeExpression(type),
				type.ContainingNamespace?.IsGlobalNamespace == false ? type.ContainingNamespace.ToDisplayString() : null,
				type.BaseType is null ? null : GetTypeExpression(type.BaseType),
				type.AllInterfaces.Select(GetTypeExpression).Distinct().ToList(),
				GetAliases(type, typeAliasAttributeSymbol),
				moduleDependencies,
				HasAttribute(type, singletonAttributeSymbol),
				defaultImplementationExpression,
				constructors,
				members);
		}

		private static List<string> GetAliases(INamedTypeSymbol type, INamedTypeSymbol typeAliasAttributeSymbol)
		{
			List<string> result = new List<string>();

			foreach (AttributeData attr in type.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, typeAliasAttributeSymbol) &&
					attr.ConstructorArguments.Length == 1 &&
					attr.ConstructorArguments[0].Value is string alias &&
					!string.IsNullOrEmpty(alias))
				{
					result.Add(alias);
				}
			}

			return result;
		}

		private static bool HasAttribute(ISymbol symbol, INamedTypeSymbol attributeSymbol)
		{
			if (symbol is null || attributeSymbol is null)
				return false;

			foreach (AttributeData attr in symbol.GetAttributes())
			{
				if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, attributeSymbol))
					return true;
			}

			return false;
		}

		private static bool IsExported(INamedTypeSymbol type)
		{
			if (!CanExport(type.DeclaredAccessibility))
				return false;

			INamedTypeSymbol current = type.ContainingType;
			while (!(current is null))
			{
				if (!CanExport(current.DeclaredAccessibility))
					return false;

				current = current.ContainingType;
			}

			return true;
		}

		private static bool CanExport(Accessibility accessibility)
		{
			return accessibility == Accessibility.Public;
		}

		private static bool CanAccess(Accessibility accessibility)
		{
			return accessibility == Accessibility.Public ||
				accessibility == Accessibility.Internal ||
				accessibility == Accessibility.ProtectedOrInternal;
		}

		private static string GetTypeExpression(ITypeSymbol type)
		{
			return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
		}

		private static string BuildTypeArray(List<string> values)
		{
			if (values.Count == 0)
				return "Types.NoTypes";

			return "new Type[] { " + string.Join(", ", values.Select(x => "typeof(" + x + ")")) + " }";
		}

		private static string BuildStringArray(List<string> values)
		{
			if (values.Count == 0)
				return "System.Array.Empty<string>()";

			return "new string[] { " + string.Join(", ", values.Select(ToLiteral)) + " }";
		}

		private static string BuildConstructors(List<ConstructorRegistration> constructors, ref int constructorIndex)
		{
			if (constructors.Count == 0)
				return "System.Array.Empty<GeneratedConstructorMetadata>()";

			List<string> items = new List<string>();
			foreach (ConstructorRegistration constructor in constructors)
			{
				string invokerName = "CreateCtor" + constructorIndex.ToString(CultureInfo.InvariantCulture);
				items.Add("new GeneratedConstructorMetadata(" +
					BuildTypeArray(constructor.ParameterTypes) + ", " +
					(constructor.IsPublic ? "true" : "false") + ", " +
					invokerName + ")");
				constructorIndex++;
			}

			return "new GeneratedConstructorMetadata[] { " + string.Join(", ", items) + " }";
		}

		private static string BuildMembers(List<MemberRegistration> members, ref int memberIndex)
		{
			if (members.Count == 0)
				return "System.Array.Empty<GeneratedMemberMetadata>()";

			List<string> items = new List<string>();
			foreach (MemberRegistration member in members)
			{
				string getter = member.CanRead ? "GetMember" + memberIndex.ToString(CultureInfo.InvariantCulture) : "null";
				string setter = member.CanWrite ? "SetMember" + memberIndex.ToString(CultureInfo.InvariantCulture) : "null";
				items.Add("new GeneratedMemberMetadata(" +
					ToLiteral(member.Name) + ", typeof(" + member.MemberTypeExpression + "), " +
					(member.CanRead ? "true" : "false") + ", " +
					(member.CanWrite ? "true" : "false") + ", " +
					(member.IsPublic ? "true" : "false") + ", " +
					(member.IsField ? "true" : "false") + ", " +
					getter + ", " +
					setter + ")");
				memberIndex++;
			}

			return "new GeneratedMemberMetadata[] { " + string.Join(", ", items) + " }";
		}

		private static string ToLiteral(string value)
		{
			if (value is null)
				return "null";

			return Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(value, true);
		}

		private static string ToTypeValue(string typeExpression)
		{
			return string.IsNullOrEmpty(typeExpression) ? "null" : "typeof(" + typeExpression + ")";
		}

		private static string GetRuntimeTypeName(ITypeSymbol type)
		{
			if (type is INamedTypeSymbol namedType && !(namedType.ContainingType is null))
				return GetRuntimeTypeName(namedType.ContainingType) + "+" + namedType.MetadataName;

			if (type.ContainingNamespace is null || type.ContainingNamespace.IsGlobalNamespace)
				return type.MetadataName;

			return type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;
		}

		private static string SanitizeIdentifier(string value)
		{
			StringBuilder sb = new StringBuilder(value.Length);

			foreach (char ch in value)
			{
				if (char.IsLetterOrDigit(ch) || ch == '_')
					sb.Append(ch);
				else
					sb.Append('_');
			}

			return sb.ToString();
		}

		private sealed class TypeRegistration
		{
			public TypeRegistration(string TypeExpression, string Namespace, string BaseTypeExpression, List<string> ImplementedInterfaces,
				List<string> TypeAliases, List<string> ModuleDependencies, bool HasSingleton, string DefaultImplementationExpression,
				List<ConstructorRegistration> Constructors, List<MemberRegistration> Members)
			{
				this.TypeExpression = TypeExpression;
				this.Namespace = Namespace;
				this.BaseTypeExpression = BaseTypeExpression;
				this.ImplementedInterfaces = ImplementedInterfaces;
				this.TypeAliases = TypeAliases;
				this.ModuleDependencies = ModuleDependencies;
				this.HasSingleton = HasSingleton;
				this.DefaultImplementationExpression = DefaultImplementationExpression;
				this.Constructors = Constructors;
				this.Members = Members;
			}

			public string TypeExpression { get; }
			public string Namespace { get; }
			public string BaseTypeExpression { get; }
			public List<string> ImplementedInterfaces { get; }
			public List<string> TypeAliases { get; }
			public List<string> ModuleDependencies { get; }
			public bool HasSingleton { get; }
			public string DefaultImplementationExpression { get; }
			public List<ConstructorRegistration> Constructors { get; }
			public List<MemberRegistration> Members { get; }
		}

		private sealed class ConstructorRegistration
		{
			public ConstructorRegistration(bool IsPublic, List<string> ParameterTypes)
			{
				this.IsPublic = IsPublic;
				this.ParameterTypes = ParameterTypes;
			}

			public bool IsPublic { get; }
			public List<string> ParameterTypes { get; }
		}

		private sealed class MemberRegistration
		{
			public MemberRegistration(string Name, string MemberTypeExpression, bool CanRead, bool CanWrite, bool IsPublic, bool IsField)
			{
				this.Name = Name;
				this.MemberTypeExpression = MemberTypeExpression;
				this.CanRead = CanRead;
				this.CanWrite = CanWrite;
				this.IsPublic = IsPublic;
				this.IsField = IsField;
			}

			public string Name { get; }
			public string MemberTypeExpression { get; }
			public bool CanRead { get; }
			public bool CanWrite { get; }
			public bool IsPublic { get; }
			public bool IsField { get; }
		}
	}
}
