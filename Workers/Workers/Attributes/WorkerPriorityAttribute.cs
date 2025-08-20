using System;

namespace Workers.Workers.Attributes
{
	/// <summary>
	/// Attribute to specify worker priorities.
	/// </summary>
	/// <param name="priorities">Worker priorities</param>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WorkerPriorityAttribute(params uint[] priorities) : Attribute
    {
		/// <summary>
		/// Worker priorities.
		/// </summary>
		public uint[] Priorities { get; } = priorities;
	}
}