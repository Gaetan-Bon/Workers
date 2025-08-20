using System;

namespace Workers.Workers.Attributes
{
	/// <summary>
	/// Attribute to indicate that a worker cannot be cancelled.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class WorkerNonCancellableAttribute : Attribute;
}