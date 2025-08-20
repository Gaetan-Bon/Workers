using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Workers.Threading
{
	/// <summary>
	/// Class which can be used to execute asynchronous tasks in pool.
	/// </summary>
	/// <remarks>
	/// Creates a new thread queue with a maximum number of threads.
	/// </remarks>
	/// <param name="maxTask">The maximum number of currently threads.</param>
	public class TaskPool(int maxTask)
	{
		#region Fields

		/// <summary>
		/// Sync lock for thread safety.
		/// </summary>
		private readonly object _syncLock = new();

		/// <summary>
		/// Tasks that are currently running.
		/// </summary>
		private readonly HashSet<Task> _workingTasks = [];

		/// <summary>
		/// Tasks that are queued and waiting to be executed.
		/// </summary>
		private readonly ConcurrentQueue<Task> _queueTasks = new();

		/// <summary>
		/// Maximum number of tasks that can run concurrently.
		/// </summary>
		private readonly int _maxTask = maxTask;

		#endregion Fields

		#region Methods

		#region Public Methods

		/// <summary>
		/// Adds a task and runs it if free thread exists, otherwise enqueues it.
		/// </summary>
		/// <param name="action">The task that will be executed.</param>
		public Task Run(Action action)
		{
			Task task = new(action, TaskCreationOptions.LongRunning);
			Run(task);

			return task;
		}

		/// <summary>
		/// Adds a task and runs it if free thread exists, otherwise enqueues it.
		/// </summary>
		/// <param name="func">The task that will be executed.</param>
		public Task<T> Run<T>(Func<T> func)
		{
			Task<T> task = new(func, TaskCreationOptions.LongRunning);
			Run(task);

			return task;
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Runs the specified task if a free thread exists, otherwise enqueues it.
		/// </summary>
		/// <param name="task">The task that will be executed.</param>
		private void Run(Task task)
		{
			task.GetAwaiter()
				.OnCompleted(() => TaskCompleted(task));

			_queueTasks.Enqueue(task);

			CheckWorkingTask();
		}

		/// <summary>
		/// Checks if there are free threads and starts tasks from the queue.
		/// </summary>
		private void CheckWorkingTask()
		{
			lock (_syncLock)
			{
				while (_workingTasks.Count < _maxTask)
				{
					if (!_queueTasks.TryDequeue(out Task task))
						break;

					task.Start();
					_workingTasks.Add(task);
				}
			}
		}

		/// <summary>
		/// Task completed callback.
		/// </summary>
		/// <param name="task">The executed task.</param>
		private void TaskCompleted(Task task)
		{
			lock (_syncLock)
			{
				_workingTasks.Remove(task);
			}

			CheckWorkingTask();
		}

		#endregion Private Methods

		#endregion Methods
	}
}