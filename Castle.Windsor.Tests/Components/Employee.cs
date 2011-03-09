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

	/// <summary>
	///   Summary description for Employee.
	/// </summary>
	public class Employee : IEmployee
	{
		private string _firstName;
		private string _lastName;
		private string _middleName;
		private string _ntLogin;
		public string Email { get; set; }

		public string EmployeeID { get; set; }

		public string FirstName
		{
			get { return _firstName; }
			set { _firstName = value; }
		}

		public string FullName
		{
			get { return String.Format("{0} {1} {2}", _firstName, _middleName, _lastName); }
		}

		public bool IsProxy { get; set; }

		public bool IsSupervisor { get; set; }

		public string LastName
		{
			get { return _lastName; }
			set { _lastName = value; }
		}

		public string MiddleName
		{
			get { return _middleName; }
			set { _middleName = value; }
		}

		public string NTLogin
		{
			get
			{
				if (_ntLogin.Length > 0)
				{
					return _ntLogin;
				}

				try
				{
					//					if (Config.IsInPortal)
					//					{
					//						_ntLogin = User.FindLoginIdFromEmpId(_empID);
					//					}
					//					else
					//					{
					//						_ntLogin = Config.DebugNtLogin;
					//					}

					return _ntLogin;
				}
				catch (Exception)
				{
					//					Logger.Error("NTLogin check failed.", e);
					//					Logger.SendMail("ERROR", "NTLogin check failed.", e);
					return null;
				}
			}
		}

		public void SetNTLogin(string ntLogin)
		{
			_ntLogin = ntLogin;
		}
	}
}