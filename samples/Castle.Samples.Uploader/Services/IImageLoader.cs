namespace Castle.Samples.Uploader.Services
{
	using System.Drawing;

	public interface IImageLoader
	{
		Image Load(string imageSource);

		void Unload(Image image);
	}
}