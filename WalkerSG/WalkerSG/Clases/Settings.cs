using SQLite;
using System;

namespace WalkerSG.Clases
{
    public class Settings
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string PhoneNumber { get; set; }
        public bool Verified { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Interval { get; set; }
        public int Steps { get; set; }
        public int ReminderType { get; set; }
        public bool Congratulate { get; set; }

        public static Settings GetDefaultSetting()
        {
            DateTime currentTime = DateTime.Now;

            Settings setting = new Settings()
            {
                StartTime = new TimeSpan(currentTime.Hour, currentTime.Minute, 0),
                Interval = 1,
                PhoneNumber = "",
                ReminderType = 0,
                Steps = 100,
                Verified = false,
                Congratulate = false
            };

            setting.EndTime = setting.StartTime.Add(new TimeSpan(1, 0, 0));

            return setting;
        }

        public static Settings SetSetting(int id, TimeSpan endTime, TimeSpan startTime, int interval, string phoneNumber, int remdinderType, string steps, bool isVerified, bool congratulate)
        {
            return new Settings()
            {
                ID = id
            };
        }

    }
}
