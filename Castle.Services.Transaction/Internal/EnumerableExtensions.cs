using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Castle.Services.Transaction.Internal
{
	/// <summary>
	/// Helper class with items originally from reactive extensions.
	/// </summary>
	public static class EnumerableExtensions
	{
		public static IEnumerable<T> Run<T>(this IEnumerable<T> items)
		{
			Contract.Requires(items != null);
			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			T x;
			foreach (var item in items)
				x = item;

			return items;
		}

		public static IEnumerable<T> Run<T>(this IEnumerable<T> items, Action<T> sideEffect)
		{
			Contract.Requires(items != null);
			Contract.Requires(sideEffect != null);
			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			foreach (var item in items)
				sideEffect(item);

			return items;
		}

		/// <summary>
		/// Perform an action as the sequence is enumerated. Is NOT eager, so you must call
		/// an eager operator to start the side-effects.
		/// </summary>
		public static IEnumerable<T> Do<T>(this IEnumerable<T> items, Action<T> sideEffect)
		{
			Contract.Requires(items != null);
			Contract.Requires(sideEffect != null);
			Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

			foreach (var item in items)
			{
				sideEffect(item);
				yield return item;
			}
		}
	}
}