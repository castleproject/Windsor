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
	/// <summary>
	///   Establish a composition interface and a subsystem.
	///   Implementors should delegate the conversion to 
	///   a instance of a type converter.
	/// </summary>
	public interface IConversionManager : ITypeConverter, ISubSystem
	{
		/// <summary>
		///   Register a type converter instance.
		/// </summary>
		/// <param name = "converter"></param>
		void Add(ITypeConverter converter);
	}
}