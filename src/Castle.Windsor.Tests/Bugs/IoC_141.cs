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

namespace Castle.MicroKernel.Tests.Bugs
{
	using System;

	using Castle.MicroKernel.Handlers;
	using Castle.MicroKernel.Registration;

	using CastleTests;

	using NUnit.Framework;

	[TestFixture]
	public class IoC_141 : AbstractContainerTestCase
	{
		public interface IService
		{
		}

		public interface IProcessor<T>
		{
		}

		public interface IAssembler<T>
		{
		}

		public class Service1 : IService
		{
			public Service1(IProcessor<object> processor)
			{
			}
		}

		public class DefaultProcessor<T> : IProcessor<T>
		{
			public DefaultProcessor(IAssembler<T> assembler)
			{
			}
		}

		public class ObjectAssembler : IAssembler<object>
		{
		}

		[Test]
		public void Can_resolve_open_generic_service_with_closed_generic_parameter()
		{
			Kernel.Register(Component.For(typeof(IProcessor<>)).ImplementedBy(typeof(DefaultProcessor<>)).Named("processor"));
			Kernel.Register(Component.For(typeof(IAssembler<object>)).ImplementedBy(typeof(ObjectAssembler)).Named("assembler"));
			Assert.IsInstanceOf(typeof(DefaultProcessor<object>), Kernel.Resolve<IProcessor<object>>());
		}

		[Test]
		public void Can_resolve_service_with_open_generic_parameter_with_closed_generic_parameter()
		{
			Kernel.Register(Component.For(typeof(IService)).ImplementedBy(typeof(Service1)).Named("service1"));
			Kernel.Register(Component.For(typeof(IProcessor<>)).ImplementedBy(typeof(DefaultProcessor<>)).Named("processor"));
			Kernel.Register(Component.For(typeof(IAssembler<object>)).ImplementedBy(typeof(ObjectAssembler)).Named("assembler"));
			Assert.IsInstanceOf(typeof(Service1), Kernel.Resolve<IService>());
		}
	}
}