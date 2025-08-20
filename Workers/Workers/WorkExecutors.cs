using Workers.Extensions;
using Workers.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Workers.Workers
{
	/// <summary>
	/// Class used to execute works with contexts of type <typeparamref name="TContext"/>.
	/// </summary>
	/// <typeparam name="TContext">The type of the work context.</typeparam>
	/// <remarks>
	/// Initializes a new instance of the <see cref="WorkExecutors{TContext}"/> class.
	/// </remarks>
	/// <param name="searchAssemblies">The assemblies to search for works.</param>
	public class WorkExecutors<TContext>(params Assembly[] searchAssemblies) where TContext : IWorkContext
	{
		#region Constants

		/// <summary>
		/// Maximum number of threads to use for executing works.
		/// </summary>
		private const int MaxThread = 20;

		#endregion Constants

		#region Instance

		#region Fields

		/// <summary>
		/// Maximum number of threads to use for executing works.
		/// </summary>
		private readonly int _maxThread = MaxThread;

		/// <summary>
		/// Assemblies to search for works.
		/// </summary>
		private readonly Assembly[] _assemblies = searchAssemblies;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkExecutors{TContext}"/> class.
		/// </summary>
		/// <param name="maxThread">The maximum number of threads.</param>
		/// <param name="searchAssemblies">The assemblies to search for works.</param>
		public WorkExecutors(int maxThread, params Assembly[] searchAssemblies)
			: this(searchAssemblies)
		{
			_maxThread = maxThread;
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Executes works synchronously or asynchronously with the specified contexts and group name.
		/// </summary>
		/// <param name="asyncExecution">Specifies whether to execute asynchronously.</param>
		/// <param name="groupName">The group name of the works.</param>
		/// <param name="contexts">The contexts associated with the works.</param>
		/// <returns>An array of <see cref="WorkerResult"/> representing the results of the execution.</returns>
		public WorkerResult[] Execute(bool asyncExecution,
									  string groupName = null,
									  params TContext[] contexts)
		{
			return asyncExecution
				? ExecuteAsync(contexts, groupName).WaitSynchronously()
				: ExecuteSync(contexts, groupName);
		}

		/// <summary>
		/// Executes works synchronously with the specified contexts and group name.
		/// </summary>
		/// <param name="workContexts">The contexts associated with the executor.</param>
		/// <param name="groupName">The group name of the works.</param>
		/// <returns>An array of <see cref="WorkerResult"/> representing the results of the execution.</returns>
		public WorkerResult[] ExecuteSync(IEnumerable<TContext> workContexts, string groupName = null)
		{
			return workContexts.Select(e => Execute(e, groupName)).ToArray();
		}

		/// <summary>
		/// Executes works asynchronously with the specified contexts and group name.
		/// </summary>
		/// <param name="workContexts">The contexts associated with the executor.</param>
		/// <param name="groupName">The group name of the works.</param>
		/// <returns>A task representing the asynchronous operation, returning an array of <see cref="WorkerResult"/> representing the results of the execution.</returns>
		public async Task<WorkerResult[]> ExecuteAsync(IEnumerable<TContext> workContexts, string groupName = null)
		{
			var pool = new TaskPool(_maxThread);
			Task<WorkerResult>[] taskArr =
				[.. workContexts.Select(c => pool.Run(() => Execute(c, groupName)))];

			return await Task.WhenAll(taskArr.ToArray());
		}

		/// <summary>
		/// Executes a single work with the specified context and group name.
		/// </summary>
		/// <param name="context">The context associated with the executor.</param>
		/// <param name="groupName">The group name of the work.</param>
		/// <returns>A <see cref="WorkerResult"/> representing the results of the execution.</returns>
		private WorkerResult Execute(TContext context, string groupName)
		{
			return
				WorkExecutorWorker.Create(typeof(TContext), string.Empty, groupName, _assemblies)
				.Execute(new WorkExecutorWorkerContext(context));
		}

		#endregion Methods

		#endregion Instance
	}
}