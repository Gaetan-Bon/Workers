using Workers.Workers.Exceptions;
using System;

namespace Workers.Workers
{
	/// <summary>
	/// Represents a worker result.
	/// </summary>
	public class WorkerResult
	{
		#region Static

		/// <summary>
		/// Creates a success result.
		/// </summary>
		/// <param name="message">Result message (optional).</param>
		public static WorkerResult Success(string message = null)
		{
			return new WorkerResult(message: message);
		}

		/// <summary>
		/// Creates a failed result.
		/// </summary>
		/// <param name="message">Result message (optional).</param>
		/// <param name="resumeOnFailure">Should resume on failure (optional).</param>
		public static WorkerResult Failed(string message, bool resumeOnFailure = false)
		{
			return new WorkerResult(WorkerResultType.Failed, resumeOnFailure, message);
		}

		/// <summary>
		/// Creates an error result.
		/// </summary>
		/// <param name="exception">Worker exception.</param>
		public static WorkerResult Error(WorkerException exception)
		{
			return new WorkerResult(WorkerResultType.Error, exception.ResumeOnError, exception);
		}

		/// <summary>
		/// Creates a fatal error result.
		/// </summary>
		/// <param name="exception">Exception.</param>
		public static WorkerResult FatalError(Exception exception)
		{
			return new WorkerResult(WorkerResultType.Exception, false, exception);
		}

		/// <summary>
		/// Creates a skip result.
		/// </summary>
		/// <param name="message">Result message (optional).</param>
		public static WorkerResult Skip(string message = null)
		{
			return new WorkerResult(WorkerResultType.Skip, message: message);
		}

		#endregion Static

		#region Instance

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerResult"/> class.
		/// </summary>
		/// <param name="workerResultType">Worker result type.</param>
		/// <param name="resumeOnFailure">Should resume on failure (optional).</param>
		/// <param name="exception">Associated exception (optional).</param>
		private WorkerResult(WorkerResultType workerResultType = WorkerResultType.Success,
							 bool resumeOnFailure = true,
							 Exception exception = null)
		{
			ResumeOnFailure = resumeOnFailure;
			WorkerResultType = workerResultType;
			Exception = exception;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerResult"/> class with a message.
		/// </summary>
		/// <param name="workerResultType">Worker result type.</param>
		/// <param name="resumeOnFailure">Should resume on failure (optional).</param>
		/// <param name="message">Associated message (optional).</param>
		private WorkerResult(WorkerResultType workerResultType = WorkerResultType.Success,
							 bool resumeOnFailure = true,
							 string message = null)
			: this(workerResultType, resumeOnFailure, default(Exception))
		{
			Message = message;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Should resume on error.
		/// </summary>
		public bool ResumeOnFailure { get; }

		/// <summary>
		/// Worker result type.
		/// </summary>
		public WorkerResultType WorkerResultType { get; }

		/// <summary>
		/// Associated result message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Result associated exception.
		/// </summary>
		public Exception Exception { get; }

		/// <summary>
		/// Worker result group.
		/// </summary>
		public string WorkerResultGroup { get; set; }

		#endregion Properties

		#endregion Instance
	}
}