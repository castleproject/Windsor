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

namespace CastleTests
{
    using System.Linq;

    using Castle.Core;
    using Castle.MicroKernel.Handlers;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Tests.ClassComponents;

    using CastleTests.ClassComponents;
    using CastleTests.Components;

    using NUnit.Framework;

    [TestFixture]
    public class ConstructorFilteringInRegistrationTestCase : AbstractContainerTestCase
    {
        [Test]
        public void Can_opt_out_of_selecting_constructor_with_single_filter()
        {
            Container.Register(
                Component.For<A>(),
                Component.For<A2>(),
                Component.For<HasDependenciesCtorAndDefaultCtor>()
                         .ConstructorsIgnore(c => c.GetParameters().Length > 0));

            var item = Container.Resolve<HasDependenciesCtorAndDefaultCtor>();
            Assert.True(item.DefaultCtorCalled);
        }

        [Test]
        public void Can_opt_out_of_selecting_constructor_with_multiple_filters()
        {
            Container.Register(
                Component.For<A>(),
                Component.For<A2>(),
                Component.For<HasDependenciesCtorAndDefaultCtor>()
                         .ConstructorsIgnore(c =>
                         {
                             var parameters = c.GetParameters();
                             return parameters.Any(p => p.ParameterType == typeof(A)) &&
                                    parameters.Length == 1;
                         })
                         .ConstructorsIgnore(c => c.GetParameters().Any(p => p.ParameterType == typeof(A2))));

            var item = Container.Resolve<HasDependenciesCtorAndDefaultCtor>();
            Assert.True(item.DefaultCtorCalled);
        }
    }
}