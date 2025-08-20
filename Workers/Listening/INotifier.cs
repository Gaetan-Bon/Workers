using System;
using System.Threading.Tasks;

namespace Workers.Listening
{
    /// <summary>
    /// Interface used for notification system.
    /// </summary>
    public interface INotifier
    {
        /// <summary>
        /// Raises the given event.
        /// </summary>
        /// <param name="eventName">Name of the event to raise.</param>
        /// <param name="responseCallback">Callback response.</param>
        void RaiseEvent(string eventName, Action<object> responseCallback = null);

		/// <summary>
		/// Asynchronously raises the given event.
		/// </summary>
		/// <param name="eventName">Name of the event to raise.</param>
		/// <param name="timeout">Callback response.</param>
		/// <returns>Task of type <see cref="object"/>.</returns>
		Task<object> RaiseEventAsync(string eventName, int timeout = -1);

        /// <summary>
        /// Asynchronouscly raises the given event of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of the event.</typeparam>
        /// <param name="eventName">Name of the event to raise.</param>
        /// <param name="timeout">Timeout.</param>
        /// <returns>Task of type <typeparamref name="T"/>.</returns>
        Task<T> RaiseEventAsync<T>(string eventName, int timeout = -1);

        /// <summary>
        /// Notifies when property changed.
        /// </summary>
        /// <param name="propertyName">Changed property name.</param>
        void NotifyPropertyChanged(string propertyName);

        /// <summary>
        /// Notifies when properties changed.
        /// </summary>
        /// <param name="propertyNames">Changed property names.</param>
        void NotifyPropertiesChanged(params string[] propertyNames);
    }
}