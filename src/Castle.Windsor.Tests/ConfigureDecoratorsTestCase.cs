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

	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[TestFixture]
    public class ConfigureDecoratorsTestCase
    {
        [Test]
        public void ShouldResolveDecoratedComponent()
        {
            WindsorContainer container = new WindsorContainer();
            ((IWindsorContainer)container).Register(Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingServiceDecorator)).Named("DoNothingServiceDecorator"));
            ((IWindsorContainer)container).Register(Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingService)).Named("DoNothingService"));
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
            ((IWindsorContainer)parent).Register(Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingService)).Named("DoNothingService"));
            ((IWindsorContainer)child).Register(Component.For(typeof(IDoSomethingService)).ImplementedBy(typeof(DoSomethingService)).Named("DoSomethingService"));
            Assert.IsNotNull(child.Resolve<IDoNothingService>());
            Assert.IsNotNull(child.Resolve<IDoSomethingService>());
        }

        [Test]
        public void ShouldResolveDecoratedComponentFromParent()
        {
            WindsorContainer parent = new WindsorContainer();
            WindsorContainer child = new WindsorContainer();
            parent.AddChildContainer(child);
            ((IWindsorContainer)parent).Register(Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingServiceDecorator)).Named("DoNothingServiceDecorator"));
            ((IWindsorContainer)parent).Register(Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingService)).Named("DoNothingService"));
            ((IWindsorContainer)child).Register(Component.For(typeof(IDoSomethingService)).ImplementedBy(typeof(DoSomethingService)).Named("DoSometingService"));
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
            ((IWindsorContainer)grandParent).Register(Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingServiceDecorator)).Named("DoNothingServiceDecorator"));
            ((IWindsorContainer)grandParent).Register(Component.For(typeof(IDoNothingService)).ImplementedBy(typeof(DoNothingService)).Named("DoNothingService"));
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
