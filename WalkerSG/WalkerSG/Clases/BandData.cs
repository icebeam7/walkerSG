using System;
using SQLite;

namespace WalkerSG.Clases
{
    public class BandData
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public long TotalDistance { get; set; }
        public long TodayDistance { get; set; }
        public long TodaySteps { get; set; }
        public long TotalSteps { get; set; }
        public long TotalCalories { get; set; }
        public long TodayCalories { get; set; }
        public DateTime CapturedAt { get; set; }
        public bool Result { get; set; }
        public int Period { get; set; }
        
        public BandData Clone()
        {
            var bandHealth = new BandData
            {
                CapturedAt = this.CapturedAt,
                TotalDistance = this.TotalDistance,
                TotalSteps = this.TotalSteps,
                TodaySteps = this.TodaySteps,
                TodayDistance = this.TodayDistance,
                Result = this.Result,
                Period = this.Period,
                TodayCalories = this.TodayCalories,
                TotalCalories = this.TotalCalories
            };

            return bandHealth;
        }
    }
}
