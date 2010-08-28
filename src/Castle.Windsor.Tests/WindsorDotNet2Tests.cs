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


namespace Castle.Windsor.Tests
{
	using System;

	using Castle.DynamicProxy;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor.Configuration.Interpreters;
	using Castle.Windsor.Tests.ClassComponents;
	using Castle.Windsor.Tests.Components;
	using Castle.Windsor.Tests.Interceptors;

	using NUnit.Framework;

	[TestFixture]
	public class WindsorDotNet2Tests
	{
#if !SILVERLIGHT
		public string GetFilePath(string fileName)
		{
			return ConfigHelper.ResolveConfigPath("DotNet2Config/" + fileName);
		}
#endif

		[Test]
		public void CanCreateANormalTypeWithCtorDependencyOnGenericType()
		{
			IWindsorContainer container = new WindsorContainer();

			container.Register(Component.For(typeof(NeedsGenericType)).Named("comp"));
			container.Register(Component.For(typeof(ICache<>)).ImplementedBy(typeof(NullCache<>)).Named("cache"));

			var needsGenericType = container.Resolve<NeedsGenericType>();

			Assert.IsNotNull(needsGenericType);
		}
		
#if !SILVERLIGHT
		[Test]
		public void ChainOfResponsability()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("chainOfRespnsability.config")));
			var resolve = container.Resolve<IResultFinder<int>>();
			Assert.IsTrue(resolve.Finder is DatabaseResultFinder<int>);
			Assert.IsTrue(resolve.Finder.Finder is WebServiceResultFinder<int>);
			Assert.IsTrue(resolve.Finder.Finder.Finder is FailedResultFinder<int>);
			Assert.IsTrue(resolve.Finder.Finder.Finder.Finder == null);
		}

		[Test]
		public void ChainOfResponsability_Smart()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("chainOfRespnsability_smart.config")));
			var resolve = container.Resolve<IResultFinder<int>>();
			Assert.IsTrue(resolve is CacheResultFinder<int>);
			Assert.IsTrue(resolve.Finder is DatabaseResultFinder<int>);
			Assert.IsTrue(resolve.Finder.Finder is WebServiceResultFinder<int>);
			Assert.IsNull(resolve.Finder.Finder.Finder);

			var resolve2 = container.Resolve<IResultFinder<String>>();
			Assert.IsTrue(resolve2 is ResultFinderStringDecorator);
			Assert.IsNotNull(resolve2.Finder);
		}

		[Test]
		public void ComplexGenericConfiguration_GetGenericRepostiory()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("ComplexGenericConfig.xml")));
			var empRepost = container.Resolve<IRepository<IEmployee>>();
			Assert.IsNotNull(empRepost);
			Assert.IsTrue(typeof(LoggingRepositoryDecorator<IEmployee>).IsInstanceOfType(empRepost));
			var log = empRepost as LoggingRepositoryDecorator<IEmployee>;
			var inner = log.inner;
			Assert.IsNotNull(inner);
			Assert.IsTrue(typeof(DemoRepository<IEmployee>).IsInstanceOfType(inner));
			var demoEmpRepost = inner as DemoRepository<IEmployee>;
			Assert.AreEqual("Generic Repostiory", demoEmpRepost.Name);
			Assert.IsNotNull(demoEmpRepost.Cache);
			Assert.IsTrue(typeof(DictionaryCache<IEmployee>).IsInstanceOfType(demoEmpRepost.Cache));
		}

		[Test]
		public void ComplexGenericConfiguration_GetRepositoryByIdAndType()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("ComplexGenericConfig.xml")));
			var repository = container.Resolve<IRepository<IReviewer>>("generic.repository");
			Assert.IsTrue(typeof(DemoRepository<IReviewer>).IsInstanceOfType(repository), "Not DemoRepository!");
		}

		[Test]
		public void ComplexGenericConfiguration_GetReviewRepository_BoundAtConfiguration()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("ComplexGenericConfig.xml")));
			var rev = container.Resolve<IRepository<IReviewer>>();

			Assert.IsTrue(typeof(ReviewerRepository).IsInstanceOfType(rev));
			var repos = rev as ReviewerRepository;
			Assert.AreEqual("Reviewer Repository", repos.Name);
			Assert.IsNotNull(repos.Cache);
			Assert.IsTrue(typeof(DictionaryCache<IReviewer>).IsInstanceOfType(repos.Cache));
		}
		[Test]
		public void
			ComplexGenericConfiguration_GetReviewableRepostiory_ShouldResolveToDemoRepostiroyInsideLoggingRepositoryWithNoCaching
			()
		{
			var container = new WindsorContainer(new XmlInterpreter(GetFilePath("ComplexGenericConfig.xml")));
			
			var repository = container.Resolve<IRepository<IReviewableEmployee>>();

			Assert.IsInstanceOf<LoggingRepositoryDecorator<IReviewableEmployee>>(repository);

			var inner = ((LoggingRepositoryDecorator<IReviewableEmployee>)repository).inner;
			Assert.IsNotNull(inner);

			Assert.IsInstanceOf<DemoRepository<IReviewableEmployee>>(inner);

			var demoRepository = ((DemoRepository<IReviewableEmployee>)inner);
			Assert.AreEqual("Generic Repostiory With No Cache", demoRepository.Name);
			var cache = (inner as DemoRepository<IReviewableEmployee>).Cache;
			Assert.IsNotNull(cache);

			Assert.IsInstanceOf<NullCache<IReviewableEmployee>>(cache);
		}

		[Test]
		public void GetGenericService()
		{
			IWindsorContainer container = new WindsorContainer(new XmlInterpreter(GetFilePath("GenericsConfig.xml")));
			var repos = container.Resolve<IRepository<int>>("int.repos.generic");

			Assert.IsInstanceOf<DemoRepository<int>>(repos);
		}

		[Test]
		public void GetGenericServiceWithDecorator()
		{
			var container = new WindsorContainer(new XmlInterpreter(GetFilePath("GenericsConfig.xml")));
			
			var repos = container.Resolve<IRepository<int>>("int.repos");

			Assert.IsInstanceOf<LoggingRepositoryDecorator<int>>(repos);
			Assert.IsInstanceOf<DemoRepository<int>>(((LoggingRepositoryDecorator<int>)repos).inner);
		}

		[Test]
		public void GetGenericServiceWithDecorator_GenericDecoratorOnTop()
		{
			IWindsorContainer container = new WindsorContainer(new XmlInterpreter(GetFilePath("DecoratorConfig.xml")));
			var repos = container.Resolve<IRepository<int>>();

			Assert.IsInstanceOf<LoggingRepositoryDecorator<int>>(repos);
			Assert.IsInstanceOf<DemoRepository<int>>(((LoggingRepositoryDecorator<int>)repos).inner);
			Assert.AreEqual("second", ((DemoRepository<int>)((LoggingRepositoryDecorator<int>)repos).inner).Name);
		}

		[Test]
		public void GetSameInstanceFromGenericType()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("GenericDecoratorConfig.xml")));

			var repos1 = container.Resolve<IRepository<string>>();
			var repos2 = container.Resolve<IRepository<string>>();

			Assert.AreSame(repos1, repos2);
		}

		[Test]
		public void GetSameInstanceOfGenericFromTwoDifferentGenericTypes()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("GenericDecoratorConfig.xml")));

			var repos1 = container.Resolve<IRepository<string>>();
			var repos2 = container.Resolve<IRepository<string>>();

			Assert.AreSame(repos1, repos2);

			var repos3 = container.Resolve<IRepository<int>>();
			var repos4 = container.Resolve<IRepository<int>>();

			Assert.AreSame(repos3, repos4);
		}

		[Test]
		public void InferGenericArgumentForComponentFromPassedType()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("GenericDecoratorConfig.xml")));

			var repos = container.Resolve<IRepository<string>>();
			Assert.IsInstanceOf<LoggingRepositoryDecorator<string>>(repos);

			var inner = ((LoggingRepositoryDecorator<string>)repos).inner as DemoRepository<string>;

			Assert.AreEqual("second", inner.Name);
		}

		[Test]
		public void ResolveGenericWithId()
		{
			IWindsorContainer container = new WindsorContainer(new XmlInterpreter(GetFilePath("GenericsConfig.xml")));
			var svr = container.Resolve<ICalcService>("calc");
			Assert.IsInstanceOf<CalculatorService>(svr);
		}

		[Test]
		public void TestGenericSpecialization()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("ComplexGenericConfig.xml")));
			var repository = container.Resolve<IRepository<IReviewer>>();
			Assert.IsInstanceOf<ReviewerRepository>(repository);
		}

		[Test]
		public void UsingResolveGenericMethodOverload()
		{
			IWindsorContainer container = new WindsorContainer(new XmlInterpreter(GetFilePath("GenericsConfig.xml")));
			var svr = container.Resolve<ICalcService>();
			Assert.IsTrue(typeof(CalculatorService).IsInstanceOfType(svr));
		}

		[Test]
		public void WillUseDefaultCtorOnGenericComponentIfTryingToResolveOnSameComponent()
		{
			IWindsorContainer container =
				new WindsorContainer(new XmlInterpreter(GetFilePath("RecursiveDecoratorConfig.xml")));
			var resolve =
				(LoggingRepositoryDecorator<int>)container.Resolve<IRepository<int>>();
			Assert.IsNull(resolve.inner);
		}
