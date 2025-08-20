using Workers.Workers.Attributes;
using System.Linq;
using System.Reflection;

namespace Workers.Workers.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="IWork"/> interface to retrieve metadata information.
    /// </summary>
    public static class IWorkExtension
    {
        /// <summary>
        /// Gets the name of the work item based on the <see cref="WorkerNameAttribute"/> applied to its type.
        /// If no attribute is found, it returns the type name of the work item.
        /// </summary>
        /// <param name="work">The work item.</param>
        /// <returns>The name of the work item.</returns>
        public static string GetName(this IWork work)
        {
            return
                work.GetType()
                .GetCustomAttributes<WorkerNameAttribute>()
                .Select(att => att.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .DefaultIfEmpty(work.GetType().Name)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the priorities of the work item based on the <see cref="WorkerPriorityAttribute"/> applied to its type.
        /// If no attribute is found, it returns an empty array.
        /// </summary>
        /// <param name="work">The work item.</param>
        /// <returns>An array of priority values.</returns>
        public static uint[] GetPriorities(this IWork work)
        {
            return
                work.GetType()
                .GetCustomAttributes<WorkerPriorityAttribute>()
                .Select(att => att.Priorities)
                .DefaultIfEmpty(new uint[0])
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the group name of the work item based on the <see cref="WorkerGroupAttribute"/> applied to its type.
        /// </summary>
        /// <param name="work">The work item.</param>
        /// <returns>The group name of the work item, or null if no group is specified.</returns>
        public static string GetGroupName(this IWork work)
        {
            return
                work.GetType()
                .GetCustomAttributes<WorkerGroupAttribute>()
                .Select(att => att.GroupName)
                .FirstOrDefault();
        }

        /// <summary>
        /// Determines whether the work item is set to be executed on error based on the <see cref="WorkerExecuteOnErrorAttribute"/> applied to its type.
        /// </summary>
        /// <param name="work">The work item.</param>
        /// <returns><c>true</c> if the work item is set to be executed on error, otherwise <c>false</c>.</returns>
        public static bool IsExecutedOnError(this IWork work)
        {
            return
                work.GetType()
                .GetCustomAttributes<WorkerExecuteOnErrorAttribute>()
                .Any();
        }

        /// <summary>
        /// Determines whether the work item is non-cancellable based on the <see cref="WorkerNonCancellableAttribute"/> applied to its type.
        /// </summary>
        /// <param name="work">The work item.</param>
        /// <returns><c>true</c> if the work item is non-cancellable, otherwise <c>false</c>.</returns>
        public static bool IsNonCancellable(this IWork work)
        {
            return
                work.GetType()
                .GetCustomAttributes<WorkerNonCancellableAttribute>()
                .Any();
        }
    }
}