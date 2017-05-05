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

namespace CastleTests.Facilities
{
	using System;

	using Castle.Core.Configuration;
	using Castle.Facilities.Startable;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Tests.ClassComponents;

	using NUnit.Framework;

	[TestFixture]
	public class FacilityTestCase
	{
		private static readonly string facilityKey = typeof(HiperFacility).FullName;
		private HiperFacility facility;
		private IKernel kernel;

		[Test]
		public void Cant_have_two_instances_of_any_facility_type()
		{
			kernel.AddFacility<StartableFacility>();

			var exception = Assert.Throws<ArgumentException>(() => kernel.AddFacility<StartableFacility>());

			Assert.AreEqual(
				"Facility of type 'Castle.Facilities.Startable.StartableFacility' has already been registered with the container. Only one facility of a given type can exist in the container.",
				exception.Message);
		}

		[Test]
		public void Creation()
		{
			var facility = kernel.GetFacilities()[0];

			Assert.IsNotNull(facility);
			Assert.AreSame(this.facility, facility);
		}

		[SetUp]
		public void Init()
		{
			kernel = new DefaultKernel();

			IConfiguration confignode = new MutableConfiguration("facility");
			IConfiguration facilityConf = new MutableConfiguration(facilityKey);
			confignode.Children.Add(facilityConf);
			kernel.ConfigurationStore.AddFacilityConfiguration(facilityKey, confignode);

			facility = new HiperFacility();

			Assert.IsFalse(facility.Initialized);
			kernel.AddFacility(facility);
		}

		[Test]
		public void LifeCycle()
		{
			Assert.IsFalse(this.facility.Terminated);

			var facility = kernel.GetFacilities()[0];

			Assert.IsTrue(this.facility.Initialized);
			Assert.IsFalse(this.facility.Terminated);

			kernel.Dispose();

			Assert.IsTrue(this.facility.Initialized);
			Assert.IsTrue(this.facility.Terminated);
		}

		[Test]
		public void OnCreationCallback()
		{
			StartableFacility facility = null;

			kernel.AddFacility<StartableFacility>(f => facility = f);

			Assert.IsNotNull(facility);
		}
	}
}