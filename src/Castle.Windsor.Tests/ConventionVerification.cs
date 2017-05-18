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


#if !(SILVERLIGHT)

namespace CastleTests
{
	using System;
	using System.ComponentModel;
	using System.Linq;
	using System.Reflection;
	using System.Text;

	using Castle.Core.Internal;
	using Castle.MicroKernel;
	using Castle.Windsor;

	using NUnit.Framework;

	[Explicit]
	[TestFixture]
	public class ConventionVerification
	{
		private void Scan(MethodInfo interfaceMethod, MethodInfo classMethod, StringBuilder message)
		{
			var obsolete = EnsureBothHave<ObsoleteAttribute>(interfaceMethod, classMethod, message);
			if (obsolete.Item3)
			{
				if (obsolete.Item1.IsError != obsolete.Item2.IsError)
				{
					message.AppendLine(string.Format("Different error levels for {0}", interfaceMethod));
				}
				if (obsolete.Item1.Message != obsolete.Item2.Message)
				{
					message.AppendLine(string.Format("Different message for {0}", interfaceMethod));
					message.AppendLine(string.Format("\t interface: {0}", obsolete.Item1.Message));
					message.AppendLine(string.Format("\t class    : {0}", obsolete.Item2.Message));
				}
			}
			else
			{
				return;
			}
			var browsable = EnsureBothHave<EditorBrowsableAttribute>(interfaceMethod, classMethod, message);
			{
				if (browsable.Item3 == false)
				{
					message.AppendLine(string.Format("EditorBrowsable not applied to {0}", interfaceMethod));
					return;
				}
				if (browsable.Item1.State != browsable.Item2.State || browsable.Item2.State != EditorBrowsableState.Never)
				{
					message.AppendLine(string.Format("Different/wrong browsable states for {0}", interfaceMethod));
				}
			}
		}

		private static Tuple<TAttribute, TAttribute, bool> EnsureBothHave<TAttribute>(MethodInfo interfaceMethod, MethodInfo classMethod, StringBuilder message)
			where TAttribute : Attribute
		{
			var fromInterface = interfaceMethod.GetAttributes<TAttribute>(true).SingleOrDefault();
			var fromClass = classMethod.GetAttributes<TAttribute>(true).SingleOrDefault();
			var bothHaveTheAttribute = true;
			if (fromInterface != null)
			{
				if (fromClass == null)
				{
					message.AppendLine(string.Format("Method {0} has {1} on the interface, but not on the class.", interfaceMethod, typeof(TAttribute)));
					bothHaveTheAttribute = false;
				}
			}
			else
			{
				if (fromClass != null)
				{
					message.AppendLine(string.Format("Method {0} has {1}  on the class, but not on the interface.", interfaceMethod, typeof(TAttribute)));
				}
				bothHaveTheAttribute = false;
			}
			return Tuple.Create(fromInterface, fromClass, bothHaveTheAttribute);
		}

		[Test]
		public void Obsolete_members_of_kernel_are_in_sync()
		{
			var message = new StringBuilder();
			var kernelMap = typeof(DefaultKernel).GetInterfaceMap(typeof(IKernel));
			for (var i = 0; i < kernelMap.TargetMethods.Length; i++)
			{
				var interfaceMethod = kernelMap.InterfaceMethods[i];
				var classMethod = kernelMap.TargetMethods[i];
				Scan(interfaceMethod, classMethod, message);
			}

			Assert.IsEmpty(message.ToString(), message.ToString());
		}

		[Test]
		public void Obsolete_members_of_windsor_are_in_sync()
		{
			var message = new StringBuilder();
			var kernelMap = typeof(WindsorContainer).GetInterfaceMap(typeof(IWindsorContainer));
			for (var i = 0; i < kernelMap.TargetMethods.Length; i++)
			{
				var interfaceMethod = kernelMap.InterfaceMethods[i];
				var classMethod = kernelMap.TargetMethods[i];
				Scan(interfaceMethod, classMethod, message);
			}

			Assert.IsEmpty(message.ToString(), message.ToString());
		}
	}
}

#endif