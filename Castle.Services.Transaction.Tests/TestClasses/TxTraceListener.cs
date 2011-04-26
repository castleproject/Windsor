#region license

// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

namespace Castle.Services.Transaction.Tests
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Text.RegularExpressions;
	using System.Xml.Serialization;

	[Serializable]
	public class TraceRecord
	{
		public string TraceIdentifier { get; set; }
		public string Description { get; set; }
		public string AppDomain { get; set; }
		public string ExtendedData { get; set; }

		public override string ToString()
		{
			return string.Format("{0}: {1}", Description, ExtendedData);
		}
	}

	public class TxTraceListener : TraceListener
	{
		public override void Write(string message)
		{
			if (message.StartsWith("System.Transactions Verbose: 0 : "))
			{
				Console.Write("Sys.Tx: ({1}) {0}", message.Substring("System.Transactions Verbose: 0 : ".Length),
				              DateTime.UtcNow.ToString("mm:ss.fffff"));
			}
			else Console.Write(message);
		}

		public override void WriteLine(string message)
		{
			TraceRecord r = null;

			if (message.StartsWith("<TraceRecord"))
			{
				using (var s = new StringReader(message))
				{
					r =
						(TraceRecord)
						new XmlSerializer(typeof(TraceRecord), "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord").
							Deserialize(s);
					r.ExtendedData = GetData(message);
				}
			}

			Console.WriteLine(r == null ? message : r.ToString());
		}

		private string GetData(string message)
		{
			if (!message.Contains("<ExtendedData")) return string.Empty;
			var start = message.IndexOf("<ExtendedData");
			var endString = "</ExtendedData>";
			var end = message.IndexOf(endString);
			var removeOuter = new Regex(">(.*)<");
			return removeOuter.Match(message.Substring(start, (end + endString.Length) - start)).Groups[1].Value;
		}

		public override bool IsThreadSafe
		{
			get { return true; }
		}
	}
}