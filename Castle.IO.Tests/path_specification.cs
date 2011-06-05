using Castle.IO;
using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.Tests.IO
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
		public void trailing_slash_is_always_normalized()
		{
			new Path(@"C:\mordor\nurn").ShouldBe(new Path(@"C:\mordor\nurn\"));
		}

		[Test]
		public void relative_path_is_not_rooted()
		{
			new Path(@"test\folder").IsRooted.ShouldBeFalse();
		}
		[Test]
		public void absolute_path_is_rooted()
		{
			new Path(@"c:\test\folder").IsRooted.ShouldBeTrue();
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
			new Path("folder").MakeRelative(new Path(@"c:\tmp")).FullPath.ShouldBe("folder");
		}
		[TestCase(@"c:\test", @"c:\")]
		[TestCase(@"c:\test\", @"c:\test")]
		public void directory_depends_on_position_of_separator(string path, string directory)
		{
			new Path(path).DirectoryName.ShouldBe(directory);
		}
	}
}