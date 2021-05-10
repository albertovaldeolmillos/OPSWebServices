using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMHelpers
{
    public class COTSStrMap
    {
        private List<KeyValuePair<string, string>> values;

        public COTSStrMap()
        {
            values = new List<KeyValuePair<string, string>>();
        }

        public void Add(KeyValuePair<string, string> value) {
            values.Add(value);
        }
        public void Clear() {
            values.Clear();
        }

        public string Lookup(string strKey) {
            KeyValuePair<string, string> strVal;
            strVal = values.FirstOrDefault(f => f.Key == strKey);
            return strVal.Equals(default(KeyValuePair<string, string>)) ? null : strVal.Value;
        }

        public int GetInt(string strKey, int defaultValue) {
            int nRdo = defaultValue;

            try
            {
                string strVal = Lookup(strKey);

                if (strVal != null)
                    nRdo = int.Parse(strVal);
            }
            catch (Exception error)
	        {
                nRdo = defaultValue;
            }

            return nRdo;
        }
        public long GetLong(string strKey, long defaultValue) {
            long nRdo = defaultValue;

            try
            {
                string strVal = Lookup(strKey);

                if (strVal != null)
                    nRdo = long.Parse(strVal);
            }
            catch (Exception error)
            {
                nRdo = defaultValue;
            }

            return nRdo;
        }
        public string GetTCHAR(string strKey) {
            string nRdo = null;

            try
            {
                string strVal = Lookup(strKey);

                if (strVal != null)
                    nRdo = strVal;
            }
            catch (Exception error)
            {
                nRdo = null;
            }

            return nRdo;
        }
        public double GetDouble(string strKey) {
            double nRdo = 0;

            try
            {
                string strVal = Lookup(strKey);

                if (strVal != null)
                    nRdo = double.Parse(strVal);
            }
            catch (Exception error)
            {
                nRdo = 0;
            }

            return nRdo;
        }
        public COPSDate GetOPSDate(string strKey, COPSDate defaultValue) {
            string date = Lookup(strKey);
            COPSDate opsDate = defaultValue.Copy();

            if (date != null) {
                opsDate.Set(date);
            }

            return opsDate;
        }
        public string fstrGetOPSPlate(string strKey) {
            COPSPlate plate = GetOPSPlate(strKey);

            return plate == null ? string.Empty : plate.ToString();
        }
        public COPSPlate GetOPSPlate(string strKey)
        {
            COPSPlate opsPlate = null;
            try
            {
                string strVal = Lookup(strKey);
                if (strVal != null)
                {
                    opsPlate = new COPSPlate(strVal);
                }
            }
            catch (Exception)
            {
            }

            return opsPlate;
        }
    }
}
