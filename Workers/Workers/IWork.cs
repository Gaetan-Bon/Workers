using Workers.Progress;
using System.Collections.Generic;
using System;

namespace Workers.Workers
{
    /// <summary>
    /// Represents a generic work interface.
    /// </summary>
    public interface IWork : IProgression
    {
        /// <summary>
        /// Workers exceptions.
        /// </summary>
        IDictionary<string, Exception> WorkerExceptions { get; set; }

        /// <summary>
        /// Composite progression.
        /// </summary>
        CompositeProgression CompositeProgression { get; }

        /// <summary>
        /// Determines whether the work item is executable within the provided context.
        /// </summary>
        /// <param name="context">The context in which the work item will be executed.</param>
        /// <returns><c>true</c> if the work item is executable, otherwise <c>false</c>.</returns>
        bool IsExecutable(IWorkContext context);

        /// <summary>
        /// Executes the work item within the provided context.
        /// </summary>
        /// <param name="context">The context in which the work item will be executed.</param>
        /// <returns>A <see cref="WorkerResult"/> representing the result of the execution.</returns>
        WorkerResult Execute(IWorkContext context);
    }

    /// <summary>
    /// Represents a work item that can be executed within a specified context.
    /// </summary>
    /// <typeparam name="TContext">The type of context required for executing the work item.</typeparam>
    public interface IWork<in TContext> : IWork
        where TContext : IWorkContext
    {
        /// <summary>
        /// Determines whether the work item is executable within the provided context.
        /// </summary>
        /// <param name="context">The context in which the work item will be executed.</param>
        /// <returns><c>true</c> if the work item is executable, otherwise <c>false</c>.</returns>
        bool IsExecutable(TContext context);

        /// <summary>
        /// Executes the work item within the provided context.
        /// </summary>
        /// <param name="context">The context in which the work item will be executed.</param>
        /// <returns>A <see cref="WorkerResult"/> representing the result of the execution.</returns>
        WorkerResult Execute(TContext context);
    }
}