using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace WalkerSG.Clases
{
    public static class Utils
    {
        public static List<Walk> GetWeekWalks(List<BandData> bandData, StatsType statsType)
        {
            DateTime now = DateTime.Now;
            DateTime start = now.Date.AddDays(-(int)now.DayOfWeek); // prev sunday 00:00
            DateTime end = start.AddDays(7); // next sunday 00:00

            var weekData = bandData.Where(x => x.CapturedAt.Date >= start && x.CapturedAt.Date < end).OrderByDescending(x => x.CapturedAt.Date).GroupBy(x => x.CapturedAt.Date).Select(xg => new Walk { Result = xg.Key.ToString("ddd dd"), Count = xg.Sum(s => (statsType == StatsType.Walks) ? s.TodaySteps : (statsType == StatsType.Calories) ? s.TodayCalories : s.TodayDistance / 100.0) }).ToList();
            /*var days = weekData.Select(w => w.Result).ToList();

            for (int i = 0; i < 7; i++)
            {
                string d = start.AddDays(i).ToString("ddd dd");
                if (!days.Contains(d))
                    weekData.Insert(i, new Walk() { Result = d, Count = 0 });
            }

            //weekData.Reverse();
            */
            return weekData;
        }

        //
        static GregorianCalendar _gc = new GregorianCalendar();
        public static int GetWeekOfMonth(this DateTime time)
        {
            DateTime first = new DateTime(time.Year, time.Month, 1);
            return time.GetWeekOfYear() - first.GetWeekOfYear() + 1;
        }

        static int GetWeekOfYear(this DateTime time)
        {
            return _gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }
        //

        public static List<Walk> GetMonthWalks(List<BandData> bandData, StatsType statsType)
        {
            Func<DateTime, int> weekProjector =
                d => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                     d,
                     CalendarWeekRule.FirstFourDayWeek,
                     DayOfWeek.Sunday);

            Func<DateTime, int> monthProjector =
                d => d.GetWeekOfYear() - new GregorianCalendar().GetWeekOfYear(new DateTime(d.Year, d.Month, 1), CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            DateTime now = DateTime.Now;
            DateTime start = now.Date.AddDays(1 - now.Day);
            DateTime end = start.AddMonths(1);

            var monthData = from m in bandData
                            where m.CapturedAt.Date >= start && m.CapturedAt.Date < end
                            orderby m.CapturedAt.Date ascending
                            group m by weekProjector(m.CapturedAt);

            var monthWalks = monthData.Select(xg => new Walk { Result = "Week " + monthProjector(xg.First().CapturedAt), Count = xg.Sum(s => (statsType == StatsType.Walks) ? s.TodaySteps : (statsType == StatsType.Calories) ? s.TodayCalories : s.TodayDistance / 100.0) }).ToList();

            /*var weeks = monthWalks.Select(w => w.Result).ToList();

            for (int i = 1; i <= 5; i++)
            {
                string w = "Week " + i;
                if (!weeks.Contains(w))
                    monthWalks.Add(new Walk() { Result = w, Count = 0 });
            }
            */
            return monthWalks.OrderBy(w => w.Result).ToList();
        }

        public static void SaveRecordedHealth()
        {
            /*
            Random r = new Random();
            DateTime now = DateTime.Now;
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(1), Period = 1, Result = true, TodaySteps = r.Next(0, 2000), TotalDistance = 0, TotalSteps = 0, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 0 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(1).AddMinutes(-35), Period = 1, Result = false, TodaySteps = r.Next(0, 2000), TotalDistance = 10, TotalSteps = 10, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 40 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(2), Period = 1, Result = true, TodaySteps = r.Next(0, 2000), TotalDistance = 0, TotalSteps = 0, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 0 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(2).AddMinutes(-35), Period = 1, Result = false, TodaySteps = r.Next(0, 2000), TotalDistance = 10, TotalSteps = 10, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 40 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(3), Period = 1, Result = true, TodaySteps = r.Next(0, 2000), TotalDistance = 0, TotalSteps = 0, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 0 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(3).AddMinutes(-35), Period = 1, Result = false, TodaySteps = r.Next(0, 2000), TotalDistance = 10, TotalSteps = 10, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 40 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(4), Period = 1, Result = true, TodaySteps = r.Next(0, 2000), TotalDistance = 0, TotalSteps = 0, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 0 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(4).AddMinutes(-35), Period = 1, Result = false, TodaySteps = r.Next(0, 2000), TotalDistance = 10, TotalSteps = 10, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 40 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(5), Period = 1, Result = true, TodaySteps = r.Next(0, 2000), TotalDistance = 0, TotalSteps = 0, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 0 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(5).AddMinutes(-35), Period = 1, Result = false, TodaySteps = r.Next(0, 2000), TotalDistance = 10, TotalSteps = 10, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 40 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(6), Period = 1, Result = true, TodaySteps = r.Next(0, 2000), TotalDistance = 0, TotalSteps = 0, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 0 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(6).AddMinutes(-35), Period = 1, Result = false, TodaySteps = r.Next(0, 2000), TotalDistance = 10, TotalSteps = 10, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 40 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(7), Period = 1, Result = true, TodaySteps = r.Next(0, 2000), TotalDistance = 0, TotalSteps = 0, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 0 });
            new WalkerDB().SaveBandData(new BandData() { CapturedAt = now.AddDays(7).AddMinutes(-35), Period = 1, Result = false, TodaySteps = r.Next(0, 2000), TotalDistance = 10, TotalSteps = 10, TodayCalories = r.Next(0, 100), TodayDistance = r.Next(0, 2000), TotalCalories = 40 });
            */
        }

        private static List<BandData> GetTodayData(List<BandData> bandData)
        {
            return bandData.Where(x => x.CapturedAt.Date == DateTime.Now.Date).ToList();
        }

        private static List<BandData> GetWeekData(List<BandData> bandData)
        {
            DateTime now = DateTime.Now;
            DateTime start = now.Date.AddDays(-(int)now.DayOfWeek); // prev sunday 00:00
            DateTime end = start.AddDays(7); // next sunday 00:00

            return bandData.Where(x => x.CapturedAt.Date > start && x.CapturedAt.Date < end).ToList();
        }

        private static List<BandData> GetMonthData(List<BandData> bandData)
        {
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, 1);
            DateTime end = start.AddMonths(1);

            return bandData.Where(x => x.CapturedAt.Date > start && x.CapturedAt.Date < end).ToList();
        }

        public static long GetTodaySteps(List<BandData> bandData)
        {
            var todayData = GetTodayData(bandData);
            return todayData.Sum(x => x.TodaySteps);
        }

        public static List<Walk> GetTodayWalks(List<BandData> bandData)
        {
            var todayData = GetTodayData(bandData);
            var results = new List<Walk>();

            results.Add(new Walk() { Result = "Success", Count = todayData.Where(x => x.Result).Count() });
            results.Add(new Walk() { Result = "Failure", Count = todayData.Where(x => !x.Result).Count() });

            return results;
        }

        public static List<Walk> GetMonthWalks(List<BandData> bandData)
        {
            var todayData = GetMonthData(bandData);
            var results = new List<Walk>();

            results.Add(new Walk() { Result = "Success", Count = todayData.Where(x => x.Result).Count() });
            results.Add(new Walk() { Result = "Failure", Count = todayData.Where(x => !x.Result).Count() });

            return results;
        }

        public static List<Walk> GetWeekWalks(List<BandData> bandData)
        {
            var todayData = GetWeekData(bandData);
            var results = new List<Walk>();

            results.Add(new Walk() { Result = "Success", Count = todayData.Where(x => x.Result).Count() });
            results.Add(new Walk() { Result = "Failure", Count = todayData.Where(x => !x.Result).Count() });

            return results;
        }

        public static List<double> GetTodayStatistics(List<BandData> bandData)
        {
            var todayData = GetTodayData(bandData);
            var results = new List<double>();
            results.Add(todayData.Sum(x => x.TodaySteps));
            results.Add(todayData.Sum(x => x.TodayCalories));
            results.Add(todayData.Sum(x => x.TodayDistance / 100.0));

            return results;
        }

        public static List<double> GetWeekStatistics(List<BandData> bandData)
        {
            var todayData = GetWeekData(bandData);
            var results = new List<double>();
            results.Add(todayData.Sum(x => x.TodaySteps));
            results.Add(todayData.Sum(x => x.TodayCalories));
            results.Add(todayData.Sum(x => x.TodayDistance / 100.0));

            return results;
        }

        public static List<double> GetMonthStatistics(List<BandData> bandData)
        {
            var todayData = GetMonthData(bandData);
            var results = new List<double>();
            results.Add(todayData.Sum(x => x.TodaySteps));
            results.Add(todayData.Sum(x => x.TodayCalories));
            results.Add(todayData.Sum(x => x.TodayDistance / 100.0));

            return results;
        }

        public static List<double> GetTotalStatistics(List<BandData> bandData)
        {
            var results = new List<double>();
            results.Add(bandData.Sum(x => x.TodaySteps));
            results.Add(bandData.Sum(x => x.TodayCalories));
            results.Add(bandData.Sum(x => x.TodayDistance / 100.0));

            return results;
        }
    }
}
