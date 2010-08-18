// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.WcfIntegration.Behaviors
{
	using System.ServiceModel.Channels;
	using System.Text;
	using System.Xml;

	public class CustomMessageFormatter : AbstractMessageFormatter
	{
		public static readonly CustomMessageFormatter Instance = new CustomMessageFormatter();

		protected override string FormatMessage(Message message, string format)
		{
			var output = new StringBuilder();

			if (string.IsNullOrEmpty(format))
			{
				format = "M";
			}

			if (TestFormat(ref format, 'h'))
			{
				FormattedHeaders(message, output);
			}

			if (format.Length > 0)
			{
				FormattedMessage(message, format[0], output);
			}

			return output.ToString();
		}

		private bool TestFormat(ref string format, char test)
		{
			if (format[0] == test)
			{
				format = format.Substring(1);
				return true;
			}
			return false;
		}

		private void FormattedHeaders(Message message, StringBuilder output)
		{
			foreach (var header in message.Headers)
			{
				output.AppendFormat("\n{0}\n", header);
			}
		}

		private void FormattedMessage(Message message, char format, StringBuilder output)
		{
			using (var writer = CreateWriter(message, format, output))
			{
				switch (format)
				{
					case 'b':
						message.WriteBody(writer);
						break;
					case 'B':
						message.WriteBodyContents(writer);
						break;
					case 's':
						message.WriteStartBody(writer);
						break;
					case 'S':
						message.WriteStartEnvelope(writer);
						break;
					case 'm':
					case 'M':
						message.WriteMessage(writer);
						break;
					default:
						return;
				}

				writer.Flush();
			}
		}

		protected virtual XmlWriter CreateWriter(StringBuilder output)
		{
			return XmlWriter.Create(output);
		}

		protected virtual XmlDictionaryWriter CreateWriter(Message message, char format, StringBuilder output)
		{
			return XmlDictionaryWriter.CreateDictionaryWriter(CreateWriter(output));
		}
	}
}
