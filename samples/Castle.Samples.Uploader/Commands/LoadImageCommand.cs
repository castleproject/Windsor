namespace Castle.Samples.Uploader.Commands
{
	using System;

	using Castle.Samples.Uploader.Services;

	public class LoadImageCommand : AbstractCommand, ILoadImageCommand
	{
		private readonly IImageSource imageSource;

		public LoadImageCommand(IImageSource imageSource, ICommandHost host):base(host)
		{
			this.imageSource = imageSource;
		}

		public override string Name
		{
			get { return "Open image..."; }
		}

		public override string Description
		{
			get { return "Opens the image you want to upload."; }
		}

		public override bool Enabled
		{
			get { return true; }
		}

		public override void Execute()
		{
			var uri = imageSource.SelectImage();
			if(string.IsNullOrEmpty(uri)) return;

			LoadImage(uri);
		}

		public event Action<string> LoadImage = delegate { };
	}
}