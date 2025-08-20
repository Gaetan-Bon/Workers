using System;

namespace Workers.Workers.Attributes
{
	/// <summary>
	/// Attribute to indicate that a worker should be executed when an error occurs.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class WorkerExecuteOnErrorAttribute : Attribute;
}