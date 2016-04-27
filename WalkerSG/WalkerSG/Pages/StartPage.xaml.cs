using System;
using System.Collections.Generic;
using System.Linq;
using WalkerSG.Clases;
using WalkerSG.Clases.Pages;
using Xamarin.Forms;

namespace WalkerSG.Pages
{
	public partial class StartPage : ContentPage
	{
		public StartPage ()
		{
			InitializeComponent ();

        }

        void btnSettings_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new SettingsPage());
        }

        void btnTodayStats_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new TodayStatsPage());
        }

        void btnWeekStats_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new WeekStatsPage());
        }

        void btnMonthStats_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MonthStatsPage());
        }

        void btnAbout_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AboutPage());
        }
    }
}
