using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using Waher.Persistence.Attributes;
using Waher.Persistence.MongoDB.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.MongoDB.Test
{
	[TestClass]
	public class MongoAotMetadataTests
	{
		[TestMethod]
		public void MongoDB_AOT_MetadataSerializer_RoundtripsDocument()
		{
			Types.Initialize(typeof(MongoDBProvider).Assembly, typeof(MongoAotMetadataTests).Assembly, typeof(Types).Assembly);

			MongoDBProvider Provider = new MongoDBProvider("MongoAotMetadataTest", "Default");
			ObjectSerializer Serializer = Provider.GetObjectSerializerEx(typeof(MetadataMongoDocument));
			MetadataMongoDocument Obj = new MetadataMongoDocument()
			{
				ObjectId = ObjectId.GenerateNewId().ToString(),
				Name = "Alpha",
				Key = "MixedCase",
				ArchiveDays = 37
			};

			BsonDocument Doc = Obj.ToBsonDocument(typeof(MetadataMongoDocument), Serializer);

			Assert.AreEqual(Obj.ObjectId, Doc["_id"].AsObjectId.ToString());
			Assert.AreEqual(Obj.Name, Doc["n"].AsString);
			Assert.AreEqual(Obj.Key.Value, Doc["cis"].AsString);
			Assert.AreEqual(Obj.Key.LowerCase, Doc["cis_L"].AsString);
			Assert.IsFalse(Doc.Contains("Count"));
			Assert.AreEqual(37, Serializer.GetArchivingTimeDays(Obj));

			BsonDocumentReader Reader = new BsonDocumentReader(Doc);
			MetadataMongoDocument Obj2 = (MetadataMongoDocument)Serializer.Deserialize(Reader, null, false);

			Assert.AreEqual(Obj.ObjectId, Obj2.ObjectId);
			Assert.AreEqual(Obj.Name, Obj2.Name);
			Assert.AreEqual(Obj.Key, Obj2.Key);
			Assert.AreEqual(Obj.ArchiveDays, Obj2.ArchiveDays);
			Assert.AreEqual(5, Obj2.Count);
		}

		[TestMethod]
		public void MongoDB_AOT_MetadataSerializer_DispatchesDerivedType()
		{
			Types.Initialize(typeof(MongoDBProvider).Assembly, typeof(MongoAotMetadataTests).Assembly, typeof(Types).Assembly);

			MongoDBProvider Provider = new MongoDBProvider("MongoAotMetadataTest", "Default");
			ObjectSerializer Serializer = Provider.GetObjectSerializerEx(typeof(MetadataMongoBase));
			MetadataMongoDerived Obj = new MetadataMongoDerived()
			{
				ObjectId = ObjectId.GenerateNewId().ToString(),
				Name = "Beta",
				Level = 9
			};

			BsonDocument Doc = Obj.ToBsonDocument(typeof(MetadataMongoBase), Serializer);
			Assert.AreEqual(typeof(MetadataMongoDerived).FullName, Doc["_type"].AsString);

			BsonDocumentReader Reader = new BsonDocumentReader(Doc);
			object Obj2 = Serializer.Deserialize(Reader, null, false);

			Assert.IsInstanceOfType<MetadataMongoDerived>(Obj2);
			Assert.AreEqual(Obj.Name, ((MetadataMongoDerived)Obj2).Name);
			Assert.AreEqual(Obj.Level, ((MetadataMongoDerived)Obj2).Level);
		}

		[GenerateRuntimeMetadata]
		[InventoryDiscoverable]
		[ArchivingTime(nameof(ArchiveDays))]
		public sealed class MetadataMongoDocument
		{
			[ObjectId]
			public string ObjectId { get; set; }

			[ShortName("n")]
			public string Name { get; set; }

			[ShortName("cis")]
			public CaseInsensitiveString Key { get; set; }

			[DefaultValue(5)]
			public int Count { get; set; } = 5;

			public int ArchiveDays { get; set; }
		}

		[GenerateRuntimeMetadata]
		[InventoryDiscoverable]
		public abstract class MetadataMongoBase
		{
			[ObjectId]
			public string ObjectId { get; set; }

			public string Name { get; set; }
		}

		[GenerateRuntimeMetadata]
		[InventoryDiscoverable]
		public sealed class MetadataMongoDerived : MetadataMongoBase
		{
			public int Level { get; set; }
		}
	}
}
