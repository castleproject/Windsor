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

namespace Castle.MicroKernel.SubSystems.Conversion
{
	using System;

	using Castle.Core.Configuration;

	/// <summary>
	///   Base implementation of <see cref = "ITypeConverter" />
	/// </summary>
	[Serializable]
	public abstract class AbstractTypeConverter : ITypeConverter
	{
		public ITypeConverterContext Context { get; set; }

		public abstract bool CanHandleType(Type type);

		public abstract object PerformConversion(String value, Type targetType);

		public abstract object PerformConversion(IConfiguration configuration, Type targetType);

		/// <summary>
		///   Returns true if this instance of <c>ITypeConverter</c>
		///   is able to handle the specified type with the specified
		///   configuration
		/// </summary>
		/// <param name = "type"></param>
		/// <param name = "configuration"></param>
		/// <returns></returns>
		/// <remarks>
		///   The default behavior is to just pass it to the normal CanHadnleType
		///   peeking into the configuration is used for some advanced functionality
		/// </remarks>
		public virtual bool CanHandleType(Type type, IConfiguration configuration)
		{
			return CanHandleType(type);
		}

		public TTarget PerformConversion<TTarget>(String value)
		{
			return (TTarget)PerformConversion(value, typeof(TTarget));
		}

		public TTarget PerformConversion<TTarget>(IConfiguration configuration)
		{
			return (TTarget)PerformConversion(configuration, typeof(TTarget));
		}
	}
}