namespace Castle.Samples.Uploader
{
	public interface ICommand
	{
		string Name { get; }
		string Description { get; }
		bool Enabled { get; }

		void Execute();
	}
}