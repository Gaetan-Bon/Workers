using System;
using System.Collections.Generic;

namespace Workers.Comparers
{
	/// <summary>
	/// Lambda expression comparer.
	/// </summary>
	/// <typeparam name="T">Type of the items to compare.</typeparam>
	/// <remarks>
	/// Initializes a new instance of the <see cref="LambdaComparer{T}"/> class with a specified comparison function.
	/// </remarks>
	/// <param name="compareFunc">Compare function.</param>
	public class LambdaComparer<T>(Func<T, T, int> compareFunc) : IComparer<T>
    {
		#region Properties

		/// <summary>
		/// Compare function.
		/// </summary>
		public Func<T, T, int> CompareFunc { get; } = compareFunc ?? throw new ArgumentNullException(nameof(compareFunc));

		#endregion Properties

		#region IEqualityComparer<T> implementation

		/// <summary>
		/// Compare method, <see cref="IComparer{T}"/>.
		/// </summary>
		/// <param name="x">First item to compare.</param>
		/// <param name="y">Second item to compare.</param>
		/// <returns>Comparison result as int.</returns>
		public int Compare(T x, T y)
        {
            return CompareFunc.Invoke(x, y);
        }

		#endregion IEqualityComparer<T>  implementation
	}
}