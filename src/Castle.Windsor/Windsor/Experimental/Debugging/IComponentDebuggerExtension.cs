namespace Castle.Windsor.Experimental.Debugging
{
	using System.Collections.Generic;

	public interface IComponentDebuggerExtension
	{
		IEnumerable<DebuggerViewItem> Attach();
	}
}