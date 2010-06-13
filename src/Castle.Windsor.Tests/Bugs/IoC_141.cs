using Castle.MicroKernel.Handlers;
using NUnit.Framework;

namespace Castle.MicroKernel.Tests.Bugs
{
    using System;

    using Castle.MicroKernel.Registration;

    [TestFixture]
	public class IoC_141
	{
		[Test]
		public void Can_resolve_open_generic_service_with_closed_generic_parameter()
		{
			var kernel = new DefaultKernel();
			((IKernel)kernel).Register(Component.For(typeof(IProcessor<>)).ImplementedBy(typeof(DefaultProcessor<>)).Named("processor"));
			((IKernel)kernel).Register(Component.For(typeof(IAssembler<object>)).ImplementedBy(typeof(ObjectAssembler)).Named("assembler"));
			Assert.IsInstanceOf(typeof(DefaultProcessor<object>), kernel.Resolve<IProcessor<object>>());
		}

		[Test]
		public void Can_resolve_service_with_open_generic_parameter_with_closed_generic_parameter()
		{
			var kernel = new DefaultKernel();
			((IKernel)kernel).Register(Component.For(typeof(IService)).ImplementedBy(typeof(Service1)).Named("service1"));
			((IKernel)kernel).Register(Component.For(typeof(IProcessor<>)).ImplementedBy(typeof(DefaultProcessor<>)).Named("processor"));
			((IKernel)kernel).Register(Component.For(typeof(IAssembler<object>)).ImplementedBy(typeof(ObjectAssembler)).Named("assembler"));
			Assert.IsInstanceOf(typeof(Service1), kernel.Resolve<IService>());
		}

		[Test]
		[ExpectedException(typeof(HandlerException))]
		public void Throws_right_exception_when_not_found_matching_generic_service()
		{
			var kernel = new DefaultKernel();
			((IKernel)kernel).Register(Component.For(typeof(IProcessor<>)).ImplementedBy(typeof(DefaultProcessor<>)).Named("processor"));
			((IKernel)kernel).Register(Component.For(typeof(IAssembler<object>)).ImplementedBy(typeof(ObjectAssembler)).Named("assembler"));
			kernel.Resolve<IProcessor<int>>();
		}

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
	}

}
