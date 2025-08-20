using System;
using System.Collections.Generic;
using System.Linq;

namespace Workers.Collections
{
    /// <summary>
    /// Represents a dictionary that maps keys to callback delegates.
    /// Provides methods for adding, removing, and retrieving delegates based on keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TDelegate">The type of the delegate handlers.</typeparam>
    public class CallbackDictionary<TKey, TDelegate> where TDelegate : Delegate
    {
        #region Private Fields

        /// <summary>
        /// The dictionary that holds the mapping of keys to delegate handlers.
        /// </summary>
        private Dictionary<TKey, TDelegate> _handlers;

        /// <summary>
        /// An object to lock access to the dictionary for thread-safety.
        /// </summary>
        private readonly object _syncLock = new();

        #endregion Private Fields

        #region Indexer

        /// <summary>
        /// Gets or sets the handler associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>The delegate handler associated with the specified key.</returns>
        public TDelegate this[TKey key]
        {
            get
            {
                TryGetCallback(key, out var callback);
                return callback;
            }
            set
            {
                AddHandler(key, value);
            }
        }

        #endregion Indexer

        #region Public Methods

        /// <summary>
        /// Gets the number of keys in the dictionary.
        /// </summary>
        public int Count => _handlers?.Count ?? 0;

        /// <summary>
        /// Determines whether the dictionary contains at least one handler for the given key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>True if the dictionary contains a handler for the key, otherwise false.</returns>
        public bool ContainsKey(TKey key) => _handlers?.ContainsKey(key) ?? false;

        /// <summary>
        /// Gets the collection of keys in the dictionary.
        /// </summary>
        public IEnumerable<TKey> Keys => _handlers?.Keys ?? Enumerable.Empty<TKey>();

        /// <summary>
        /// Removes the specified handlers from the dictionary.
        /// </summary>
        /// <param name="handlers">The collection of handlers to remove.</param>
        public void RemoveHandlers(IEnumerable<TDelegate> handlers)
        {
            if (_handlers == null)
                return;

            for (int keyValueIndex = 0; keyValueIndex < _handlers.Count; keyValueIndex++)
            {
                var item = _handlers.ElementAt(keyValueIndex);
                TKey key = item.Key;
                TDelegate hndlrs = item.Value;

                Delegate[] delegates = hndlrs?.GetInvocationList();
                for (int handlerIndex = 0; handlerIndex < delegates.Length; handlerIndex++)
                {
                    var handler = delegates[handlerIndex] as TDelegate;

                    if (handlers.Contains(handler))
                        RemoveHandler(key, handler);
                }
            }
        }

        /// <summary>
        /// Adds the specified handler for the given key.
        /// </summary>
        /// <param name="key">The key to associate with the handler.</param>
        /// <param name="handler">The handler to add.</param>
        public void AddHandler(TKey key, TDelegate handler)
        {
            lock (_syncLock)
            {
                _handlers ??= [];

                if (!_handlers.TryGetValue(key, out TDelegate outAction))
                    _handlers.Add(key, handler);
                else
                    _handlers[key] = Delegate.Combine(outAction, handler) as TDelegate;
            }
        }

        /// <summary>
        /// Removes the specified handler for the given key.
        /// </summary>
        /// <param name="key">The key associated with the handler.</param>
        /// <param name="handler">The handler to remove.</param>
        public void RemoveHandler(TKey key, TDelegate handler)
        {
            if (_handlers != null && _handlers.TryGetValue(key, out TDelegate outAction))
            {
                outAction = Delegate.Remove(outAction, handler) as TDelegate;

                if (outAction == null)
                    _handlers.Remove(key);
                else
                    _handlers[key] = outAction;
            }
        }

        /// <summary>
        /// Removes all handlers associated with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key whose handlers should be removed.</param>
        public void ClearHandlers(TKey key)
        {
            lock (_syncLock)
            {
                if (_handlers?.ContainsKey(key) ?? false)
                    _handlers.Remove(key);
            }
        }

        /// <summary>
        /// Removes all handlers from the dictionary.
        /// </summary>
        public void ClearHandlers()
        {
            lock (_syncLock)
                _handlers?.Clear();
        }

        /// <summary>
        /// Tries to get the callback associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the callback.</param>
        /// <param name="callback">When this method returns, contains the callback associated with the specified key, if the key is found, otherwise null.</param>
        /// <returns>True if the callback is found, otherwise false.</returns>
        public bool TryGetCallback(TKey key, out TDelegate callback)
        {
            lock (_syncLock)
            {
                callback = default;
                return _handlers?.TryGetValue(key, out callback) ?? false;
            }
        }

        /// <summary>
        /// Tries to get and remove the callback associated with the specified key.
        /// </summary>
        /// <param name="key">The key associated with the callback.</param>
        /// <param name="callback">When this method returns, contains the callback associated with the specified key, if the key is found, otherwise null.</param>
        /// <returns>True if the callback is found and removed, otherwise false.</returns>
        public bool TryPopCallback(TKey key, out TDelegate callback)
        {
            if (TryGetCallback(key, out callback))
            {
                ClearHandlers(key);
                return true;
            }

            return false;
        }

        #endregion Public Methods
    }
}