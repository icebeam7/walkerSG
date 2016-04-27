using Plugin.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using WalkerSG.Clases;
using Xamarin.Forms;

namespace WalkerSG.Pages
{
	public partial class TodayStatsPage : TabbedPage
	{
        public List<BandData> BandData { get; set; }

        public TodayStatsPage ()
		{
			InitializeComponent ();

            Utils.SaveRecordedHealth();
            BandData = new WalkerDB().GetBandData().Where(x => x.Period > 0).ToList();
            pieSeries.ItemsSource = Utils.GetTodayWalks(BandData);

            var todayStats = Utils.GetTodayStatistics(BandData);
            lblSteps.Text = todayStats[0].ToString();
            lblCalories.Text = todayStats[1].ToString();
            lblDistance.Text = todayStats[2].ToString();

            var totalStats = Utils.GetTotalStatistics(BandData);
            lblTotalSteps.Text = totalStats[0].ToString();
            lblTotalCalories.Text = totalStats[1].ToString();
            lblTotalDistance.Text = totalStats[2].ToString();
        }

        void btnShare_Clicked(object sender, EventArgs e)
        {
            CrossShare.Current.Share(String.Format("Walker has helped me to take {0} steps, burn {1} calories, and walk {2} meters today. Can you beat my score?", lblSteps.Text, lblCalories.Text, lblDistance.Text), "Walker Info");
        }
    }
}
