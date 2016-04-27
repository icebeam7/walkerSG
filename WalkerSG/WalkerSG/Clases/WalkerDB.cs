using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SQLite;

namespace WalkerSG.Clases
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }

    public class WalkerDB
    {
        static object locker = new object();

        SQLiteConnection database;

        string DatabasePath
        {
            get
            {
                var sqliteFilename = "Walker.db3";
#if __IOS__
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
                string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
                var path = Path.Combine(libraryPath, sqliteFilename);
#else
#if __ANDROID__
				string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal); // Documents folder
				var path = Path.Combine(documentsPath, sqliteFilename);
#else
				// WinPhone
				var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, sqliteFilename);;
#endif
#endif
                return path;
            }
        }

        public WalkerDB()
        {
            database = new SQLiteConnection(DatabasePath);
            database.CreateTable<Settings>();
            database.CreateTable<BandData>();
        }

        public IEnumerable<BandData> GetBandData()
        {
            lock (locker)
            {
                return (from i in database.Table<BandData>() select i).OrderByDescending(x => x.CapturedAt).ToList();
            }
        }

        public Settings GetSettings()
        {
            lock (locker)
            {
                var setting = (from i in database.Table<Settings>() select i).FirstOrDefault();
                return (setting != null) ? setting : Settings.GetDefaultSetting();
            }
        }

        public IEnumerable<BandData> GetBandDataNotDone()
        {
            lock (locker)
            {
                return database.Query<BandData>("SELECT * FROM [BandData] WHERE [Done] = 0");
            }
        }

        public BandData GetBandData(int id)
        {
            lock (locker)
            {
                return database.Table<BandData>().FirstOrDefault(x => x.ID == id);
            }
        }

        public int SaveBandData(BandData item)
        {
            lock (locker)
            {
                if (item.ID != 0)
                {
                    database.Update(item);
                    return item.ID;
                }
                else {
                    return database.Insert(item);
                }
            }
        }

        public int SaveSetting(Settings item)
        {
            lock (locker)
            {
                if (item.ID != 0)
                {
                    database.Update(item);
                    return item.ID;
                }
                else {
                    return database.Insert(item);
                }
            }
        }

        public int DeleteBandData(int id)
        {
            lock (locker)
            {
                return database.Delete<BandData>(id);
            }
        }
    }
}
