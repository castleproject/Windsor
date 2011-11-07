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

namespace Castle.Windsor.Tests.Bugs.IoC_169
{
	using Castle.Core;
	using Castle.Facilities.Startable;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;

	using NUnit.Framework;

	public interface IBlackboard
	{
	}

	public interface IChalk
	{
	}

	public class Chalk : IChalk
	{
	}

	public abstract class AbstractBlackboard : IBlackboard, IStartable
	{
		public static bool Started;

		public void Start()
		{
			Started = true;
		}

		public void Stop()
		{
		}

		public static void PrepareForTest()
		{
			Started = false;
		}
	}

	public class Blackboard : AbstractBlackboard
	{
		public Blackboard(IChalk chalk)
		{
		}
	}

	public interface IServiceWithoutImplementation
	{
	}

	[TestFixture]
	public class IoC_169
	{
		[Test]
		public void BulkRegistrations_WhenRegistrationMatchesNoInstancesOfService_StopsStartableFacilityFromWorking()
		{
			AbstractBlackboard.PrepareForTest();

			var container = new WindsorContainer();

			container.AddFacility(new StartableFacility());

			container.Register(Component.For(typeof(IBlackboard)).ImplementedBy(typeof(Blackboard)).Named("blackboard"));

			var registrations = AllTypes.
				FromAssembly(GetType().Assembly)
				.BasedOn<IServiceWithoutImplementation>()
				.Unless(t => container.Kernel.HasComponent(t));

			container.Register(registrations);

			container.Kernel.Register(Component.For<IChalk>().Named("chalk").Instance(new Chalk()));

			Assert.True(AbstractBlackboard.Started); // fails here, service is never started
		}
	}
}