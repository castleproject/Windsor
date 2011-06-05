using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Castle.Transactions.Utils
{
	/// <summary>
	/// 	A functional-programming class which can help in memoizing function calls,
	/// 	i.e. cache them.
	/// </summary>
	public static class Fun
	{
		/// <summary>
		/// 	Memoize this function, indefinately.
		/// </summary>
		/// <typeparam name = "TRes">Result type</typeparam>
		/// <param name = "f">Function to memoize</param>
		/// <returns>A memoized function</returns>
		[Pure]
		public static Func<TRes> Memoize<TRes>(this Func<TRes> f)
		{
			Contract.Requires(f != null);
			Contract.Ensures(Contract.Result<Func<TRes>>() != null, "memoize doesn't return nulls");
			return Memoize(f, TimeSpan.MaxValue);
		}

		/// <summary>
		/// 	See <see cref = "Memoize{TA,TRes}(System.Func{TA,TRes},System.TimeSpan,System.Func{TRes,System.Boolean})" />
		/// </summary>
		/// <typeparam name = "TA"></typeparam>
		/// <typeparam name = "TRes"></typeparam>
		/// <param name = "f"></param>
		/// <returns></returns>
		[Pure]
		public static Func<TA, TRes> Memoize<TA, TRes>(this Func<TA, TRes> f)
		{
			Contract.Requires(f != null);
			Contract.Ensures(Contract.Result<Func<TA, TRes>>() != null, "memoize doesn't return nulls");
			return Memoize(f, TimeSpan.MaxValue, _ => true);
		}

		/// <summary>
		/// 	Memoize this function.
		/// </summary>
		/// <typeparam name = "TA">First function argument</typeparam>
		/// <typeparam name = "TRes">Type of result</typeparam>
		/// <param name = "f">Function to memoize</param>
		/// <param name = "pDur">Duration during which to save the value.</param>
		/// <param name = "keepIt">Whether to save/memoize the function or not</param>
		/// <returns>A memoized function.</returns>
		[Pure]
		public static Func<TA, TRes> Memoize<TA, TRes>(this Func<TA, TRes> f, TimeSpan pDur, Func<TRes, bool> keepIt)
		{
			Contract.Requires(f != null);
			Contract.Requires(keepIt != null);
			Contract.Ensures(Contract.Result<Func<TA, TRes>>() != null);

			var map = new Dictionary<TA, Tuple<TRes, DateTime>>();
			var keepForever = pDur == TimeSpan.MaxValue;

			return a =>
			       	{
			       		Tuple<TRes, DateTime> value;
			       		lock (map)
			       		{
			       			while (true)
			       			{
			       				if (!map.TryGetValue(a, out value)) break;
			       				if (!keepForever && value.Item2.Add(pDur) < DateTime.UtcNow)
			       				{
			       					map.Remove(a);
			       					continue;
			       				}

			       				return value.Item1;
			       			}

			       			var retVal = f(a);

			       			if (!keepIt(retVal))
			       				return retVal;

			       			value = new Tuple<TRes, DateTime>(retVal, DateTime.UtcNow);
			       			map.Add(a, value);
			       		}

			       		return value.Item1;
			       	};
		}

		/// <summary>
		/// 	Memoize this function, a given timespan.
		/// </summary>
		/// <typeparam name = "TRes">Result type</typeparam>
		/// <param name = "f">Function to memoize</param>
		/// <param name = "pDur">Timespan during which to keep the results.</param>
		/// <returns>A memoized function</returns>
		[Pure]
		public static Func<TRes> Memoize<TRes>(this Func<TRes> f, TimeSpan pDur)
		{
			var locker = new object();
			var indefinately = TimeSpan.MaxValue == pDur;
			var called = DateTime.MinValue;
			var value = default(TRes);
			var hasValue = false;
			return () =>
			       	{
			       		// avoiding race-conditions.
			       		lock (locker)
			       		{
			       			if (!hasValue) called = DateTime.UtcNow;
			       			if (!hasValue || (!indefinately && called.Add(pDur) < DateTime.UtcNow))
			       			{
			       				hasValue = true;
			       				value = f();
			       				called = DateTime.UtcNow;
			       			}
			       		}
			       		return value;
			       	};
		}
	}
}