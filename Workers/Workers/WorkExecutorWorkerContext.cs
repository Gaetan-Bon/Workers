using System.Threading;

namespace Workers.Workers
{
	/// <summary>
	/// Represents a worker context to use within a work executor.
	/// </summary>
	public sealed class WorkExecutorWorkerContext : WorkerContextBase
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkExecutorWorkerContext"/> class with the specified context,
		/// cancellation token, and search assemblies.
		/// </summary>
		/// <param name="context">The underlying worker context.</param>
		/// <param name="token">The cancellation token.</param>
		public WorkExecutorWorkerContext(IWorkContext context, CancellationToken? token = null)
			: base(token)
		{
			Context = context;
			Context.Token = Token;
		}

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Gets the underlying worker context.
		/// </summary>
		public IWorkContext Context { get; }

		#endregion Properties
	}
}