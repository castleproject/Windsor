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
	using System;
	using System.ServiceModel.Channels;

	public abstract class AbstractMessageFormatter : IFormatProvider, ICustomFormatter
	{
		public object GetFormat(Type formatType)
		{
			return (typeof(ICustomFormatter).Equals(formatType)) ? this : null;
		}

		public string Format(string format, object arg, IFormatProvider formatProvider)
		{
			if (arg == null)
			{
				return string.Empty;
			}

			var message = arg as Message;

			if (message != null)
			{
				return FormatMessage(message, format);
			}

			return arg.ToString();
		}

		protected abstract string FormatMessage(Message message, string format);
	}
}
