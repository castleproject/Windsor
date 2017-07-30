#if NET45

namespace Castle.Facilities.Owin.WebHost.Lifestyles
{
	using System;
	using System.Web;

	using Castle.Core.Internal;

	internal class MsSystemWebHttpContextScopeWrapper
	{
		private static readonly Lock @lock = Lock.Create();

		public static T GetOrSet<T>(string key, Func<T> create)
		{
			var result = Get<T>(key);
			if (result != null) 
				return result;

			using (@lock.ForWriting())
			{
				result = Get<T>(key);
				if (result != null)
					return result;

				result = create();
				HttpContext.Current.Items[key] = result;
			}

			return result;
		}

		public static T Get<T>(string key)
		{
			using (@lock.ForReading())
			{
				return (T) HttpContext.Current.Items[key];
			}
		}
	}
}

#endif
