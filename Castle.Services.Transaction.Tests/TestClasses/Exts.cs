using System.IO;

namespace Castle.Services.Transaction.Tests
{
	public static class Exts
	{
		/// <summary>
		/// Combines an input path and a path together
		/// using <see cref="System.IO.Path.Combine"/> and returns the result.
		/// </summary>
		public static string Combine(this string input, string path)
		{
			return System.IO.Path.Combine(input, path);
		}

		/// <summary>
		/// Combines two paths and makes sure the 
		/// DIRECTORY resulting from the combination exists
		/// by creating it with default permissions if it doesn't.
		/// </summary>
		/// <param name="input">The path to combine the latter with.</param>
		/// <param name="path">The latter path.</param>
		/// <returns>The combined path string.</returns>
		public static string CombineAssert(this string input, string path)
		{
			string p = input.Combine(path);

			if (!Directory.Exists(p))
				Directory.CreateDirectory(p);

			return p;
		}
	}
}