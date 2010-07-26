using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Castle.Samples.Uploader
{
	public partial class Shell : Form, ICommandHost
	{
		// TODO: zarejestrowac z type forwarding na ICommandHost

		public Shell()
		{
			InitializeComponent();
		}

		public void Attach(ICommand command)
		{
			throw new NotImplementedException();
		}

		public void Detach(ICommand command)
		{
			throw new NotImplementedException();
		}
	}
}
