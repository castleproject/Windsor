using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SharpTestsEx;

namespace Castle.IO.Tests
{
	internal class pathinfo_devices_specification : context
	{
		[TestCase(@"\\.\dev\res", @"\.\dev")]
		[TestCase(@"\\.\dev\res\", @"\.\dev")]
		[TestCase(@"\\.\dev", @"\.\dev")]
		[TestCase(@"\\.\dev\", @"\.\dev")]
		public void device_regex_should_match(string regex_input, string deviceMatch)
		{
			var matches = Regex.Matches(regex_input, PathInfo.DeviceRegex, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

			matches.Satisfy("the normalized device prefix", x => PathInfo.GetMatch(x, "dev_name") == "dev");
			PathInfo.GetMatch(matches, "device").Should().Be.EqualTo(deviceMatch);
			PathInfo.GetMatch(matches, "dev_prefix").Should().Be.EqualTo(@"\.\");
		}

		[TestCase(@"\\?\.\dev\res", @"\.\dev")]
		[TestCase(@"\\?\.\dev\res\", @"\.\dev")]
		[TestCase(@"\\?\.\dev", @"\.\dev")]
		[TestCase(@"\\?\.\dev\", @"\.\dev")]
		[TestCase(@"\\?\.\my-device", @"\.\my-device")]
		[TestCase(@"\\?\.\my-device\resource1", @"\.\my-device")]
		[TestCase(@"\\?\UNC\.\my-device\resource1", @"\.\my-device")]
		public void device_regex_should_match_unc_prefix(string regex_input, string deviceMatch)
		{
			var matches = Regex.Matches(regex_input, PathInfo.DeviceRegex, RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

			matches.Satisfy("the normalized device prefix", x => 
				PathInfo.GetMatch(x, "dev_name") == "dev"
				|| PathInfo.GetMatch(x, "dev_name") == "my-device");
			PathInfo.GetMatch(matches, "device").Should().Be.EqualTo(deviceMatch);
			PathInfo.GetMatch(matches, "dev_prefix").Should().Be.EqualTo(@"\.\");
		}

		[TestCase(@"\\?\.\my-device")]
		[TestCase(@"\\?\.\my-device\resource1")]
		[TestCase(@"\\?\UNC\.\my-device\resource1")]
		public void device_paths_for_named_device(string device)
		{
			var info = PathInfo.Parse(device);
			info.DevicePrefix.Should().Be.EqualTo(@"\.\");
			info.Satisfy(x => x.UNCPrefix == @"\\?" || x.UNCPrefix == @"\\?\UNC");
			info.IsRooted.Should().Be.True();
			info.DeviceName.Should().Be.EqualTo("my-device");
			info.Device.Should().StartWith(@"\.\my-device");
		}

		[TestCase(@"\\?\.\{834F1858-7E98-46DC-B36C-283C648A7C33}")]
		[TestCase(@"\\.\{834F1858-7E98-46DC-B36C-283C648A7C33}")]
		public void device_paths_for_guid_device(string device)
		{
			PathInfo.Parse(device)
				.Satisfy(x => x.DeviceGuid == new Guid("834F1858-7E98-46DC-B36C-283C648A7C33"));
		}
		
	}
}