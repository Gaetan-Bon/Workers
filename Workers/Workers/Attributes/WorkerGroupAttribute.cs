using System;

namespace Workers.Workers.Attributes
{
	/// <summary>
	/// Attribute to specify the worker group name.
	/// </summary>
	/// <param name="groupName">Worker group name.</param>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WorkerGroupAttribute(string groupName) : Attribute
    {
		/// <summary>
		/// Worker group name.
		/// </summary>
		public string GroupName { get; } = groupName;
	}
}