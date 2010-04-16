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

namespace Castle.MicroKernel.SubSystems.Conversion
{
	public class TypeNameParser : ITypeNameParser
	{
		public TypeName Parse(string name)
		{

			//ok first let us make sure we're not dealing with fully qualified type name, because in that case there's nothing we can really do.
			if (name.IndexOf(',') != -1)
			{
				return null;
			}
			var genericIndex = name.IndexOf('`');
			var genericTypes = new TypeName[] { };
			if (genericIndex > -1)
			{
				var start = name.IndexOf("[[", genericIndex);
				if (start != -1)
				{
					int count;
					var countString = name.Substring(genericIndex + 1, start - genericIndex - 1);
					if (int.TryParse(countString, out count) == false)
					{
						return null;
					}
					var genericsString = name.Substring(start + 2, name.Length - start - 4);
					genericTypes = ParseNames(genericsString, count);
					if (genericTypes == null)
					{
						return null;
					}
					name = name.Substring(0, start);
				}
			}
			// at this point we assume we have just the type name, probably prefixed with namespace so let's see which one is it
			var typeStartsHere = name.LastIndexOf('.');
			string typeName;
			string @namespace = null;

			if (typeStartsHere > -1 && typeStartsHere < (name.Length - 1))
			{
				typeName = name.Substring(typeStartsHere + 1);
				@namespace = name.Substring(0, typeStartsHere);
			}
			else
			{
				typeName = name;
			}
			return new TypeName(@namespace, typeName, genericTypes);
		}

		private TypeName[] ParseNames(string substring, int count)
		{
			if (count == 1)
			{
				var name = Parse(substring);
				if (name == null)
				{
					return new TypeName[0];
				}
				return new[] { name };
			}
			return new TypeName[0];
		}
	}
}