namespace Castle.Windsor.Debugging
{
	using System.Collections.Generic;

	public interface IComponentDebuggerExtension
	{
		IEnumerable<DebuggerViewItem> Attach();
	}
}