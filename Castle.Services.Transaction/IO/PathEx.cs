namespace Castle.Services.Transaction.IO
{
	public static class PathEx
	{
		public static PathInfo ToPathInfo(this string input)
		{
			return PathInfo.Parse(input);
		}

		/// <summary>
		/// Combines an input path and a path together
		/// using <see cref="System.IO.Path.Combine"/> and returns the result.
		/// </summary>
		public static string Combine(this string input, string path)
		{
			return System.IO.Path.Combine(input, path);
		}
	}
}