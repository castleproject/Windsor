﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
	public class InitializableDisposableComponent : IInitializable, IDisposable
	{
		private bool isDisposed;
		private bool isInitialized;

		public bool IsDisposed
		{
			get { return isDisposed; }
		}

		public bool IsInitialized
		{
			get { return isInitialized; }
		}

		public void Dispose()
		{
			isDisposed = true;
		}

		public void Initialize()
		{
			isInitialized = true;
		}
	}
}