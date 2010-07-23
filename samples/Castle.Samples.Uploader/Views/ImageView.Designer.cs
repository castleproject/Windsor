namespace Castle.Samples.Uploader.Views
{
	partial class ImageView
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.image = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.image)).BeginInit();
			this.SuspendLayout();
			// 
			// image
			// 
			this.image.Dock = System.Windows.Forms.DockStyle.Fill;
			this.image.Location = new System.Drawing.Point(0, 0);
			this.image.Name = "image";
			this.image.Size = new System.Drawing.Size(581, 456);
			this.image.TabIndex = 0;
			this.image.TabStop = false;
			// 
			// ImageView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.image);
			this.Name = "ImageView";
			this.Size = new System.Drawing.Size(581, 456);
			((System.ComponentModel.ISupportInitialize)(this.image)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox image;
	}
}
