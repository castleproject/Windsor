#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Diagnostics.CodeAnalysis;

namespace System.Diagnostics.Contracts
{
	/// <summary>
	/// 	Enables factoring legacy if-then-throw into separate methods for reuse and full control over
	/// 	thrown exception and arguments
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[Conditional("CONTRACTS_FULL")]
	internal sealed class ContractArgumentValidatorAttribute : Attribute
	{
	}

	/// <summary>
	/// 	Enables writing abbreviations for contracts that get copied to other methods
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	[Conditional("CONTRACTS_FULL")]
	internal sealed class ContractAbbreviatorAttribute : Attribute
	{
	}

	/// <summary>
	/// 	Allows setting contract and tool options at assembly, type, or method granularity.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	[Conditional("CONTRACTS_FULL")]
	internal sealed class ContractOptionAttribute : Attribute
	{
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "category",
			Justification = "Build-time only attribute")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "setting",
			Justification = "Build-time only attribute")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "toggle",
			Justification = "Build-time only attribute")]
		public ContractOptionAttribute(string category, string setting, bool toggle)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "category",
			Justification = "Build-time only attribute")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "setting",
			Justification = "Build-time only attribute")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value",
			Justification = "Build-time only attribute")]
		public ContractOptionAttribute(string category, string setting, string value)
		{
		}
	}
}