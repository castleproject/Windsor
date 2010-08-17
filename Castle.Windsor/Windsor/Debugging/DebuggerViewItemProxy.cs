namespace Castle.Windsor.Debugging
{
	using System.Diagnostics;

	public class DebuggerViewItemProxy
	{
		private readonly DebuggerViewItem item;

		public DebuggerViewItemProxy(DebuggerViewItem item)
		{
			this.item = item;
		}

		[DebuggerDisplay("{item.value}", Name = "{item.name}")]
		public object Value
		{
			get { return item.value; }
		}
	}
}