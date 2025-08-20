using System;
using System.Resources;

namespace Workers.Workers.Attributes
{
	/// <summary>
	/// Attribute to specify the name of a worker class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class WorkerNameAttribute : Attribute
	{
		#region Fields

		/// <summary>
		/// Type of the resource containing the worker name.
		/// </summary>
		private readonly Type _resourceType;

		/// <summary>
		/// Key of the resource string containing the worker name.
		/// </summary>
		private readonly string _resourceKey;

		/// <summary>
		/// Name of the worker.
		/// </summary>
		private string _name;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerNameAttribute"/> class with the specified name.
		/// </summary>
		/// <param name="name">The name of the worker.</param>
		public WorkerNameAttribute(string name)
		{
			_name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerNameAttribute"/> class with the specified resource type and resource key.
		/// </summary>
		/// <param name="resourceType">The type of the resource containing the name.</param>
		/// <param name="resourceKey">The key of the resource string containing the name.</param>
		public WorkerNameAttribute(Type resourceType, string resourceKey)
		{
			_resourceType = resourceType;
			_resourceKey = resourceKey;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Name of the worker.
		/// </summary>
		public string Name
		{
			get
			{
				if (_name is not null) return _name;

				_name = new ResourceManager(_resourceType).GetString(_resourceKey) ?? _resourceKey;

				return _name;
			}
		}

		#endregion Properties
	}
}