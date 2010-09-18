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

namespace Castle.Facilities.NHibernateIntegration.Tests.SessionCreation
{
	using MicroKernel.Registration;
	using NUnit.Framework;

	[TestFixture]
	public class DaoTestCase : AbstractNHibernateTestCase
	{
		protected override void ConfigureContainer()
		{
			container.Register(Component.For<MyDao>().Named("mydao"));
			container.Register(Component.For<MySecondDao>().Named("myseconddao"));
		}

		[Test]
		public void SessionIsShared()
		{
			MyDao dao = container.Resolve<MyDao>();

			dao.PerformComplexOperation1();
		}

		[Test]
		public void SessionDisposedIsNotReused()
		{
			MyDao dao = container.Resolve<MyDao>();

			dao.PerformComplexOperation2();
		}

		[Test]
		public void ClosingAndDisposing()
		{
			MyDao dao = container.Resolve<MyDao>();

			dao.DoOpenCloseAndDispose();
		}
	}
}
