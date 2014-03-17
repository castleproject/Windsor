// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilities.Logging
{
	using System;
	using System.Diagnostics;

	using Castle.Core;
	using Castle.Core.Logging;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Custom resolver used by Windsor. It gives
	///   us some contextual information that we use to set up a logging
	///   before satisfying the dependency
	/// </summary>
	public class LoggerResolver : ISubDependencyResolver
	{
		private readonly IExtendedLoggerFactory extendedLoggerFactory;
		private readonly ILoggerFactory loggerFactory;
		private readonly string logName;

		public LoggerResolver(ILoggerFactory loggerFactory)
		{
			if (loggerFactory == null)
			{
				throw new ArgumentNullException("loggerFactory");
			}

			this.loggerFactory = loggerFactory;
		}

		public LoggerResolver(IExtendedLoggerFactory extendedLoggerFactory)
		{
			if (extendedLoggerFactory == null)
			{
				throw new ArgumentNullException("extendedLoggerFactory");
			}

			this.extendedLoggerFactory = extendedLoggerFactory;
		}

		public LoggerResolver(ILoggerFactory loggerFactory, string name) : this(loggerFactory)
		{
			logName = name;
		}

		public LoggerResolver(IExtendedLoggerFactory extendedLoggerFactory, string name) : this (extendedLoggerFactory)
		{
			logName = name;
		}

		public bool CanResolve(CreationContext context, ISubDependencyResolver parentResolver, ComponentModel model, DependencyModel dependency)
		{
			return dependency.TargetType == typeof(ILogger) || dependency.TargetType == typeof(IExtendedLogger);
		}

		public object Resolve(CreationContext context, ISubDependencyResolver parentResolver, ComponentModel model, DependencyModel dependency)
		{
			Debug.Assert(CanResolve(context, parentResolver, model, dependency));
			if (extendedLoggerFactory != null)
			{
				return string.IsNullOrEmpty(logName) 
					? extendedLoggerFactory.Create(model.Implementation) 
					: extendedLoggerFactory.Create(logName).CreateChildLogger(model.Implementation.FullName);
			}

			Debug.Assert(loggerFactory != null);
			return string.IsNullOrEmpty(logName) 
				? loggerFactory.Create(model.Implementation)
				: loggerFactory.Create(logName).CreateChildLogger(model.Implementation.FullName);
		}
	}
}