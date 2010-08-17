namespace Castle.Windsor.Debugging
{
	using System.Diagnostics;

	[DebuggerTypeProxy(typeof(DebuggerViewItemProxy))]
	[DebuggerDisplay("{value}", Name = "{name}")]
	public class DebuggerViewItem : IDebuggerViewItem
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal readonly string name;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal readonly object value;

		public DebuggerViewItem(string name, object value)
		{
			this.name = name;
			this.value = value;
		}
	}
}