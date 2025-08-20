using Workers.Progress;
using Workers.Workers.Extensions;
using System;
using System.Collections.Generic;

namespace Workers.Workers
{
	/// <summary>
	/// Base class for implementing a worker that executes a task with a specific context.
	/// </summary>
	/// <typeparam name="TContext">The type of context required for executing the progress worker.</typeparam>
	public abstract class WorkerBase<TContext> : Progression, IWork<TContext> where TContext : IWorkContext
    {
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerBase{TContext}"/> class.
		/// </summary>
		protected WorkerBase()
        {
            Description = this.GetName();
            WorkerExceptions = new Dictionary<string, Exception>();
        }

		#endregion Constructors

		#region Properties

		/// <summary>
		/// Composite progression that aggregates the progress of this worker.
		/// </summary>
		private CompositeProgression _compositeProgression;

        /// <inheritdoc/>
        public virtual CompositeProgression CompositeProgression
        {
            get => _compositeProgression ??= new CompositeProgression(Description);
            set { _compositeProgression = value; }
        }

        /// <inheritdoc/>
        public IDictionary<string, Exception> WorkerExceptions { get; set; }

		#endregion Properties

		#region Methods

		#region IWork implementation

		/// <inheritdoc/>
		bool IWork.IsExecutable(IWorkContext context)
        {
            if (context is TContext workerContext) return IsExecutable(workerContext);

            throw new InvalidOperationException($"{nameof(context)} of type {context.GetType().Name} should be of type {typeof(TContext)}.");
        }

		/// <inheritdoc/>
		WorkerResult IWork.Execute(IWorkContext context)
        {
            if (context is TContext workerContext) return Execute(workerContext);

            throw new InvalidOperationException($"{nameof(context)} of type {context.GetType().Name} should be of type {typeof(TContext)}.");
        }

        #endregion IWork implementation

        /// <inheritdoc/>
        public virtual bool IsExecutable(TContext context)
        {
            return true;
        }

        /// <inheritdoc/>
        public virtual WorkerResult Execute(TContext context)
        {
            try
            {
                if (HasStarted) throw new InvalidOperationException("Already executed.");

                CompositeProgression.ProgressChanged += OnProgressChanged;

                RaiseBegin();

                ThrowIfCancellationPending(context);

                var result = ExecuteOverride(context);

                ThrowIfCancellationPending(context);

                switch (result.WorkerResultType)
                {
                    case WorkerResultType.Success:
                        RaiseCompleted();
                        break;

                    case WorkerResultType.Failed:
                        RaiseAborted(result.Message);
                        break;

                    case WorkerResultType.Error:
                    case WorkerResultType.Exception:
                        RaiseAborted(result.Exception);
                        break;

                    case WorkerResultType.Skip:
                        RaiseSkipped();
                        break;
                }

                return result;
            }
            catch (OperationCanceledException ocex)
            {
                RaiseAborted(ocex);
                WorkerExceptions.Add(this.GetName(), ocex);
                return WorkerResult.FatalError(ocex);
            }
            catch (Exception ex)
            {
                RaiseAborted(ex);
                WorkerExceptions.Add(this.GetName(), ex);
                throw;
            }
            finally
            {
                CompositeProgression.ProgressChanged -= OnProgressChanged;
            }
        }

		/// <summary>
		/// Execute the worker with the provided context.
		/// </summary>
		/// <param name="context">The context in which the progress worker will be executed.</param>
		/// <returns>A <see cref="WorkerResult"/> representing the result of the execution.</returns>
		protected abstract WorkerResult ExecuteOverride(TContext context);

		/// <summary>
		/// Checks if the cancellation token in the provided context has been requested and throws an exception if it has.
		/// </summary>
		/// <param name="context">The context where the cancellation token to check is located.</param>
		protected virtual void ThrowIfCancellationPending(TContext context)
        {
            if (this.IsNonCancellable()) return;

            context.Token.ThrowIfCancellationRequested();
        }

        /// <inheritdoc/>
        public override void RaiseBegin()
        {
            base.RaiseBegin();
            CompositeProgression?.RaiseBegin();
        }

        /// <inheritdoc/>
        public override void RaiseCompleted()
        {
            base.RaiseCompleted();
            CompositeProgression?.RaiseCompleted();
        }

        /// <inheritdoc/>
        public override void RaiseAborted(Exception exception)
        {
            base.RaiseAborted(exception);
            CompositeProgression?.RaiseAborted(exception);
        }

        /// <inheritdoc/>
        public override void RaiseAborted(string message)
        {
            base.RaiseAborted(message);
            CompositeProgression?.RaiseAborted(message);
        }

        /// <inheritdoc/>
        public override void RaiseSkipped()
        {
            base.RaiseSkipped();
            CompositeProgression?.RaiseSkipped();
        }

		#endregion Methods

		#region Handlers

		/// <summary>
		/// Raises the <see cref="Progression.ProgressChanged"/> event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            RaiseProgressChanged(e.Progress);
        }

        #endregion Handlers
    }
}