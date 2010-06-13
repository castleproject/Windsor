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

using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core;
using Castle.MicroKernel.Handlers;
using NUnit.Framework;

namespace Castle.Windsor.Tests.Bugs.IoC_78
{
    using Castle.MicroKernel.Registration;

    [TestFixture]
    public class IoC78
    {
        [Test]
        public void WillIgnoreComponentsThatAreAlreadyInTheDependencyTracker_Constructor()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Register(Component.For(typeof(IChain)).ImplementedBy(typeof(MyChain)).Named("chain"));
            container.Register(Component.For(typeof(IChain)).ImplementedBy(typeof(MyChain2)).Named("chain2"));

            IChain resolve = container.Resolve<IChain>("chain2");
            Assert.IsNotNull(resolve);
        }


        [Test]
        public void WillIgnoreComponentsThatAreAlreadyInTheDependencyTracker_Property()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Register(Component.For(typeof(IChain)).ImplementedBy(typeof(MyChain3)).Named("chain"));

            IChain resolve = container.Resolve<IChain>("chain");
            Assert.IsNotNull(resolve);
        }

        [Test]
        [ExpectedException(typeof(HandlerException))]
        public void WillNotTryToResolveAComponentToItself()
        {
            IWindsorContainer container = new WindsorContainer();
            container.Register(Component.For(typeof(IChain)).ImplementedBy(typeof(MyChain4)).Named("chain"));

            container.Resolve<IChain>("chain");
        }
    }

    public interface IChain
    {

    }

    public class MyChain : IChain
    {
        public MyChain()
        {

        }

        public MyChain(IChain chain)
        {

        }
    }

    public class MyChain2 : IChain
    {
        public MyChain2()
        {

        }

        public MyChain2(IChain chain)
        {

        }
    }

    public class MyChain4 : IChain
    {
        public MyChain4(IChain chain)
        {

        }
    }


    public class MyChain3 : IChain
    {
        private IChain chain;

        public IChain Chain
        {
            get { return chain; }
            set { chain = value; }
        }
    }
}
