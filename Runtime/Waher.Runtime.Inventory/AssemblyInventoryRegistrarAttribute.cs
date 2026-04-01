using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Assembly-level attribute pointing to generated inventory registrar infrastructure.
	/// </summary>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
	public sealed class AssemblyInventoryRegistrarAttribute : Attribute
	{
		private readonly Type registrarType;
		private readonly string methodName;

		/// <summary>
		/// Assembly-level attribute pointing to generated inventory registrar infrastructure.
		/// </summary>
		/// <param name="RegistrarType">Registrar type.</param>
		/// <param name="MethodName">Static registration method name. Defaults to <c>Register</c>.</param>
		public AssemblyInventoryRegistrarAttribute(Type RegistrarType, string MethodName = "Register")
		{
			this.registrarType = RegistrarType ?? throw new ArgumentNullException(nameof(RegistrarType));
			this.methodName = string.IsNullOrEmpty(MethodName) ? "Register" : MethodName;
		}

		/// <summary>
		/// Registrar type.
		/// </summary>
		public Type RegistrarType => this.registrarType;

		/// <summary>
		/// Registration method name.
		/// </summary>
		public string MethodName => this.methodName;
	}
}
