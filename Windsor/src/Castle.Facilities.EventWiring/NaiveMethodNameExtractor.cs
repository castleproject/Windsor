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

namespace Castle.Facilities.EventWiring
{
#if !SILVERLIGHT
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;

	/// <summary>
	///   Extracts MethodInfo of metho invoked in delegate. Based on ILReader class from http://www.gocosmos.org project
	/// </summary>
	internal class NaiveMethodNameExtractor
	{
		private readonly MethodBody body;
		private readonly MethodBase delegateMethod;
		private readonly Module module;
		private readonly MemoryStream stream;

		private MethodBase calledMethod;

		public NaiveMethodNameExtractor(Delegate @delegate)
		{
			delegateMethod = @delegate.Method;
			body = delegateMethod.GetMethodBody();
			Debug.Assert(body != null);
			module = delegateMethod.Module;
			stream = new MemoryStream(body.GetILAsByteArray());
			Read();
		}

		public MethodBase CalledMethod
		{
			get { return calledMethod; }
		}

		private MethodBase GetCalledMethod(byte[] rawOperand)
		{
			Type[] genericTypeArguments = null;
			Type[] genericMethodArguments = null;
			if (delegateMethod.DeclaringType.IsGenericType)
			{
				genericTypeArguments = delegateMethod.DeclaringType.GetGenericArguments();
			}
			if (delegateMethod.IsGenericMethod)
			{
				genericMethodArguments = delegateMethod.GetGenericArguments();
			}
			var methodBase = module.ResolveMethod(OperandValueAsInt32(rawOperand), genericTypeArguments, genericMethodArguments);
			return methodBase;
		}

		private void Read()
		{
			OpCodeValues currentOpCode;
			while (ReadOpCode(out currentOpCode))
			{
				if (IsSupportedOpCode(currentOpCode) == false)
				{
					return;
				}

				if (currentOpCode == OpCodeValues.Callvirt || currentOpCode == OpCodeValues.Call)
				{
					calledMethod = GetCalledMethod(ReadOperand(32));
					return;
				}
			}
		}

		private bool ReadOpCode(out OpCodeValues opCodeValue)
		{
			var valueInt = stream.ReadByte();
			if (valueInt == -1)
			{
				opCodeValue = 0;
				return false;
			}
			var xByteValue = (byte)valueInt;
			if (xByteValue == 0xFE)
			{
				valueInt = stream.ReadByte();
				if (valueInt == -1)
				{
					opCodeValue = 0;
					return false;
				}
				opCodeValue = (OpCodeValues)(xByteValue << 8 | valueInt);
			}
			else
			{
				opCodeValue = (OpCodeValues)xByteValue;
			}
			return true;
		}

		private byte[] ReadOperand(byte operandSize)
		{
			var bytes = new byte[operandSize/8];
			var actualSize = stream.Read(bytes, 0, bytes.Length);
			if (actualSize < bytes.Length)
			{
				throw new NotSupportedException();
			}
			return bytes;
		}

		private static bool IsSupportedOpCode(OpCodeValues currentOpCode)
		{
			return Enum.IsDefined(typeof(OpCodeValues), currentOpCode);
		}

		private static int OperandValueAsInt32(byte[] rawOperand)
		{
			var value = new byte[4];
			Array.Copy(rawOperand, value, Math.Min(4, rawOperand.Length));
			return BitConverter.ToInt32(value, 0);
		}
	}
#endif
}