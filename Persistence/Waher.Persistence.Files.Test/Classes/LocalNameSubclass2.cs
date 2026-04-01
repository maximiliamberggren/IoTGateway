#if !LW
using Waher.Runtime.Inventory;
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
using Waher.Runtime.Inventory;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[GenerateRuntimeMetadata]
	public class LocalNameSubclass2 : LocalNameBase
	{
		public string Value;
	}
}
