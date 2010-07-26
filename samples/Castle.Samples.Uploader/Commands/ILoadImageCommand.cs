namespace Castle.Samples.Uploader.Commands
{
	using System;

	public interface ILoadImageCommand:ICommand
	{
		event Action<string> LoadImage;
	}
}