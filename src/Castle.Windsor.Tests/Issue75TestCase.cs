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

namespace CastleTests
{
    using Castle.MicroKernel.Registration;

    using CastleTests.Generics;

    using NUnit.Framework;

    [TestFixture]
    public class Issue75TestCase : AbstractContainerTestCase
    {
        [Test]
        public void SemiOpen_Generics_Still_Resolve()
        {
            this.Container.Register(
            Classes
                .FromAssemblyContaining<ISingleton>()
                .BasedOn<ISingleton>()
                .WithServiceFromInterface(typeof(ISingleton))
                .WithServiceSelf());

            var component = this.Container.Resolve<Component<int, int>>();

            Assert.IsInstanceOf<ISingleton>(component);
            Assert.IsInstanceOf<IOuterService<IWrap1<int>, int>>(component);
            Assert.IsInstanceOf<IOuterService<IWrap2<int>, int>>(component);
            Assert.IsInstanceOf<IService1<int, int>>(component);
            Assert.IsInstanceOf<IService2<int, int>>(component);
        }
    }
}