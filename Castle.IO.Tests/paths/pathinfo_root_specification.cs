using NUnit.Framework;
using SharpTestsEx;

namespace Castle.IO.Tests
{
	internal class pathinfo_root_specification : context
	{
		[TestCase(@"C:\", @"C:\")]
		[TestCase(@"c:\", @"c:\")]
		[TestCase(@"ABC:\", @"ABC:\")]
		[TestCase(@"/", @"/")]
		[TestCase(@"\", @"\")]
		[TestCase(@"C:/", @"C:/")]
		[TestCase(@"\\?\C:/", @"\\?\C:/")]
		[TestCase(@"/dev", @"/")]
		[TestCase(@"\\[::1]\local_dir\file.txt.bak", @"\\[::1]\")]
		public void path_info_with_root(string path, string root)
		{
			PathInfo.Parse(path)
				.Root
				.Should(" As passed in the test fixture")
				.Be(root);

			PathInfo.Parse(path.Substring(root.Length))
				.IsRooted
				.Should("also, the path without the root must be non-root").Be.False();
		}
		
		[TestCase(@"c:\test\folder")]
		[TestCase(@"\\?\c:\test\folder")]
		[TestCase(@"\\?\UNC\c:\test\folder")]
		[TestCase(@"\folder")]
		[TestCase(@"/folder")]
		[TestCase(@"\\server\folder", Description = "unc by name-paths")]
		[TestCase(@"\\192.168.100.30\folder", Description = "unc by ipv4")]
		[TestCase(@"\\[::1]\folder", Description = "unc by ipv6")]
		[TestCase(@"C:/folder", Description = "Forward slashes.")]
		[TestCase(@"C:/test/folder")]
		[TestCase(@"C:\test\folder")]
		[TestCase(@"c:\test\folder\")]
		public void path_info_with_root_further(string dir)
		{
			new Path(dir)
				.IsRooted
				.Should().Be.True();
		}

		[TestCase(@"../file")]
		[TestCase(@"../file/a")]
		[TestCase(@"../file/a/b.txt")]
		public void path_info_relative(string path)
		{
			var info = PathInfo.Parse(path);

			info.Satisfy(x => x.IsRooted == false &&
			                  x.FolderAndFiles.StartsWith("../file"));
		}

		[TestCase(@"C:../file")]
		[TestCase(@"C:../dir/a")]
		[TestCase(@"C:../$dir$/b/c.txt")]
		public void path_info_relative_with_reldrive(string path)
		{
			var pathInfo = PathInfo.Parse(path);

			pathInfo.Satisfy(
				pi => pi.IsRooted == false && pi.RelDrive == "C:"
				);
		}
	}
}