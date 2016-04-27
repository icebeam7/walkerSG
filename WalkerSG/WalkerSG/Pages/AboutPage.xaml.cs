using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace WalkerSG.Pages
{
	public partial class AboutPage : ContentPage
	{
		public AboutPage ()
		{
			InitializeComponent ();
		}

        void btnSendEmail_Clicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("mailto:beltran_prieto@fai.utb.cz"));
        }
    }
}
