using System;

namespace Workers.Progress
{
	/// <summary>
	/// Represents the arguments for the ProgressChanged event.
	/// </summary>
	public class ProgressChangedEventArgs
    {
		#region Properties

		/// <summary>
		/// Progression value.
		/// </summary>
		public double Progress { get; }

		#endregion Properties

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ProgressChangedEventArgs"/> class.
		/// </summary>
		/// <param name="progress">Current progression.</param>
		/// <exception cref="InvalidOperationException">If progress is out of range.</exception>
		public ProgressChangedEventArgs(double progress)
        {
            if (progress < 0 || 1 < progress)
                throw new ArgumentOutOfRangeException(nameof(progress), "Out of range, progress must be between 0 and 1");

            Progress = progress;
        }

        #endregion Constructor
    }
}
