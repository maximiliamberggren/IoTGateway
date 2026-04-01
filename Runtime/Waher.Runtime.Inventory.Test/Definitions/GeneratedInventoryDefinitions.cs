using System;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Runtime.Inventory.Test.Definitions
{
	[WaherInventoryInclude]
	internal class InternalGeneratedExample : ExampleBase
	{
		public override double Eval(double x) => x + 10;
	}

	[WaherAotModel]
	[WaherInventoryInclude]
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

	[WaherInventoryInclude]
	internal class InternalGeneratedDefaultService : IGeneratedDefaultService
	{
		public string Id => "generated-default";
	}

	[WaherInventoryInclude]
	[Singleton]
	internal class InternalGeneratedSingleton
	{
		public Guid Id { get; } = Guid.NewGuid();
	}

	[WaherInventoryInclude]
	internal class RootGeneratedModule : IModule
	{
		public Task Start() => Task.CompletedTask;
		public Task Stop() => Task.CompletedTask;
	}

	[WaherInventoryInclude]
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
