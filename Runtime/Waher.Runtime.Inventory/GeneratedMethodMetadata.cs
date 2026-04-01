using System;

namespace Waher.Runtime.Inventory
{
	/// <summary>
	/// Delegate for generated method invocations.
	/// </summary>
	/// <param name="Instance">Object instance.</param>
	/// <param name="Arguments">Method arguments.</param>
	/// <returns>Method result.</returns>
	public delegate object GeneratedMethodInvoker(object Instance, object[] Arguments);

	/// <summary>
	/// Generated method metadata.
	/// </summary>
	public sealed class GeneratedMethodMetadata
	{
		private readonly string name;
		private readonly Type[] parameterTypes;
		private readonly Type returnType;
		private readonly bool isPublic;
		private readonly GeneratedMethodInvoker invoker;

		/// <summary>
		/// Generated method metadata.
		/// </summary>
		/// <param name="Name">Method name.</param>
		/// <param name="ParameterTypes">Parameter types.</param>
		/// <param name="ReturnType">Return type.</param>
		/// <param name="IsPublic">If the method is public.</param>
		/// <param name="Invoker">Generated method invoker.</param>
		public GeneratedMethodMetadata(string Name, Type[] ParameterTypes, Type ReturnType, bool IsPublic,
			GeneratedMethodInvoker Invoker)
		{
			this.name = Name ?? throw new ArgumentNullException(nameof(Name));
			this.parameterTypes = ParameterTypes ?? Types.NoTypes;
			this.returnType = ReturnType ?? typeof(void);
			this.isPublic = IsPublic;
			this.invoker = Invoker ?? throw new ArgumentNullException(nameof(Invoker));
		}

		/// <summary>
		/// Method name.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Parameter types.
		/// </summary>
		public Type[] ParameterTypes => this.parameterTypes;

		/// <summary>
		/// Return type.
		/// </summary>
		public Type ReturnType => this.returnType;

		/// <summary>
		/// If the method is public.
		/// </summary>
		public bool IsPublic => this.isPublic;

		/// <summary>
		/// Generated invoker.
		/// </summary>
		public GeneratedMethodInvoker Invoker => this.invoker;
	}
}
