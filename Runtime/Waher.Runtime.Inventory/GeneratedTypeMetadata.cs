using System;
using System.Collections.Generic;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Generated type metadata.
	/// </summary>
	public sealed class GeneratedTypeMetadata
	{
		private readonly Type type;
		private readonly string @namespace;
		private readonly Type baseType;
		private readonly Type[] implementedInterfaces;
		private readonly string[] typeAliases;
		private readonly object[] typeAttributes;
		private readonly GeneratedMemberMetadata[] members;
		private readonly GeneratedMethodMetadata[] methods;
		private readonly GeneratedConstructorMetadata[] constructors;
		private readonly string[] moduleDependencies;
		private readonly bool hasSingletonAttribute;
		private readonly Type defaultImplementationType;
		private Dictionary<string, GeneratedMemberMetadata> membersByName = null;
		private Dictionary<string, GeneratedMethodMetadata[]> methodsByName = null;

		/// <summary>
		/// Generated type metadata.
		/// </summary>
		/// <param name="Type">Type being described.</param>
		/// <param name="Namespace">Namespace override.</param>
		/// <param name="BaseType">Base type.</param>
		/// <param name="ImplementedInterfaces">Implemented interfaces.</param>
		/// <param name="TypeAliases">Type aliases.</param>
		/// <param name="TypeAttributes">Generated type attributes.</param>
		/// <param name="Members">Generated members.</param>
		/// <param name="Methods">Generated methods.</param>
		/// <param name="Constructors">Generated constructors.</param>
		/// <param name="ModuleDependencies">Module dependency full names.</param>
		/// <param name="HasSingletonAttribute">If the type has a singleton attribute.</param>
		/// <param name="DefaultImplementationType">Default implementation type.</param>
		public GeneratedTypeMetadata(Type Type, string Namespace = null, Type BaseType = null,
			Type[] ImplementedInterfaces = null, string[] TypeAliases = null, object[] TypeAttributes = null,
			GeneratedMemberMetadata[] Members = null, GeneratedMethodMetadata[] Methods = null,
			GeneratedConstructorMetadata[] Constructors = null,
			string[] ModuleDependencies = null, bool HasSingletonAttribute = false, Type DefaultImplementationType = null)
		{
			this.type = Type ?? throw new ArgumentNullException(nameof(Type));
			this.@namespace = Namespace;
			this.baseType = BaseType;
			this.implementedInterfaces = ImplementedInterfaces ?? Types.NoTypes;
			this.typeAliases = TypeAliases ?? Array.Empty<string>();
			this.typeAttributes = TypeAttributes ?? Array.Empty<object>();
			this.members = Members ?? Array.Empty<GeneratedMemberMetadata>();
			this.methods = Methods ?? Array.Empty<GeneratedMethodMetadata>();
			this.constructors = Constructors ?? Array.Empty<GeneratedConstructorMetadata>();
			this.moduleDependencies = ModuleDependencies ?? Array.Empty<string>();
			this.hasSingletonAttribute = HasSingletonAttribute;
			this.defaultImplementationType = DefaultImplementationType;
		}

		/// <summary>
		/// Type being described.
		/// </summary>
		public Type Type => this.type;

		/// <summary>
		/// Namespace override.
		/// </summary>
		public string Namespace => this.@namespace;

		/// <summary>
		/// Base type.
		/// </summary>
		public Type BaseType => this.baseType;

		/// <summary>
		/// Implemented interfaces.
		/// </summary>
		public Type[] ImplementedInterfaces => this.implementedInterfaces;

		/// <summary>
		/// Type aliases.
		/// </summary>
		public string[] TypeAliases => this.typeAliases;

		/// <summary>
		/// Type attributes.
		/// </summary>
		public object[] TypeAttributes => this.typeAttributes;

		/// <summary>
		/// Generated members.
		/// </summary>
		public GeneratedMemberMetadata[] Members => this.members;

		/// <summary>
		/// Generated methods.
		/// </summary>
		public GeneratedMethodMetadata[] Methods => this.methods;

		/// <summary>
		/// Generated constructors.
		/// </summary>
		public GeneratedConstructorMetadata[] Constructors => this.constructors;

		/// <summary>
		/// Module dependency full names.
		/// </summary>
		public string[] ModuleDependencies => this.moduleDependencies;

		/// <summary>
		/// If the type has a singleton attribute.
		/// </summary>
		public bool HasSingletonAttribute => this.hasSingletonAttribute;

		/// <summary>
		/// Default implementation type.
		/// </summary>
		public Type DefaultImplementationType => this.defaultImplementationType;

		/// <summary>
		/// Tries to get generated metadata for a member.
		/// </summary>
		public bool TryGetMember(string Name, out GeneratedMemberMetadata Member)
		{
			if (this.membersByName is null)
			{
				Dictionary<string, GeneratedMemberMetadata> MembersByName = new Dictionary<string, GeneratedMemberMetadata>(StringComparer.Ordinal);

				foreach (GeneratedMemberMetadata Item in this.members)
					MembersByName[Item.Name] = Item;

				this.membersByName = MembersByName;
			}

			return this.membersByName.TryGetValue(Name, out Member);
		}

		internal bool TryGetAttribute<T>(out T Attribute)
			where T : class
		{
			foreach (object Obj in this.typeAttributes)
			{
				if (Obj is T Match)
				{
					Attribute = Match;
					return true;
				}
			}

			Attribute = null;
			return false;
		}

		/// <summary>
		/// Tries to get generated metadata for a method.
		/// </summary>
		public bool TryGetMethod(string Name, Type[] ParameterTypes, out GeneratedMethodMetadata Method)
		{
			if (this.methodsByName is null)
			{
				Dictionary<string, List<GeneratedMethodMetadata>> MethodsByName = new Dictionary<string, List<GeneratedMethodMetadata>>(StringComparer.Ordinal);

				foreach (GeneratedMethodMetadata Item in this.methods)
				{
					if (!MethodsByName.TryGetValue(Item.Name, out List<GeneratedMethodMetadata> Methods))
					{
						Methods = new List<GeneratedMethodMetadata>();
						MethodsByName[Item.Name] = Methods;
					}

					Methods.Add(Item);
				}

				Dictionary<string, GeneratedMethodMetadata[]> Cached = new Dictionary<string, GeneratedMethodMetadata[]>(StringComparer.Ordinal);

				foreach (KeyValuePair<string, List<GeneratedMethodMetadata>> P in MethodsByName)
					Cached[P.Key] = P.Value.ToArray();

				this.methodsByName = Cached;
			}

			if (this.methodsByName.TryGetValue(Name, out GeneratedMethodMetadata[] Methods2))
			{
				foreach (GeneratedMethodMetadata Candidate in Methods2)
				{
					if (SameTypes(Candidate.ParameterTypes, ParameterTypes))
					{
						Method = Candidate;
						return true;
					}
				}
			}

			Method = null;
			return false;
		}

		private static bool SameTypes(Type[] A, Type[] B)
		{
			if (A is null)
				A = Types.NoTypes;

			if (B is null)
				B = Types.NoTypes;

			if (A.Length != B.Length)
				return false;

			for (int i = 0; i < A.Length; i++)
			{
				if (A[i] != B[i])
					return false;
			}

			return true;
		}
	}
}
