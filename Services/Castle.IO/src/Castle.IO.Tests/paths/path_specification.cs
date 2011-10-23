using Castle.IO.Extensions;
using NUnit.Framework;
using SharpTestsEx;

namespace Castle.IO.Tests.paths
{
	public class path_specification : context
	{
		[Test]
		public void path_has_segments()
		{
			var path = new Path(@"C:\mordor\nurn");
			path.Segments.ShouldHaveSameElementsAs(new[] { @"C:", "mordor", "nurn" });
		}

		[Test]
		public void path_with_unc_root_doesnt_include_unc_root_in_segments()
		{
			@"\\?\C:\mordor\nurn".ToPath()
				.Segments
				.Should().Not.Contain("?");
		}

		[Test, Ignore("@serialseb: I kind of disagree with this; a trailing slash implies, " 
			+ "it's a directory, but w/o one, it can be either, and we don't know if it's a path.")]
		public void trailing_slash_is_always_normalized()
		{
			new Path(@"C:\mordor\nurn")
				.ShouldBe(new Path(@"C:\mordor\nurn\"));
		}

		[TestCase(@"test\folder")]
		[TestCase(@"test/folder")]
		public void relative_path_is_not_rooted()
		{
			new Path(@"test\folder").IsRooted.Should().Be.False();
		}

		[TestCase(@"c:\test\folder", @"c:\test", "folder")]
		[TestCase(@"c:\test\folder", @"c:\test\another", @"..\folder")]
		[TestCase(@"c:\test\folder", @"c:\test\nested\folder", @"..\..\folder")]
		public void absolute_path_is_made_relative(string source, string basePath, string result)
		{
			new Path(source)
				.MakeRelative(new Path(basePath))
				.FullPath.ShouldBe(result);
		}

		[Test]
		public void relative_path_is_made_relative_by_returning_itself()
		{
			new Path("folder")
				.MakeRelative(new Path(@"c:\tmp"))
				.FullPath.Should().Be("folder");
		}

		[TestCase(@"C:\a", @"C:\")]
		[TestCase(@"C:\a\b", @"C:\a")]
		[TestCase(@"C:\a\b\c", @"C:\a\b")]
		[TestCase(@"C:\a", @"C:\")]
		[TestCase(@"C:\a", @"C:\")]
		[TestCase(@"C:\a\b\c.txt", @"C:\a\b")]
		[TestCase(@"\\?\C:\a\b\c d e.txt", @"C:\a\b")]
		[TestCase(@"\\?\C:\a\b\c d e.txt", @"C:\a\b")]
		[TestCase(@"\\?\C:\a\b\", @"C:\a")]
		public void gettting_without_last_bit_should_act_only_on_folder_and_file_part(
			string firstPart, string result)
		{
			Path.GetPathWithoutLastBit(firstPart)
				.Should().Be(new Path(result));
		}

		[TestCase(@"c:\test", @"c:\")]
		[TestCase(@"c:\test\", @"c:\test\")]
		public void drive_and_directory_depends_on_position_of_separator(string path, string directory)
		{
			new Path(path)
				.DriveAndDirectory
				.Should().Be(directory);
		}
	}
}