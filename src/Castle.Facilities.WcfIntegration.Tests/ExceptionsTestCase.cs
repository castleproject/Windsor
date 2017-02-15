// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.WcfIntegration.Tests
{
	using System;
	using System.Runtime.CompilerServices;
	using Castle.Facilities.WcfIntegration.Internal;
	using NUnit.Framework;

	[TestFixture, IntegrationTest]
	public class ExceptionsTestCase
	{
		[Test]
		public void Preserves_exception_stack_trace()
		{
			var exception = Assert.Throws<InvalidOperationException>(OuterMethod);

			StringAssert.Contains("InnerMethod", exception.StackTrace);
		}


		[MethodImpl(MethodImplOptions.NoInlining)]
		private void OuterMethod()
		{
			try
			{
				InnerMethod();
			}
			catch (InvalidOperationException e)
			{
				var exception = ExceptionHelper.PreserveStackTrace(e);
				throw exception;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void InnerMethod()
		{
			throw new InvalidOperationException("boom!");
		}
	}
}