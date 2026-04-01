using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Runtime.Inventory.Test.Definitions;

namespace Waher.Runtime.Inventory.Test
{
	[TestClass]
	public class GeneratedInventoryTests
	{
		[TestInitialize]
		public async Task TestInitialize()
		{
			await Types.StopAllModules();
			Types.Invalidate();
			Types.Initialize(typeof(GeneratedInventoryTests).Assembly);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			await Types.StopAllModules();
			Types.Invalidate();
			Types.Initialize(typeof(InstantiationTests).Assembly);
		}

		[TestMethod]
		public void Test_01_InternalTypesAreAutoRegistered()
		{
			Type[] Implementations = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.IsTrue(Implementations.Contains(typeof(InternalGeneratedExample)));
			Assert.AreEqual(typeof(InternalGeneratedExample), Types.GetType(typeof(InternalGeneratedExample).FullName));
			Assert.IsTrue(Types.TryGetGeneratedMetadata(typeof(InternalGeneratedExample), out GeneratedTypeMetadata Metadata));
			Assert.IsTrue(Metadata.ImplementedInterfaces.Contains(typeof(IExample)));
		}

		[TestMethod]
		public void Test_02_AotModelSupportsGeneratedMemberAccess()
		{
			Type ModelType = Types.GetType(typeof(InternalGeneratedModel).FullName);
			object Model = Types.Instantiate(ModelType);

			Types.SetProperty(Model, nameof(InternalGeneratedModel.Name), "Alpha");
			Types.SetProperty(Model, nameof(InternalGeneratedModel.Value), 42);

			Assert.AreEqual("Alpha", Types.GetProperty(Model, nameof(InternalGeneratedModel.Name)));
			Assert.AreEqual(42, Types.GetProperty(Model, nameof(InternalGeneratedModel.Value)));
			Assert.IsTrue(Types.TryGetGeneratedMetadata(ModelType, out GeneratedTypeMetadata Metadata));
			Assert.AreEqual(2, Metadata.Members.Length);
		}

		[TestMethod]
		public void Test_03_DefaultImplementationAndSingletonUseGeneratedMetadata()
		{
			IGeneratedDefaultService Service = Types.Instantiate<IGeneratedDefaultService>(false);
			InternalGeneratedSingleton Singleton1 = Types.Instantiate<InternalGeneratedSingleton>(false);
			InternalGeneratedSingleton Singleton2 = Types.Instantiate<InternalGeneratedSingleton>(false);

			Assert.IsInstanceOfType(Service, typeof(InternalGeneratedDefaultService));
			Assert.AreEqual(Singleton1.Id, Singleton2.Id);
			Assert.IsTrue(Types.TryGetGeneratedMetadata(typeof(InternalGeneratedSingleton), out GeneratedTypeMetadata Metadata));
			Assert.IsTrue(Metadata.HasSingletonAttribute);
		}

		[TestMethod]
		public void Test_04_ModuleDependenciesOrderGeneratedInternalModules()
		{
			IModule[] Modules = Types.GetLoadedModules(new DependencyOrder());
			int RootIndex = Array.FindIndex(Modules, x => x.GetType() == typeof(RootGeneratedModule));
			int DependentIndex = Array.FindIndex(Modules, x => x.GetType() == typeof(DependentGeneratedModule));

			Assert.IsTrue(RootIndex >= 0);
			Assert.IsTrue(DependentIndex >= 0);
			Assert.IsTrue(RootIndex < DependentIndex);
		}

		[TestMethod]
		public void Test_05_ManualFactoryRegistrationUsesGeneratedPath()
		{
			Types.RegisterFactory(typeof(FactoryOnlyType), FactoryOnlyType.Create);

			FactoryOnlyType Obj = (FactoryOnlyType)Types.Create(false, typeof(FactoryOnlyType));

			Assert.IsNotNull(Obj);
			Assert.AreEqual("factory", Obj.Value);
			Assert.IsTrue(Types.TryGetGeneratedMetadata(typeof(FactoryOnlyType), out GeneratedTypeMetadata Metadata));
			Assert.IsTrue(Metadata.Constructors.Length > 0);
		}
	}
}
