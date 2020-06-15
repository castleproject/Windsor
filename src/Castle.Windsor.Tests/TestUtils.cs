// Copyright 2018–2020 Castle Project – http://www.castleproject.org/
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

namespace CastleTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.ExceptionServices;
	using System.Text;
	using System.Text.RegularExpressions;

	using NUnit.Framework;

	public static class TestUtils
	{
#if !NETCOREAPP1_0 // FirstChanceException event was added in .NET Core 2.0
		public static void AssertNoFirstChanceExceptions(Action action)
		{
			var firstChanceExceptions = new List<Exception>();

			var handler = new EventHandler<FirstChanceExceptionEventArgs>((sender, e) =>
				firstChanceExceptions.Add(e.Exception));

			AppDomain.CurrentDomain.FirstChanceException += handler;
			try
			{
				action.Invoke();
			}
			finally
			{
				AppDomain.CurrentDomain.FirstChanceException -= handler;
			}

			if (firstChanceExceptions.Any())
			{
				var message = new StringBuilder();
				for (var i = 0; i < firstChanceExceptions.Count; i++)
				{
					message.Append("First-chance exception ").Append(i + 1).Append(" of ").Append(firstChanceExceptions.Count).AppendLine(":");
					message.AppendLine(firstChanceExceptions[i].ToString());
					message.AppendLine();
				}

				message.Append("Expected: no first-chance exceptions.");

				Assert.Fail(message.ToString());
			}
		}
#endif

		public static string ConvertToEnvironmentLineEndings(this string value)
		{
			return Regex.Replace(value, @"\r?\n", Environment.NewLine);
		}
	}
}
