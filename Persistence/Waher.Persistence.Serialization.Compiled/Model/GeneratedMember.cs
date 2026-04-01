using System;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Serialization.Model
{
	/// <summary>
	/// Metadata-backed member.
	/// </summary>
	public class GeneratedMember : Member
	{
		private readonly GeneratedMemberGetter getter;
		private readonly GeneratedMemberSetter setter;

		/// <summary>
		/// Metadata-backed member.
		/// </summary>
		public GeneratedMember(string Name, ulong FieldCode, Type MemberType, bool Encrypted, int DecryptedMinLength,
			GeneratedMemberGetter Getter, GeneratedMemberSetter Setter)
			: base(Name, FieldCode, MemberType, Encrypted, DecryptedMinLength)
		{
			this.getter = Getter;
			this.setter = Setter;
		}

		/// <inheritdoc/>
		public override object Get(object Object)
		{
			return this.getter(Object);
		}

		/// <inheritdoc/>
		public override void Set(object Object, object Value)
		{
			this.setter(Object, Value);
		}
	}
}
