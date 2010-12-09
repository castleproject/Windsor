namespace Castle.Facilities.NHibernateIntegration.Internal
{
	using System.Linq;
	using System.Reflection;
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ModelBuilder;

	/// <summary>
	/// Inspect components searching for Session Aware services.
	/// </summary>
	public class NHSessionComponentInspector : IContributeComponentModelConstruction
	{
		internal const string SessionRequiredMetaInfo = "nhfacility.SessionRequiredMetaInfo";

		/// <summary>
		/// Process the model
		/// </summary>
		public void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (model.Implementation.IsDefined(typeof(NHSessionAwareAttribute), true))
			{
				model.Dependencies.Add(new DependencyModel(DependencyType.Service, null, typeof(NHSessionInterceptor), false));

				var methods = model
					.Implementation
					.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy)
					.Where(m => m.IsDefined(typeof(NHSessionRequiredAttribute), false));

				model.ExtendedProperties[SessionRequiredMetaInfo] = methods.ToArray();

				model.Interceptors.Add(new InterceptorReference(typeof(NHSessionInterceptor)));
			}
		}
	}
}