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

namespace CastleTests.Components
{
	using System;

	using Castle.Core;

	[Transient]
#if (SILVERLIGHT)
	public class CalculatorServiceWithLifecycle : ICalcService, IInitializable, IDisposable
#else
	public class CalculatorServiceWithLifecycle : MarshalByRefObject, ICalcService, IInitializable, IDisposable
#endif
	{
		private bool initialized;
		private bool disposed;

		public int Sum(int x, int y)
		{
			return x + y;
		}

		public void Initialize()
		{
			initialized = true;
		}

		public void Dispose()
		{
			disposed = true;
		}

		public bool Initialized
		{
			get { return initialized; }
		}

		public bool Disposed
		{
			get { return disposed; }
		}
	}
}