#endif

		[Test]
		public void Can_resolve_type_with_by_ref_dependency_provided_inline()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<HasByRefCtorArgument>());

			container.Resolve<HasByRefCtorArgument>(new Arguments().Insert("a", new A()));
		}

		[Test]
		public void Can_resolve_type_with_by_ref_dependency()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<A>(),
							   Component.For<HasByRefCtorArgument>());

			container.Resolve<HasByRefCtorArgument>();
		}


		[Test]
		public void InterceptGeneric1()
		{
			var container = new WindsorContainer();

			container.AddFacility("interceptor-facility", new MyInterceptorGreedyFacility());
			container.Register(Component.For<StandardInterceptor>().Named("interceptor"));
			container.Register(Component.For<IRepository<Employee>>().ImplementedBy<DemoRepository<Employee>>().Named("key"));

			var store = container.Resolve<IRepository<Employee>>();

			Assert.IsFalse(typeof(DemoRepository<Employee>).IsInstanceOfType(store), "This should have been a proxy");
		}

		[Test]
		public void InterceptGeneric2()
		{
			var container = new WindsorContainer();

			container.AddFacility("interceptor-facility", new MyInterceptorGreedyFacility2());
			container.Register(Component.For<StandardInterceptor>().Named("interceptor"));
			container.Register(
				Component.For(typeof(IRepository<>)).ImplementedBy(typeof(DemoRepository<>)).Named("key"));

			var store = container.Resolve<IRepository<Employee>>();

			Assert.IsFalse(typeof(DemoRepository<Employee>).IsInstanceOfType(store), "This should have been a proxy");
		}

		[Test]
		public void InterceptorInheritFromGenericType()
		{
			var container = new WindsorContainer();

			container.Register(Component.For<CollectInterceptedIdInterceptor>().Named("interceptor"));
			container.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(DemoRepository<>)).Named("key")
			                   	.Interceptors<CollectInterceptedIdInterceptor>());

			var demoRepository = container.Resolve<IRepository<object>>();
			demoRepository.Get(12);

			Assert.AreEqual(12, CollectInterceptedIdInterceptor.InterceptedId, "invocation should have been intercepted by MyInterceptor");
		}

		[Test]
		public void LifestyleIsInheritsFromGenericType()
		{
			var container = new WindsorContainer();

			container.AddFacility("interceptor-facility", new MyInterceptorGreedyFacility2());
			container.Register(Component.For(typeof(StandardInterceptor)).Named("interceptor"));
			container.Register(Component.For(typeof(IRepository<>))
			                   	.ImplementedBy(typeof(DemoRepository<>))
			                   	.Named("key").LifeStyle.Transient);
			var store = container.Resolve<IRepository<Employee>>();
			var anotherStore = container.Resolve<IRepository<Employee>>();

			Assert.IsFalse(typeof(DemoRepository<Employee>).IsInstanceOfType(store), "This should have been a proxy");
			Assert.AreNotSame(store, anotherStore, "This should be two different instances");
		}

		[Test]
		public void ParentResolverIntercetorShouldNotAffectGenericComponentInterceptor()
		{
			var container = new WindsorContainer();
			container.Register(Component.For<CollectInterceptedIdInterceptor>());

			container.Register(
				Component.For<ISpecification>()
					.ImplementedBy<MySpecification>()
					.Interceptors<CollectInterceptedIdInterceptor>());
			container.Register(
				Component.For(typeof(IRepository<>)).ImplementedBy(typeof(TransientRepository<>)).Named("repos"));

			var specification = container.Resolve<ISpecification>();
			var isProxy = specification.Repository.GetType().FullName.Contains("Proxy");
			Assert.IsFalse(isProxy);
		}

		[Test]
		public void TestComponentLifestylePerGenericType()
		{
			var container = new WindsorContainer();

			container.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(TransientRepository<>)).Named("comp"));

			object o1 = container.Resolve<IRepository<Employee>>();
			object o2 = container.Resolve<IRepository<Employee>>();
			object o3 = container.Resolve<IRepository<Reviewer>>();
			object o4 = container.Resolve<IRepository<Reviewer>>();

			Assert.IsFalse(ReferenceEquals(o1, o2));
			Assert.IsFalse(ReferenceEquals(o1, o3));
			Assert.IsFalse(ReferenceEquals(o1, o4));
		}

		[Test]
		public void TestComponentLifestylePerGenericTypeNotMarkedAsTransient()
		{
			IWindsorContainer container = new WindsorContainer();

			container.Register(
				Component.For(typeof(IRepository<>)).ImplementedBy(typeof(RepositoryNotMarkedAsTransient<>)).Named("comp").LifeStyle
					.Transient);

			object o1 = container.Resolve<IRepository<Employee>>();
			object o2 = container.Resolve<IRepository<Employee>>();
			object o3 = container.Resolve<IRepository<Reviewer>>();
			object o4 = container.Resolve<IRepository<Reviewer>>();

			Assert.IsFalse(ReferenceEquals(o1, o2));
			Assert.IsFalse(ReferenceEquals(o1, o3));
			Assert.IsFalse(ReferenceEquals(o1, o4));
		}
	}
}