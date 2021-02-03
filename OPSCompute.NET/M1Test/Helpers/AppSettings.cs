using System;
using System.Configuration;

namespace M1Test.Helpers
{

    public class AppSettings
    {
        
        private static AppSettingsReader appSettings = new AppSettingsReader();

        protected AppSettings() { }

        public static string ConnectionString
        {
            get
            {
                string strConnectionString = null;
                strConnectionString = (string)appSettings.GetValue("OPSConnectionString", typeof(string));

                return strConnectionString;
            }
        }
        public static int NumberOfIterations
        {
            get
            {
                int numberOfIterations;
                numberOfIterations = (int)appSettings.GetValue("NumberOfIterations", typeof(int));

                return numberOfIterations;
            }
        }
    }
}
