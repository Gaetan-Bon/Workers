using System.Threading;

namespace Workers.Workers
{
    /// <summary>
    /// Represents a context for a worker process.
    /// </summary>
    public interface IWorkContext
    {
        /// <summary>
        /// Gets the cancellation token for the worker process.
        /// </summary>
        CancellationToken Token { get; set; }
    }
}