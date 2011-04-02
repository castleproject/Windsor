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

namespace Castle.Windsor.Experimental.Diagnostics
{
	using System;

	/// <summary>
	/// Hosts different diagnostics available in the container.
	/// </summary>
	public interface IDiagnosticsHost
	{
		/// <summary>
		/// Adds <paramref name="diagnostic"/> and makes it available as <typeparamref name="TDiagnostic"/>.
		/// </summary>
		/// <exception cref="ArgumentException">Thrown when a diagnostic for <typeparamref name="TDiagnostic"/>has already been added.</exception>
		void AddDiagnostic<TDiagnostic>(TDiagnostic diagnostic) where TDiagnostic : IDiagnostic<object>;

		/// <summary>
		/// Returns diagnostic registered with <typeparamref name="TDiagnostic"/> or <c>null</c> if not present.
		/// </summary>
		/// <typeparam name="TDiagnostic"></typeparam>
		/// <returns></returns>
		TDiagnostic GetDiagnostic<TDiagnostic>() where TDiagnostic : IDiagnostic<object>;
	}
}