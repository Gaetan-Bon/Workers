using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Workers.Progress;
using Workers.Workers.Exceptions;
using Workers.Workers.Extensions;
using Workers.Workers.Helpers;

namespace Workers.Workers
{
	/// <summary>
	/// Represents a worker used to execute other workers.
	/// </summary>
	public class WorkExecutorWorker : WorkerBase<WorkExecutorWorkerContext>
	{
		#region Static

		#region Methods

		/// <summary>
		/// Creates a new instance of <see cref="WorkExecutorWorker"/> with the specified description and context type.
		/// </summary>
		/// <typeparam name="TContext">The type of worker context.</typeparam>
		/// <param name="description">The description associated with the worker.</param>
		/// <param name="groupName">The group name of the workers.</param>
		/// <param name="searchAssemblies">The assemblies to search for workers.</param>
		/// <returns>A new instance of <see cref="WorkExecutorWorker"/>.</returns>
		public static WorkExecutorWorker Create<TContext>(string description,
														  string groupName = null,
														  params Assembly[] searchAssemblies)
			where TContext : IWorkContext
		{
			return Create(typeof(TContext), description, groupName, searchAssemblies);
		}

		/// <summary>
		/// Creates a new instance of <see cref="WorkExecutorWorker"/> with the specified description and context type.
		/// </summary>
		/// <param name="contextType">The type of worker context.</param>
		/// <param name="description">The description associated with the worker.</param>
		/// <param name="groupName">The group name of the workers.</param>
		/// <param name="searchAssemblies">The assemblies to search for workers.</param>
		/// <returns>A new instance of <see cref="WorkExecutorWorker"/>.</returns>
		public static WorkExecutorWorker Create(Type contextType,
												string description,
												string groupName = null,
												params Assembly[] searchAssemblies)
		{
			Type baseContextType = typeof(IWorkContext);
			if (!baseContextType.IsAssignableFrom(contextType))
				throw new InvalidOperationException($"{nameof(contextType)} of type {contextType.Name} should be assignable to type {baseContextType.FullName}");

			return new WorkExecutorWorker(description, IWorkHelper.GetGroupedOrderedWorkers(contextType, groupName, searchAssemblies).ToArray());
		}

		#endregion Methods

		#endregion Static

		#region Instance

		#region Fields

		/// <summary>
		/// Collection of works to execute.
		/// </summary>
		private readonly IWork[] _works;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Creates a new instance of <see cref="WorkExecutorWorker"/>.
		/// </summary>
		/// <param name="description">Worker description.</param>
		/// <param name="works">Works to execute.</param>
		protected WorkExecutorWorker(string description, IWork[] works)
			: base()
		{
			Description = description;
			_works = works;
			CompositeProgression = new CompositeProgression(_works, Description);
		}

		#endregion Constructors

		#region Methods

		/// <summary>
		/// Asynchronously executes the work item within the provided context.
		/// </summary>
		/// <param name="context">The context in which the work item will be executed.</param>
		/// <returns>A <see cref="Task"/> representing the result of the execution.</returns>
		public virtual Task<WorkerResult> ExecuteAsync(WorkExecutorWorkerContext context)
		{
			return Task.Run(() => ((IWork)this).Execute(context));
		}

		/// <inheritdoc/>
		protected override WorkerResult ExecuteOverride(WorkExecutorWorkerContext context)
		{
			var results =
				_works.Select(e => ExecuteWorker(e, context.Context))
				.Where(e => e.WorkerResultType == WorkerResultType.Failed
						 || e.WorkerResultType == WorkerResultType.Error
						 || e.WorkerResultType == WorkerResultType.Exception)
				.ToArray();

			if (!IsFaulted) return WorkerResult.Success();

			if (WorkerExceptions.Count == 1) return WorkerResult.FatalError(WorkerExceptions.First().Value);
			if (WorkerExceptions.Count > 1) return WorkerResult.FatalError(new AggregateException(WorkerExceptions.Select(e => e.Value)));

			return WorkerResult.Failed(
				Array.Find(
					results,
					r => !string.IsNullOrWhiteSpace(r.Message))?.Message ?? string.Empty);
		}

		/// <summary>
		/// Executes the specified workers with the provided context.
		/// </summary>
		/// <param name="work">The work item to execute.</param>
		/// <param name="context">The context in which to execute the work item.</param>
		/// <returns>The result of executing the work item.</returns>
		protected virtual WorkerResult ExecuteWorker(IWork work, IWorkContext context)
		{
			WorkerResult result = null;
			try
			{
				work.WorkerExceptions = WorkerExceptions;

				if (!work.IsExecutable(context) || (IsFaulted && !work.IsExecutedOnError()))
					result = WorkerResult.Skip();

				result = result ?? work.Execute(context);

				switch (result.WorkerResultType)
				{
					case WorkerResultType.Skip:
						work.RaiseSkipped();
						break;

					case WorkerResultType.Failed:
					case WorkerResultType.Error:
					case WorkerResultType.Exception:
						if (!result.ResumeOnFailure) IsFaulted = true;
						break;
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (WorkerException wex) when (wex.ResumeOnError)
			{
				result = WorkerResult.Error(wex);
			}
			catch (Exception ex)
			{
				IsFaulted = true;
				result = WorkerResult.FatalError(ex);
			}

			return result;
		}

		#endregion Methods

		#endregion Instance
	}
}