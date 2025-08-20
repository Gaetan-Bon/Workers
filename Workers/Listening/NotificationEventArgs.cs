using System;
using System.ComponentModel;

namespace Workers.Listening
{
	/// <summary>
	/// Represents the arguments for a notification event, providing data and optional callbacks for event handling.
	/// </summary>
	public class NotificationEventArgs
	{
		#region Events

		/// <summary>
		/// On detached event.
		/// </summary>
		internal event Action OnDetached;

		#endregion Events

		#region Fields

		/// <summary>
		/// Response callback to be invoked when the event is handled.
		/// </summary>
		private Action<object> _responseCallback;

		/// <summary>
		/// Is the event args responded.
		/// </summary>
		private bool _responded;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Name of the event.
		/// </summary>
		public string EventName { get; }

		/// <summary>
		/// Current result data.
		/// </summary>
		public object Data { get; }

		/// <summary>
		/// Is the event sender is waiting for a response.
		/// </summary>
		public bool ResponseAwaited => _responseCallback != null && !_responded;

		#endregion Properties

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="NotificationEventArgs"/> class with the specified event name, data, and optional response callback.
		/// </summary>
		/// <param name="eventName">Name of the event.</param>
		/// <param name="data">Event data (optional).</param>
		/// <param name="responseCallback">Response callback (optional).</param>
		internal NotificationEventArgs(string eventName, object data = default, Action<object> responseCallback = null)
		{
			EventName = eventName;
			Data = data;
			_responseCallback = responseCallback;
		}

		#endregion Constructor

		#region Public methods

		/// <summary>
		/// Calls the response callback without any arguments.
		/// </summary>
		public void CallResponse()
		{
			CallResponse(null);
		}

		/// <summary>
		/// Calls the response callback with the provided arguments.
		/// </summary>
		public void CallResponse(params object[] args)
		{
			if (_responded)
				return;

			_responded = true;

			if (args?.Length == 1)
				_responseCallback?.Invoke(args[0]);
			else
				_responseCallback?.Invoke(args);

			_responseCallback = null;
		}

		/// <summary>
		/// Checks if the event name matches the property changed event for the specified property name.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <returns>true if the event arguments represent a propertyChanged event for the given propertyName, otherwise false.</returns>
		public bool IsPropertyChanged(string propertyName)
		{
			return EventName == GetPropertyChangedEvent(propertyName);
		}

		/// <summary>
		/// Generates the full event name for a property changed event based on the provided property name.
		/// </summary>
		/// <param name="propertyName">The name of the property for which the event name is generated.</param>
		/// <returns>A string representing the full event name for the property changed event,
		/// which includes the property name followed by the property changed event suffix.</returns>
		public string GetPropertyChangedEvent(string propertyName)
		{
			return $"{propertyName}{EventNotifier<INotifyPropertyChanged>.PropertyChangedEventSuffix}";
		}

		/// <summary>
		/// Removes this handler from the notifier.
		/// <para>Equivalent to sender.RemoveHandler(eventName, handler) where handler has been invoked.</para>
		/// </summary>
		public void Detach() => OnDetached?.Invoke();

		#endregion Public methods
	}
}