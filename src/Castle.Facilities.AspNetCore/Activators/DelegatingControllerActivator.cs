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

namespace Castle.Facilities.AspNetCore.Activators
{
	using System;

	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.Controllers;

	internal sealed class DelegatingControllerActivator : IControllerActivator
	{
		private readonly Func<ControllerContext, object> controllerCreator;
		private readonly Action<ControllerContext, object> controllerReleaser;

		public DelegatingControllerActivator(
			Func<ControllerContext, object> controllerCreator, 
			Action<ControllerContext, object> controllerReleaser)
		{
			this.controllerCreator = controllerCreator ?? throw new ArgumentNullException(nameof(controllerCreator));
			this.controllerReleaser = controllerReleaser ?? throw new ArgumentNullException(nameof(controllerReleaser));
		}

		public object Create(ControllerContext context)
		{
			return controllerCreator(context);
		}

		public void Release(ControllerContext context, object controller)
		{
			controllerReleaser(context, controller);
		}
	}
}