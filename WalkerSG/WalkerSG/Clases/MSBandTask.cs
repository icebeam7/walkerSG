using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Band.Portable;
using Microsoft.Band.Portable.Sensors;

namespace WalkerSG.Clases
{
    public class MSBandTask
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private BandClient bandClient;
        private BandData bandData;
        private BandDeviceInfo bandInfo;
        private BandData RecordedData { get; set; }

        private bool isDistanceReceived;
        private bool isPedometerReceived;
        private bool isCaloriesReceived;
        private bool isDistanceOn;
        private bool isPedometerOn;
        private bool isCaloriesOn;

        private bool isDataSaving;
        private Timer recordingTimer;

        private Settings setting;
        private DateTime? startTime, endTime;
        private long startSteps, endSteps, startCalories, endCalories, startDistance, endDistance;
        private int interval, seconds;
        private bool changeDay = true;
        private bool firstTimeD = true;
        private bool firstTimeC = true;
        private bool firstTimeP = true;

        public bool IsDataReceived
        {
            get
            {
                if (!this.isDistanceOn)
                    this.isDistanceReceived = true;

                if (!this.isPedometerOn)
                    this.isPedometerReceived = true;

                if (!this.isCaloriesOn)
                    this.isCaloriesReceived = true;

                return this.isDistanceReceived && this.isPedometerReceived && this.isCaloriesReceived;
            }
        }

        public bool AnySensorsOn { get { return this.isDistanceOn || this.isPedometerOn || this.isCaloriesOn; } }

        public async void Run()
        {
            try { await this.SetupBand(); }
            catch(Exception ex) { this.CompleteDeferral(); }
        }

        private async Task SetupBand()
        {
            this.RecordedData = new BandData();
            GetSetting();

            this.bandInfo = (await BandClientManager.Instance.GetPairedBandsAsync()).FirstOrDefault();

            if (this.bandInfo == null)
                throw new InvalidOperationException("No Microsoft Band available to connect to.");

            var isConnecting = false;

            using (new DisposableAction(() => isConnecting = true, () => isConnecting = false))
            {
                this.bandClient = await BandClientManager.Instance.ConnectAsync(this.bandInfo);

                if (this.bandClient == null)
                    throw new InvalidOperationException("Could not connect to the Microsoft Band available.");

                this.ResetReceivedFlags();
                this.bandData = new BandData();

                await this.SetupSensors();
            }
        }

        private void GetSetting()
        {
            var db = new WalkerDB();
            setting = db.GetSettings();
            interval = setting.Interval;

            seconds = 0;
            firstTimeC = true;
            firstTimeD = true;
            firstTimeP = true;

            if (changeDay)
            {
                DateTime now = DateTime.Now;

                endTime = new DateTime(now.Year, now.Month, now.Day, setting.EndTime.Hours, setting.EndTime.Minutes, 0);
                startTime = new DateTime(now.Year, now.Month, now.Day, setting.StartTime.Hours, setting.StartTime.Minutes, 0);
                changeDay = false;

                if (setting.EndTime.Subtract(setting.StartTime).TotalMinutes < 0)
                    endTime = endTime.Value.AddDays(1);
            }
        }

        private async void SaveData(object state)
        {
            System.Diagnostics.Debug.WriteLine("Called at " + DateTime.Now.ToString() + " seconds vale " + seconds);
            seconds += 30;
            await this.SaveData();
        }

        private async Task SaveData()
        {
            if (this.isDataSaving)
                return;

            if (this.IsDataReceived)
            {
                this.isDataSaving = true;
                this.bandData.CapturedAt = DateTime.Now;

                var health = this.bandData.Clone();

                /*if (this.RecordedData != null)
                    if (!this.RecordedData.Contains(health))
                        this.RecordedData.Add(health);*/

                RecordedData = health;

                if (health.CapturedAt >= startTime && health.CapturedAt <= endTime)
                {
                    this.isDataSaving = false;

                    if (seconds >= interval * 60)
                    {
                        health.Period = interval;
                        await CheckAndSave(health, true);
                        return;
                    }
                    else
                    {
                        await CheckAndSave(health, false);
                        return;
                    }
                }
                else
                    changeDay = true;

                await CheckAndSave(health, false);
            }

            this.isDataSaving = false;
        }

        async Task CheckAndSave(BandData health, bool save)
        {
            if (save)
            {
                System.Diagnostics.Debug.WriteLine("Saving " + DateTime.Now.ToString() + " seconds vale " + seconds);
                endSteps = health.TotalSteps;
                endCalories = health.TotalCalories;
                endDistance = health.TotalDistance;

                health.TodaySteps = endSteps - startSteps;
                health.TodayDistance = endDistance - startDistance;
                health.TodayCalories = endCalories - startCalories;

                startSteps = endSteps;
                startCalories = endCalories;
                startDistance = endDistance;
                seconds = 0;

                health.Result = (health.TodaySteps >= setting.Steps);
                
                if (setting.Verified)
                {
                    if (health.Result)
                        await new NexmoSMSVoice().Send(setting.PhoneNumber, String.Format("Hello. This is Walker. Well done! You walked {0} steps in the last {1} minutes. Congratulations!", health.TodaySteps, interval), setting.ReminderType);
                    else if (setting.ReminderType != 2)
                        await new NexmoSMSVoice().Send(setting.PhoneNumber, String.Format("Hello. This is Walker. Remember to walk at least {0} steps in the next {1} minutes. You can do it!", setting.Steps, interval), setting.ReminderType);
                }

                GetSetting();

                System.Diagnostics.Debug.WriteLine("{0}", health.CapturedAt);
                System.Diagnostics.Debug.WriteLine("Cals: {0} Total Cals: {1}", health.TodayCalories, health.TotalCalories);
                System.Diagnostics.Debug.WriteLine("Steps: {0} Total Steps: {1}", health.TodaySteps, health.TotalSteps);
                System.Diagnostics.Debug.WriteLine("Distance: {0} Total Distance: {1}", health.TodayDistance, health.TotalDistance);
                    
                await this.SaveRecordedHealth(save);
            }
        }

