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

namespace Castle.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Castle.Core.Internal;
    
    public delegate ConstructorInfo[] ConstructorFilter(ComponentModel model, ICollection<ConstructorInfo> constructors);
    
    public class StandardConstructorFilters
    {
        public static ICollection<ConstructorFilter> GetConstructorFilters(ComponentModel componentModel, bool createIfMissing)
        {
            var filters = (ICollection<ConstructorFilter>)componentModel.ExtendedProperties[Constants.ConstructorFilters];
            
            if (filters == null && createIfMissing)
            {
                filters = new List<ConstructorFilter>();
                componentModel.ExtendedProperties[Constants.ConstructorFilters] = filters;
            }
         
            return filters;
        }

        public static ConstructorFilter IgnoreSelected(Func<ComponentModel, ConstructorInfo, bool> predicate)
        {
            return (model, constructors) => constructors.Where(c => !predicate(model, c)).ToArray();
        }
    }
}