namespace Castle.Samples.Uploader.Presenters
{
	using System;
	using System.Drawing;

	using Castle.Samples.Uploader.Services;

	public class ImagePresenter:IDisposable
	{
		private readonly IImageView view;
		private ILoadImageCommand loadImage;
		private readonly IImageLoader imageLoader;

		public ImagePresenter(IImageView view, ILoadImageCommand loadImage, IImageLoader imageLoader)
		{
			this.view = view;
			this.imageLoader = imageLoader;
			this.loadImage = loadImage;
			this.loadImage.LoadImage += OnLoadImage;
		}

		private void OnLoadImage(string imageSource )
		{
			var image = imageLoader.Load(imageSource);
			var oldImage = view.Image;
			view.Image = image;
			imageLoader.Unload(oldImage);
		}

		public object Activate()
		{
			return view;
		}

		public void Dispose()
		{
			if (loadImage != null)
			{
				loadImage.LoadImage -= OnLoadImage;
				loadImage = null;
			}
		}
	}

	public interface ILoadImageCommand:ICommand
	{
		event Action<string> LoadImage;
	}

	public interface IImageView
	{
		Image Image { set; get; }
	}
}