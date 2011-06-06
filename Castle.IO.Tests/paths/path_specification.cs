using NUnit.Framework;
using SharpTestsEx;
using System.Net;

namespace Castle.IO.Tests
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
		[TestCase(@"C:\a\b\c", @"C:\")]
		[TestCase(@"C:\a", @"C:\")]
		[TestCase(@"C:\a", @"C:\")] // TODO
		public void gettting_without_last_bit_should_act_only_on_folder_and_file_part()
		{
			Path.GetPathWithoutLastBit(@"C:\a")
				.Should().Be(@"C:\");
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

	public class path_equality_specification : context
	{
		[Test]
		public void ordinary_root_equality()
		{
			var p = new Path(@"C:\");
			p.Equals(new Path(@"C:\"))
				.Should().Be.True();

			p.Equals(new Path(@"\\?\C:\"))
				.Should().Be.True();
		}
	}

	public class path_info_server_specification : context
	{
		[TestCase(@"\\server\resource", @"\server", "resource")]
		[TestCase(@"\\server", @"\server", "")]
		[TestCase(@"\\?\server\resource", @"\server", "resource")]
		[TestCase(@"\\?\server\resource\resource2", @"\server", @"resource\resource2")]
		[TestCase(@"\\server\resource", @"\server", "resource")]
		[TestCase(@"\\server-with-dash\resource", @"\server-with-dash", "resource")]
		[TestCase(@"\\?\UNC\server\resource", @"\server", "resource")]
		[TestCase(@"\\?\UNC\server-with-dash\resource", @"\server-with-dash", "resource")]
		[TestCase(@"\\server", @"\server", "")]
		[TestCase(@"\\?\UNC\server", @"\server", "")]
		[TestCase(@"\\?\UNC\server\resource", @"\server", "resource")]
		public void named_server_path(string input, string serverMatch, string nonRootPath)
		{
			var i = PathInfo.Parse(input);
			i.Server.Should().Be(serverMatch);
			i.ServerName.Satisfy(x => x == "server" || x == "server-with-dash");
			i.NonRootPath.Should().Be(nonRootPath);
			i.Type.Should().Be.EqualTo(PathType.Server);
		}

		[TestCase(@"\\[::1]\resource", "::1", "resource")]
		[TestCase(@"\\[2001:45:35:22::1]\resource", "2001:45:35:22::1", "resource")]
		[TestCase(@"\\[fe80::1cf3:8a4d:ab4:665e]\$resource$", "fe80::1cf3:8a4d:ab4:665e", "$resource$")]
		[TestCase(@"\\[fe80:1cf3::8a4d:ab4:665e]\resource", "fe80:1cf3::8a4d:ab4:665e", "resource")]
		public void ipv6_server_path(string input, string serverIp, string nonRootPath)
		{
			var i = PathInfo.Parse(input);

			i.IPv4.Should().Be(IPAddress.None);
			i.IPv6.Should().Be.EqualTo(IPAddress.Parse(serverIp));
			i.NonRootPath.Should().Be.EqualTo(nonRootPath);
			i.Type.Should().Be.EqualTo(PathType.IPv6);
		}

		[TestCase(@"\\[::12345]\resource")]
		public void invalid_ipv5_server_path(string input)
		{
			var i = PathInfo.Parse(input);
			i.IPv6.Should().Be.EqualTo(IPAddress.IPv6None);
			i.NonRootPath.Should().Be.EqualTo("resource");
			// later:
			//i.Type.Should().Be.EqualTo(PathType.Invalid);
		}


	}
}