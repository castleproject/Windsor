using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using SharpTestsEx;
using System.Linq;

namespace Castle.IO.Tests.paths
{
	internal class pathinfo_drive_specification : context
	{
		[Test, Explicit("perf-test")]
		public void getting_stack_overflow()
		{
			var l = new List<Stopwatch>();
			for (var i = 0; i < 10000; i++)
			{
				var w = Stopwatch.StartNew();
				PathInfo.Parse("C:");
				w.Stop();
				l.Add(w);
			}
			Console.WriteLine("mean time: {0}", l.Average(x => x.ElapsedTicks));
		}

		[Test]
		public void feeding_parse_bad_data_to_see_behaviour()
		{
			PathInfo.Parse(@"?\");
			PathInfo.Parse(@"?\a");
			PathInfo.Parse(@"?\b:\d");
		}

		[TestCase(@"\")]
		[TestCase(@"\\.\device\resource\")]
		[TestCase(@"\\server\resource")]
		[TestCase(@"\\server\resource\")]
		[TestCase(@"\\127.0.0.1\resource\")]
		[TestCase(@"/usr/xyz/Home/haskell")]
		public void with_no_drive(string path)
		{
			PathInfo.Parse(path)
				.Drive
				.Should().Be.Empty();
		}

		[TestCase(@"C:\a\b")]
		[TestCase(@"C:\")]
		[TestCase(@"C:\a.txt")]
		[TestCase(@"C:\b\a.txt")]
		public void with_root_as_drive(string path)
		{
			PathInfo.Parse(path)
				.Drive
				.Should().Be(@"C:\");
		}

		[TestCase(@"\\?\C:\a\b")]
		[TestCase(@"\\?\C:\")]
		[TestCase(@"\\?\C:\a.txt")]
		[TestCase(@"\\?\C:\b\a.txt")]
		public void with_unc_root_as_drive(string path)
		{
			PathInfo.Parse(path)
				.Drive
				.Should().Be(@"C:\");
		}
		[TestCase(@"C:a\b", @"a\b")]
		[TestCase(@"C:../a", @"../a")]
		[TestCase(@"C:a.txt", @"a.txt")]
		[TestCase(@"C:b\a.txt", @"b\a.txt")]
		public void with_relative_path_with_drive(string path, string folderAndFiles)
		{
			var info = PathInfo.Parse(path);

			info.RelDrive.Should().Be(@"C:");
			info.FolderAndFiles.Should().Be.EqualTo(folderAndFiles);
		}
	}
}