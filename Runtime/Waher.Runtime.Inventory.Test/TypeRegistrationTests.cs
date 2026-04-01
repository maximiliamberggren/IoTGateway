using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Runtime.Inventory.Test.Definitions;

namespace Waher.Runtime.Inventory.Test
{
	[TestClass]
	public class TypeRegistrationTests
	{
		internal class InternalRegisteredExample : ExampleBase
		{
			public override double Eval(double x) => x - 1;
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			await Types.StopAllModules();
			Types.Invalidate();
			Types.Initialize(typeof(TypeRegistrationTests).Assembly);
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			await Types.StopAllModules();
			Types.Invalidate();
			Types.Initialize(typeof(InstantiationTests).Assembly);
		}

		[TestMethod]
		public void Test_01_ReflectionDiscoveryRemainsDefault()
		{
			Type[] TypesImplementing = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.IsTrue(TypesImplementing.Contains(typeof(Example)));
			Assert.IsFalse(TypesImplementing.Contains(typeof(InternalRegisteredExample)));
		}

		[TestMethod]
		public void Test_02_RegisterTypeAfterInitialize()
		{
			Types.RegisterType(typeof(InternalRegisteredExample));

			Type[] TypesImplementing = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.IsTrue(TypesImplementing.Contains(typeof(Example)));
			Assert.IsTrue(TypesImplementing.Contains(typeof(InternalRegisteredExample)));
			Assert.AreEqual(1, TypesImplementing.Count(T => T == typeof(InternalRegisteredExample)));
			Assert.AreEqual(typeof(InternalRegisteredExample), Types.GetType(typeof(InternalRegisteredExample).FullName));
		}

		[TestMethod]
		public void Test_03_RegisterTypeBeforeInitialize()
		{
			Types.Invalidate();
			Types.RegisterType(typeof(InternalRegisteredExample));
			Types.Initialize(typeof(TypeRegistrationTests).Assembly);

			Type[] TypesImplementing = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.IsTrue(TypesImplementing.Contains(typeof(Example)));
			Assert.IsTrue(TypesImplementing.Contains(typeof(InternalRegisteredExample)));
			Assert.AreEqual(typeof(InternalRegisteredExample), Types.GetType(typeof(InternalRegisteredExample).FullName));
		}

		[TestMethod]
		public void Test_04_RegisterTypesGenericAndPlural()
		{
			Types.RegisterType<InternalRegisteredExample>();

			Type[] TypesImplementing = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.IsTrue(TypesImplementing.Contains(typeof(InternalRegisteredExample)));

			Types.Invalidate();
			Types.Initialize(typeof(TypeRegistrationTests).Assembly);
			Types.RegisterTypes(typeof(InternalRegisteredExample));

			TypesImplementing = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.IsTrue(TypesImplementing.Contains(typeof(InternalRegisteredExample)));
		}

		[TestMethod]
		public void Test_05_RegisterSameTypeTwiceDoesNotDuplicateResults()
		{
			Types.RegisterType(typeof(InternalRegisteredExample));
			Types.RegisterType(typeof(InternalRegisteredExample));

			Type[] TypesImplementing = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.AreEqual(1, TypesImplementing.Count(T => T == typeof(InternalRegisteredExample)));
		}

		[TestMethod]
		public void Test_06_RegisteredTypeAppearsInNamespaceAndQualifiedNameLookups()
		{
			Types.RegisterType(typeof(InternalRegisteredExample));

			Type[] TypesInNamespace = Types.GetTypesInNamespace(typeof(TypeRegistrationTests).Namespace);
			string UnqualifiedName = typeof(InternalRegisteredExample).FullName;
			int i = UnqualifiedName.LastIndexOf('.');
			if (i >= 0)
				UnqualifiedName = UnqualifiedName.Substring(i + 1);

			Assert.IsTrue(TypesInNamespace.Contains(typeof(InternalRegisteredExample)));
			Assert.IsTrue(Types.TryGetQualifiedNames(UnqualifiedName, out string[] QualifiedNames));
			Assert.IsTrue(QualifiedNames.Contains(typeof(InternalRegisteredExample).FullName));
		}

		[TestMethod]
		public void Test_07_RegisteredTypeSurvivesInvalidateAndReinitialize()
		{
			Types.RegisterType(typeof(InternalRegisteredExample));

			Types.Invalidate();
			Types.Initialize(typeof(TypeRegistrationTests).Assembly);

			Type[] TypesImplementing = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.IsTrue(TypesImplementing.Contains(typeof(InternalRegisteredExample)));
		}

		[TestMethod]
		public void Test_08_RegisterTypeNotImplementingInterfaceDoesNotPolluteInterfaceResults()
		{
			Types.RegisterType(typeof(DefaultConstructor));

			Type[] TypesImplementing = Types.GetTypesImplementingInterface(typeof(IExample));

			Assert.IsFalse(TypesImplementing.Contains(typeof(DefaultConstructor)));
			Assert.AreEqual(typeof(DefaultConstructor), Types.GetType(typeof(DefaultConstructor).FullName));
		}

		[TestMethod]
		public void Test_09_RegisteredAssemblyIsAvailable()
		{
			Types.RegisterType(typeof(InternalRegisteredExample));

			Assert.IsTrue(Types.Assemblies.Contains(typeof(TypeRegistrationTests).Assembly));
		}
	}
}
