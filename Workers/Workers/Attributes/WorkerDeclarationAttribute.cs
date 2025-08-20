using System;

namespace Workers.Workers.Attributes
{
	/// <summary>
	/// Attribute used to declare a worker type for the assembly.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see cref="WorkerDeclarationAttribute"/> class with the specified worker type.
	/// </remarks>
	/// <param name="workerType">The type of the worker.</param>
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class WorkerDeclarationAttribute(Type workerType) : Attribute
    {
		#region Properties

		/// <summary>
		/// Type of the worker.
		/// </summary>
		public Type WorkerType { get; } = workerType;

		#endregion Properties

		#region Methods

		/// <summary>
		/// Determines whether the worker type specified by this attribute is valid for the given context type.
		/// </summary>
		/// <param name="contextType">The type of context required for executing the worker.</param>
		/// <returns><c>true</c> if the worker type is valid, otherwise <c>false</c>.</returns>
		public bool IsValid(Type contextType)
        {
            return
                WorkerType != null
                && typeof(IWork<>).MakeGenericType(contextType).IsAssignableFrom(WorkerType)
                && WorkerType.GetConstructor(Array.Empty<Type>()) != null;
        }

        /// <summary>
        /// Determines whether the worker type specified by this attribute is valid for the given context type <typeparamref name="TContext"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of context required for executing the worker.</typeparam>
        /// <returns><c>true</c> if the worker type is valid, otherwise <c>false</c>.</returns>
        public bool IsValid<TContext>() where TContext : IWorkContext
        {
            return IsValid(typeof(TContext));
        }

        /// <summary>
        /// Creates an instance of the worker specified by this attribute.
        /// </summary>
        /// <returns>An instance of the worker.</returns>
        public IWork CreateWorker()
        {
            return (IWork)Activator.CreateInstance(WorkerType);
        }

        /// <summary>
        /// Creates an instance of the worker specified by this attribute for the given context type <typeparamref name="TContext"/>.
        /// </summary>
        /// <typeparam name="TContext">The type of context required for executing the worker.</typeparam>
        /// <returns>An instance of the worker.</returns>
        public IWork<TContext> CreateWorker<TContext>() where TContext : IWorkContext
        {
            return (IWork<TContext>)CreateWorker();
        }

        #endregion Methods
    }
}