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

namespace Castle.Windsor.Tests
{
#if !SILVERLIGHT
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Installer;
	using Castle.Windsor.Tests.Components;

	using NUnit.Framework;

	public class ContainerAndGenericsInConfigTestCase : AbstractContainerTestCase
	{
		private IWindsorInstaller FromFile(string fileName)
		{
			return Configuration.FromXmlFile(
				GetFilePath(fileName));
		}

		private string GetFilePath(string fileName)
		{
			return ConfigHelper.ResolveConfigPath("Config/" + fileName);
		}

		[Test]
		public void Can_build_dependency_chains_of_open_generics()
		{
			Container.Install(FromFile("chainOfResponsibility.config"));

			var resolve = Container.Resolve<IResultFinder<int>>();

			Assert.IsTrue(resolve.Finder is DatabaseResultFinder<int>);
			Assert.IsTrue(resolve.Finder.Finder is WebServiceResultFinder<int>);
			Assert.IsTrue(resolve.Finder.Finder.Finder is FailedResultFinder<int>);
			Assert.IsTrue(resolve.Finder.Finder.Finder.Finder == null);
		}

		[Test]
		public void Can_resolve_closed_generic_service()
		{
			Container.Install(FromFile("GenericsConfig.xml"));
			var repos = Container.Resolve<IRepository<int>>("int.repos.generic");

			Assert.IsInstanceOf<DemoRepository<int>>(repos);
		}

		[Test]
		public void Can_resolve_closed_generic_service_decorator()
		{
			Container.Install(FromFile("GenericsConfig.xml"));

			var repository = Container.Resolve<IRepository<int>>("int.repos");

			Assert.IsInstanceOf<LoggingRepositoryDecorator<int>>(repository);
			Assert.IsInstanceOf<DemoRepository<int>>(((LoggingRepositoryDecorator<int>)repository).inner);
		}

		[Test]
		public void Can_resolve_closed_generic_service_decorator_with_service_override()
		{
			Container.Install(FromFile("DecoratorConfig.xml"));
			var repos = Container.Resolve<IRepository<int>>();

			Assert.IsInstanceOf<LoggingRepositoryDecorator<int>>(repos);
			Assert.IsInstanceOf<DemoRepository<int>>(((LoggingRepositoryDecorator<int>)repos).inner);
			Assert.AreEqual("second", ((DemoRepository<int>)((LoggingRepositoryDecorator<int>)repos).inner).Name);
		}

		[Test]
		public void Can_resolve_open_generic_service_with_service_overrides()
		{
			Container.Install(FromFile("ComplexGenericConfig.xml"));

			var repository = Container.Resolve<IRepository<IEmployee>>();
			Assert.IsInstanceOf<LoggingRepositoryDecorator<IEmployee>>(repository);

			var inner = ((LoggingRepositoryDecorator<IEmployee>)repository).inner;
			Assert.IsInstanceOf<DemoRepository<IEmployee>>(inner);

			var actualInner = (DemoRepository<IEmployee>)inner;
			Assert.AreEqual("Generic Repostiory", actualInner.Name);
			Assert.IsInstanceOf<DictionaryCache<IEmployee>>(actualInner.Cache);
		}

		[Test]
		public void Closes_generic_dependency_over_correct_type_for_open_generic_components()
		{
			Container.Install(FromFile("GenericDecoratorConfig.xml"));

			var repos = Container.Resolve<IRepository<string>>();
			Assert.IsInstanceOf<LoggingRepositoryDecorator<string>>(repos);

			Assert.AreEqual("second", ((DemoRepository<string>)((LoggingRepositoryDecorator<string>)repos).inner).Name);
		}

		[Test]
		public void Correctly_detects_unresolvable_dependency_on_same_closed_generic_service()
		{
			Container.Install(FromFile("RecursiveDecoratorConfig.xml"));

			var repository = Container.Resolve<IRepository<int>>();

			Assert.IsNull(((LoggingRepositoryDecorator<int>)repository).inner);
		}

