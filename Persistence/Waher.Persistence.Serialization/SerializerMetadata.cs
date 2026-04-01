using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;
using Waher.Persistence.Exceptions;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.Serialization
{
	internal sealed class SerializerTypeDescriptor
	{
		public SerializerTypeDescriptor(Type Type)
		{
			this.Type = Type;
		}

		public Type Type { get; }
		public string CollectionName { get; set; }
		public bool BackupCollection { get; set; } = true;
		public string NoBackupReason { get; set; }
		public TypeNameSerialization TypeNameSerialization { get; set; } = TypeNameSerialization.FullName;
		public string TypeFieldName { get; set; } = "_type";
		public bool Archive { get; set; }
		public bool ArchiveDynamic { get; set; }
		public int ArchiveDays { get; set; }
		public GeneratedMemberGetter ArchiveGetter { get; set; }
		public string ArchiveMemberName { get; set; }
		public GeneratedMethodInvoker ObsoleteMethodInvoker { get; set; }
		public Type ObsoleteMethodReturnType { get; set; }
		public string[][] Indices { get; set; } = Array.Empty<string[]>();
		public SerializerMemberDescriptor[] Members { get; set; } = Array.Empty<SerializerMemberDescriptor>();

		public static bool TryCreate(Type Type, out SerializerTypeDescriptor Descriptor)
		{
			if (!Types.TryGetGeneratedMetadata(Type, out GeneratedTypeMetadata Metadata) ||
				!TryGetAttribute(Metadata.TypeAttributes, out GenerateRuntimeMetadataAttribute _))
			{
				Descriptor = null;
				return false;
			}

			List<GeneratedTypeMetadata> Chain = new List<GeneratedTypeMetadata>();
			GeneratedTypeMetadata Current = Metadata;

			while (!(Current is null))
			{
				Chain.Add(Current);

				if (Current.BaseType is null || !Types.TryGetGeneratedMetadata(Current.BaseType, out Current))
					break;
			}

			Descriptor = new SerializerTypeDescriptor(Type);

			foreach (GeneratedTypeMetadata Item in Chain)
			{
				if (Descriptor.CollectionName is null && TryGetAttribute(Item.TypeAttributes, out CollectionNameAttribute CollectionNameAttribute))
					Descriptor.CollectionName = CollectionNameAttribute.Name;

				if (TryGetAttribute(Item.TypeAttributes, out NoBackupAttribute NoBackupAttribute))
				{
					Descriptor.BackupCollection = false;
					Descriptor.NoBackupReason = NoBackupAttribute.Reason;
				}

				if (TryGetAttribute(Item.TypeAttributes, out TypeNameAttribute TypeNameAttribute))
				{
					Descriptor.TypeNameSerialization = TypeNameAttribute.TypeNameSerialization;
					Descriptor.TypeFieldName = TypeNameAttribute.FieldName;
				}

				if (TryGetAttribute(Item.TypeAttributes, out ArchivingTimeAttribute ArchivingTimeAttribute))
				{
					Descriptor.Archive = true;
					if (!string.IsNullOrEmpty(ArchivingTimeAttribute.PropertyName))
					{
						if (!TryFindMember(Chain, ArchivingTimeAttribute.PropertyName, out GeneratedMemberMetadata ArchiveMember) ||
							ArchiveMember.MemberType != typeof(int) || ArchiveMember.Getter is null)
						{
							throw new SerializationException("Archiving time property or field not found: " +
								ArchivingTimeAttribute.PropertyName, Type);
						}

						Descriptor.ArchiveDynamic = true;
						Descriptor.ArchiveMemberName = ArchivingTimeAttribute.PropertyName;
						Descriptor.ArchiveGetter = ArchiveMember.Getter;
					}
					else
						Descriptor.ArchiveDays = ArchivingTimeAttribute.Days;
				}

				if (TryGetAttribute(Item.TypeAttributes, out ObsoleteMethodAttribute ObsoleteMethodAttribute) &&
					Item.TryGetMethod(ObsoleteMethodAttribute.MethodName, new Type[] { typeof(Dictionary<string, object>) },
						out GeneratedMethodMetadata Method))
				{
					Descriptor.ObsoleteMethodInvoker = Method.Invoker;
					Descriptor.ObsoleteMethodReturnType = Method.ReturnType;
				}
			}

			List<string[]> Indices = new List<string[]>();
			foreach (GeneratedTypeMetadata Item in Chain)
			{
				foreach (object Attribute in Item.TypeAttributes)
				{
					if (Attribute is IndexAttribute IndexAttribute)
						Indices.Add(IndexAttribute.FieldNames);
				}
			}

			Descriptor.Indices = Indices.Count == 0 ? Array.Empty<string[]>() : Indices.ToArray();

			HashSet<string> Names = new HashSet<string>(StringComparer.Ordinal);
			List<SerializerMemberDescriptor> Members = new List<SerializerMemberDescriptor>();

			foreach (GeneratedTypeMetadata Item in Chain)
			{
				foreach (GeneratedMemberMetadata Member in Item.Members)
				{
					if (!Names.Add(Member.Name))
						continue;

					Members.Add(new SerializerMemberDescriptor(Member));
				}
			}

			Descriptor.Members = Members.ToArray();
			return true;
		}

		private static bool TryFindMember(IEnumerable<GeneratedTypeMetadata> Chain, string Name, out GeneratedMemberMetadata Member)
		{
			foreach (GeneratedTypeMetadata Metadata in Chain)
			{
				if (Metadata.TryGetMember(Name, out Member))
					return true;
			}

			Member = null;
			return false;
		}

		internal static bool TryGetAttribute<T>(IEnumerable<object> Attributes, out T Attribute)
			where T : class
		{
			foreach (object Obj in Attributes)
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

	internal sealed class SerializerMemberDescriptor
	{
		public SerializerMemberDescriptor(GeneratedMemberMetadata Metadata)
		{
			this.Metadata = Metadata;
			this.Name = Metadata.Name;
			this.MemberType = Metadata.MemberType;
			this.CanRead = Metadata.CanRead;
			this.CanWrite = Metadata.CanWrite;
			this.IsPublic = Metadata.IsPublic;
			this.IsField = Metadata.IsField;
			this.Getter = Metadata.Getter;
			this.Setter = Metadata.Setter;

			foreach (object Attribute in Metadata.Attributes)
			{
				switch (Attribute)
				{
					case IgnoreMemberAttribute _:
						this.Ignore = true;
						break;

					case DefaultValueAttribute DefaultValueAttribute:
						this.HasDefaultValue = true;
						this.DefaultValue = DefaultValueAttribute.Value;
						break;

					case ByReferenceAttribute _:
						this.ByReference = true;
						break;

					case ObjectIdAttribute _:
						this.ObjectId = true;
						break;

					case ShortNameAttribute ShortNameAttribute:
						this.ShortName = ShortNameAttribute.Name;
						break;

					case EncryptedAttribute EncryptedAttribute:
						this.Encrypted = true;
						this.DecryptedMinLength = EncryptedAttribute.MinLength;
						break;
				}
			}
		}

		public GeneratedMemberMetadata Metadata { get; }
		public string Name { get; }
		public Type MemberType { get; }
		public bool CanRead { get; }
		public bool CanWrite { get; }
		public bool IsPublic { get; }
		public bool IsField { get; }
		public GeneratedMemberGetter Getter { get; }
		public GeneratedMemberSetter Setter { get; }
		public bool Ignore { get; }
		public bool HasDefaultValue { get; }
		public object DefaultValue { get; }
		public bool ByReference { get; }
		public bool ObjectId { get; }
		public string ShortName { get; }
		public bool Encrypted { get; }
		public int DecryptedMinLength { get; }
		public bool ReturnsTask => this.ObsoleteMethodReturnType == typeof(Task);
		public Type ObsoleteMethodReturnType { get; set; }
	}
}
