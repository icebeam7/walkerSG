using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace WalkerSG.Clases
{
    public static class StorageService
    {
        static IDictionary<string, object> localSettings = Application.Current.Properties;

        public static void SaveSetting(string key, string value)
        {
            localSettings[key] = value;
        }

        public static void DeleteSetting(string key)
        {
            localSettings.Remove(key);
        }

        public static string LoadSetting(string key)
        {
            var value = localSettings[key];

            if (value == null)
                return null;

            return value.ToString();
        }
    }
}
