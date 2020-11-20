using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle.Scoped;

namespace Castle.Windsor.Extensions.DependencyInjection.Scope
{
	public interface IExtensionContainerScope
	{
		void Dispose();
		ExtensionContainerScope Current1 { get; }
	}
}