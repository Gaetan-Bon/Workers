using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Workers.Collections;

namespace Workers.Listening
{
	/// <summary>
	/// Provides a flexible event notification system for a specified notifier object, enabling dynamic registration and
	/// management of event listeners and handlers.
	/// </summary>
	/// <remarks><para> <see cref="EventNotifier{TNotifier}"/> allows you to attach listener objects that
	/// handle events raised by a notifier of type <typeparamref name="TNotifier"/>. Listeners can define methods marked
	/// with <see cref="NotificationHandlerAttribute"/> to respond to specific events or property changes. </para>
	/// <para> If property change notifications are enabled, the notifier must implement <see
	/// cref="INotifyPropertyChanged"/>. Property change events are relayed as event notifications, allowing handlers to
	/// respond to property updates. </para> <para> Event handlers can be added or removed for specific events or
	/// property changes. Events can be raised synchronously or asynchronously, and listeners can be dynamically managed
	/// at runtime. </para> <para> This class is thread-safe for listener registration and event notification. However,
	/// ensure that listener methods themselves are thread-safe if events may be raised from multiple threads.
	/// </para></remarks>
	/// <typeparam name="TNotifier">The type of the notifier object whose events are managed by this instance.</typeparam>
	public class EventNotifier<TNotifier>
	{
		#region Static

		#region Static Fields

		/// <summary>
		/// Listener cache for storing method information and attributes for each listener type.
		/// </summary>
		private static readonly Dictionary<Type, List<Tuple<string, MethodInfo, MethodParamInfo, NotificationHandlerAttribute>>> s_listenerCache = [];

		#endregion Static Fields

		#region Static Methods

		/// <summary>
		/// Checks if the specified method is a valid event handler for the notifier type.
		/// </summary>
		/// <param name="method">Method to check.</param>
		/// <param name="paramInfo">Output parameter that will contain the method's parameter information.</param>
		/// <returns>True if the method is a valid event handler, otherwise false.</returns>
		private static bool IsValidEventHandler(MethodInfo method, out MethodParamInfo paramInfo)
		{
			ParameterInfo[] parameters = method.GetParameters();

			paramInfo = MethodParamInfo.NoParameters;

			if (parameters.Length > 2 || method.ReturnType != typeof(void))
				return false;

			if (parameters.Length > 0)
			{
				Type param1 = parameters[0].ParameterType;
				if (typeof(TNotifier).IsAssignableFrom(param1))
					paramInfo = MethodParamInfo.Notifier;
				else if (param1 == typeof(NotificationEventArgs))
					paramInfo = MethodParamInfo.EventArgs;
			}

			if (parameters.Length > 1)
			{
				Type param2 = parameters[1].ParameterType;
				if (paramInfo == MethodParamInfo.EventArgs)
				{
					paramInfo = MethodParamInfo.EventArgs_Notifier;
					return typeof(TNotifier).IsAssignableFrom(param2);
				}
				else
				{
					paramInfo = MethodParamInfo.Notifier_EventArgs;
					return param2 == typeof(NotificationEventArgs);
				}
			}

			return true;
		}

		#endregion Static Methods

		#endregion Static

		#region Instance

		#region Constants

		/// <summary>
		/// Property changed event suffix.
		/// </summary>
		public const string PropertyChangedEventSuffix = "Changed";

		#endregion Constants

		#region Fields

		/// <summary>
		/// Notifier to use for this event notifier.
		/// </summary>
		private readonly TNotifier _notifier;

		/// <summary>
		/// Callback dictionary to manage event handlers.
		/// </summary>
		private readonly CallbackDictionary<string, NotificationHandler<TNotifier>> _callbacks = new();

		/// <summary>
		/// Listeners dictionary that maps listener objects to their registered notification handlers.
		/// </summary>
		private Dictionary<object, List<NotificationHandler<TNotifier>>> __listeners;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Listeners dictionary that maps listener objects to their registered notification handlers.
		/// </summary>
		private Dictionary<object, List<NotificationHandler<TNotifier>>> _listeners => __listeners ??= [];

		/// <summary>
		/// Listeners registered with this event notifier.
		/// </summary>
		public IEnumerable<object> Listeners => _listeners.Keys;

