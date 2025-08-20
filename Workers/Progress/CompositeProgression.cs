using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workers.Progress
{
	/// <summary>
	/// Represents a composite progression that can contain multiple step progresses.
	/// </summary>
	public class CompositeProgression : Progression, ICollection<IProgression>
	{
		#region Fields

		/// <summary>
		/// Child step progresses information.
		/// </summary>
		private readonly List<ProgressionInformation> _childStepInformations;

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeProgression"/> class.
		/// </summary>
		public CompositeProgression()
		{
			_childStepInformations = [];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeProgression"/> class.
		/// </summary>
		/// <param name="description">The description associated with the progression.</param>
		public CompositeProgression(string description) : this()
		{
			Description = description;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeProgression"/> class with the specified step progresses.
		/// </summary>
		/// <param name="stepProgresses">The step progresses to add to the composite progression.</param>
		/// <param name="description">The description associated with the progression.</param>
		public CompositeProgression(IEnumerable<IProgression> stepProgresses, string description = "")
			: this(description)
		{
			foreach (IProgression stepProgress in stepProgresses)
				Add(stepProgress);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CompositeProgression"/> class with the specified step progresses and their weights.
		/// </summary>
		/// <param name="stepProgresses">The step progresses to add to the composite progression.</param>
		/// <param name="description">The description associated with the progression.</param>
		public CompositeProgression(IEnumerable<KeyValuePair<IProgression, double>> stepProgresses, string description = "")
			: this(description)
		{
			foreach (KeyValuePair<IProgression, double> stepProgress in stepProgresses)
				Add(stepProgress.Key, stepProgress.Value);
		}

		#endregion Constructors

		#region ICollection<IStepProgress> implementation

		/// <inheritdoc/>
		public IProgression this[int index]
		{
			get
			{
				return _childStepInformations[index].StepProgress;
			}
		}

		/// <inheritdoc/>
		public int IndexOf(IProgression item)
		{
			for (int i = 0; i < _childStepInformations.Count; i++)
			{
				if (_childStepInformations[i].StepProgress == item)
					return i;
			}

			return -1;
		}

		/// <inheritdoc/>
		public int Count
		{
			get
			{
				return _childStepInformations.Count;
			}
		}

		/// <inheritdoc/>
		public bool IsReadOnly
		{
			get
			{
				return ((ICollection<ProgressionInformation>)_childStepInformations).IsReadOnly;
			}
		}

		/// <inheritdoc/>
		public void Add(IProgression item)
		{
			Add(item, 1);
		}

		/// <inheritdoc/>
		public void Add(IProgression item, double coeff)
		{
			ProgressionInformation information = new(item, coeff);
			_childStepInformations.Add(information);
			AttachStepInformation(information);

			RaiseProgressChanged();
		}

		/// <inheritdoc/>
		public void Insert(int index, IProgression item)
		{
			Insert(index, item, 1);
		}

		/// <inheritdoc/>
		public void Insert(int index, IProgression item, double coeff)
		{
			ProgressionInformation information = new(item, coeff);

			_childStepInformations.Insert(index, information);
			AttachStepInformation(information);

			RaiseProgressChanged();
		}

		/// <inheritdoc/>
		public bool Contains(IProgression item)
		{
			lock (((ICollection)_childStepInformations).SyncRoot)
				return _childStepInformations.Exists(spi => spi.StepProgress == item);
		}

		/// <inheritdoc/>
		public void CopyTo(IProgression[] array, int arrayIndex)
		{
			lock (((ICollection)_childStepInformations).SyncRoot)
				for (int i = 0; i < _childStepInformations.Count; i++)
					array[arrayIndex + i] = _childStepInformations[i].StepProgress;
		}

		/// <inheritdoc/>
		public bool Remove(IProgression item)
		{
			int index = _childStepInformations.FindIndex(spi => spi.StepProgress == item);
			if (index < 0) return false;

			RemoveAt(index);
			return true;
		}

		/// <inheritdoc/>
		public void RemoveAt(int index)
		{
			ProgressionInformation information = _childStepInformations.ElementAtOrDefault(index);
			if (information == null) return;

			DetachStepInformation(information);
			_childStepInformations.RemoveAt(index);

			information.Dispose();

			RaiseProgressChanged();
		}

		/// <inheritdoc/>
		public void Clear()
		{
			lock (((ICollection)_childStepInformations).SyncRoot)
			{
				while (_childStepInformations.Count != 0)
					RemoveAt(0);
			}

			RaiseProgressChanged();
		}

		/// <inheritdoc/>
		public IEnumerator<IProgression> GetEnumerator()
		{
			return _childStepInformations.Select(spi => spi.StepProgress).GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion ICollection<IStepProgress> implementation

		#region Private Methods

		/// <summary>
		/// Raises the progress changed event with the current progress.
		/// </summary>
		private void RaiseProgressChanged()
		{
			double progress;

			lock (((ICollection)_childStepInformations).SyncRoot)
			{
				IEnumerable<ProgressionInformation> validChildSteps = _childStepInformations.Where(spi => spi.Coeff > 0);

				if (validChildSteps.Any())
					progress = validChildSteps.Sum(spi => spi.Progress * spi.Coeff) / validChildSteps.Sum(spi => spi.Coeff);
				else
					progress = 1;
			}

			RaiseProgressChanged(progress);
		}

		/// <summary>
		/// Attaches the step information to the progression and subscribes to its progress changed event.
		/// </summary>
		/// <param name="stepInformation"></param>
		private void AttachStepInformation(ProgressionInformation stepInformation)
		{
			DetachStepInformation(stepInformation);

			stepInformation.ProgressChanged += ItemProgressChanged;
		}

		/// <summary>
		/// detaches the step information from the progression and unsubscribes from its progress changed event.
		/// </summary>
		/// <param name="stepInformation"></param>
		private void DetachStepInformation(ProgressionInformation stepInformation)
		{
			stepInformation.ProgressChanged -= ItemProgressChanged;
		}

		#endregion Private Methods

		#region Event

		/// <summary>
		/// Item progress changed event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void ItemProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			RaiseProgressChanged();
		}

		#endregion Event

		#region Class

		/// <summary>
		/// Represents information about a step progression, including its current progress and coefficient.
		/// </summary>
		private sealed class ProgressionInformation : IDisposable
		{
			#region Fields

			/// <summary>
			/// Lock object to ensure thread safety.
			/// </summary>
			private readonly object _lock = new();

			#endregion Fields

			#region Constructor

			/// <summary>
			/// Initializes a new instance of the <see cref="ProgressionInformation"/> class with the specified step progression and coefficient.
			/// </summary>
			/// <param name="stepProgress">Step progression.</param>
			/// <param name="coeff">Step coefficient.</param>
			/// <exception cref="ArgumentNullException">If step progress argument is null.</exception>
			/// <exception cref="ArgumentException">If coeff argument is invalid.</exception>
			public ProgressionInformation(IProgression stepProgress, double coeff)
			{
				StepProgress = stepProgress ?? throw new ArgumentNullException(nameof(stepProgress));
				if (coeff < 0) throw new ArgumentException("coeff is invalid");
				Coeff = coeff;

				Progress = 0;

				AttachStepProgressEvent();
			}

			#endregion Constructor

			#region Properties

			/// <summary>
			/// Is the progression information disposed.
			/// </summary>
			public bool Disposed { get; private set; }

			/// <summary>
			/// Step progress.
			/// </summary>
			public IProgression StepProgress { get; private set; }

			/// <summary>
			/// Progression value.
			/// </summary>
			public double Progress { get; private set; }

			/// <summary>
			/// Coefficient value.
			/// </summary>
			public double Coeff { get; }

			#endregion Properties

			#region Private Methods

			/// <summary>
			/// Attaches the step progress event to the StepProgress and subscribes to its ProgressChanged event.
			/// </summary>
			private void AttachStepProgressEvent()
			{
				lock (_lock)
				{
					ThrowIfDisposed();
					StepProgress.ProgressChanged -= StepProgressChanged;
					StepProgress.ProgressChanged += StepProgressChanged;
				}
			}

			/// <summary>
			/// Detaches the step progress event from the StepProgress and unsubscribes from its ProgressChanged event.
			/// </summary>
			private void DetachStepProgressEvent()
			{
				lock (_lock)
				{
					if (StepProgress != null)
						StepProgress.ProgressChanged -= StepProgressChanged;
				}
			}

			#endregion Private Methods

			#region IDisposable implementation

			/// <summary>
			/// Disposes the ProgressionInformation, detaching the step progress event and releasing resources.
			/// </summary>
			public void Dispose()
			{
				ThrowIfDisposed();

				lock (_lock)
				{
					if (!Disposed)
						Disposed = true;

					DetachStepProgressEvent();

					StepProgress = null;
					_progressChanged = null;
				}
			}

			/// <summary>
			/// Throws an exception if the ProgressionInformation is disposed.
			/// </summary>
			/// <exception cref="ObjectDisposedException">If object is disposed.</exception>
			private void ThrowIfDisposed()
			{
				lock (_lock)
				{
					if (Disposed)
						throw new ObjectDisposedException(nameof(ProgressionInformation));
				}
			}

			#endregion IDisposable implementation

			#region Event

			/// <summary>
			/// Step progress changed event.
			/// </summary>
			/// <param name="sender">Sender.</param>
			/// <param name="e">Event arguments.</param>
			private void StepProgressChanged(object sender, ProgressChangedEventArgs e)
			{
				Progress = e.Progress;

				_progressChanged?.Invoke(this, new ProgressChangedEventArgs(e.Progress));
			}

			#endregion Event

			#region Handler

			/// <summary>
			/// Progress changed event handler.
			/// </summary>
			private EventHandler<ProgressChangedEventArgs> _progressChanged;

			/// <summary>
			/// Progress changed event.
			/// </summary>
			public event EventHandler<ProgressChangedEventArgs> ProgressChanged
			{
				add
				{
					lock (_lock)
					{
						ThrowIfDisposed();
						_progressChanged += value;
					}
				}
				remove
				{
					lock (_lock)
					{
						ThrowIfDisposed();
						_progressChanged -= value;
					}
				}
			}

			#endregion Handler
		}

		#endregion Class
	}
}