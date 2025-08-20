using Workers.Listening;
using System;

namespace Workers.Progress
{
	/// <summary>
	/// Represents a progression of a task, providing properties and events to track its state.
	/// </summary>
	public class Progression : ListenableBase<Progression>, IProgression
	{
		#region Properties

		/// <summary>
		/// Current progression value, ranging from 0.0 to 1.0.
		/// </summary>
		private double _currentProgression;

		/// <inheritdoc/>
		public virtual double CurrentProgression
		{
			get => GetProperty(ref _currentProgression, () => default);
			protected set => UpdateProperty(ref _currentProgression, value);
		}

		/// <summary>
		/// Description of the progression, providing additional context or information.
		/// </summary>
		private string _description;

		/// <inheritdoc/>
		public virtual string Description
		{
			get => GetProperty(ref _description, () => default);
			protected set => UpdateProperty(ref _description, value);
		}

		/// <summary>
		/// Abort message indicating why the progression was aborted, if applicable.
		/// </summary>
		private string _abortMessage;

		/// <inheritdoc/>
		public virtual string AbortMessage
		{
			get => GetProperty(ref _abortMessage, () => default);
			protected set => UpdateProperty(ref _abortMessage, value);
		}

		/// <summary>
		/// Has the progression started.
		/// </summary>
		private bool _hasStarted;

		/// <inheritdoc/>
		public virtual bool HasStarted
		{
			get => GetProperty(ref _hasStarted, () => default);
			protected set => UpdateProperty(ref _hasStarted, value);
		}

		/// <summary>
		/// Has the progression ended.
		/// </summary>
		private bool _hasEnded;

		/// <inheritdoc/>
		public virtual bool HasEnded
		{
			get => GetProperty(ref _hasEnded, () => default);
			protected set => UpdateProperty(ref _hasEnded, value);
		}

		/// <summary>
		/// Is the progression faulted.
		/// </summary>
		private bool _isFaulted;

		/// <inheritdoc/>
		public virtual bool IsFaulted
		{
			get => GetProperty(ref _isFaulted, () => default);
			protected set => UpdateProperty(ref _isFaulted, value);
		}

		/// <summary>
		/// Has the progression been skipped.
		/// </summary>
		private bool _hasSkipped;

		/// <inheritdoc/>
		public virtual bool HasSkipped
		{
			get => GetProperty(ref _hasSkipped, () => default);
			protected set => UpdateProperty(ref _hasSkipped, value);
		}

		#endregion Properties

		#region Methods

		/// <inheritdoc/>
		public virtual void RaiseBegin()
		{
			HasStarted = true;
			RaiseProgressChanged(0d);
			Begin?.Invoke(this, EventArgs.Empty);
		}

		/// <inheritdoc/>
		public virtual void RaiseProgressChanged(double progress)
		{
			CurrentProgression = progress;
			ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(CurrentProgression));
		}

		/// <inheritdoc/>
		public virtual void RaiseAborted(string message)
		{
			IsFaulted = true;
			AbortMessage = message;
			Abort?.Invoke(this, new ProgressAbortEventArgs(message));
		}

		/// <inheritdoc/>
		public virtual void RaiseAborted(Exception exception)
		{
			IsFaulted = true;
			AbortMessage = exception.Message;
			Abort?.Invoke(this, new ProgressAbortEventArgs(exception));
		}

		/// <inheritdoc/>
		public virtual void RaiseCompleted()
		{
			HasEnded = true;
			RaiseProgressChanged(1d);
			Completed?.Invoke(this, EventArgs.Empty);
		}

		/// <inheritdoc/>
		public virtual void RaiseSkipped()
		{
			HasSkipped = true;
			Skipped?.Invoke(this, EventArgs.Empty);
		}

		#endregion Methods

		#region Events

		/// <inheritdoc/>
		public event EventHandler Begin;

		/// <inheritdoc/>
		public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

		/// <inheritdoc/>
		public event EventHandler<ProgressAbortEventArgs> Abort;

		/// <inheritdoc/>
		public event EventHandler Completed;

		/// <inheritdoc/>
		public event EventHandler Skipped;

		#endregion Events
	}
}