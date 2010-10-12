// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

using Castle.Core;

namespace Castle.MicroKernel.Proxy
{
	/// <summary>
	/// Select the appropriate interceptors based on the application specific
	/// business logic
	/// </summary>
	public interface IModelInterceptorsSelector
	{
		/// <summary>
		/// Determine whatever the specified has interceptors.
		/// The selector should only return true from this method if it has determined that is
		/// a model that it would likely add interceptors to.
		/// </summary>
		/// <param name="model">The model</param>
		/// <returns>Whatever this selector is likely to add interceptors to the specified model</returns>
		bool HasInterceptors(ComponentModel model);

		/// <summary>
		/// Select the appropriate interceptor references.
		/// The interceptor references aren't necessarily registered in the model.Intereceptors
		/// </summary>
		/// <param name="model">The model to select the interceptors for</param>
		/// <param name="interceptors">The interceptors selected by previous selectors in the pipeline or <see cref="ComponentModel.Interceptors"/> if this is the first interceptor in the pipeline.</param>
		/// <returns>The interceptor for this model (in the current context) or a null reference</returns>
		/// <remarks>
		/// If the selector is not interested in modifying the interceptors for this model, it 
		/// should return <paramref name="interceptors"/> and the next selector in line would be executed.
		/// If the selector wants no interceptors to be used it can either return <c>null</c> or empty array.
		/// However next interceptor in line is free to override this choice.
		/// </remarks>
		InterceptorReference[] SelectInterceptors(ComponentModel model, InterceptorReference[] interceptors);
	}
}