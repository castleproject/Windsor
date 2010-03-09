using System;

namespace Castle.Services.Transaction.Misc
{
	/// <summary>
	/// Utility class for whatever is needed to make the code better.
	/// </summary>
	internal static class Fun
	{
		public static void Fire<TEventArgs>(this EventHandler<TEventArgs> handler,
			object sender, TEventArgs args)
			where TEventArgs : EventArgs
		{
			if (handler == null) return;
			handler(sender, args);
		}
	}
}