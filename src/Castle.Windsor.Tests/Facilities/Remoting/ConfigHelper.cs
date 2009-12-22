// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if (!SILVERLIGHT)
namespace Castle.Facilities.Remoting.Tests
{
	using System.Configuration;
	using System.IO;

	using NUnit.Framework;

	sealed class ConfigHelper
	{
		public static string ResolveConfigPath(string configFilePath)
		{
			var appSetting = ConfigurationManager.AppSettings["tests.src"];
			var merged = Path.Combine(appSetting, @"Facilities/Remoting/Configs");
			var path = Path.Combine(merged, configFilePath);
			return ToAbsolute(path);
		}

		private static string ToAbsolute(string path)
		{
			var file = new FileInfo(path);
			Assert.That(file.Exists);
			return file.FullName;
		}
	}
}
#endif
