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
	using Castle.Core;
	using Castle.MicroKernel.Registration;

	using NUnit.Framework;

	[PerThread]
	public class R
	{
	}

	public interface IC
	{
		IN N { get; set; }
	}

	public class CImpl : IC
	{
		private R _r = null;

		public R R
		{
			set { _r = value; }
		}

		public CImpl()
		{
			N = null;
		}

		public IN N { get; set; }
	}

	public interface IN
	{
		IS CS { get; }
	}

	[Transient]
	public class DN : IN
	{
		private IWM vm;
		private ISP sp;

		public IS CS { get; private set; }

		public DN(IWM vm, ISP sp)
		{
			this.vm = vm;
			this.sp = sp;
			CS = new BS();
		}
	}

	public interface IWM
	{
		void A(IN n);
	}

	public class WM : IWM
	{
		public void A(IN n)
		{
			//...
		}
	}

	public interface IS
	{
		ISP SP { get; set; }
	}

	[Transient]
	public class BS : IS
	{
		private ISP _sp = null;

		public ISP SP
		{
			get { return _sp; }
			set { _sp = value; }
		}
	}

	public interface ISP
	{
		void Save(IS s);
	}

	public class SP : ISP
	{
		public void Save(IS s)
		{
		}
	}

	[TestFixture]
	public class ContainerProblem2
	{
		[Test]
		public void CausesStackOverflow()
		{
			IWindsorContainer container = new WindsorContainer();

			container.Register(Component.For(typeof(IS)).ImplementedBy(typeof(BS)).Named("BS"));
			container.Register(Component.For(typeof(IC)).ImplementedBy(typeof(CImpl)).Named("C"));
			container.Register(Component.For(typeof(IWM)).ImplementedBy(typeof(WM)).Named("WM"));
			container.Register(Component.For(typeof(ISP)).ImplementedBy(typeof(SP)).Named("SP"));

			//TODO: dead code - why is it here?
			// ComponentModel model = new ComponentModel("R", typeof(R), typeof(R));
			// model.LifestyleType = LifestyleType.Custom;
			// model.CustomLifestyle = typeof(PerThreadLifestyleManager);

			// container.Kernel.AddCustomComponent(model);
			// container.Kernel.AddComponent("R", typeof(R), LifestyleType.Thread);
			container.Kernel.Register(Component.For(typeof(R)).Named("R"));

			IC c = container.Resolve<IC>("C");
			Assert.IsNotNull(c);
		}
	}
}