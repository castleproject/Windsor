#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System.Diagnostics.Contracts;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Registration.Lifestyle;

namespace Castle.Services.vNextTransaction.Lifestyles
{
	public static class LifestyleRegistrationExtensions
	{
		public static ComponentRegistration<TService> HybridPerTransactionTransient<TService>(
			this LifestyleGroup<TService> @group)
		{
			Contract.Requires(group != null);
			return @group.Custom<HybridPerTransactionTransientLifestyleManager>();
		}

		public static ComponentRegistration<TService> HybridPerTopTransactionTransient<TService>(
			this LifestyleGroup<TService> @group)
		{
			Contract.Requires(group != null);
			return @group.Custom<HybridPerTopTransactionTransientLifestyleManager>();
		}
	}
}