namespace Castle.MicroKernel.Tests.Registration
{
	using System.Linq;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class WithServiceTestCase:RegistrationTestCaseBase
	{
		[Test]
		public void Component_not_specified_uses_service_itself()
		{
			Kernel.Register(Component.For<CommonImpl1>());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(CommonImpl1), handler.Service);
		}
		
		[Test]
		public void AllTypes_not_specified_uses_service_itself()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)));
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(CommonImpl1), handler.Service);
		}

		[Test]
		public void Self_uses_service_itself()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)).WithService.Self());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(CommonImpl1), handler.Service);
		}

		[Test]
		public void FirstInterface_uses_single_interface()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)).WithService.FirstInterface());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(ICommon), handler.Service);
		}

		[Test]
		public void AllInterfaces_uses_single_interface()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(CommonImpl1)).WithService.AllInterfaces());
			var handler = Kernel.GetAssignableHandlers(typeof(object)).Single();
			Assert.AreEqual(typeof(ICommon), handler.Service);
		}

		[Test]
		public void AllInterfaces_uses_all_implemented_interfaces()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(TwoInterfacesImpl)).WithService.AllInterfaces());
			var handlers = Kernel.GetAssignableHandlers(typeof(object));
			Assert.AreEqual(2, handlers.Length);
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon)));
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon2)));
		}

		[Test]
		public void Base_uses_type_from_BasedOn()
		{
			Kernel.Register(AllTypes.FromThisAssembly().BasedOn<ICommon>().WithService.Base());

			var handlers = Kernel.GetAssignableHandlers(typeof(object));

			Assert.IsNotEmpty(handlers);
			Assert.True(handlers.All(h => h.Service == typeof(ICommon)));
		}

		[Test]
		public void FromInterface_uses_subtypes_of_type_from_BasedOn_but_not_the_type_itself()
		{
			Kernel.Register(AllTypes.FromThisAssembly().BasedOn<ICommon>().WithService.FromInterface());

			var handlers = Kernel.GetAssignableHandlers(typeof(object));

			Assert.IsNotEmpty(handlers);
			Assert.True(handlers.All(h => typeof(ICommon).IsAssignableFrom(h.Service)));
			Assert.True(handlers.Any(h => typeof(ICommon) != h.Service));
		}

		[Test]
		public void Can_cumulate_services()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(TwoInterfacesImpl))
			                	.WithService.AllInterfaces()
			                	.WithService.Self());
			var handlers = Kernel.GetAssignableHandlers(typeof(object));
			Assert.AreEqual(3, handlers.Length);
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon)));
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon2)));
			Assert.True(handlers.Any(h => h.Service == typeof(TwoInterfacesImpl)));
		}

		[Test]
		public void Can_cumulate_services_without_duplication()
		{
			Kernel.Register(AllTypes.FromThisAssembly().Where(t => t == typeof(TwoInterfacesImpl))
			                	.WithService.AllInterfaces()
			                	.WithService.FirstInterface());
			var handlers = Kernel.GetAssignableHandlers(typeof(object));
			Assert.AreEqual(2, handlers.Length);
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon)));
			Assert.True(handlers.Any(h => h.Service == typeof(ICommon2)));
		}
	}
}