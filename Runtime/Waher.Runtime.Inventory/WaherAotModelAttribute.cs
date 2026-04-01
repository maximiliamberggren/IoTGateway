using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Marks a model type for generated AOT metadata.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class WaherAotModelAttribute : Attribute
	{
	}
}
