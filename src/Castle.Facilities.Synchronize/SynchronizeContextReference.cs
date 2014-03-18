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

namespace Castle.Facilities.Synchronize
{
	using System;
	using System.Threading;

	using Castle.MicroKernel;

	/// <summary>
	///   Represents a reference to a SynchronizeContext component.
	/// </summary>
	[Serializable]
	public class SynchronizeContextReference : ComponentReference<SynchronizationContext>
	{
		public SynchronizeContextReference(Type componentType) : base(componentType)
		{
		}

		public SynchronizeContextReference(string referencedComponentName) : base(referencedComponentName)
		{
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(SynchronizeContextReference))
			{
				return false;
			}
			var other = (SynchronizeContextReference)obj;
			return Equals(other.referencedComponentName, referencedComponentName);
		}

		public override int GetHashCode()
		{
			return (referencedComponentName != null ? referencedComponentName.GetHashCode() : 0);
		}

		protected override Type ComponentType
		{
			get { return referencedComponentType; }
		}
	}
}