		[Test]
		public void Correctly_detects_unresolvable_dependency_on_same_open_generic_service()
		{
			Container.Install(FromFile("RecursiveDecoratorConfigOpenGeneric.xml"));

			var repository = Container.Resolve<IRepository<int>>();

			Assert.IsNull(((LoggingRepositoryDecorator<int>)repository).inner);
		}

		[Test]
		public void Prefers_closed_service_over_open()
		{
			Container.Install(FromFile("chainOfResponsibility_smart.config"));

			var ofInt = Container.Resolve<IResultFinder<int>>();
			var ofString = Container.Resolve<IResultFinder<string>>();

			Assert.IsInstanceOf<CacheResultFinder<int>>(ofInt);
			Assert.IsInstanceOf<DatabaseResultFinder<int>>(ofInt.Finder);
			Assert.IsInstanceOf<WebServiceResultFinder<int>>(ofInt.Finder.Finder);
			Assert.IsNull(ofInt.Finder.Finder.Finder);

			Assert.IsInstanceOf<ResultFinderStringDecorator>(ofString);
			Assert.IsNotNull(ofString.Finder);
		}

		[Test]
		public void Prefers_closed_service_over_open_2()
		{
			Container.Install(FromFile("ComplexGenericConfig.xml"));

			var repository = Container.Resolve<IRepository<IReviewer>>();

			Assert.IsInstanceOf<ReviewerRepository>(repository);
		}

		[Test]
		public void Prefers_closed_service_over_open_and_uses_default_components_for_dependencies()
		{
			Container.Install(FromFile("ComplexGenericConfig.xml"));

			var repository = Container.Resolve<IRepository<IReviewer>>();

			Assert.IsInstanceOf<ReviewerRepository>(repository);
			Assert.IsInstanceOf<DictionaryCache<IReviewer>>(((ReviewerRepository)repository).Cache);
		}

		[Test]
		public void Prefers_closed_service_over_open_and_uses_service_overrides_for_dependencies()
		{
			Container.Install(FromFile("ComplexGenericConfig.xml"));

			var repository = Container.Resolve<IRepository<IReviewableEmployee>>();

			Assert.IsInstanceOf<LoggingRepositoryDecorator<IReviewableEmployee>>(repository);

			var inner = ((LoggingRepositoryDecorator<IReviewableEmployee>)repository).inner;
			Assert.IsInstanceOf<DemoRepository<IReviewableEmployee>>(inner);

			var actualInner = ((DemoRepository<IReviewableEmployee>)inner);
			Assert.AreEqual("Generic Repostiory With No Cache", actualInner.Name);
			Assert.IsInstanceOf<NullCache<IReviewableEmployee>>(actualInner.Cache);
		}

		[Test]
		public void Resolves_named_open_generic_service_even_if_closed_version_with_different_name_exists()
		{
			Container.Install(FromFile("ComplexGenericConfig.xml"));

			var repository = Container.Resolve<IRepository<IReviewer>>("generic.repository");

			Assert.IsNotInstanceOf<ReviewerRepository>(repository);
			Assert.IsInstanceOf<DemoRepository<IReviewer>>(repository);
		}

		[Test]
		public void Returns_same_instance_for_open_generic_singleton_service()
		{
			Container.Install(FromFile("GenericDecoratorConfig.xml"));

			var one = Container.Resolve<IRepository<string>>();
			var two = Container.Resolve<IRepository<string>>();

			Assert.AreSame(one, two);
		}

		[Test]
		public void Returns_same_instance_for_open_generic_singleton_service_multiple_closed_types()
		{
			Container.Install(FromFile("GenericDecoratorConfig.xml"));

			var one = Container.Resolve<IRepository<string>>();
			var two = Container.Resolve<IRepository<string>>();
			Assert.AreSame(one, two);

			var three = Container.Resolve<IRepository<int>>();
			var four = Container.Resolve<IRepository<int>>();
			Assert.AreSame(three, four);
		}
	}
#endif
}