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

namespace Castle.Facilities.WcfIntegration
{
	using System;

	public interface IWcfClientFactory
	{
		T GetClient<T>(string name) where T : class;

		T GetClient<T>(IWcfClientModel model) where T : class;

		T GetClient<T>(string name, IWcfClientModel model) where T : class;

		T GetClient<T>(IWcfEndpoint endpoint) where T : class;

		T GetClient<T>(string name, IWcfEndpoint endpoint) where T : class;

		T GetClient<T>(Uri address) where T : class;

		T GetClient<T>(string name, Uri address) where T : class;

		void Release(object client);
	}
}
