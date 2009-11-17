namespace Castle.Facilities.LightweighFactory.Tests
{
	using Castle.Core;
	using Castle.MicroKernel.Resolvers;
	using Castle.Windsor;

	using NUnit.Framework;

	[TestFixture]
	public class LightweightFactoryTestCase
	{
		#region Setup/Teardown

		[SetUp]
		public void SetUpTests()
		{
			container = new WindsorContainer();
			// TODO: wrap this with a facility... or nor? We could use ExpressionTreeBasedDelegateBuilder
			// as default with 'poor mans dependency injection'...
			container.AddComponent<ILazyComponentLoader, LightweightFactory>("lightweight factory");
			container.AddComponent<IDelegateBuilder, ExpressionTreeBasedDelegateBuilder>();
		}

		#endregion

		private WindsorContainer container;

		[Test]
		public void Can_resolve_service_via_delegate()
		{
			container.AddComponentLifeStyle<Foo>("MyFoo", LifestyleType.Transient);
			container.AddComponent<UsesFooDelegate>();
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			Foo foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(2, foo.Number);
		}

		[Test]
		public void Delegate_obeys_lifestyle()
		{
			container.AddComponentLifeStyle<Foo>("MyFoo", LifestyleType.Singleton);
			container.AddComponent<UsesFooDelegate>();
			var dependsOnFoo = container.Resolve<UsesFooDelegate>();
			Foo foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
			foo = dependsOnFoo.GetFoo();
			Assert.AreEqual(1, foo.Number);
		}
	}
}