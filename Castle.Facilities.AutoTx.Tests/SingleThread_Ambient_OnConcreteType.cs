#region license

// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

namespace Castle.Facilities.Transactions.Tests
{
	using System.Transactions;
	using MicroKernel.Registration;
	using NUnit.Framework;
	using TestClasses;
	using Testing;
	using Windsor;

	public class SingleThread_Ambient_OnConcreteType
	{
		private WindsorContainer _Container;

		[SetUp]
		public void SetUp()
		{
			_Container = new WindsorContainer();
			_Container.AddFacility("autotx", new AutoTxFacility());
			_Container.Register(Component.For<ConcreteService>());
		}

		[TearDown]
		public void TearDown()
		{
			_Container.Dispose();
		}

		[Test]
		public void NonRecursive()
		{
			using (var scope = new ResolveScope<ConcreteService>(_Container))
				scope.Service.VerifyInAmbient();
		}

		[Test]
		public void Recursive()
		{
			using (var scope = new ResolveScope<ConcreteService>(_Container))
			{
				scope.Service.VerifyInAmbient(() =>
					scope.Service.VerifyInAmbient(() => Assert.That(System.Transactions.Transaction.Current != null
																	&& System.Transactions.Transaction.Current is DependentTransaction)
				));
			}
		}
	}
}