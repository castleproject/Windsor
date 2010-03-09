using System;
using System.Collections.Generic;
using System.Threading;
using Castle.Core;
using log4net;

namespace Castle.Services.Transaction
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

		public static void AtomRead(this ReaderWriterLockSlim sem, Action a)
		{
			if (sem == null) throw new ArgumentNullException("sem");
			if (a == null) throw new ArgumentNullException("a");

			sem.EnterReadLock();

			try
			{
				a();
			}
			finally
			{
				sem.ExitReadLock();	
			}
		}

		public static void AtomWrite(this ReaderWriterLockSlim sem, Action a)
		{
			if (sem == null) throw new ArgumentNullException("sem");
			if (a == null) throw new ArgumentNullException("a");

			sem.EnterWriteLock();

			try
			{
				a();
			}
			finally
			{
				sem.ExitWriteLock();
			}
		}

		/// <summary>
		/// Iterates over a sequence and performs the action on each.
		/// </summary>
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (items == null) throw new ArgumentNullException("items");
			if (action == null) throw new ArgumentNullException("action");
			foreach (var item in items) action(item);
		}

		/// <summary>
		/// Given a logger and action, performs the action and catches + logs exceptions.
		/// </summary>
		/// <returns>Whether the lambda was a success or not.</returns>
		public static Result TryAndLog(this ILog logger, Action a)
		{
			try
			{
				a();
				return Result.OK;
			}
			catch (Exception e)
			{
				logger.Error(e);
				return new Result(false, e);
			}
		}

		public static Pair<T,T2> And<T, T2>(this T first, T2 second)
		{
			return new Pair<T, T2>(first, second);
		}
	}

	internal struct Result
	{
		public static Result OK = new Result(true, null);

		private readonly Exception _Ex;
		private readonly bool _Success;

		public Result(bool success, Exception ex)
		{
			_Success = success;
			_Ex = success ? null : ex;
		}

		/// <summary>
		/// Takes a lambda what to do if the result failed. Returns the result so 
		/// that it can be managed in whatevery way is needed.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public Result Exception(Action<Exception> a)
		{
			if (!_Success) a(_Ex);
			return this;
		}

		public Result Success(Action a)
		{
			if (_Success) a();
			return this;
		}
	}

	internal struct Result<T>
	{
		private readonly Exception _Ex;
		private readonly bool _Success;
		private readonly T _Param;

		public Result(bool success, Exception ex, T param)
		{
			_Success = success;
			_Param = param;
			_Ex = success ? null : ex;
		}

		/// <summary>
		/// Takes a lambda what to do if the result failed. Returns the result so 
		/// that it can be managed in whatevery way is needed.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		public Result<T> Exception(Action<Exception> a)
		{
			if (!_Success) a(_Ex);
			return this;
		}

		public Result<T> Success(Action<T> a)
		{
			if (_Success) a(_Param);
			return this;
		}
	}
}