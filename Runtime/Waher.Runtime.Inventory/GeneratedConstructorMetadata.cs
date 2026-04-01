using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Delegate for generated constructor invocations.
	/// </summary>
	/// <param name="Arguments">Constructor arguments.</param>
	/// <returns>Created object instance.</returns>
	public delegate object GeneratedConstructorInvoker(object[] Arguments);

	/// <summary>
	/// Generated constructor metadata.
	/// </summary>
	public sealed class GeneratedConstructorMetadata
	{
		private readonly Type[] parameterTypes;
		private readonly bool isPublic;
		private readonly GeneratedConstructorInvoker invoker;

		/// <summary>
		/// Generated constructor metadata.
		/// </summary>
		/// <param name="ParameterTypes">Parameter types.</param>
		/// <param name="IsPublic">If the constructor is public.</param>
		/// <param name="Invoker">Generated constructor invoker.</param>
		public GeneratedConstructorMetadata(Type[] ParameterTypes, bool IsPublic, GeneratedConstructorInvoker Invoker)
		{
			this.parameterTypes = ParameterTypes ?? Types.NoTypes;
			this.isPublic = IsPublic;
			this.invoker = Invoker ?? throw new ArgumentNullException(nameof(Invoker));
		}

		/// <summary>
		/// Constructor parameter types.
		/// </summary>
		public Type[] ParameterTypes => this.parameterTypes;

		/// <summary>
		/// If the constructor is public.
		/// </summary>
		public bool IsPublic => this.isPublic;

		/// <summary>
		/// Constructor invoker.
		/// </summary>
		public GeneratedConstructorInvoker Invoker => this.invoker;
	}
}
