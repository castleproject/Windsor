using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Castle.Windsor.Extensions.DependencyInjection.Tests
{
	internal sealed class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
	{
	}

	internal sealed class RealTestServiceCollection : ServiceCollection
	{
	}
}
