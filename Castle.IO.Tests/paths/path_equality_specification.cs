using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpTestsEx;

namespace Castle.IO.Tests.paths
{
	public class path_equality_specification : context
	{
		[Test]
		public void ordinary_root_equality()
		{
			var p = new Path(@"C:\");
			p.Equals(new Path(@"C:\"))
				.Should().Be.True();
		}

		//[Test]
	}
}
