namespace Castle.Windsor.Debugging
{
	using System.Diagnostics;

	[DebuggerDisplay("{value}", Name = "{name,nq}")]
	public class DebuggerViewItem
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string name;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		private readonly object value;

		public DebuggerViewItem(string name, object value)
		{
			this.name = name;
			this.value = value;
		}
	}
}