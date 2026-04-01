using System;
using Waher.Persistence.Attributes;
using Waher.Runtime.Inventory;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[GenerateRuntimeMetadata]
	[TypeName(TypeNameSerialization.FullName)]
	[ArchivingTime("Days")]
	public class ArchivingTimeModel
	{
		[ObjectId]
		public Guid ObjectId;
		public int Days;
		public string Value;
	}
}
