namespace Castle.Samples.Uploader.Services
{
	using System.Drawing;

	public class ImageLoader : IImageLoader
	{
		public Image Load(string imageSource)
		{
			return Image.FromFile(imageSource);
		}

		public void Unload(Image image)
		{
			image.Dispose();
		}
	}
}