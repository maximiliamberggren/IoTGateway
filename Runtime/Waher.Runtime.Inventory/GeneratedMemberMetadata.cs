using System;
using System.Collections.Generic;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Delegate for generated member getters.
	/// </summary>
	/// <param name="Object">Object instance.</param>
	/// <returns>Member value.</returns>
	public delegate object GeneratedMemberGetter(object Object);

	/// <summary>
	/// Delegate for generated member setters.
	/// </summary>
	/// <param name="Object">Object instance.</param>
	/// <param name="Value">New value.</param>
	public delegate void GeneratedMemberSetter(object Object, object Value);

	/// <summary>
	/// Generated member metadata.
	/// </summary>
	public sealed class GeneratedMemberMetadata
	{
		private readonly string name;
		private readonly Type memberType;
		private readonly bool canRead;
		private readonly bool canWrite;
		private readonly bool isPublic;
		private readonly bool isField;
		private readonly GeneratedMemberGetter getter;
		private readonly GeneratedMemberSetter setter;
		private readonly object[] attributes;

		/// <summary>
		/// Generated member metadata.
		/// </summary>
		/// <param name="Name">Member name.</param>
		/// <param name="MemberType">Member type.</param>
		/// <param name="CanRead">If the member can be read.</param>
		/// <param name="CanWrite">If the member can be written.</param>
		/// <param name="IsPublic">If the member is public.</param>
		/// <param name="IsField">If the member is a field.</param>
		/// <param name="Getter">Generated getter.</param>
		/// <param name="Setter">Generated setter.</param>
		/// <param name="Attributes">Generated attributes.</param>
		public GeneratedMemberMetadata(string Name, Type MemberType, bool CanRead, bool CanWrite, bool IsPublic,
			bool IsField, GeneratedMemberGetter Getter, GeneratedMemberSetter Setter, object[] Attributes = null)
		{
			this.name = Name ?? throw new ArgumentNullException(nameof(Name));
			this.memberType = MemberType ?? throw new ArgumentNullException(nameof(MemberType));
			this.canRead = CanRead;
			this.canWrite = CanWrite;
			this.isPublic = IsPublic;
			this.isField = IsField;
			this.getter = Getter;
			this.setter = Setter;
			this.attributes = Attributes ?? Array.Empty<object>();
		}

		/// <summary>
		/// Member name.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Member type.
		/// </summary>
		public Type MemberType => this.memberType;

		/// <summary>
		/// If the member can be read.
		/// </summary>
		public bool CanRead => this.canRead;

		/// <summary>
		/// If the member can be written.
		/// </summary>
		public bool CanWrite => this.canWrite;

		/// <summary>
		/// If the member is public.
		/// </summary>
		public bool IsPublic => this.isPublic;

		/// <summary>
		/// If the member is a field.
		/// </summary>
		public bool IsField => this.isField;

		/// <summary>
		/// Generated getter.
		/// </summary>
		public GeneratedMemberGetter Getter => this.getter;

		/// <summary>
		/// Generated setter.
		/// </summary>
		public GeneratedMemberSetter Setter => this.setter;

		/// <summary>
		/// Generated attributes.
		/// </summary>
		public object[] Attributes => this.attributes;

		internal bool TryGetAttribute<T>(out T Attribute)
			where T : class
		{
			foreach (object Obj in this.attributes)
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
