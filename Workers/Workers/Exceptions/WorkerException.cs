using System;

namespace Workers.Workers.Exceptions
{
	/// <summary>
	/// Represents an exception specific to worker operations.
	/// </summary>
	public class WorkerException : Exception
	{
		#region Constructors

		/// <inheritdoc/>
		private WorkerException()
		{ }

		/// <inheritdoc/>
		public WorkerException(string message) : base(message) { }

		/// <inheritdoc/>
		public WorkerException(string message, Exception innerException) : base(message, innerException) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerException"/> class with a specified worker and error message.
		/// </summary>
		/// <param name="worker">Associated worker.</param>
		/// <param name="message">Associated message.</param>
		/// <param name="resumeOnError">Should resume on error (optional).</param>
		public WorkerException(IWork worker, string message, bool resumeOnError = false) : base(message)
		{
			Worker = worker;
			ResumeOnError = resumeOnError;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerException"/> class with a specified worker, error message, and inner exception.
		/// </summary>
		/// <param name="worker">Associated worker.</param>
		/// <param name="message">Associated message.</param>
		/// <param name="innerException">Associated inner exception.</param>
		/// <param name="resumeOnError">Should resume on error (optional).</param>
		public WorkerException(IWork worker, string message, Exception innerException, bool resumeOnError = false) : base(message, innerException)
		{
			Worker = worker;
			ResumeOnError = resumeOnError;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Associated worker.
		/// </summary>
		public IWork Worker { get; }

		/// <summary>
		/// Should resume on error.
		/// </summary>
		public bool ResumeOnError { get; }

		#endregion Properties
	}
}