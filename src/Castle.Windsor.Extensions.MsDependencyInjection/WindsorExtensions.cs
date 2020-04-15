// Copyright 2004-2020 Castle Project - http://www.castleproject.org/
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

namespace Castle.Windsor.Extensions.MsDependencyInjection
{
    using System;
    
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Registration.Lifestyle;

    public static class WindsorExtensions
    {
        /// <summary>
        /// Scopes the lifestyle of the component to a scope started by <see name="IServiceScopeFactory.CreateScope" />
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        public static ComponentRegistration<TService> ScopedToNetCoreScope<TService>(this LifestyleGroup<TService> lifestyle) where TService : class
        {
            return lifestyle.Scoped<NetCoreScopeAccessor>();
        }

        /// <summary>
        /// Returns new instances everytime it's resolved but disposes it on <see name="IServiceScope" /> end
        /// </summary>
        /// <typeparam name="TService">Service type</typeparam>
        public static ComponentRegistration<TService> LifestyleNetCoreTransient<TService>(this ComponentRegistration<TService> registration) where TService : class
        {
            return registration
                .Attribute(NetCoreScope.NetCoreTransientMarker).Eq(Boolean.TrueString)
                .LifeStyle.ScopedToNetCoreScope();  //.NET core expects new instances but release on scope dispose
        }

        /// <summary>
        /// Singleton instance with .NET Core semantics
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        public static ComponentRegistration<TService> NetCoreStatic<TService>(this LifestyleGroup<TService> lifestyle) where TService : class
        {
            return lifestyle
                .Scoped<NetCoreRootScopeAccessor>();
        }
    }
}