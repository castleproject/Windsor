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

namespace Castle.Windsor.Diagnostics
{
	/// <summary>
	///   Exposes diagnostics about itself to the <see cref = "IDiagnosticsInspector{TData,TContext}" />.
	/// </summary>
	/// <typeparam name = "TData">Usually simple type containing information provided to the <see
	///    cref = "IDiagnosticsInspector{TData,TContext}" />.</typeparam>
	/// <remarks>
	///   Can be implemented by any type constituting part of container infrastructure. Should have a matching <see
	///    cref = "IDiagnosticsInspector{TData,TContext}" /> registred in the container that knows
	///   how to find it and that prepares information from it for consumption.
	/// </remarks>
	public interface IExposeDiagnostics<out TData>
	{
		/// <summary>
		///   Collects <typeparamref name = "TData" /> for the <paramref name = "inspector" /> and calls <see
		///    cref = "IDiagnosticsInspector{TData,TContext}.Inspect" /> if any data available.
		/// </summary>
		/// <param name = "inspector"></param>
		/// <param name = "context">pass-through context. Used by the inspector.</param>
		void Visit<TContext>(IDiagnosticsInspector<TData, TContext> inspector, TContext context);
	}
}