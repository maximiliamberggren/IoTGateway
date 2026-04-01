using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Persistence.Attributes;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.MongoDB.Serialization
{
	internal sealed class MetadataObjectSerializer : IObjectSerializer
	{
		private readonly MongoDBProvider provider;
		private readonly Type type;
		private readonly TypeInfo typeInfo;
		private readonly SerializerTypeDescriptor descriptor;
		private readonly bool isNullable;
		private readonly Dictionary<string, SerializerMemberDescriptor> membersByName;
		private readonly Dictionary<string, SerializerMemberDescriptor> membersBySerializedName;

		public MetadataObjectSerializer(MongoDBProvider Provider, Type Type, SerializerTypeDescriptor Descriptor, bool IsNullable)
		{
			this.provider = Provider;
			this.type = Type;
			this.typeInfo = Type.GetTypeInfo();
			this.descriptor = Descriptor;
			this.isNullable = IsNullable;
			this.membersByName = new Dictionary<string, SerializerMemberDescriptor>(StringComparer.Ordinal);
			this.membersBySerializedName = new Dictionary<string, SerializerMemberDescriptor>(StringComparer.Ordinal);

			foreach (SerializerMemberDescriptor Member in Descriptor.Members)
			{
				this.membersByName[Member.Name] = Member;
				this.membersBySerializedName[Member.SerializedName] = Member;
			}
		}

		public Type ValueType => this.type;

		public bool IsNullable => this.isNullable;

		public object Deserialize(IBsonReader Reader, BsonType? DataType, bool Embedded)
		{
			BsonReaderBookmark Bookmark = Reader.GetBookmark();
			BsonType? DataTypeBak = DataType;
			Dictionary<string, object> Obsolete = null;

			if (!DataType.HasValue)
			{
				DataType = Reader.ReadBsonType();
				switch (DataType.Value)
				{
					case BsonType.Null:
						Reader.ReadNull();
						return null;

					case BsonType.Document:
						break;

					default:
						throw new Exception("Expected object document or null.");
				}
			}

			if (this.descriptor.TypeNameSerialization != TypeNameSerialization.None)
			{
				Reader.ReadStartDocument();
				if (!Reader.FindElement(this.descriptor.TypeFieldName))
					throw new Exception("Type name not available.");

				string TypeName = Reader.ReadString();
				if (this.descriptor.TypeNameSerialization == TypeNameSerialization.LocalName && TypeName.IndexOf('.') < 0)
					TypeName = this.type.Namespace + "." + TypeName;

				Type DesiredType = Types.GetType(TypeName) ?? typeof(GenericObject);
				Reader.ReturnToBookmark(Bookmark);

				if (DesiredType != this.type)
				{
					IObjectSerializer Serializer2 = this.provider.GetObjectSerializer(DesiredType);
					return Serializer2.Deserialize(Reader, DataTypeBak, Embedded);
				}
			}

			if (this.typeInfo.IsAbstract)
				throw new Exception("Unable to create an instance of an abstract class.");

			object Result = Types.Create(false, this.type);
			Reader.ReadStartDocument();

			while (Reader.State == BsonReaderState.Type)
			{
				BsonType FieldType = Reader.ReadBsonType();
				if (FieldType == BsonType.EndOfDocument)
					break;

				string FieldName = Reader.ReadName();

				if (FieldName == "_id")
				{
					if (TryGetObjectIdMember(out SerializerMemberDescriptor ObjectIdMember))
					{
						ObjectId ObjectId = ReadObjectId(Reader, FieldType);
						ObjectIdMember.Setter(Result, ConvertObjectId(ObjectId, ObjectIdMember.MemberType));
					}
					else
					{
						if (FieldType == BsonType.ObjectId)
							Reader.ReadObjectId();
						else
							throw new Exception("Object ID parameter _id must be an Object ID value, but was a " + FieldType + ".");
					}

					continue;
				}

				if (FieldName == this.descriptor.TypeFieldName)
				{
					if (FieldType == BsonType.String)
						Reader.ReadString();
					else
						throw new Exception("Type parameter " + this.descriptor.TypeFieldName +
							" must be a string value, but was a " + FieldType + ".");

					continue;
				}

				if (TryGetMember(FieldName, out SerializerMemberDescriptor Member))
				{
					if (FieldName.EndsWith("_L", StringComparison.Ordinal) && Member.IsCaseInsensitiveString)
					{
						Reader.SkipValue();
						continue;
					}

					object Value = this.ReadMemberValue(Member, Reader, FieldType, Embedded);
					Member.Setter(Result, Value);
				}
				else
				{
					if (Obsolete is null)
						Obsolete = new Dictionary<string, object>(StringComparer.Ordinal);

					Obsolete[FieldName] = ReadUntypedValue(this.provider, Reader, FieldType);
				}
			}

			Reader.ReadEndDocument();

			if (!(this.descriptor.ObsoleteMethodInvoker is null) && !(Obsolete is null))
			{
				object InvocationResult = this.descriptor.ObsoleteMethodInvoker(Result, new object[] { Obsolete });
				if (this.descriptor.ObsoleteMethodReturnType == typeof(System.Threading.Tasks.Task) &&
					InvocationResult is System.Threading.Tasks.Task Task &&
					!Task.Wait(10000))
				{
					throw new Exception("Obsolete method timed out.");
				}
			}

			return Result;
		}

		public void Serialize(IBsonWriter Writer, bool WriteTypeCode, bool Embedded, object Value)
		{
			if (Value is null)
			{
				if (!WriteTypeCode)
					throw new NullReferenceException("Value cannot be null.");

				Writer.WriteNull();
				return;
			}

			Type RuntimeType = Value.GetType();
			if (RuntimeType != this.type)
			{
				IObjectSerializer Serializer = this.provider.GetObjectSerializer(RuntimeType);
				Serializer.Serialize(Writer, WriteTypeCode, Embedded, Value);
				return;
			}

			Writer.WriteStartDocument();

			switch (this.descriptor.TypeNameSerialization)
			{
				case TypeNameSerialization.LocalName:
					Writer.WriteName(this.descriptor.TypeFieldName);
					Writer.WriteString(this.type.Name);
					break;

				case TypeNameSerialization.FullName:
					Writer.WriteName(this.descriptor.TypeFieldName);
					Writer.WriteString(this.type.FullName);
					break;
			}

			foreach (SerializerMemberDescriptor Member in this.descriptor.Members)
			{
				object FieldValue = Member.Getter(Value);
				if (Member.HasDefaultValue && AreEqual(FieldValue, Member.DefaultValue))
					continue;

				if (Member.ObjectId)
				{
					if (!(FieldValue is null))
					{
						Writer.WriteName("_id");
						Writer.WriteObjectId(ConvertToObjectId(FieldValue, Member.MemberType));
					}

					continue;
				}

				Writer.WriteName(Member.SerializedName);
				this.WriteMemberValue(Writer, Member, FieldValue, Embedded);

				if (Member.IsCaseInsensitiveString && FieldValue is CaseInsensitiveString Cis)
				{
					Writer.WriteName(Member.SerializedName + "_L");
					Writer.WriteString(Cis.LowerCase);
				}
			}

			Writer.WriteEndDocument();
		}

		public bool TryGetFieldValue(string FieldName, object Object, out object Value)
		{
			if (this.membersByName.TryGetValue(FieldName, out SerializerMemberDescriptor Member))
			{
				Value = Member.Getter(Object);
				return true;
			}

			Value = null;
			return false;
		}

		public bool TryGetFieldType(string FieldName, object Object, out Type FieldType)
		{
			if (this.membersByName.TryGetValue(FieldName, out SerializerMemberDescriptor Member))
			{
				FieldType = Member.MemberType;
				return true;
			}

			FieldType = null;
			return false;
		}

		private bool TryGetMember(string FieldName, out SerializerMemberDescriptor Member)
		{
			if (this.membersBySerializedName.TryGetValue(FieldName, out Member))
				return true;

			if (FieldName.EndsWith("_L", StringComparison.Ordinal) &&
				this.membersBySerializedName.TryGetValue(FieldName.Substring(0, FieldName.Length - 2), out Member))
			{
				return true;
			}

			return false;
		}

		private bool TryGetObjectIdMember(out SerializerMemberDescriptor Member)
		{
			foreach (SerializerMemberDescriptor Item in this.descriptor.Members)
			{
				if (Item.ObjectId)
				{
					Member = Item;
					return true;
				}
			}

			Member = null;
			return false;
		}

		private object ReadMemberValue(SerializerMemberDescriptor Member, IBsonReader Reader, BsonType FieldType, bool Embedded)
		{
			if (Member.ByReference)
			{
				switch (FieldType)
				{
					case BsonType.ObjectId:
						ObjectId ObjectId = Reader.ReadObjectId();
						System.Threading.Tasks.Task<object> Task = this.provider.TryLoadObject(Member.MemberType, ObjectId);
						if (!Task.Wait(10000))
							throw new Exception("Unable to load referenced object. Database timed out.");

						return Task.Result;

					case BsonType.Null:
						Reader.ReadNull();
						return null;

					default:
						throw new Exception("Object ID expected for " + Member.Name + ".");
				}
			}

			if (Member.MemberType.IsArray)
				return GeneratedObjectSerializerBase.ReadArray(Member.MemberType.GetElementType(), this.provider, Reader, FieldType);

			if (Member.MemberType == typeof(Guid))
			{
				switch (FieldType)
				{
					case BsonType.String:
						return Guid.Parse(Reader.ReadString());

					case BsonType.ObjectId:
						return GeneratedObjectSerializerBase.ObjectIdToGuid(Reader.ReadObjectId());

					case BsonType.Null:
						Reader.ReadNull();
						return null;
				}
			}

			if (Member.MemberType == typeof(Guid?))
			{
				switch (FieldType)
				{
					case BsonType.String:
						return (Guid?)Guid.Parse(Reader.ReadString());

					case BsonType.ObjectId:
						return (Guid?)GeneratedObjectSerializerBase.ObjectIdToGuid(Reader.ReadObjectId());

					case BsonType.Null:
						Reader.ReadNull();
						return null;
				}
			}

			IObjectSerializer Serializer = this.provider.GetObjectSerializer(Member.MemberType);
			return Serializer.Deserialize(Reader, FieldType, Embedded);
		}

		private void WriteMemberValue(IBsonWriter Writer, SerializerMemberDescriptor Member, object Value, bool Embedded)
		{
			if (Value is null)
			{
				Writer.WriteNull();
				return;
			}

			if (Member.ByReference)
			{
				ObjectSerializer Serializer = this.provider.GetObjectSerializerEx(Member.MemberType);
				Writer.WriteObjectId(Serializer.GetObjectId(Value, true).GetAwaiter().GetResult());
				return;
			}

			if (Member.MemberType == typeof(Guid))
			{
				Guid Guid = (Guid)Value;
				if (GeneratedObjectSerializerBase.TryConvertToObjectId(Guid, out ObjectId ObjectId))
					Writer.WriteObjectId(ObjectId);
				else
					Writer.WriteString(Guid.ToString());

				return;
			}

			if (Member.MemberType == typeof(Guid?))
			{
				Guid? Guid = (Guid?)Value;
				if (!Guid.HasValue)
					Writer.WriteNull();
				else if (GeneratedObjectSerializerBase.TryConvertToObjectId(Guid.Value, out ObjectId ObjectId))
					Writer.WriteObjectId(ObjectId);
				else
					Writer.WriteString(Guid.Value.ToString());

				return;
			}

			if (Member.MemberType.IsArray)
			{
				GeneratedObjectSerializerBase.WriteArray(Member.MemberType.GetElementType(), this.provider, (BsonWriter)Writer, (Array)Value);
				return;
			}

			IObjectSerializer Serializer2 = this.provider.GetObjectSerializer(Value.GetType());
			Serializer2.Serialize(Writer, true, Embedded, Value);
		}

		private static ObjectId ReadObjectId(IBsonReader Reader, BsonType FieldType)
		{
			if (FieldType != BsonType.ObjectId)
				throw new Exception("Object ID parameter _id must be an Object ID value, but was a " + FieldType + ".");

			return Reader.ReadObjectId();
		}

		private static object ConvertObjectId(ObjectId ObjectId, Type MemberType)
		{
			if (MemberType == typeof(ObjectId))
				return ObjectId;
			if (MemberType == typeof(string))
				return ObjectId.ToString();
			if (MemberType == typeof(byte[]))
				return ObjectId.ToByteArray();
			if (MemberType == typeof(Guid))
				return GeneratedObjectSerializerBase.ObjectIdToGuid(ObjectId);

			throw new Exception("Invalid Object ID type.");
		}

		private static ObjectId ConvertToObjectId(object Value, Type MemberType)
		{
			if (MemberType == typeof(ObjectId))
				return (ObjectId)Value;
			if (MemberType == typeof(string))
				return new ObjectId((string)Value);
			if (MemberType == typeof(byte[]))
				return new ObjectId((byte[])Value);
			if (MemberType == typeof(Guid))
				return GeneratedObjectSerializerBase.GuidToObjectId((Guid)Value);

			throw new Exception("Invalid Object ID type.");
		}

		private static object ReadUntypedValue(MongoDBProvider Provider, IBsonReader Reader, BsonType FieldType)
		{
			switch (FieldType)
			{
				case BsonType.Array:
					return GeneratedObjectSerializerBase.ReadArray(null, Provider, Reader, FieldType);

				case BsonType.Binary:
					return Reader.ReadBytes();

				case BsonType.Boolean:
					return Reader.ReadBoolean();

				case BsonType.DateTime:
					return ObjectSerializer.UnixEpoch.AddMilliseconds(Reader.ReadDateTime());

				case BsonType.Decimal128:
					return (decimal)Reader.ReadDecimal128();

				case BsonType.Document:
					return GeneratedObjectSerializerBase.ReadEmbeddedObject(Provider, Reader, FieldType);

				case BsonType.Double:
					return Reader.ReadDouble();

				case BsonType.Int32:
					return Reader.ReadInt32();

				case BsonType.Int64:
					return Reader.ReadInt64();

				case BsonType.JavaScript:
					return Reader.ReadJavaScript();

				case BsonType.JavaScriptWithScope:
					return Reader.ReadJavaScriptWithScope();

				case BsonType.Null:
					Reader.ReadNull();
					return null;

				case BsonType.ObjectId:
					return Reader.ReadObjectId().ToString();

				case BsonType.String:
					return Reader.ReadString();

				case BsonType.Symbol:
					return Reader.ReadSymbol();

				default:
					Reader.SkipValue();
					return null;
			}
		}

		private static bool AreEqual(object Value, object DefaultValue)
		{
			if ((Value is null) ^ (DefaultValue is null))
				return false;

			if (Value is null)
				return true;

			return DefaultValue.Equals(Value);
		}
	}
}
