using System;

namespace Workers.Progress
{
	/// <summary>
	/// Represents event arguments for progress abort events.
	/// </summary>
	public class ProgressAbortEventArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// Abort message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Abort exception.
		/// </summary>
		public Exception Exception { get; }

		#endregion Properties

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ProgressAbortEventArgs"/> class.
		/// </summary>
		/// <param name="message">Abort message.</param>
		public ProgressAbortEventArgs(string message)
		{
			Message = message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProgressAbortEventArgs"/> class.
		/// </summary>
		/// <param name="exception">Abort exception.</param>
		public ProgressAbortEventArgs(Exception exception)
		{
			Message = exception.Message;
			Exception = exception;
		}

		#endregion Constructors
	}
}