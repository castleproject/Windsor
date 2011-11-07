using Castle.IO.Extensions;
using NUnit.Framework;
using SharpTestsEx;

namespace Castle.IO.Tests.paths
{
	public class path_equality_specification : context
	{
		[Test]
		public void identical_root_equality()
		{
			var p = new Path(@"C:\");
			p.Equals(new Path(@"C:\"))
				.Should().Be.True();
		}

		[TestCase(@"\\?\C:\", @"C:\")]
		[TestCase(@"\\?\C:\a", @"C:\a")]
		[TestCase(@"\\?\C:\a\b", @"C:\a\b")]
		[TestCase(@"\\?\C:\a\b.txt", @"C:\a\b.txt")]
		[TestCase(@"\\?\C:\a\b\", @"C:\a\b\")]
		[TestCase(@"\\?\server\b\", @"\\server\b\")]
		[TestCase(@"\\?\server-a\", @"\\server-a",
			Description = "In this case they are actual equal, because the second MUST mean the root of the server")]
		[TestCase(@"\\?\.\COM2\r1\r2", @"\\.\COM2\r1\r2")]
		public void unc_root_equality(string a_path, string equal_to)
		{
			a_path.ToPath()
				.Should().Be.EqualTo(
					equal_to.ToPath()
				);

			a_path.ToPath().GetHashCode()
				.Should().Be.EqualTo(
					equal_to.ToPath().GetHashCode()
				);
		}

		[TestCase(@"\\?\C:\", @"C:\a")]
		[TestCase(@"\\?\C:\a", @"C:\")]
		[TestCase(@"\\?\C:\a\b", @"C:\a\b\")]
		[TestCase(@"\\?\C:\a\b.txt", @"C:\a\b")]
		[TestCase(@"\\?\C:\a\b\", @"C:\a\b")]
		[TestCase(@"\\?\server\b\", @"\\server\b")]
		[TestCase(@"\\?\server-a", @"\\server-a\a")]
		[TestCase(@"\\?\.\COM2\r1\r2", @"\\.\COM2\r1\r2\")]
		public void not_equal(string a_path, string other)
		{
			a_path.ToPath()
				.Should().Not.Be.EqualTo(
					other.ToPath()
				);

			a_path.ToPath().GetHashCode()
				.Should().Not.Be.EqualTo(
					other.ToPath().GetHashCode()
				);
		}
	}
}
