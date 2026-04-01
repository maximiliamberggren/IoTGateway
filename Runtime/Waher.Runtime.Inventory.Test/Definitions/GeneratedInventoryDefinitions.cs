using System;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	[InventoryDiscoverable]
	internal class InternalGeneratedExample : ExampleBase
	{
		public override double Eval(double x) => x + 10;
	}

	[GenerateRuntimeMetadata]
	[InventoryDiscoverable]
	internal class InternalGeneratedModel
	{
		public string Name { get; set; } = string.Empty;
		public int Value;
	}

	[DefaultImplementation(typeof(InternalGeneratedDefaultService))]
	public interface IGeneratedDefaultService
	{
		string Id { get; }
	}

	[InventoryDiscoverable]
	internal class InternalGeneratedDefaultService : IGeneratedDefaultService
	{
		public string Id => "generated-default";
	}

	[InventoryDiscoverable]
	[Singleton]
	internal class InternalGeneratedSingleton
	{
		public Guid Id { get; } = Guid.NewGuid();
	}

	[InventoryDiscoverable]
	internal class RootGeneratedModule : IModule
	{
		public Task Start() => Task.CompletedTask;
		public Task Stop() => Task.CompletedTask;
	}

	[InventoryDiscoverable]
	[ModuleDependency(typeof(RootGeneratedModule))]
	internal class DependentGeneratedModule : IModule
	{
		public Task Start() => Task.CompletedTask;
		public Task Stop() => Task.CompletedTask;
	}

	internal class FactoryOnlyType
	{
		private FactoryOnlyType()
		{
		}

		public string Value => "factory";

		public static FactoryOnlyType Create()
		{
			return new FactoryOnlyType();
		}
	}
}
