using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Serialization;

#if !LW
using Waher.Persistence.Files.Test.Classes;

namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
using Waher.Persistence.FilesLW.Test.Classes;

namespace Waher.Persistence.FilesLW.Test
#endif
{
	[TestClass]
	public class DBFilesAotMetadataTests
	{
		private static FilesProvider provider;

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext Context)
		{
			DBFilesBTreeTests.DeleteFiles();

#if LW
			provider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000);
#else
			provider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true, true);
#endif
			await provider.GetFile("Default");
		}

		[ClassCleanup]
		public static async Task ClassCleanup()
		{
			await provider.DisposeAsync();
			provider = null;
		}

		[TestMethod]
		public async Task DBFiles_ObjSerialization_AOT_ArchivingTimeMetadata()
		{
			ArchivingTimeModel Obj = new()
			{
				Days = 37,
				Value = "abc"
			};

			ObjectSerializer Serializer = (ObjectSerializer)await provider.GetObjectSerializer(typeof(ArchivingTimeModel));
			Assert.AreEqual(37, Serializer.GetArchivingTimeDays(Obj));

			DebugSerializer Writer = new(new BinarySerializer(provider.DefaultCollectionName, Encoding.UTF8), null);
			await Serializer.Serialize(Writer, false, false, Obj, null);

			byte[] Data = Writer.GetSerialization();
			DebugDeserializer Reader = new(new BinaryDeserializer(provider.DefaultCollectionName, Encoding.UTF8, Data, uint.MaxValue), null);
			ArchivingTimeModel Obj2 = (ArchivingTimeModel)await Serializer.Deserialize(Reader, ObjectSerializer.TYPE_OBJECT, false);

			Assert.AreEqual(Obj.Days, Obj2.Days);
			Assert.AreEqual(Obj.Value, Obj2.Value);
			Assert.AreNotEqual(default, Obj2.ObjectId);
		}
	}
}