		#endregion Properties

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="EventNotifier{TNotifier}"/> class with the specified notifier.
		/// </summary>
		/// <param name="notifier">Notifier to use.</param>
		/// <param name="propertyChangedEnabled">Is property changed enabled.</param>
		/// <exception cref="InvalidOperationException">Thrown if INotifyPropertyChanged is not implemented by the notifier.</exception>
		public EventNotifier(TNotifier notifier, bool propertyChangedEnabled = true)
		{
			_notifier = notifier;

			if (propertyChangedEnabled && _notifier is INotifyPropertyChanged inpc)
				inpc.PropertyChanged += OnNotifierPropertyChanged;
			else
				throw new InvalidOperationException("The given notifier must implement INotifyPropertyChanged");
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		/// <summary>
		/// Represents the parameter information of a method used in the event notifier.
		/// </summary>
		private enum MethodParamInfo
		{
			/// <summary>
			/// The method has no parameters.
			/// </summary>
			NoParameters,

			/// <summary>
			/// The method has a single parameter which is the notifier (sender).
			/// </summary>
			Notifier,

			/// <summary>
			/// The method has a single parameter which is the event args.
			/// </summary>
			EventArgs,

			/// <summary>
			/// The method has two parameters: the first is the notifier (sender) and the second is the event args.
			/// </summary>
			Notifier_EventArgs,

			/// <summary>
			/// The method has two parameters: the first is the event args and the second is the notifier (sender).
			/// </summary>
			EventArgs_Notifier
		}

		/// <summary>
		/// Adds a listener to the given event notifier.
		/// <para>Listeners can be any object that has methods decorated with the <see cref="NotificationHandlerAttribute"/>.</para>
		/// </summary>
		/// <param name="listener">Listener to add.</param>
		public void AddListener(object listener)
		{
			if (!_listeners.TryGetValue(listener, out List<NotificationHandler<TNotifier>> handlers))
				_listeners.Add(listener, handlers = []);
			else
				return;

			Type listenerType = listener.GetType();
			List<Tuple<string, MethodInfo, MethodParamInfo, NotificationHandlerAttribute>> cache;

			lock (s_listenerCache)
			{
				if (!s_listenerCache.TryGetValue(listenerType, out cache))
				{
					cache = [];

					foreach (MethodInfo method in listenerType
					.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
					.Where(m => m.IsDefined(typeof(NotificationHandlerAttribute), false)))
					{
						if (!EventNotifier<TNotifier>.IsValidEventHandler(method, out MethodParamInfo paramInfo))
							return;

						IEnumerable<NotificationHandlerAttribute> attributes = method.GetCustomAttributes<NotificationHandlerAttribute>(false);
						foreach (var attribute in attributes)
						{
							string eventName = null;

							if (attribute.Key != null)
								eventName = attribute.IsPropertyChanged ? $"{attribute.Key}{PropertyChangedEventSuffix}" : attribute.Key;

							cache.Add(Tuple.Create(eventName, method, paramInfo, attribute));
						}
					}

					s_listenerCache.Add(listenerType, cache);
				}
			}

			foreach (var eventCache in cache)
			{
				NotificationHandler<TNotifier> handler = null;
				NotificationHandlerAttribute attribute = eventCache.Item4;

				if (attribute.TargetType != null)
				{
					if (attribute.AllowInheritance && !attribute.TargetType.IsInstanceOfType(_notifier))
						continue;
					else if (!attribute.AllowInheritance && attribute.TargetType != _notifier.GetType())
						continue;
				}

				Delegate del;
				switch (eventCache.Item3)
				{
					case MethodParamInfo.NoParameters:
						Action act = Delegate.CreateDelegate(typeof(Action), listener, eventCache.Item2) as Action;
						handler = (s, e) => act();
						break;

					case MethodParamInfo.Notifier:
						del = Delegate.CreateDelegate(typeof(Action<>).MakeGenericType(eventCache.Item2.GetParameters()[0].ParameterType), listener, eventCache.Item2);

						if (del is not Action<TNotifier> actv)
							handler = (s, e) => del.DynamicInvoke(s);
						else
							handler = (s, e) => actv(s);

						break;

					case MethodParamInfo.EventArgs:
						Action<NotificationEventArgs> acte = Delegate.CreateDelegate(typeof(Action<NotificationEventArgs>), listener, eventCache.Item2) as Action<NotificationEventArgs>;
						handler = (s, e) => acte(e);
						break;

					case MethodParamInfo.Notifier_EventArgs:
						del = Delegate.CreateDelegate(typeof(NotificationHandler<>).MakeGenericType(eventCache.Item2.GetParameters()[0].ParameterType), listener, eventCache.Item2);
						handler = del as NotificationHandler<TNotifier>;

						if (handler == null)
							handler = (s, e) => del.DynamicInvoke(s, e);

						break;

					case MethodParamInfo.EventArgs_Notifier:
						del = Delegate.CreateDelegate(typeof(Action<NotificationEventArgs, TNotifier>), listener, eventCache.Item2);

						if (del is not Action<NotificationEventArgs, TNotifier> actev)
							handler = (s, e) => del.DynamicInvoke(e, s);
						else
							handler = (s, e) => actev(e, s);
						break;
				}

				if (handler != null)
				{
					_callbacks.AddHandler(eventCache.Item1 ?? string.Empty, handler);
					handlers.Add(handler);
				}
			}
		}

		/// <summary>
		/// Removes a listener from the given event notifier.
		/// </summary>
		/// <param name="listener">Listener to remove.</param>
		public void RemoveListener(object listener)
		{
			if (_listeners.TryGetValue(listener, out List<NotificationHandler<TNotifier>> handlers))
			{
				_callbacks.RemoveHandlers(handlers);
				_listeners.Remove(listener);
			}
		}

		/// <summary>
		/// Adds PropertyChangedHandler to the given event notifier.
		/// </summary>
		/// <param name="propertyName">Property to listen.</param>
		/// <param name="handler">The handler to add.</param>
		public void AddPropertyChangedHandler(string propertyName, NotificationHandler<TNotifier> handler)
			=> AddHandler($"{propertyName}{PropertyChangedEventSuffix}", handler);

		/// <summary>
		/// Removes PropertyChangedHandler from the given event notifier.
		/// </summary>
		/// <param name="propertyName">The property listened.</param>
		/// <param name="handler">The handler to remove.</param>
		public void RemovePropertyChangedHandler(string propertyName, NotificationHandler<TNotifier> handler)
			=> RemoveHandler($"{propertyName}{PropertyChangedEventSuffix}", handler);

		/// <summary>
		/// Adds a handler for the specified event.
		/// </summary>
		/// <param name="eventName">The name of the event to handle.</param>
		/// <param name="action">The action to perform when the event is raised.</param>
		public void AddHandler(string eventName, NotificationHandler<TNotifier> action)
			=> _callbacks.AddHandler(eventName, action);

		/// <summary>
		/// Clears all handlers for the specified event.
		/// </summary>
		/// <param name="eventName">The name of the event to clear all handlers from.</param>
		public void ClearHandlers(string eventName)
			=> _callbacks.ClearHandlers(eventName);

		/// <summary>
		/// Clears event handlers for all events.
		/// </summary>
		public void ClearHandlers()
			=> _callbacks.ClearHandlers();

		/// <summary>
		/// Removes a specific handler for the specified event.
		/// </summary>
		/// <param name="eventName">The name of the event for which to remove the handler.</param>
		/// <param name="action">The action to remove from the event handlers.</param>
		public void RemoveHandler(string eventName, NotificationHandler<TNotifier> action)
			=> _callbacks.RemoveHandler(eventName, action);

		/// <summary>
		/// Raises the PropertyChanged event for the specified property.
		/// </summary>
		/// <param name="propertyName">The name of the property that has changed.</param>
		public void RaisePropertyChanged(string propertyName)
			=> RaiseEvent($"{propertyName}{PropertyChangedEventSuffix}");

		/// <summary>
		/// Raises the specified event with optional data and a callback.
		/// </summary>
		/// <param name="eventName">The name of the event to raise.</param>
		/// <param name="data">The data to pass with the event.</param>
		/// <param name="callback">An optional callback to invoke after the event is handled.</param>
		public void RaiseEvent(string eventName, object data = default, Action<object> callback = null)
		{
			NotificationHandler<TNotifier> handlers = null;
			if (_callbacks.TryGetCallback(eventName, out NotificationHandler<TNotifier> eventHanlders))
				handlers = Delegate.Combine(handlers, eventHanlders) as NotificationHandler<TNotifier>;

			if (_callbacks.TryGetCallback(string.Empty, out NotificationHandler<TNotifier> multiHandlers))
				handlers = Delegate.Combine(handlers, multiHandlers) as NotificationHandler<TNotifier>;

			if (handlers != null)
			{
				var args = new NotificationEventArgs(eventName, data, callback);
				NotificationHandler<TNotifier> currentHandler = default;

				args.OnDetached += onDetached;

				foreach (var handler in handlers.GetInvocationList())
					(currentHandler = (NotificationHandler<TNotifier>)handler)(_notifier, args);

				args.OnDetached -= onDetached;

				void onDetached() => RemoveHandler(eventName, currentHandler);
			}
		}

		/// <summary>
		/// Asynchronously raises the specified event with optional data, returning a task that completes with the result.
		/// </summary>
		/// <param name="eventName">The name of the event to raise.</param>
		/// <param name="data">The data to pass with the event.</param>
		/// <returns>A task representing the asynchronous operation, containing the result of the event.</returns>
		public Task<object> RaiseEventAsync(string eventName, object data = default)
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

			void _callback(object obj)
				=> tcs.SetResult(obj);

			Task.Run(() => RaiseEvent(eventName, data, _callback));

			return tcs.Task;
		}

		/// <summary>
		/// Asynchronously raises the specified event with optional data, returning a task that completes with the result cast to the specified type.
		/// </summary>
		/// <typeparam name="T">The type of the result expected from the event.</typeparam>
		/// <param name="eventName">The name of the event to raise.</param>
		/// <param name="data">The data to pass with the event.</param>
		/// <returns>A task representing the asynchronous operation, containing the result of the event cast to the specified type.</returns>
		public async Task<T> RaiseEventAsync<T>(string eventName, object data = default)
		{
			if (await RaiseEventAsync(eventName, data) is T tvalue)
				return tvalue;

			return default;
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// On notifier property changed event handler.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">Event arguments.</param>
		private void OnNotifierPropertyChanged(object sender, PropertyChangedEventArgs e)
			=> RaiseEvent($"{e.PropertyName}{PropertyChangedEventSuffix}");

		#endregion Private Methods

		#endregion Methods

		#endregion Instance
	}
}