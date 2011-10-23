namespace Castle.Facilities.WcfIntegration.Tests
{
	using System.ServiceModel;

	using Castle.DynamicProxy;
	using Castle.Facilities.WcfIntegration.Async;
	using NUnit.Framework;

	[TestFixture]
	public class AsyncChannelFactoryFixture
	{
		[Test]
		public void Should_cache_channelFactory_proxy_types()
		{
			var builder = new AsynChannelFactoryBuilder<DefaultClientModel>(new ProxyGenerator());
			var factory1 = builder.CreateChannelFactory<ChannelFactory<IOperations>>(new DefaultClientModel());
			var factory2 = builder.CreateChannelFactory<ChannelFactory<IOperations>>(new DefaultClientModel());
			Assert.AreSame(factory1.GetType(), factory2.GetType());
		}
	}
}