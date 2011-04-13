using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Castle.Services.Transaction.Tests.vNext
{
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
				Console.Write("Sys.Tx: ({1}) {0}", message.Substring("System.Transactions Verbose: 0 : ".Length), DateTime.UtcNow.ToString("mm:ss.fffff"));
			else Console.Write(message);
		}

		public override void WriteLine(string message)
		{
			TraceRecord r = null;

			if (message.StartsWith("<TraceRecord"))
				using (var s = new StringReader(message))
				{
					r = (TraceRecord)new XmlSerializer(typeof(TraceRecord), "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord").Deserialize(s);
					r.ExtendedData = GetData(message);
				}

			Console.WriteLine(r == null ? message : r.ToString());
		}

		private string GetData(string message)
		{
			if (!message.Contains("<ExtendedData")) return string.Empty;
			int start = message.IndexOf("<ExtendedData");
			var endString = "</ExtendedData>";
			int end = message.IndexOf(endString);
			var removeOuter = new Regex(">(.*)<");
			return removeOuter.Match( message.Substring(start, (end + endString.Length)- start) ).Groups[1].Value;
		}

		public override bool IsThreadSafe
		{
			get { return true; }
		}
	}
}