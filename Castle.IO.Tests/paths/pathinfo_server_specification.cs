using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;
using System.Net;

namespace Castle.IO.Tests.paths
{
	public class pathinfo_server_specification : context
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
