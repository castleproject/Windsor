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
	public class TypeNameParser : ITypeNameParser
	{
		public TypeName Parse(string name)
		{
			var isPotentiallyFullyQualifiedName = false;
			if (name.IndexOf(',') != -1)
			{
				isPotentiallyFullyQualifiedName = true;
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
					genericTypes = ParseNames(name.Substring(start + 2, name.LastIndexOf("]]") - 2 - start), count);
					if (genericTypes == null)
					{
						return null;
					}
					isPotentiallyFullyQualifiedName = false;
					name = name.Substring(0, start);
				}
			}
			if (isPotentiallyFullyQualifiedName)
			{
				//well at this point it either is a fully qualified name, or invalid string
				return new TypeName(name);
			}
			// at this point we assume we have just the type name, probably prefixed with namespace so let's see which one is it
			return BuildName(name, genericTypes);
		}

		private TypeName BuildName(string name, TypeName[] genericTypes)
		{
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

		private int MoveToBeginning(int location, string text)
		{
			var currentLocation = location;
			while (currentLocation < text.Length)
			{
				if (text[currentLocation] == '[')
				{
					return currentLocation;
				}
				currentLocation++;
			}
			return currentLocation;
		}

		private int MoveToEnd(int location, string text)
		{
			var open = 1;
			var currentLocation = location;
			while (currentLocation < text.Length)
			{
				var current = text[currentLocation];
				if (current == '[')
				{
					open++;
				}
				else if (current == ']')
				{
					open--;
					if (open == 0)
					{
						return currentLocation;
					}
				}
				currentLocation++;
			}
			return currentLocation;
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
			var names = new TypeName[count];

			var location = 0;
			for (var i = 0; i < count; i++)
			{
				var newLocation = MoveToEnd(location, substring);
				names[i] = Parse(substring.Substring(location, newLocation - location));
				location = MoveToBeginning(newLocation, substring) + 1;
			}
			return names;
		}
	}
}