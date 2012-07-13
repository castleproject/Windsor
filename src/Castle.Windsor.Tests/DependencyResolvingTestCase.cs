// Copyright 2004-2012 Castle Project - http://www.castleproject.org/
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

namespace CastleTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Castle.MicroKernel.Registration;
	using Castle.MicroKernel.Resolvers;

	using CastleTests.Components;

	using NUnit.Framework;

	[TestFixture]
	public class DependencyResolvingTestCase : AbstractContainerTestCase
	{

		[Test]
		public void First_by_registration_order_available_component_is_used_to_satisfy_dependency()
		{
			Kernel.Register(Component.For<IAlarmSender>().ImplementedBy<EmailSender>(),
			                Component.For<IAlarmSender>().ImplementedBy<SmsSender>(),
			                Component.For<AlarmGenerator>());

			var gen = Kernel.Resolve<AlarmGenerator>();

			Assert.IsInstanceOf<EmailSender>(gen.Sender);
		}

		[Test]
		public void First_by_registration_order_available_component_is_used_to_satisfy_dependency_flipped_order()
		{
			Kernel.Register(Component.For<IAlarmSender>().ImplementedBy<SmsSender>(),
			                Component.For<IAlarmSender>().ImplementedBy<EmailSender>(),
			                Component.For<AlarmGenerator>());

			var gen = Kernel.Resolve<AlarmGenerator>();

			Assert.IsInstanceOf<SmsSender>(gen.Sender);
		}

		[Test]
		public void First_by_registration_order_available_component_is_used_to_satisfy_dependency_regardless_of_dependency_name_if_no_override()
		{
			Kernel.Register(Component.For<IAlarmSender>().ImplementedBy<SmsSender>(),
			                Component.For<IAlarmSender>().ImplementedBy<EmailSender>().Named("Sender"),
			                Component.For<AlarmGenerator>());

			var gen = Kernel.Resolve<AlarmGenerator>();

			Assert.IsInstanceOf<SmsSender>(gen.Sender);
		}

		[Test]
		public void First_by_registration_order_available_component_is_used_to_satisfy_dependency_regardless_of_dependency_name_if_no_override_missing_sub_dependency()
		{
			Kernel.Register(Component.For<IAlarmSender>().ImplementedBy<SmsSender>(),
			                Component.For<IAlarmSender>().ImplementedBy<EmailSenderWithDependency>().Named("Sender"),
			                Component.For<AlarmGenerator>());

			var gen = Kernel.Resolve<AlarmGenerator>();

			Assert.IsInstanceOf<SmsSender>(gen.Sender);
		}

		[Test]
		public void Parameter_wins_over_service()
		{
			Kernel.Register(Component.For<Uri>().Instance(new Uri("http://component.com")),
			                Component.For<UsesUri>().DependsOn(Parameter.ForKey("uri").Eq("http://parameter.com")));
		}

		[Test]
		public void Service_override_and_parameter_for_the_same_dependency_not_legal_via_name()
		{
			var exception =
				Assert.Throws<ArgumentException>(() =>
				                                 Kernel.Register(Component.For<UsesUri>().DependsOn(Parameter.ForKey("uri").Eq("http://parameter.com"))
					                                                 .DependsOn(Property.ForKey("uri").Is("uriComponent")),
				                                                 Component.For<Uri>().Named("uriComponent").Instance(new Uri("http://component.com"))));

			Assert.AreEqual("Parameter 'uri' already exists.", exception.Message);
		}

		[Test]
		public void Service_override_and_parameter_for_the_same_dependency_not_legal_via_type()
		{
			Kernel.Register(Component.For<UsesUri>().DependsOn(Parameter.ForKey("uri").Eq("http://parameter.com"))
				                .DependsOn(Property.ForKey<Uri>().Is("uriComponent")),
			                Component.For<Uri>().Named("uriComponent").Instance(new Uri("http://component.com")));

			var exception =
				Assert.Throws<DependencyResolverException>(() =>
				                                           Kernel.Resolve<UsesUri>());

			Assert.AreEqual("Could not convert parameter 'uri' to type 'Uri'.", exception.Message);
		}

		[Test]
		public void Service_override_wins_over_service()
		{
			Kernel.Register(Component.For<Uri>().Instance(new Uri("http://service.com")),
			                Component.For<UsesUri>().DependsOn(Property.ForKey("uri").Is("uriComponent")),
			                Component.For<Uri>().Named("uriComponent").Instance(new Uri("http://serviceOverride.com")));

			var instance = Kernel.Resolve<UsesUri>();

			Assert.AreEqual("http://serviceOverride.com", instance.Uri.OriginalString);
		}

		[Test]
		public void String_class_can_NOT_be_a_service()
		{
			var uriString = "http://castleproject.org";

			Assert.Throws<ArgumentException>(() => Kernel.Register(Component.For<string>().Instance(uriString)));
		}

		[Test(Description = "see http://stackoverflow.com/questions/1839199/how-to-register-a-uri-dependency-to-return-httpcontext-current-request-url-using")]
		public void Uri_class_can_be_a_component()
		{
			var uriString = "http://castleproject.org";
			Kernel.Register(Component.For<UsesUri>(),
			                Component.For<Uri>().DependsOn(new { uriString }));

			var instance = Kernel.Resolve<UsesUri>();

			Assert.AreEqual(uriString, instance.Uri.OriginalString);
		}

		[Test(Description = "see http://stackoverflow.com/questions/1839199/how-to-register-a-uri-dependency-to-return-httpcontext-current-request-url-using")]
		public void Uri_class_can_be_a_parameter()
		{
			var uriString = "http://castleproject.org";
			Kernel.Register(Component.For<UsesUri>().DependsOn(Parameter.ForKey("uri").Eq(uriString)));

			var instance = Kernel.Resolve<UsesUri>();

			Assert.AreEqual(uriString, instance.Uri.OriginalString);
		}

		[Test(Description = "It's not a good idea but we're leaving it in for backward compatibity and convenience")]
		public void ValueType_array_can_be_a_service()
		{
			var instance = new[] { DateTime.Now };

			Kernel.Register(Component.For<DateTime[]>().Instance(instance));
			var resolved = Kernel.Resolve<DateTime[]>();

			Assert.AreSame(instance, resolved);
		}

		[Test]
		public void ValueType_can_NOT_be_a_service()
		{
			Assert.Throws<ArgumentException>(() => Kernel.Register(Component.For(typeof(DateTime)).Instance(DateTime.Now)));
		}

		[Test(Description = "It's not a good idea but we're leaving it in for backward compatibity and convenience")]
		public void ValueType_collection_can_be_a_service()
		{
			var instance = new[] { DateTime.Now }.AsEnumerable();

			Kernel.Register(Component.For<IEnumerable<DateTime>>().Instance(instance));
			var resolved = Kernel.Resolve<IEnumerable<DateTime>>();

			Assert.AreSame(instance, resolved);
		}

		[Test]
		public void Value_types_cannot_be_used_as_components()
		{
			var exception = Assert.Throws<ArgumentException>(() =>
			                                                 Kernel.Register(Component.For(typeof(int))));

			Assert.AreEqual("Type System.Int32 is not a class nor an interface, and those are the only values allowed.", exception.Message);
		}
	}
}