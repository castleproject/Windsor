// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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

namespace Castle.Bugs
{
#if !SILVERLIGHT
	using System;
	using System.CodeDom.Compiler;
	using System.Linq;

	using Castle.MicroKernel;

	using Microsoft.CSharp;

	using NUnit.Framework;

	[TestFixture]
	public class IoC_267
	{
		[Test]
		public void When_attemting_to_register_component_descended_from_valuetype_should_not_compile()
		{
			var csharpCode =
				@"
                                using System;
                                using Castle.MicroKernel;
                                using Castle.MicroKernel.Registration;

                                public class ShouldNotCompile 
                                {
                                    public void MethodContainsInvalidCode()
                                    {
                                        DefaultKernel kernel = new DefaultKernel();
                                        kernel.Register(Component.For<Int32>().Instance(1));
                                    }
                                }
                                ";

			var compiler = new CSharpCodeProvider();
			var coreAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Castle.Core").Location;
			var windsorAssembly = typeof(DefaultKernel).Assembly.Location;
			var results = compiler.CompileAssemblyFromSource(new CompilerParameters(new[] { coreAssembly, windsorAssembly }), csharpCode);
			Assert.True(results.Errors.HasErrors);
			Assert.AreEqual("CS0452", results.Errors[0].ErrorNumber);
				// The type 'int' must be a reference type in order to use it as parameter 'S' in the generic type or method 'Castle.MicroKernel.Registration.Component.For<S>()'
		}
	}
#endif
}