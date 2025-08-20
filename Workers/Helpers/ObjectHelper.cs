using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Workers.Helpers
{
	#region EventHandlers

	/// <summary>
	/// Represents a method that validates a requested value of type T and returns the validated value.
	/// </summary>
	/// <typeparam name="T">The type of the requested and validated values.</typeparam>
	/// <param name="requestedValue">The requested value to be validated.</param>
	/// <param name="validatedValue">The validated value, if the validation is successful.</param>
	/// <returns>True if the validation is successful, otherwise false.</returns>
	public delegate bool ValidateValueEventHandler<T>(T requestedValue, out T validatedValue);

    /// <summary>
    /// Represents a method that handles a value change event with the current value.
    /// </summary>
    /// <typeparam name="T">The type of the value being changed.</typeparam>
    /// <param name="currentValue">The current value.</param>
    public delegate void ValueChangedEventHandler<in T>(T currentValue);

    /// <summary>
    /// Represents a method that handles a value change event with both the old and new values.
    /// </summary>
    /// <typeparam name="T">The type of the value being changed.</typeparam>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    public delegate void OldValueChangedEventHandler<in T>(T oldValue, T newValue);

    #endregion EventHandlers

	/// <summary>
	/// Helper class for <see cref="object"/> property get/set operations."/>
	/// </summary>
	[DebuggerStepThrough]
    public static class ObjectHelper
    {
        /// <summary>
        /// Gets the value of a property, initializing it if necessary using the specified initializer function.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="backingField">A reference to the field that stores the property's value. If the field is uninitialized, it will be set
        /// using the <paramref name="initializer"/> function.</param>
        /// <param name="initializer">A function that provides the value to assign to the property if initialization is required. This function is
        /// called only when the property needs to be initialized.</param>
        /// <param name="shouldInitialize">An optional function that determines whether the property should be initialized. If <see langword="null"/>,
        /// the property is initialized if <paramref name="backingField"/> is <see langword="null"/>.</param>
        /// <param name="onInitialized">An optional event handler that is invoked after the property has been initialized. Receives the newly
        /// initialized value as an argument.</param>
        /// <returns>The current value of the property. If the property was uninitialized, returns the value provided by <paramref name="initializer"/>.</returns>
        public static T GetProperty<T>(ref T backingField, Func<T> initializer, Func<bool> shouldInitialize = null, ValueChangedEventHandler<T> onInitialized = null)
        {
            bool refShouldInitialize = (shouldInitialize != null && shouldInitialize()) || (shouldInitialize == null && backingField == null);

            return GetProperty(
                ref backingField,
                initializer: initializer,
                shouldInitialize: ref refShouldInitialize,
                onInitialized: onInitialized
            );
        }

		/// <summary>
		/// Gets the value of a property, initializing it if necessary using the specified initializer function.
		/// </summary>
		/// <typeparam name="T">The type of the property value.</typeparam>
		/// <param name="backingField">A reference to the field that stores the property's value. If the field is uninitialized, it will be set
		/// using the <paramref name="initializer"/> function.</param>
		/// <param name="initializer">A function that provides the value to assign to the property if initialization is required. This function is
		/// called only when the property needs to be initialized.</param>
		/// <param name="shouldInitialize">A reference to a boolean that indicates whether the property should be initialized.</param>
		/// <param name="onInitialized">An optional event handler that is invoked after the property has been initialized. Receives the newly
		/// initialized value as an argument.</param>
		/// <returns>The current value of the property. If the property was uninitialized, returns the value provided by <paramref name="initializer"/>.</returns>
		public static T GetProperty<T>(ref T backingField, Func<T> initializer, ref bool shouldInitialize, ValueChangedEventHandler<T> onInitialized = null)
        {
            if (shouldInitialize)
            {
                T old = backingField;

                backingField = initializer();
                shouldInitialize = false;

                if (!EqualityComparer<T>.Default.Equals(backingField, old))
                    onInitialized?.Invoke(backingField);
            }

            return backingField;
        }

		/// <summary>
		/// Sets the value of a backing field and optionally invokes validation and change notification callbacks.
		/// </summary>
		/// <remarks>The backing field is only updated if the new value is not equal to the current value,
		/// as determined by the specified or default comparer, and if the validation callback (if provided) returns
		/// <see langword="true"/>.</remarks>
		/// <typeparam name="T">The type of the property value.</typeparam>
		/// <param name="backingField">A reference to the field that stores the property's value. The field will be updated if the new value
		/// differs from the current value.</param>
		/// <param name="newValue">The new value to assign to the backing field.</param>
		/// <param name="onChanged">An optional callback invoked after the backing field has been updated. Receives the new value as its
		/// argument.</param>
		/// <param name="beforeChanges">An optional callback invoked before the backing field is updated. Receives the current value as its
		/// argument.</param>
		/// <param name="validateValue">An optional validation callback that determines whether the new value is valid and can modify it before
		/// assignment. If provided, the new value is passed by reference and may be replaced.</param>
		/// <param name="comparer">An optional equality comparer used to determine whether the new value differs from the current value. If
		/// <see langword="null"/>, <see cref="EqualityComparer{T}.Default"/> is used.</param>
		[DebuggerStepThrough]
        public static void SetProperty<T>(
            ref T backingField,
            T newValue,
            ValueChangedEventHandler<T> onChanged = null,
            ValueChangedEventHandler<T> beforeChanges = null,
            ValidateValueEventHandler<T> validateValue = null,
            EqualityComparer<T> comparer = null)
        {
            if (!(comparer ?? EqualityComparer<T>.Default).Equals(backingField, newValue) && (validateValue?.Invoke(newValue, out newValue) ?? true))
            {
                beforeChanges?.Invoke(backingField);
                backingField = newValue;
                onChanged?.Invoke(backingField);
            }
        }
    }
}