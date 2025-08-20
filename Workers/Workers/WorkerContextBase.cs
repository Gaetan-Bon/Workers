using System.Threading;

namespace Workers.Workers
{
	/// <summary>
	/// Base class of context for workers.
	/// </summary>
	public abstract class WorkerContextBase : IWorkContext
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerContextBase"/> class.
		/// </summary>
		/// <param name="token">Associated cancellation token (optional).</param>
		protected WorkerContextBase(CancellationToken? token = null)
		{
			Token = token ?? default;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerContextBase"/> class by copying another context.
		/// </summary>
		/// <param name="other">Other context to copy.</param>
		protected WorkerContextBase(IWorkContext other)
		{
			Token = other.Token;
		}

		#endregion Constructors

		#region Properties

		/// <inheritdoc/>
		public CancellationToken Token { get; set; }

		#endregion Properties
	}
}