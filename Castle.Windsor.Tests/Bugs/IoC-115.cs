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

using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Windsor.Tests.Bugs
{
    using System;

    using Castle.MicroKernel.Registration;

    [TestFixture]
    public class IoC_115
    {
        [Test]
        public void  Cannot_resolve_a_dependency_from_a_parent_container_under_certain_circumstances()
        {
            WindsorContainer parent = new WindsorContainer();
            WindsorContainer child = new WindsorContainer();

            parent.AddChildContainer(child);

            ((IWindsorContainer)parent).Register(Component.For(typeof(IParentService)).ImplementedBy(typeof(ParentService)).Named("service"));
            ((IWindsorContainer)child).Register(Component.For(typeof(IChildService1)).ImplementedBy(typeof(ChildService1)).Named("service1"));
            ((IWindsorContainer)child).Register(Component.For(typeof(IChildService2)).ImplementedBy(typeof(ChildService2)).Named("service2"));


            child.Resolve<IChildService1>();
        }


        [Test]
        public void Should_resolve_child_from_childs_container()
        {
            WindsorContainer parent = new WindsorContainer();
            WindsorContainer child = new WindsorContainer();

            parent.AddChildContainer(child);

            ((IWindsorContainer)parent).Register(Component.For(typeof(IParentService)).ImplementedBy(typeof(ParentService)).Named("service1"));
            ((IWindsorContainer)parent).Register(Component.For(typeof(IChildService2)).ImplementedBy(typeof(ChildService2)).Named("service3"));
            ((IWindsorContainer)child).Register(Component.For(typeof(IParentService)).ImplementedBy(typeof(AnotherParentService)).Named("service2"));

			IChildService2 resolve = child.Resolve<IChildService2>();
            Assert.IsInstanceOf(typeof(AnotherParentService),resolve.Parent);
        }

        public interface IParentService
        {
        }

        public class ParentService : IParentService
        {
        }

        public interface IChildService1
        {
        }

        public class ChildService1 : IChildService1
        {
            public ChildService1(IChildService2 xxx)
            {
            }
        }

        public interface IChildService2
        {
            IParentService Parent { get; }
        }

        public class ChildService2 : IChildService2
        {
            private readonly IParentService xxx;

            public ChildService2(IParentService xxx)
            {
                this.xxx = xxx;
            }

            public IParentService Parent
            {
                get { return xxx; }
            }
        }

        public class AnotherParentService : IParentService
        {
        }
    }
}