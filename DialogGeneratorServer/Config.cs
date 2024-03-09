using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialogGeneratorServer
{
    internal class Config
    {
        public static string GetString(string key)
        {
            return GetString(key, string.Empty);
        }

        public static string GetString(string key, string defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value)) value = defaultValue;

            return value;
        }

        public static int GetInt(string key)
        {
            return GetInt(key, 0);
        }

        public static int GetInt(string key, int defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (!int.TryParse(value, out int intValue)) intValue = defaultValue;

            return intValue;
        }
    }
}
