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

namespace Castle.Facilities.Synchronize.Tests
{
	using System.Threading;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Threading;

	public interface IDummyWindow
	{
		int AddControl(Control control);
	}

	public class DummyWindow : Window, IDummyWindow
	{
		private readonly StackPanel stackPanel;

		public DummyWindow()
		{
			stackPanel = new StackPanel();
			Content = stackPanel;
		}

		public Panel Panel
		{
			get { return stackPanel; }
		}

		public virtual int AddControl(Control control)
		{
			stackPanel.Children.Add(control);
			return stackPanel.Children.Count;
		}
	}

	public class ClassUsingWindow
	{
		[Synchronize]
		public virtual int DoWork(Panel panel)
		{
			panel.Children.Add(new Button());
			return panel.Children.Count;
		}
	}

	[Synchronize(typeof(DispatcherSynchronizationContext))]
	public class ClassUsingWindowInWindowsContext : ClassUsingWindow
	{
	}

	[Synchronize]
	public class ClassUsingWindowInAmbientContext
	{
		[Synchronize(UseAmbientContext = true)]
		public virtual int DoWork(Panel panel)
		{
			panel.Children.Add(new Button());
			return panel.Children.Count;
		}
	}

	[Synchronize(typeof(DispatcherSynchronizationContext))]
	public class ClassInDispatcherContextWithoutVirtualMethod
	{
		[Synchronize]
		public void DoWork(Panel panel)
		{
			panel.Children.Add(new Button());
		}
	}

	[Synchronize(typeof(SynchronizationContext))]
	public class ClassInDepdnencyContextWithMissingDependency
	{
		[Synchronize("foo")]
		public virtual void DoWork(Panel panel)
		{
			panel.Children.Add(new Button());
		}
	}

	public interface IClassUsingDepedenecyContext<T> where T : Panel
	{
		T DoWork(T work);
	}

	[Synchronize]
	public class ClassUsingDispatcherContext<T> : IClassUsingDepedenecyContext<T> where T : Panel
	{
		[Synchronize(typeof(DispatcherSynchronizationContext))]
		public T DoWork(T work)
		{
			work.Children.Add(new Button());
			return work;
		}
	}
}