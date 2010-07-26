namespace Castle.Samples.Uploader.Services
{
	using System;
	using System.Windows.Forms;

	public class WinFormsFileImageSource : IImageSource,IDisposable
	{
		private OpenFileDialog dialog = new OpenFileDialog { CheckFileExists = true };

		public string SelectImage()
		{
			if (dialog.ShowDialog() == DialogResult.OK)
			{
				return dialog.FileName;
			}

			return null;
		}

		public void Dispose()
		{
			if (dialog == null)
			{
				return;
			}

			dialog.Dispose();
			dialog = null;
		}
	}
}