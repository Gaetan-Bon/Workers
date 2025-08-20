namespace Workers.Workers
{
	/// <summary>
	/// Worker result type.
	/// </summary>
	public enum WorkerResultType
	{
		/// <summary>
		/// Success result.
		/// </summary>
		Success = 0,

		/// <summary>
		/// Skip result.
		/// </summary>
		Skip = 1,

		/// <summary>
		/// Failed result.
		/// </summary>
		Failed = 2,

		/// <summary>
		/// Error result.
		/// </summary>
		Error = 3,

		/// <summary>
		/// Exception result.
		/// </summary>
		Exception = 4,
	}
}