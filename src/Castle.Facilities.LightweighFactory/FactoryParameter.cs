namespace Castle.Facilities.LightweighFactory
{
	using System;

	public class FactoryParameter
	{
		private readonly Type type;
		private readonly string name;
		private readonly int position;
		private readonly object value;
		private bool used;

		public FactoryParameter(Type type, string name,int position, object value)
		{
			this.type = type;
			this.name = name;
			this.position = position;
			this.value = value;
		}

		public int Position
		{
			get { return position; }
		}

		public bool Used
		{
			get { return used; }
		}

		public Type Type
		{
			get { return type; }
		}

		public string Name
		{
			get { return name; }
		}

		public object ResolveValue()
		{
			used = true;
			return value;
		}
	}
}