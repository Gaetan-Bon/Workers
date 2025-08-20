using System.ComponentModel;

namespace Workers.Listening
{
    /// <summary>
    /// Interface that represents a listenable object, which notifies listeners of property changes.
    /// This interface extends <see cref="INotifyPropertyChanged"/> and allows for the observation of property changes.
    /// </summary>
    /// <typeparam name="TNotifier">The type of the notifier that raises the property change notifications.</typeparam>
    public interface IListenable<out TNotifier> : INotifyPropertyChanged
    {
        /// <summary>
        /// Adds a listener object to this <see cref="IListenable{TNotifier}"/> instance.
        /// The listener object should have methods with <see cref="NotificationHandlerAttribute"/> to handle notifications.
        /// </summary>
        /// <param name="listener">Listener to add.</param>
        void AddListener(object listener);

        /// <summary>
        /// Removes a listener object from this <see cref="IListenable{TNotifier}"/> instance.
        /// The listener object should have methods with <see cref="NotificationHandlerAttribute"/> to handle notifications.
        /// </summary>
        /// <param name="listener">Listener to remove.</param>
        void RemoveListener(object listener);

        /// <summary>
        /// Adds handler for an event.
        /// </summary>
        /// <param name="eventName">The event name to handle, handle all events if empty.</param>
        /// <param name="eventHandler">The event handler to add.</param>
        void AddHandler(string eventName, NotificationHandler<TNotifier> eventHandler);

		/// <summary>
		/// Removes handler for event an event.
		/// </summary>
		/// <param name="eventName"> The event name.</param>
		/// <param name="eventHandler">The event handler to remove.</param>
		void RemoveHandler(string eventName, NotificationHandler<TNotifier> eventHandler);

        /// <summary>
        /// Adds a property changed handler.
        /// </summary>
        /// <param name="propertyName">The property to listen.</param>
        /// <param name="eventHandler">The handler called when the given property name has changed.</param>
        void AddPropertyChangedHandler(string propertyName, NotificationHandler<TNotifier> eventHandler);

        /// <summary>
        /// Removes a property changed handler.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="eventHandler">The handler to remove for the given property name.</param>
        void RemovePropertyChangedHandler(string propertyName, NotificationHandler<TNotifier> eventHandler);
    }
}