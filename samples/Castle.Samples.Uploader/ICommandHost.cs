namespace Castle.Samples.Uploader
{
	public interface ICommandHost
	{
		void Attach(ICommand command);

		void Detach(ICommand command);
	}
}