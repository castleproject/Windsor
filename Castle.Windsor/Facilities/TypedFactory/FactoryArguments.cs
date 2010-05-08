namespace Castle.Facilities.LightweighFactory
{
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;

	public class FactoryArguments : Arguments
	{
		protected override void AddStores(IList<IArgumentsStore> list)
		{
			base.AddStores(list);
			list.Add(new FactoryParametersStore());

		}
	}
}