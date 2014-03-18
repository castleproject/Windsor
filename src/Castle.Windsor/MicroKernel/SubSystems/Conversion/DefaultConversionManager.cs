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

namespace Castle.MicroKernel.SubSystems.Conversion
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	using Castle.Core;
	using Castle.Core.Configuration;
	using Castle.MicroKernel.Context;

	/// <summary>
	///   Composition of all available conversion managers
	/// </summary>
	[Serializable]
	public class DefaultConversionManager : AbstractSubSystem, IConversionManager, ITypeConverterContext
	{
#if (!SILVERLIGHT)
		private static readonly LocalDataStoreSlot slot = Thread.AllocateDataSlot();
#else
		[ThreadStatic]
		private static Stack<Pair<ComponentModel,CreationContext>> slot;
#endif
		private readonly IList<ITypeConverter> converters = new List<ITypeConverter>();
		private readonly IList<ITypeConverter> standAloneConverters = new List<ITypeConverter>();

		public DefaultConversionManager()
		{
			InitDefaultConverters();
		}

		protected virtual void InitDefaultConverters()
		{
			Add(new PrimitiveConverter());
			Add(new TimeSpanConverter());
			Add(new TypeNameConverter(new TypeNameParser()));
			Add(new EnumConverter());
			Add(new ListConverter());
			Add(new DictionaryConverter());
			Add(new GenericDictionaryConverter());
			Add(new GenericListConverter());
			Add(new ArrayConverter());
            Add(new AttributeAwareConverter());
            Add(new ComponentConverter());
#if (SILVERLIGHT)
			Add(new NullableConverter(this));
#endif
			Add(new ComponentModelConverter());
		}

		public void Add(ITypeConverter converter)
		{
			converter.Context = this;

			converters.Add(converter);

			if (!(converter is IKernelDependentConverter))
			{
				standAloneConverters.Add(converter);
			}
		}

		public ITypeConverterContext Context
		{
			get { return this; }
			set { throw new NotImplementedException(); }
		}

		public bool CanHandleType(Type type)
		{
			foreach (var converter in converters)
			{
				if (converter.CanHandleType(type))
				{
					return true;
				}
			}

			return false;
		}

		public bool CanHandleType(Type type, IConfiguration configuration)
		{
			foreach (var converter in converters)
			{
				if (converter.CanHandleType(type, configuration))
				{
					return true;
				}
			}

			return false;
		}

		public object PerformConversion(String value, Type targetType)
		{
			foreach (var converter in converters)
			{
				if (converter.CanHandleType(targetType))
				{
					return converter.PerformConversion(value, targetType);
				}
			}

			var message = String.Format("No converter registered to handle the type {0}",
			                            targetType.FullName);

			throw new ConverterException(message);
		}

		public object PerformConversion(IConfiguration configuration, Type targetType)
		{
			foreach (var converter in converters)
			{
				if (converter.CanHandleType(targetType, configuration))
				{
					return converter.PerformConversion(configuration, targetType);
				}
			}

			var message = String.Format("No converter registered to handle the type {0}",
			                            targetType.FullName);

			throw new ConverterException(message);
		}

		public TTarget PerformConversion<TTarget>(string value)
		{
			return (TTarget)PerformConversion(value, typeof(TTarget));
		}

		public TTarget PerformConversion<TTarget>(IConfiguration configuration)
		{
			return (TTarget)PerformConversion(configuration, typeof(TTarget));
		}

		IKernelInternal ITypeConverterContext.Kernel
		{
			get { return Kernel; }
		}

		public void Push(ComponentModel model, CreationContext context)
		{
			CurrentStack.Push(new Pair<ComponentModel, CreationContext>(model, context));
		}

		public void Pop()
		{
			CurrentStack.Pop();
		}

		public ComponentModel CurrentModel
		{
			get
			{
				if (CurrentStack.Count == 0)
				{
					return null;
				}

				return CurrentStack.Peek().First;
			}
		}

		public CreationContext CurrentCreationContext
		{
			get
			{
				if (CurrentStack.Count == 0)
				{
					return null;
				}

				return CurrentStack.Peek().Second;
			}
		}

		public ITypeConverter Composition
		{
			get { return this; }
		}

		private Stack<Pair<ComponentModel, CreationContext>> CurrentStack
		{
			get
			{
#if (SILVERLIGHT)
				if(slot == null)
				{
					slot = new Stack<Pair<ComponentModel,CreationContext>>();
				}

				return slot;
#else
				var stack = (Stack<Pair<ComponentModel, CreationContext>>)Thread.GetData(slot);
				if (stack == null)
				{
					stack = new Stack<Pair<ComponentModel, CreationContext>>();
					Thread.SetData(slot, stack);
				}

				return stack;

#endif
			}
		}
	}
}