#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
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

using System;
using System.Linq;
using Castle.Facilities.AutoTx.Lifestyles;
using Castle.Facilities.FactorySupport;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Services.Transaction;
using Castle.Windsor;
using NUnit.Framework;

namespace Castle.Facilities.AutoTx.Tests
{
	[Explicit("to try things out")]
	internal class Scratch
	{
		[Test]
		public void GetFacilities()
		{
			var c = new WindsorContainer();
			c.AddFacility<FactorySupportFacility>().AddFacility<TypedFactoryFacility>();
			c.Kernel.GetFacilities().Do(Console.WriteLine).Run();
		}

		// v3.1:
		[Test]
		public void HandlerSelector_ReturnsTransientComponent_IsNoAmbientTransaction()
		{
			// given
			var c = new WindsorContainer();

			c.Kernel.AddHandlerSelector(new TransactionManagerCurrentTransactionSelector());

			c.AddFacility<AutoTxFacility>();

			c.Register(
				Component.For<ITransactionalComponent>()
					.ImplementedBy<ExampleTransactionalComponent>()
					.LifeStyle.PerTransaction(),
				Component.For<DependencyA1>(),
				Component.For<DependencyA2>()
				);

			// then
			var component = c.Resolve<ExampleTransactionalComponent>();

			
		}
	}

	internal class DependencyA1
	{
	}

	internal class DependencyA2
	{
	}

	internal class ExampleTransactionalComponent : ITransactionalComponent
	{
		private readonly DependencyA1 _A1;
		private readonly Func<DependencyA2> _A2;

		public ExampleTransactionalComponent(DependencyA1 a1, Func<DependencyA2> a2)
		{
			_A1 = a1;
			_A2 = a2;
		}

		[Transaction]
		public virtual void TransactionalOperation()
		{
		}
	}

	internal interface ITransactionalComponent
	{
	}
}