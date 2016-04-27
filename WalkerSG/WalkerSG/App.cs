using System.Collections.ObjectModel;
using WalkerSG.Clases;
using WalkerSG.Clases.Pages;
using Xamarin.Forms;

namespace WalkerSG
{
    public class App : Application
	{
		public App ()
		{
            // The root page of your application
            //MainPage = new NavigationPage(new SettingsPage());
            MainPage = new NavigationPage(new Pages.StartPage());
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
