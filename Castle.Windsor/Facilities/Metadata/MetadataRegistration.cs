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

namespace Castle.Facilities.Metadata
{
	using System;
	using System.Collections;

	using Castle.Components.DictionaryAdapter;
	using Castle.MicroKernel.Registration;

	public static class MetadataRegistration
	{
		private static readonly DictionaryAdapterFactory dictionaryAdapterFactory = new DictionaryAdapterFactory();

		public static ComponentRegistration<TComponent> LazyInit<TComponent>(
			this ComponentRegistration<TComponent> registration)
		{
			return registration.ExtendedProperties(
				Property.ForKey("metadata.load-lazily").Eq(true));
		}

		public static ComponentRegistration<TComponent> WithMetadata<TComponent, TMetadata>(
			this ComponentRegistration<TComponent> registration, Action<TMetadata> values)
		{
			var metadata = new Hashtable();
			var forWriting = GetAdapterForWriting<TMetadata>(metadata);

			values(forWriting);

			var forReading = GetAdapterForReading<TMetadata>(metadata);

			return registration.AddDescriptor(new ExtendedPropertiesDescriptor<TComponent>(metadata))
				.ExtendedProperties(Property.ForKey("metadata.metadata-type").Eq(typeof(TMetadata)),
				                    Property.ForKey("metadata.metadata-adapter").Eq(forReading));
		}

		private static TMetadata GetAdapterForReading<TMetadata>(IDictionary metadata)
		{
			var builder = new PropertyDescriptor()
				.AddKeyBuilder(new KeyPrefixAttribute("metadata."))
				.AddSetter(new NoOpSetter());
			return (TMetadata)dictionaryAdapterFactory.GetAdapter(typeof(TMetadata), metadata, builder);
		}

		private static TMetadata GetAdapterForWriting<TMetadata>(IDictionary metadata)
		{
			var builder = new PropertyDescriptor()
				.AddKeyBuilder(new KeyPrefixAttribute("metadata."));
			return (TMetadata)dictionaryAdapterFactory.GetAdapter(typeof(TMetadata), metadata, builder);
		}
	}
}