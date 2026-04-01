using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Marks a model type for generated runtime metadata.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	public sealed class GenerateRuntimeMetadataAttribute : Attribute
	{
	}
}
