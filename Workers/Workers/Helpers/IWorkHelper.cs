using Workers.Comparers;
using Workers.Workers.Attributes;
using Workers.Workers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Workers.Workers.Helpers
{
    /// <summary>
    /// Provides helper methods for working with <see cref="IWork"/> and <see cref="IWork{TContext}"/> instances.
    /// </summary>
    public static class IWorkHelper
    {
		#region Fields

		/// <summary>
		/// Worker cache that stores the worker declarations for each assembly.
		/// </summary>
		private static readonly IDictionary<Assembly, WorkerDeclarationAttribute[]> s_workersCache = new Dictionary<Assembly, WorkerDeclarationAttribute[]>();

		/// <summary>
		/// Lock object to ensure thread-safe access to the worker cache.
		/// </summary>
		private static readonly object s_lock = new();

        #endregion Fields

        #region Methods

        /// <summary>
        /// Retrieves all workers that are applicable for the specified context from the provided assemblies.
        /// </summary>
        /// <typeparam name="TContext">The type of context required for executing the workers.</typeparam>
        /// <param name="searchAssemblies">The assemblies to search for workers.</param>
        /// <returns>An enumerable collection of workers.</returns>
        public static IEnumerable<IWork<TContext>> GetWorkers<TContext>(params Assembly[] searchAssemblies) where TContext : IWorkContext
        {
            return GetWorkers(typeof(TContext), searchAssemblies).Cast<IWork<TContext>>();
        }

        /// <summary>
        /// Retrieves all workers that are applicable for the specified context from the provided assemblies.
        /// </summary>
        /// <param name="contextType">The type of context required for executing the workers.</param>
        /// <param name="searchAssemblies">The assemblies to search for workers.</param>
        /// <returns>An enumerable collection of workers.</returns>
        public static IEnumerable<IWork> GetWorkers(Type contextType, params Assembly[] searchAssemblies)
        {
            foreach (var assembly in searchAssemblies.Where(a => !s_workersCache.ContainsKey(a)))
            {
                lock (s_lock)
                {
                    if (s_workersCache.ContainsKey(assembly)) continue;

                    s_workersCache.Add(assembly, assembly.GetCustomAttributes<WorkerDeclarationAttribute>().ToArray());
                }
            }

            return
                s_workersCache.Where(c => searchAssemblies.Contains(c.Key))
                .SelectMany(c => c.Value)
                .Where(att => att.IsValid(contextType))
                .Select(att => att.CreateWorker());
        }

        /// <summary>
        /// Retrieves all workers that are applicable for the specified context from the provided assemblies
        /// and orders them based on their priorities.
        /// </summary>
        /// <typeparam name="TContext">The type of context required for executing the workers.</typeparam>
        /// <param name="searchAssemblies">The assemblies to search for workers.</param>
        /// <returns>An enumerable collection of workers ordered by priority.</returns>
        public static IEnumerable<IWork<TContext>> GetOrderedWorkers<TContext>(params Assembly[] searchAssemblies) where TContext : IWorkContext
        {
            return GetOrderedWorkers(typeof(TContext), searchAssemblies).Cast<IWork<TContext>>();
        }

        /// <summary>
        /// Retrieves all workers that are applicable for the specified context from the provided assemblies
        /// and orders them based on their priorities.
        /// </summary>
        /// <param name="contextType">The type of context required for executing the workers.</param>
        /// <param name="searchAssemblies">The assemblies to search for workers.</param>
        /// <returns>An enumerable collection of workers ordered by priority.</returns>
        public static IEnumerable<IWork> GetOrderedWorkers(Type contextType, params Assembly[] searchAssemblies)
        {
            return
                GetWorkers(contextType, searchAssemblies)
                .OrderBy(w => w.GetPriorities(),
                         new LambdaComparer<uint[]>((l, r) =>
                         {
                             for (int index = 0; ; index++)
                             {
                                 if (l.Length <= index && r.Length <= index) return 0;

                                 uint lValue = index < l.Length ? l[index] : 0;
                                 uint rValue = index < r.Length ? r[index] : 0;

                                 if (lValue == rValue) continue;

                                 return (int)lValue - (int)rValue;
                             }
                         }));
        }

        /// <summary>
        /// Retrieves all workers that are applicable for the specified context from the provided assemblies,
        /// orders them based on their priorities, and filters them by a specified group name.
        /// </summary>
        /// <typeparam name="TContext">The type of context required for executing the workers.</typeparam>
        /// <param name="groupName">The name of the group by which to filter the workers.</param>
        /// <param name="searchAssemblies">The assemblies to search for workers.</param>
        /// <returns>An enumerable collection of workers filtered by group name and ordered by priority.</returns>
        public static IEnumerable<IWork<TContext>> GetGroupedOrderedWorkers<TContext>(string groupName = null, params Assembly[] searchAssemblies)
            where TContext : IWorkContext
        {
            return GetGroupedOrderedWorkers(typeof(TContext), groupName, searchAssemblies).Cast<IWork<TContext>>();
        }

        /// <summary>
        /// Retrieves all workers that are applicable for the specified context from the provided assemblies,
        /// orders them based on their priorities, and filters them by a specified group name.
        /// </summary>
        /// <param name="contextType">The type of context required for executing the workers.</param>
        /// <param name="groupName">The name of the group by which to filter the workers.</param>
        /// <param name="searchAssemblies">The assemblies to search for workers.</param>
        /// <returns>An enumerable collection of workers filtered by group name and ordered by priority.</returns>
        public static IEnumerable<IWork> GetGroupedOrderedWorkers(Type contextType, string groupName = null, params Assembly[] searchAssemblies)
        {
            return GetOrderedWorkers(contextType, searchAssemblies).Where(w => w.GetGroupName() == groupName);
        }

        #endregion Methods
    }
}