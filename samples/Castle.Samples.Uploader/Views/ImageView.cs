using System.Drawing;
using System.Windows.Forms;

namespace Castle.Samples.Uploader.Views
{
	using Castle.Samples.Uploader.Presenters;

	public partial class ImageView : UserControl,IImageView
	{
		public ImageView()
		{
			InitializeComponent();
		}

		public Image Image
		{
			set { image.Image = value; }
			get { return image.Image; }
		}
	}
}
