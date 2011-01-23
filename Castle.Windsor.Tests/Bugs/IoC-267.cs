namespace Castle.Bugs
{
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
            var csharpCode = @"
                                using System;
                                using Castle.MicroKernel;
                                using Castle.MicroKernel.Registration;

                                public class ShouldNotCompile 
                                {
                                    public void MethodContainsInvalidCode()
                                    {
                                        var kernel = new DefaultKernel();
                                        kernel.Register(Component.For<Int32>().Instance(1));
                                    }
                                }
                                ";

            var compiler = new CSharpCodeProvider();
            var coreAssembly = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Castle.Core").Location;
            var windsorAssembly = typeof(DefaultKernel).Assembly.Location;
            var results = compiler.CompileAssemblyFromSource(new CompilerParameters(new[] { coreAssembly, windsorAssembly }), csharpCode);
            Assert.True(results.Errors.HasErrors);
            Assert.AreEqual("CS0452", results.Errors[0].ErrorNumber); // The type 'int' must be a reference type in order to use it as parameter 'S' in the generic type or method 'Castle.MicroKernel.Registration.Component.For<S>()'
        }
    }
}