        private void OnDistanceChanged(object sender, BandSensorReadingEventArgs<BandDistanceReading> e)
        {
            if (this.isDataSaving)
                return;

            this.bandData.TotalDistance = e.SensorReading.TotalDistance;

            if (firstTimeD)
            {
                startDistance = this.bandData.TotalDistance;
                firstTimeD = false;
            }

            this.isDistanceReceived = true;
        }

        private void onPedometerChanged(object sender, BandSensorReadingEventArgs<BandPedometerReading> e)
        {
            if (this.isDataSaving)
                return;

            this.bandData.TotalSteps = e.SensorReading.TotalSteps;

            if (firstTimeP)
            {
                startSteps = this.bandData.TotalSteps;
                firstTimeP = false;
            }

            this.isPedometerReceived = true;
        }

        private void onCaloriesChanged(object sender, BandSensorReadingEventArgs<BandCaloriesReading> e)
        {
            if (this.isDataSaving)
                return;

            this.bandData.TotalCalories = e.SensorReading.Calories;

            if (firstTimeC)
            {
                startCalories = this.bandData.TotalCalories;
                firstTimeC = false;
            }

            this.isCaloriesReceived = true;
        }

        private async Task SetupSensors()
        {
            seconds -= 30;
            this.recordingTimer = new Timer(this.SaveData, null, Timeout.Infinite, Timeout.Infinite);
            System.Diagnostics.Debug.WriteLine("Iniciando a las " + DateTime.Now.ToString() + " seconds vale " + seconds);

            try
            {
                if (this.AnySensorsOn)
                    await this.StopSensorsRunning();

                await this.StartSensorsRunning();

                this.recordingTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
            }
            catch { this.CompleteDeferral(); }
        }

        private async Task StartSensorsRunning()
        {
            try
            {
                this.bandClient.SensorManager.Distance.ReadingChanged += this.OnDistanceChanged;
                await this.bandClient.SensorManager.Distance.StartReadingsAsync();

                this.isDistanceOn = true;
            }
            catch { this.isDistanceOn = false; }

            try
            {
                this.bandClient.SensorManager.Pedometer.ReadingChanged += onPedometerChanged;
                await this.bandClient.SensorManager.Pedometer.StartReadingsAsync();

                this.isPedometerOn = true;
            }
            catch { this.isPedometerOn = false; }

            try
            {
                this.bandClient.SensorManager.Calories.ReadingChanged += onCaloriesChanged;
                await this.bandClient.SensorManager.Calories.StartReadingsAsync();

                this.isCaloriesOn = true;
            }
            catch { this.isCaloriesOn = false; }
        }

        private void ResetReceivedFlags()
        {
            this.isDistanceReceived = false;
            this.isPedometerReceived = false;
            this.isCaloriesReceived = false;
        }

        private async void OnTaskCanceled()
        {
            await this.CompleteDeferral();
        }

        private async Task CompleteDeferral()
        {
            try { await this.SaveRecordedHealth(false); }
            catch (Exception ex) { /* Handle saving failure */ }

            if (this.bandClient != null)
            {
                await this.StopSensorsRunning();
                this.bandClient = null;
            }
        }

        private async Task SaveRecordedHealth(bool save)
        {
            await this.semaphore.WaitAsync();

            try
            {
                var db = new WalkerDB();
                db.SaveBandData(this.RecordedData);

            }
            catch { }
            finally { this.semaphore.Release(); }
        }

        private async Task StopSensorsRunning()
        {
            try { await this.bandClient.SensorManager.Distance.StopReadingsAsync(); }
            catch { }
            finally
            {
                this.bandClient.SensorManager.Distance.ReadingChanged -= this.OnDistanceChanged;
                this.isDistanceOn = false;
            }

            try { await this.bandClient.SensorManager.Pedometer.StopReadingsAsync(); }
            catch { }
            finally
            {
                this.bandClient.SensorManager.Pedometer.ReadingChanged -= this.onPedometerChanged;
                this.isPedometerOn = false;
            }

            try { await this.bandClient.SensorManager.Calories.StopReadingsAsync(); }
            catch { }
            finally
            {
                this.bandClient.SensorManager.Calories.ReadingChanged -= this.onCaloriesChanged;
                this.isDistanceOn = false;
            }
        }

        public async void Dispose()
        {
            if (null != this.bandClient)
                await this.CompleteDeferral();
        }
    }
}
