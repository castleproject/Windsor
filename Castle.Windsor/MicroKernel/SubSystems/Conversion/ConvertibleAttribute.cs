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

	using Castle.Core.Internal;

	/// <summary>
	///   Declares a type as being convertible by a <see cref = "ITypeConverter" /> and optionally defines the converter to be used
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
	public class ConvertibleAttribute : Attribute
	{
		private readonly Type converterType;

		/// <summary>
		///   Defines the <see cref = "DefaultComplexConverter " /> to be used to convert the type
		/// </summary>
		public ConvertibleAttribute() : this(typeof(DefaultComplexConverter))
		{
		}

		/// <summary>
		///   Defines the <see cref = "ITypeConverter" /> to be used to convert the type
		/// </summary>
		/// <param name = "converterType"></param>
		public ConvertibleAttribute(Type converterType)
		{
			if (converterType.Is<ITypeConverter>() == false)
			{
				throw new ArgumentException(
					string.Format("ConverterType {0} does not implement {1} interface", converterType.FullName,
					              typeof(ITypeConverter).FullName), "converterType");
			}

			this.converterType = converterType;
		}

		public Type ConverterType
		{
			get { return converterType; }
		}
	}
}