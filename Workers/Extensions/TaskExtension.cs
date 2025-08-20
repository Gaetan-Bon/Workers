using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Workers.Extensions
{
    /// <summary>
    /// Extension class for <see cref="Task"/>.
    /// </summary>
    public static class TaskExtension
    {
        /// <summary>
        /// Synchronously waits for the completion of a task of type <typeparamref name="T"/> and returns the result.
        /// </summary>
        /// <remarks>It avoid to lock the current thread.</remarks>
        /// <typeparam name="T">The type of the task result.</typeparam>
        /// <param name="task">The task to wait for.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the task.</param>
        /// <returns>The result of the completed task.</returns>
        /// <exception cref="Exception">Throws any exception that occurs during task execution.</exception>
        public static T WaitSynchronously<T>(this Task<T> task, CancellationToken cancellationToken = default)
            => Task.Run(() => task, cancellationToken).GetAwaiter().TryGetResult();

		/// <summary>
		/// Safely retrieves the result of a task with a return value, throwing the inner exception of an <see cref="AggregateException"/> if one occurs.
		/// </summary>
		/// <typeparam name="T">The type of the result returned by the task.</typeparam>
		/// <param name="taskAwaiter">The task awaiter to get the result from.</param>
		/// <returns>The result of the task if successful.</returns>
		public static T TryGetResult<T>(this TaskAwaiter<T> taskAwaiter)
		{
			try
			{
				return taskAwaiter.GetResult();
			}
			catch (AggregateException ex)
			{
				throw ex.InnerException;
			}
		}
	}
}