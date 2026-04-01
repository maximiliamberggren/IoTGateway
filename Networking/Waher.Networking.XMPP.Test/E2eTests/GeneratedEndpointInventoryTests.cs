using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Test.E2eTests
{
	[TestClass]
	public class GeneratedEndpointInventoryTests
	{
		[TestMethod]
		public void Test_E2E_EndpointsExposeGeneratedMetadata()
		{
			Types.Invalidate();
			Types.Initialize(typeof(EndpointSecurity).Assembly);

			Type[] EndpointTypes = Types.GetTypesImplementingInterface(typeof(IE2eEndpoint));

			Assert.IsTrue(EndpointTypes.Contains(typeof(RsaEndpoint)));
			Assert.IsTrue(Types.TryGetGeneratedMetadata(typeof(RsaEndpoint), out GeneratedTypeMetadata Metadata));
			Assert.IsTrue(Metadata.ImplementedInterfaces.Contains(typeof(IE2eEndpoint)));
			Assert.IsTrue(Metadata.Constructors.Length > 0);
		}
	}
}
