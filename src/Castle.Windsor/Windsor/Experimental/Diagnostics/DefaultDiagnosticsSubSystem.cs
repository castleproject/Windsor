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
	using System.Collections;
	using System.Collections.Generic;

	using Castle.MicroKernel;
	using Castle.Windsor.Experimental.Diagnostics.Extensions;
#if !SILVERLIGHT

#endif

	public class DefaultDiagnosticsSubSystem :
		ISubSystem, IDiagnosticsHost
#if !SILVERLIGHT
		, IContainerDebuggerExtensionHost
#endif
	{
		private readonly IDictionary<Type, IDiagnostic<object>> diagnostics = new Dictionary<Type, IDiagnostic<object>>();

#if !SILVERLIGHT
		private readonly IList<IContainerDebuggerExtension> extensions = new List<IContainerDebuggerExtension>();
#endif
		private IKernel kernel;

#if !SILVERLIGHT
		public void Add(IContainerDebuggerExtension item)
		{
			item.Init(kernel, this);
			extensions.Add(item);
		}
#endif

		public void AddDiagnostic<TDiagnostic>(TDiagnostic diagnostic) where TDiagnostic : IDiagnostic<object>
		{
			diagnostics.Add(typeof(TDiagnostic), diagnostic);
		}

		public TDiagnostic GetDiagnostic<TDiagnostic>() where TDiagnostic : IDiagnostic<object>
		{
			IDiagnostic<object> value;
			diagnostics.TryGetValue(typeof(TDiagnostic), out value);
			return (TDiagnostic)value;
		}

#if !SILVERLIGHT
		public IEnumerator<IContainerDebuggerExtension> GetEnumerator()
		{
			return extensions.GetEnumerator();
		}
#endif

		public void Init(IKernelInternal kernel)
		{
			this.kernel = kernel;
#if !SILVERLIGHT
			InitStandardExtensions();
#endif
		}

		public void Terminate()
		{
		}

#if !SILVERLIGHT
		protected virtual void InitStandardExtensions()
		{
			Add(new AllComponents());
			Add(new DefaultComponentPerService());
			Add(new PotentiallyMisconfiguredComponents());
			Add(new PotentialLifestyleMismatches());
			Add(new ReleasePolicyTrackedObjects());
			Add(new Facilities());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
#endif
	}
}