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

namespace CastleTests.Facilities.Startable
{
    using Castle.Core;
    using Castle.Facilities.Startable;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Tests;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class StartableWithPriorityTestCase
    {
        [Test]
        public void Startables_are_started_by_defined_priority()
        {
            using (var container = new WindsorContainer())
            {
                var state = new StartingState();
                container.AddFacility<StartableFacility>(f => f.DeferredStart());
                container.Install(new ActionBasedInstaller(c => c.Register(
                    Component.For<StartingState>().Instance(state),
                    Component.For<StartableWithPriority2>().ImplementedBy<StartableWithPriority2>().StartPriority(10),
                    Component.For<StartableWithPriority1>().ImplementedBy<StartableWithPriority1>().StartPriority(1)
                )));

                Assert.AreEqual(state.Progress, "12");
            }
        }

        [Test]
        public void Startables_Without_priority_are_started_last()
        {
            using (var container = new WindsorContainer())
            {
                var state = new StartingState();
                container.AddFacility<StartableFacility>(f => f.DeferredStart());
                container.Install(new ActionBasedInstaller(c => c.Register(
                    Component.For<StartingState>().Instance(state),
                    Component.For<StartableWithPriority2>().ImplementedBy<StartableWithPriority2>(),
                    Component.For<StartableWithPriority1>().ImplementedBy<StartableWithPriority1>().StartPriority(1)
                )));

                Assert.AreEqual(state.Progress, "12");
            }
        }
    }

    public class StartingState
    {
        public string Progress { get; set; }
        public StartingState()
        {
            Progress = String.Empty;
        }
    }

    public class StartableWithPriority1 : IStartable
    {
        StartingState state;
        public StartableWithPriority1(StartingState state)
        {
            this.state = state;
        }

        public void Start()
        {
            state.Progress += "1";
        }

        public void Stop()
        {

        }
    }

    public class StartableWithPriority2 : IStartable
    {
        StartingState state;
        public StartableWithPriority2(StartingState state)
        {
            this.state = state;
        }

        public void Start()
        {
            state.Progress += "2";
        }

        public void Stop()
        {

        }
    }
}
