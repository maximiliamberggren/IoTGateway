using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Opts a non-public type into generated inventory registration.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
	public sealed class WaherInventoryIncludeAttribute : Attribute
	{
	}
}
