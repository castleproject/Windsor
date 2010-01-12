// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Tests
{
	using System;

	using NUnit.Framework;

	[TestFixture]
    public class ConfigureDecoratorsTestCase
    {
        [Test]
        public void ShouldResolveDecoratedComponent()
        {
            WindsorContainer container = new WindsorContainer();
            container.AddComponent("DoNothingServiceDecorator", typeof(IDoNothingService), typeof(DoNothingServiceDecorator));
            container.AddComponent("DoNothingService", typeof(IDoNothingService), typeof(DoNothingService));
            IDoNothingService service = container.Resolve<IDoNothingService>();
            Assert.IsNotNull(service);
            Assert.IsInstanceOf(typeof(DoNothingServiceDecorator), service);
            Assert.IsInstanceOf(typeof(DoNothingService), ((DoNothingServiceDecorator)service).Inner);
        }

        [Test]
        public void ShouldResolveComponentFromParent()
        {
            WindsorContainer parent = new WindsorContainer();
            WindsorContainer child = new WindsorContainer();
            parent.AddChildContainer(child);
            parent.AddComponent("DoNothingService", typeof(IDoNothingService), typeof(DoNothingService));
            child.AddComponent("DoSomethingService", typeof(IDoSomethingService), typeof(DoSomethingService));
            Assert.IsNotNull(child.Resolve<IDoNothingService>());
            Assert.IsNotNull(child.Resolve<IDoSomethingService>());
        }

        [Test]
        public void ShouldResolveDecoratedComponentFromParent()
        {
            WindsorContainer parent = new WindsorContainer();
            WindsorContainer child = new WindsorContainer();
            parent.AddChildContainer(child);
            parent.AddComponent("DoNothingServiceDecorator", typeof(IDoNothingService), typeof(DoNothingServiceDecorator));
            parent.AddComponent("DoNothingService", typeof(IDoNothingService), typeof(DoNothingService));
            child.AddComponent("DoSometingService", typeof(IDoSomethingService), typeof(DoSomethingService));
            IDoNothingService service = child.Resolve<IDoNothingService>();
            Assert.IsNotNull(service);
            Assert.IsInstanceOf(typeof(DoNothingServiceDecorator), service);
        }

        [Test]
        public void ShouldResolveDecoratedComponentFromGrandParent()
        {
            WindsorContainer grandParent = new WindsorContainer();
            WindsorContainer parent = new WindsorContainer();
            WindsorContainer child = new WindsorContainer();
            grandParent.AddChildContainer(parent);
            parent.AddChildContainer(child);
            grandParent.AddComponent("DoNothingServiceDecorator", typeof(IDoNothingService), typeof(DoNothingServiceDecorator));
            grandParent.AddComponent("DoNothingService", typeof(IDoNothingService), typeof(DoNothingService));
            IDoNothingService service = child.Resolve<IDoNothingService>();
            Assert.IsNotNull(service);
            Assert.IsInstanceOf(typeof(DoNothingServiceDecorator), service);
        }

        private interface IDoNothingService
        {
            void DoNothing();
        }

        private interface IDoSomethingService
        {
            void DoSomething();
        }

        private class DoNothingService : IDoNothingService
        {
            public void DoNothing()
            {
                throw new NotImplementedException();
            }
        }

        private class DoNothingServiceDecorator : IDoNothingService
        {
            public IDoNothingService Inner { get; set; }

            public DoNothingServiceDecorator(IDoNothingService inner)
            {
                Inner = inner;
            }

            public void DoNothing()
            {
                throw new NotImplementedException();
            }
        }

        private class DoSomethingService : IDoSomethingService
        {
            public DoSomethingService(IDoNothingService service)
            {
            }

            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }
    }
}
