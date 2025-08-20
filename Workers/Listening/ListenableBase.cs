using Workers.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Workers.Listening
{
	/// <summary>
	/// Abstract base used to provide foundational functionality for listenable objects, which notify listeners of property changes.
	/// Implements the <see cref="IListenable{TNotifier}"/> interface and provides common functionality for derived classes.
	/// </summary>
	/// <typeparam name="TNotifier">The type of the notifier, which must implement <see cref="INotifyPropertyChanged"/>.</typeparam>
	public abstract class ListenableBase<TNotifier> : IListenable<TNotifier>
		where TNotifier : INotifyPropertyChanged
	{
		#region Constants

		/// <summary>
		/// Property changed event suffix.
		/// </summary>
		internal const string PropertyChangedEventSuffix = "Changed";

		#endregion Constants

		#region Fields

		/// <summary>
		/// Redirections dictionary that maps property names to lists of redirected property names.
		/// </summary>
		private Dictionary<string, List<string>> _redirections;

		/// <summary>
		/// Event notifier that manages and raises events for the associated notifier.
		/// </summary>
		private EventNotifier<TNotifier> _eventNotifier;

		/// <summary>
		/// Invalidating sources dictionary that maps listenable sources to lists of invalidating properties
		/// and their corresponding properties to invalidate.
		/// </summary>
		private Dictionary<IListenable<TNotifier>, List<Tuple<string, List<string>>>> _invalidatingSources;

		#endregion Fields

		#region Events

		/// <summary>
		/// PropertyChanged event handler.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion Events

		#region Properties

		/// <summary>
		/// Redirection dictionary that maps property names to lists of redirected property names.
		/// </summary>
		private Dictionary<string, List<string>> Redirection => GetOrInitializeProperty(ref _redirections);

		/// <summary>
		/// <see cref="EventNotifier{TNotifier}"/> instance associated with this object, initializing it if necessary.
		/// The event notifier is responsible for managing and raising events for the associated notifier.
		/// </summary>
		/// <value>An instance of <see cref="EventNotifier{TNotifier}"/>, initialized with the notifier returned by <see cref="OnGetNotifier"/>.</value>
		protected EventNotifier<TNotifier> EventNotifier
			=> GetProperty(ref _eventNotifier, () => new EventNotifier<TNotifier>(OnGetNotifier()));

		/// <summary>
		/// Invalidating sources dictionary that maps listenable sources to lists of invalidating properties
		/// </summary>
		private Dictionary<IListenable<TNotifier>, List<Tuple<string, List<string>>>> InvalidatingSources
			=> ObjectHelper.GetProperty(ref _invalidatingSources, () => new Dictionary<IListenable<TNotifier>, List<Tuple<string, List<string>>>>());

		#endregion Properties

		#region Methods

		#region Protected Methods

		/// <summary>
		/// Calls PropertyChanged event for the given properties.
		/// </summary>
		/// <param name="propertyNames">Names of the properties to notify when changed.</param>
		protected virtual void NotifyPropertiesChanged(params string[] propertyNames)
		{
			if (PropertyChanged == null)
				return;

			List<string> redirections = null;

			foreach (var propertyName in propertyNames)
			{
				RaiseNotifyPropertyChanged(propertyName);

				if (_redirections?.TryGetValue(propertyName, out redirections) ?? false)
					foreach (var redirected in redirections)
						RaiseNotifyPropertyChanged(redirected);
			}
		}

		/// <summary>
		/// Calls PropertyChanged event for the given propertyName. <paramref name="propertyName"/> contains the caller member name by default.
		/// </summary>
		/// <param name="propertyName">Name of the property that has changed. Contain the caller member name by default.</param>
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
		{
			NotifyPropertiesChanged(propertyName);
		}

		/// <summary>
		/// Removes all handlers attached to this listenable object.
		/// </summary>
		protected void Clean()
		{
			PropertyChanged = null;

			_redirections?.Clear();
			_redirections = null;

			_eventNotifier?.ClearHandlers();
			_eventNotifier = null;
		}

		/// <summary>
		/// Adds an invalidating property that will invalidate the given properties when it changes.
		/// </summary>
		/// <param name="source">Notifying source.</param>
		/// <param name="invalidatingProperty">Invalidating property.</param>
		/// <param name="propertiesToInvalidate">Properties to invalidate.</param>
		protected void AddInvalidatingProperty(
			IListenable<TNotifier> source,
			string invalidatingProperty,
			params string[] propertiesToInvalidate)
		{
			if (!InvalidatingSources.TryGetValue(source, out var invalidatings))
			{
				InvalidatingSources.Add(source, invalidatings = []);
				invalidatings.Add(Tuple.Create(invalidatingProperty, new List<string>(propertiesToInvalidate)));
				source.AddPropertyChangedHandler(invalidatingProperty, OnSourceInvalidatingChanged);
			}
			else
			{
				var existing = invalidatings.Find(x => x.Item1 == invalidatingProperty);

				if (existing != null)
				{
					existing.Item2.AddRange(propertiesToInvalidate.Except(existing.Item2).ToArray());
				}
				else
				{
					invalidatings.Add(Tuple.Create(invalidatingProperty, new List<string>(propertiesToInvalidate)));
					source.AddPropertyChangedHandler(invalidatingProperty, OnSourceInvalidatingChanged);
				}
			}
		}

		/// <summary>
		/// Adds an invalidating property that will invalidate the given properties when it changes.
		/// </summary>
		/// <param name="invalidatingProperty">Invalidating property.</param>
		/// <param name="propertiesToInvalidate">Properties to invalidate.</param>
		protected void AddInvalidatingProperty(string invalidatingProperty, params string[] propertiesToInvalidate)
		{
			if (!Redirection.TryGetValue(invalidatingProperty, out List<string> invalidated))
				Redirection.Add(invalidatingProperty, [.. propertiesToInvalidate]);
			else
				invalidated.AddRange(propertiesToInvalidate.Except(invalidated));
		}

		/// <summary>
		/// Removes an invalidating property that will invalidate the given properties when it changes.
		/// </summary>
		/// <param name="invalidatingProperty">Invalidating property.</param>
		/// <param name="propertiesToRemove">Properties to remove.</param>
		protected void RemoveInvalidatingProperty(string invalidatingProperty, params string[] propertiesToRemove)
		{
			if (Redirection.TryGetValue(invalidatingProperty, out List<string> invalidated))
			{
				foreach (var removed in invalidated.Intersect(propertiesToRemove))
					invalidated.Remove(removed);

				if (invalidated.Count == 0)
				{
					Redirection.Remove(invalidatingProperty);

					if (Redirection.Count == 0)
						_redirections = null;
				}
			}
		}

		/// <summary>
		/// Removes an invalidating property from the given source, which will no longer invalidate the specified properties.
		/// </summary>
		/// <param name="source">Notifying source.</param>
		/// <param name="invalidatingProperty">Invalidating property.</param>
		/// <param name="propertiesToRemove">Properties to remove.</param>
		protected void RemoveInvalidatingProperty(
			IListenable<TNotifier> source,
			string invalidatingProperty,
			params string[] propertiesToRemove)
		{
			if (InvalidatingSources.TryGetValue(source, out var toInvalidate))
			{
				var existing = toInvalidate.Find(x => x.Item1 == invalidatingProperty);

				if (existing != null)
				{
					existing.Item2.RemoveAll(x => propertiesToRemove.Contains(x));

					if (existing.Item2.Count == 0)
					{
						toInvalidate.Remove(existing);
						source.RemovePropertyChangedHandler(invalidatingProperty, OnSourceInvalidatingChanged);
					}
				}

				if (toInvalidate.Count == 0)
					InvalidatingSources.Remove(source);
			}

			if (InvalidatingSources.Count == 0)
				_invalidatingSources = null;
		}

		/// <summary>
		/// Retrieves the wrapped notifier instance.
		/// </summary>
		/// <returns>The wrapped notifier of type <typeparamref name="TNotifier"/>.</returns>
		protected virtual TNotifier OnGetNotifier()
		{
			if (this is TNotifier notifier)
				return notifier;
			else
				throw new InvalidOperationException("You should override OnGetNotifier method when the current class is not the notifier.");
		}

		/// <summary>
		/// Gets the given reference backing field.
		/// <para>If the reference backing field is null, this one will be initialized with the given initalizer function.</para>
		/// <para>For value type, you can use the shouldInitialize function instead of null check.</para>
		/// </summary>
		/// <typeparam name="T">Type of the backing field.</typeparam>
		/// <param name="backingField">Backing field.</param>
		/// <param name="initializer">Initializer.</param>
		/// <param name="shouldInitialize">A function that must return true when the intializer method should be called (use this callback for value type).</param>
		/// <param name="onInitialized">On initialized event handler (optional).</param>
		/// <param name="notifyPropertyChanged">Should notify property changed (optional).</param>
		/// <param name="propertyName">Name of the property (optional).</param>
		/// <returns>The retrieved backing field.</returns>
		protected T GetProperty<T>(
			ref T backingField,
			Func<T> initializer,
			Func<bool> shouldInitialize = null,
			ValueChangedEventHandler<T> onInitialized = null,
			bool notifyPropertyChanged = true,
			[CallerMemberName] string propertyName = null)
		{
			bool refShouldInitialize = (shouldInitialize != null && shouldInitialize()) || (shouldInitialize == null && backingField == null);

			return GetProperty(ref backingField,
				shouldInitialize: ref refShouldInitialize,
				initializer: initializer,
				onInitialized: onInitialized,
				notifyPropertyChanged: notifyPropertyChanged,
				propertyName: propertyName);
		}

		/// <summary>
		/// Gets the given reference backing field.
		/// <para>If the reference backing field is null, this one will be initialized with the given initalizer function.</para>
		/// <para>For value type, you can use the shouldInitialize function instead of null check.</para>
		/// </summary>
		/// <typeparam name="T">Type of the backing field.</typeparam>
		/// <param name="backingField">Backing field.</param>
		/// <param name="shouldInitialize">A function that must return true when the intializer method should be called (use this callback for value type).</param>
		/// <param name="initializer">Initializer.</param>
		/// <param name="onInitialized">On initialized event handler (optional).</param>
		/// <param name="notifyPropertyChanged">Should notify property changed (optional).</param>
		/// <param name="propertyName">Name of the property (optional).</param>
		/// <returns>The retrieved backing field.</returns>
		protected T GetProperty<T>(
			ref T backingField,
			ref bool shouldInitialize,
			Func<T> initializer,
			ValueChangedEventHandler<T> onInitialized = null,
			bool notifyPropertyChanged = true,
			[CallerMemberName] string propertyName = null)
		{
			return ObjectHelper.GetProperty(
				ref backingField,
				initializer: initializer,
				shouldInitialize: ref shouldInitialize,
				onInitialized: (ValueChangedEventHandler<T>)((v) =>
				{
					if (notifyPropertyChanged)
						this.NotifyPropertyChanged(propertyName);

					onInitialized?.Invoke(v);
				})
			);
		}

		/// <summary>
		/// Gets or initializes the given reference backing field.
		/// <para>If the reference backing field is null, this one will be initialized with its default constructor.</para>
		/// </summary>
		/// <typeparam name="T">Type of the backing field.</typeparam>
		/// <param name="backingField">Backing field.</param>
		/// <param name="shouldInitialize">A function that must return true when the intializer method should be called (use this callback for value type).</param>
		/// <param name="onInitialized">On initialized event handler (optional).</param>
		/// <returns>The retrieved or initialized backing field.</returns>
		[DebuggerStepThrough]
		protected T GetOrInitializeProperty<T>(
			ref T backingField,
			Func<bool> shouldInitialize = null,
			ValueChangedEventHandler<T> onInitialized = null) where T : new()
			=> GetProperty(ref backingField, () => new T(), shouldInitialize, onInitialized);

		/// <summary>
		/// Updates the backing field value with the given value, if they are not equals.
		/// </summary>
		/// <typeparam name="T">Type of the backing field.</typeparam>
		/// <param name="backingField">The backing field reference to update.</param>
		/// <param name="newValue">The new value for the backing field.</param>
		/// <param name="onChanged">Handler called after value changed.</param>
		/// <param name="beforeChange">Handler called before value changed.</param>
		/// <param name="validateValue">Handler that can approve, discard or edit the new value.</param>
		/// <param name="notifyPropertyChanged">If set to false, <see cref="NotifyPropertyChanged(string)"/> will not be called.</param>
		/// <param name="propertyName">The name of the property used with the <see cref="NotifyPropertyChanged(string)"/> call.</param>
		/// <returns>The updated backing field.</returns>
		[DebuggerStepThrough]
		protected T UpdateProperty<T>(
			ref T backingField,
			T newValue,
			ValueChangedEventHandler<T> onChanged = null,
			ValueChangedEventHandler<T> beforeChange = null,
			ValidateValueEventHandler<T> validateValue = null,
			bool notifyPropertyChanged = true,
			[CallerMemberName] string propertyName = null)
		{
			void OnValueChanged(T value)
			{
				onChanged?.Invoke(value);

				if (notifyPropertyChanged)
					NotifyPropertyChanged(propertyName);
			}

			ObjectHelper.SetProperty(
				ref backingField,
				newValue,
				OnValueChanged,
				beforeChange,
				validateValue
			);

			return backingField;
		}

		/// <summary>
		/// Updates the backing field value with the given value, if they are not equals.
		/// </summary>
		/// <typeparam name="T">Type of the backing field.</typeparam>
		/// <param name="backingField">The backing field reference to update.</param>
		/// <param name="newValue">The new value for the backing field.</param>
		/// <param name="onChanged">Handler called after value changed.</param>
		/// <param name="validateValue">Handler that can approve, discard or edit the new value.</param>
		/// <param name="notifyPropertyChanged">If set to false, <see cref="NotifyPropertyChanged(string)"/> will not be called.</param>
		/// <param name="propertyName">The name of the property used with the <see cref="NotifyPropertyChanged(string)"/> call.</param>
		/// <returns>The updated backing field.</returns>
		[DebuggerStepThrough]
		protected T UpdateProperty<T>(
			ref T backingField,
			T newValue,
			OldValueChangedEventHandler<T> onChanged,
			ValidateValueEventHandler<T> validateValue = null,
			bool notifyPropertyChanged = true,
			[CallerMemberName] string propertyName = null)
		{
			T old = backingField;
			return UpdateProperty(ref backingField,
				newValue,
				validateValue: validateValue,
				notifyPropertyChanged: notifyPropertyChanged,
				propertyName: propertyName,
				onChanged: (v) => onChanged(old, v)
			);
		}

		#endregion Protected Methods

		#region Private Methods

		/// <summary>
		/// Raises the PropertyChanged event for the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		private void RaiseNotifyPropertyChanged(string propertyName) => PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

		/// <summary>
		/// On source invalidating changed event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Event arguments.</param>
		private void OnSourceInvalidatingChanged(TNotifier sender, NotificationEventArgs args)
		{
			if (InvalidatingSources.TryGetValue((IListenable<TNotifier>)sender, out var toInvalidate))
				foreach (var invalidated in toInvalidate.Where(invalidated => args.IsPropertyChanged(invalidated.Item1)))
					NotifyPropertiesChanged(invalidated.Item2.ToArray());
		}

		#endregion Private Methods

		#region Public Methods

		/// <summary>
		/// Adds the given listener to this <see cref="ListenableBase{TNotifier}"/> instance.
		/// </summary>
		/// <param name="listener">Listener to add.</param>
		public virtual void AddListener(object listener)
			=> EventNotifier.AddListener(listener);

		/// <summary>
		/// Removes the given listener from this <see cref="ListenableBase{TNotifier}"/> instance.
		/// </summary>
		/// <param name="listener">Listener to remove.</param>
		public virtual void RemoveListener(object listener)
			=> EventNotifier.RemoveListener(listener);

		/// <summary>
		/// Adds a handler for the given event name.
		/// </summary>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="eventHandler">Event handler to add.</param>
		public void AddHandler(string eventName, NotificationHandler<TNotifier> eventHandler)
			=> EventNotifier.AddHandler(eventName, eventHandler);

		/// <summary>
		/// Adds a PropertyChanged handler for the given property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="eventHandler">Event handler to add.</param>
		public void AddPropertyChangedHandler(string propertyName, NotificationHandler<TNotifier> eventHandler)
			=> EventNotifier.AddHandler($"{propertyName}{EventNotifier<object>.PropertyChangedEventSuffix}", eventHandler);

		/// <summary>
		/// Removes the given handler for the specified event name.
		/// </summary>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="eventHandler">Event handler to remove.</param>
		public void RemoveHandler(string eventName, NotificationHandler<TNotifier> eventHandler)
			=> EventNotifier.RemoveHandler(eventName, eventHandler);

		/// <summary>
		/// Removes the given PropertyChanged handler for the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="eventHandler">Event handler to add.</param>
		public void RemovePropertyChangedHandler(string propertyName, NotificationHandler<TNotifier> eventHandler)
			=> EventNotifier.RemoveHandler($"{propertyName}{EventNotifier<object>.PropertyChangedEventSuffix}", eventHandler);

		#endregion Public Methods

		#endregion Methods
	}
}