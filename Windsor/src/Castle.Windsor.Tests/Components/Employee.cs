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

namespace CastleTests.Components
{
	using System;

	public class Employee : IEmployee
	{
		private string ntLogin;
		public string Email { get; set; }

		public string EmployeeID { get; set; }

		public string FirstName { get; set; }

		public string FullName
		{
			get { return String.Format("{0} {1} {2}", FirstName, MiddleName, LastName); }
		}

		public bool IsProxy { get; set; }

		public bool IsSupervisor { get; set; }

		public string LastName { get; set; }

		public string MiddleName { get; set; }

		public string NTLogin
		{
			get
			{
				if (ntLogin.Length > 0)
				{
					return ntLogin;
				}
				return ntLogin;
			}
		}

		public void SetNTLogin(string ntLogin)
		{
			this.ntLogin = ntLogin;
		}
	}
}