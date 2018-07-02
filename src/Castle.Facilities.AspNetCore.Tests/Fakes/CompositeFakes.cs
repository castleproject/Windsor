// Copyright 2004-2018 Castle Project - http://www.castleproject.org/
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

namespace Castle.Facilities.AspNetCore.Tests.Fakes
{
	using System;

	public class CompositeController
	{
		public CompositeController(
			ControllerCrossWired crossWiredController, 
			ControllerServiceProviderOnly serviceProviderOnlyController, 
			ControllerWindsorOnly windsorOnlyController)
		{
			if (crossWiredController == null) throw new ArgumentNullException(nameof(crossWiredController));
			if (serviceProviderOnlyController == null) throw new ArgumentNullException(nameof(serviceProviderOnlyController));
			if (windsorOnlyController == null) throw new ArgumentNullException(nameof(windsorOnlyController));
		}
	}

	public class CompositeTagHelper
	{
		public CompositeTagHelper(
			TagHelperCrossWired crossWiredTagHelper, 
			TagHelperServiceProviderOnly serviceProviderOnlyTagHelper, 
			TagHelperWindsorOnly windsorOnlyTagHelper)
		{
			if (crossWiredTagHelper == null) throw new ArgumentNullException(nameof(crossWiredTagHelper));
			if (serviceProviderOnlyTagHelper == null) throw new ArgumentNullException(nameof(serviceProviderOnlyTagHelper));
			if (windsorOnlyTagHelper == null) throw new ArgumentNullException(nameof(windsorOnlyTagHelper));
		}
	}

	public class CompositeViewComponent
	{
		public CompositeViewComponent(
			ViewComponentCrossWired crossWiredViewComponent, 
			ViewComponentServiceProviderOnly serviceProviderOnlyViewComponent, 
			ViewComponentWindsorOnly windsorOnlyViewComponent)
		{
			if (crossWiredViewComponent == null) throw new ArgumentNullException(nameof(crossWiredViewComponent));
			if (serviceProviderOnlyViewComponent == null) throw new ArgumentNullException(nameof(serviceProviderOnlyViewComponent));
			if (windsorOnlyViewComponent == null) throw new ArgumentNullException(nameof(windsorOnlyViewComponent));
		}
	}
}
