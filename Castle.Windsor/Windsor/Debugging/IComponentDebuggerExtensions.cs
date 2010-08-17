namespace Castle.Windsor.Debugging
{
	using System.Collections.Generic;

	public interface IComponentDebuggerExtensions
	{
		IEnumerable<IDebuggerViewItem> Attach();
	}
}