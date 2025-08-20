using System;

namespace Workers.Listening
{
	/// <summary>
	/// Represents the arguments for a notification event.
	/// </summary>
	/// <typeparam name="TNofitier">Notifier type.</typeparam>
	/// <param name="sender">Sender.</param>
	/// <param name="args">Event arguments.</param>
	public delegate void NotificationHandler<in TNofitier>(TNofitier sender, NotificationEventArgs args);

	/// <summary>
	/// Attribute to define a method as a notification handler.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NotificationHandlerAttribute : Attribute
    {
		#region Properties

		/// <summary>
		/// Does the propertyd changed.
		/// </summary>
		public bool IsPropertyChanged { get; set; } = false;

        /// <summary>
        /// Property key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Type of the ViewModel this method can handle.
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// Does the target ViewModel can be a sub class of the TargetType.
        /// True by default.
        /// </summary>
        public bool AllowInheritance { get; set; } = true;

        #endregion Properties

		#region Constructors

		/// <summary>
		/// Defines a handler for all notifications on a ViewModel.
		/// </summary>
		public NotificationHandlerAttribute() {}

        /// <summary>
        /// Defines a handler for notifications of the given key.
        /// <paramref name="key">Name of the event to handle.</paramref>
        /// </summary>
        public NotificationHandlerAttribute(string key)
        {
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

            Key = key;
        }

        #endregion Constructors
    }
}