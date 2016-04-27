using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace WalkerSG.Clases.Pages
{
	public partial class SettingsPage : ContentPage
	{
        NumberVerify nexmo;
        VerifyResponse verifyResponse;

        bool originalIsVerified;
        int interval;
        string phoneNumber;
        bool firstTime = true;
        bool isVerified = false;

        Settings setting;

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            await WaitAndExecute(1000, () =>
            {
                lblTitle.Text = "Settings";
                firstTime = true;
                nexmo = new NumberVerify();
                setting = new WalkerDB().GetSettings();

                firstTime = true;
                tmpEnd.Time = setting.EndTime;
                tmpStart.Time = setting.StartTime;
                txtPhoneNumber.Text = setting.PhoneNumber;
                pckReminderType.SelectedIndex = setting.ReminderType;
                sldStepsGoal.Value = setting.Steps;
                lblStepsGoal.Text = setting.Steps.ToString();
                isVerified = setting.Verified;
                swtCongratulations.IsToggled = setting.Congratulate;

                if (!isVerified)
                {
                    btnRequestCode.IsEnabled = true;
                    btnRequestCode.Text = "Request code";
                }
                else
                {
                    btnRequestCode.IsEnabled = false;
                    btnRequestCode.Text = "Verified";
                    lblVerifyMessage.Text = "Your number has been verified";
                    lblVerifyMessage.TextColor = Color.Green;
                }

                interval = setting.Interval;
                phoneNumber = setting.PhoneNumber;
                originalIsVerified = isVerified;

                switch (interval)
                {
                    case 1: pckInterval.SelectedIndex = 0; break;
                    case 5: pckInterval.SelectedIndex = 1; break;
                    case 15: pckInterval.SelectedIndex = 2; break;
                    case 60: pckInterval.SelectedIndex = 3; break;
                    case 120: pckInterval.SelectedIndex = 4; break;
                    default: pckInterval.SelectedIndex = 0; break;
                }
            });
        }

        protected async Task WaitAndExecute(int milisec, Action actionToExecute)
        {
            lblTitle.Text = "Loading...";
            await Task.Delay(milisec);
            actionToExecute();
        }

        public SettingsPage ()
		{
			InitializeComponent ();
        }

        async Task RequestCode()
        {
            verifyResponse = await nexmo.Verify(txtPhoneNumber.Text);

            if (verifyResponse.status != "0")
            {
                lblRequestMessage.Text = "Code was not sent. Please try again";
                lblRequestMessage.TextColor = Color.Red;
                stpCode.IsVisible = false;
            }
            else
            {
                lblRequestMessage.Text = "Code was sent. Please type it below";
                lblRequestMessage.TextColor = Color.Blue;
                stpCode.IsVisible = true;
            }
        }

        async void btnRequestCode_Clicked(object sender, EventArgs e)
        {
            bool show = false;

            if (txtPhoneNumber.Text != phoneNumber || txtPhoneNumber.Text == "")
                show = true;
            else if (txtPhoneNumber.Text == phoneNumber && txtPhoneNumber.Text != "")
                show = !originalIsVerified;

            if (show)
                await RequestCode();
        }

        async void btnVerifyCode_Clicked(object sender, EventArgs e)
        {
            lblRequestMessage.Text = "";

            var checkResponse = await nexmo.Check(verifyResponse.request_id, txtCode.Text);

            if (checkResponse.status == "0")
            {
                stpCode.IsVisible = false;
                phoneNumber = txtPhoneNumber.Text;
                originalIsVerified = true;
                btnRequestCode.Text = "Verified";
                btnRequestCode.IsEnabled = false;
                lblVerifyMessage.Text = "Your number has been verified";
                lblVerifyMessage.TextColor = Color.Green;
                isVerified = true;
            }
            else
            {
                lblVerifyMessage.Text = "Wrong code. Please try again";
                lblVerifyMessage.TextColor = Color.Red;
                isVerified = false;
            }
        }

        private async void btnResendCode_Clicked(object sender, EventArgs e)
        {
            lblVerifyMessage.Text = "";
            var controlResponse = await nexmo.Control(verifyResponse.request_id);

            if (controlResponse.status != "0")
            {
                lblRequestMessage.Text = "Code was not sent. Please try again";
                lblRequestMessage.TextColor = Color.Red;
            }
            else
            {
                lblRequestMessage.Text = "Code was sent. Please type it below";
                lblRequestMessage.TextColor = Color.Blue;
            }
        }

        void tmpStart_TimeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
            {
                if (tmpEnd == null)
                    return;

                UpdateEndTimer();
            }
        }

        void UpdateEndTimer()
        {
            var diferencia = tmpEnd.Time.Subtract(tmpStart.Time).TotalMinutes;

            if (diferencia >= 0 && diferencia < interval)
                tmpEnd.Time = tmpStart.Time.Add(new TimeSpan(0, interval, 0));
        }

        void tmpEnd_TimeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
            {
                if (tmpStart == null)
                    return;

                var diferencia = tmpEnd.Time.Subtract(tmpStart.Time).TotalMinutes;

                if (diferencia >= 0 && diferencia < interval)
                    tmpStart.Time = tmpEnd.Time.Add(new TimeSpan(0, -interval, 0));
            }
        }

        private void pckInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (pckInterval.SelectedIndex)
            {
                case 0: interval = 1; break;
                case 1: interval = 5; break;
                case 2: interval = 15; break;
                case 3: interval = 60; break;
                case 4: interval = 120; break;
                default: interval = 1; break;
            }

            UpdateEndTimer();
        }

        private void sldStepsGoal_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            lblStepsGoal.Text = String.Format("{0}", (int)(sldStepsGoal.Value));
        }

        async void btnSave_Clicked(object sender, EventArgs e)
        {
            if (isVerified)
                Save();
            else
            {
                bool result = await DisplayAlert("Save?", "You must register your phone number in order to receive notifications from Walker. Do you want to save the settings?", "Yes", "No");

                if (result)
                    Save();
            }
        }

        async void btnStart_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Starting...", "Remember to pair your phone/tablet with your Microsoft Band", "Done!");
            MSBandTask task = new MSBandTask();
            task.Run();
            await DisplayAlert("Started", "Walker is tracking your activity.", "OK");
        }

        async void Save()
        {
            setting.EndTime = tmpEnd.Time;
            setting.StartTime = tmpStart.Time;
            setting.Interval = interval;
            setting.PhoneNumber = txtPhoneNumber.Text;
            setting.ReminderType = pckReminderType.SelectedIndex;
            setting.Steps = (int)sldStepsGoal.Value;
            setting.Verified = isVerified;
            setting.Congratulate = swtCongratulations.IsToggled;

            new WalkerDB().SaveSetting(setting);
            await DisplayAlert("Saved", "Settings saved successfully", "OK");
        }

    }
}
