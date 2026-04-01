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
		private readonly GeneratedConstructorMetadata[] constructors;
		private readonly string[] moduleDependencies;
		private readonly bool hasSingletonAttribute;
		private readonly Type defaultImplementationType;
		private Dictionary<string, GeneratedMemberMetadata> membersByName = null;

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
		/// <param name="Constructors">Generated constructors.</param>
		/// <param name="ModuleDependencies">Module dependency full names.</param>
		/// <param name="HasSingletonAttribute">If the type has a singleton attribute.</param>
		/// <param name="DefaultImplementationType">Default implementation type.</param>
		public GeneratedTypeMetadata(Type Type, string Namespace = null, Type BaseType = null,
			Type[] ImplementedInterfaces = null, string[] TypeAliases = null, object[] TypeAttributes = null,
			GeneratedMemberMetadata[] Members = null, GeneratedConstructorMetadata[] Constructors = null,
			string[] ModuleDependencies = null, bool HasSingletonAttribute = false, Type DefaultImplementationType = null)
		{
			this.type = Type ?? throw new ArgumentNullException(nameof(Type));
			this.@namespace = Namespace;
			this.baseType = BaseType;
			this.implementedInterfaces = ImplementedInterfaces ?? Types.NoTypes;
			this.typeAliases = TypeAliases ?? Array.Empty<string>();
			this.typeAttributes = TypeAttributes ?? Array.Empty<object>();
			this.members = Members ?? Array.Empty<GeneratedMemberMetadata>();
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

		internal bool TryGetMember(string Name, out GeneratedMemberMetadata Member)
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
	}
}
