using System;
using System.ComponentModel;

namespace Workers.Progress
{
	/// <summary>
	/// Represents a progression interface for tracking progress of a process.
	/// </summary>
	public interface IProgression : INotifyPropertyChanged
	{
		#region Properties

		/// <summary>
		/// Progression value.
		/// </summary>
		double CurrentProgression { get; }

		/// <summary>
		/// Progress description.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Message indicating the reason for aborting the progression, if any.
		/// </summary>
		string AbortMessage { get; }

		/// <summary>
		/// Has the progression started.
		/// </summary>
		bool HasStarted { get; }

		/// <summary>
		/// Has the progression ended.
		/// </summary>
		bool HasEnded { get; }

		/// <summary>
		/// Has the progression been skipped.
		/// </summary>
		bool HasSkipped { get; }

		/// <summary>
		/// Is the progression faulted.
		/// </summary>
		bool IsFaulted { get; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Raise the event when the step progression begins.
		/// </summary>
		void RaiseBegin();

		/// <summary>
		/// Raise the event when the step progression changed.
		/// </summary>
		/// <param name="progress">current value of the progression.</param>
		void RaiseProgressChanged(double progress);

		/// <summary>
		/// Raise the event when the step is aborted with a message.
		/// </summary>
		/// <param name="message">Abort message.</param>
		void RaiseAborted(string message);

		/// <summary>
		/// Raise the event when the step is aborted with an exception.
		/// </summary>
		/// <param name="exception">Abort exception.</param>
		void RaiseAborted(Exception exception);

		/// <summary>
		/// Raise the event when the step is completed.
		/// </summary>
		void RaiseCompleted();

		/// <summary>
		/// Raise the event when the step is skipped.
		/// </summary>
		void RaiseSkipped();

		#endregion Methods

		#region Handlers

		/// <summary>
		/// Step begin event handler.
		/// </summary>
		event EventHandler Begin;

		/// <summary>
		/// Step progress changed event handler.
		/// </summary>
		event EventHandler<ProgressChangedEventArgs> ProgressChanged;

		/// <summary>
		/// Abort step event handler.
		/// </summary>
		event EventHandler<ProgressAbortEventArgs> Abort;

		/// <summary>
		/// Step completed event handler.
		/// </summary>
		event EventHandler Completed;

		/// <summary>
		/// Step skipped event handler.
		/// </summary>
		event EventHandler Skipped;

		#endregion Handlers
	}
}