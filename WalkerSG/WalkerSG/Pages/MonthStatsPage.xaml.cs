using Plugin.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using WalkerSG.Clases;
using Xamarin.Forms;

namespace WalkerSG.Pages
{
	public partial class MonthStatsPage : TabbedPage
    {
        public List<BandData> BandData { get; set; }

        public MonthStatsPage ()
		{
			InitializeComponent ();

            Utils.SaveRecordedHealth();
            BandData = new WalkerDB().GetBandData().Where(x => x.Period > 0).ToList();
            pieSeries.ItemsSource = Utils.GetMonthWalks(BandData);

            var stats = Utils.GetMonthStatistics(BandData);
            lblSteps.Text = stats[0].ToString();
            lblCalories.Text = stats[1].ToString();
            lblDistance.Text = stats[2].ToString();

            var totalStats = Utils.GetTotalStatistics(BandData);
            lblTotalSteps.Text = totalStats[0].ToString();
            lblTotalCalories.Text = totalStats[1].ToString();
            lblTotalDistance.Text = totalStats[2].ToString();

            StepsSeries.ItemsSource = Utils.GetMonthWalks(BandData, StatsType.Walks);
            CaloriesSeries.ItemsSource = Utils.GetMonthWalks(BandData, StatsType.Calories);
            DistanceSeries.ItemsSource = Utils.GetMonthWalks(BandData, StatsType.Distance);
        }

        void btnShare_Clicked(object sender, EventArgs e)
        {
            CrossShare.Current.Share(String.Format("Walker has helped me to take {0} steps, burn {1} calories, and walk {2} meters this month. Can you beat my score?", lblSteps.Text, lblCalories.Text, lblDistance.Text), "Walker Info");
        }

    }
}
