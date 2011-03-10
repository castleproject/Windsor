// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MicroKernel.Tests.ClassComponents
{
	using System;

	using Castle.Windsor.Tests;

	using CastleTests.Components;

	public class ServiceUser2 : ServiceUser
	{
		private string _name;
		private int _port;
		private int _scheduleinterval;

		public ServiceUser2(A a, String name, int port) : base(a)
		{
			_name = name;
			_port = port;
		}

		public ServiceUser2(A a, String name, int port, int scheduleinterval) : this(a, name, port)
		{
			_scheduleinterval = scheduleinterval;
		}

		public String Name
		{
			get { return _name; }
		}

		public int Port
		{
			get { return _port; }
		}

		public int ScheduleInterval
		{
			get { return _scheduleinterval; }
		}
	}
}