using System;
using System.Globalization;

namespace PDMHelpers
{
    public class COPSDate 
    {
        public const int OPSDATE_LEN = 12;
        public const int OPSDATE_TRACESTRINGLEN = 19;

        public const string HOUR_24_FORMAT_BACK_COMP = "hh";
        public const string HOUR_24_FORMAT = "h2";
        public const string HOUR_12_FORMAT = "h1";
        public const string AMPM_1_FORMAT = "pm";
        public const string AMPM_2_FORMAT = "am";
        public const string MINUTE_FORMAT = "mi";
        public const string MINUTE_FORMAT_BACK_COMP = "mm";
        public const string SECOND_FORMAT = "ss";
        public const string DAY_FORMAT = "dd";
        public const string DAY_FORMAT_2 = "DD";
        public const string MONTH_FORMAT = "MM";
        public const string YEAR_4_FORMAT = "yyyy";
        public const string YEAR_2_FORMAT = "yy";
        public const string YEAR_4_FORMAT_2 = "YYYY";
        public const string YEAR_2_FORMAT_2 = "YY";

        
        protected ITraceable m_pTrace;
        private DateTime dt;
        public DateTime Value {
            get => dt;
            protected set {
                try
                {
                    dt = value;
                    SetStatus(COPSDateStatus.Valid);
                }
                catch (Exception)
                {
                    SetStatus(COPSDateStatus.Invalid);
                }
            }
        }

        public int TimeToMinutes()
        {
            return Value.Hour * 60 + Value.Minute;
        }

        private COPSDateStatus status;
        public COPSDateStatus GetStatus()
        {
            return status;
        }
        public void SetStatus(COPSDateStatus value)
        {
            status = value;
        }

        public COPSDate(double systime)
        {
            try
            {
                Value = DateTime.FromOADate(systime);
            }
            catch (Exception)
            {
                SetStatus(COPSDateStatus.Invalid);
            }
        }
        public COPSDate(DateTime date)
        {
            try
            {
                Value = DateTime.FromOADate(date.ToOADate());
            }
            catch (Exception)
            {
                SetStatus(COPSDateStatus.Invalid);
            }
            
        }
        public COPSDate(COPSDate date)
        {
            try
            {
                if (!date.IsValid())
                {
                    throw new InvalidOperationException("INVALID DATE");
                }

                Value = DateTime.FromOADate(date.Value.ToOADate());

            }
            catch (Exception)
            {
                SetStatus(COPSDateStatus.Invalid);
            }
            
        }
        public COPSDate(string date)
        {
            try
            {
                Value = DateTime.ParseExact(date, "HHmmssddMMyy", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                SetStatus(COPSDateStatus.Invalid);
            }

        }
        public COPSDate(int hour, int minute, int second, int day, int month, int year)
        {
            try
            {
                Value = new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception)
            {
                SetStatus(COPSDateStatus.Invalid);
            }
        }

        public COPSDate()
        {
        }

        /// <summary>
        /// HHmmssddMMyy
        /// </summary>
        /// <param name="strDate"></param>
        public void Set(string strDate) {
            // HHMMSSDDMMYY - Formato 
            if (strDate.Length == OPSDATE_LEN)
            {
                try
                {
                    Value = DateTime.ParseExact(strDate, "HHmmssddMMyy", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    SetStatus(COPSDateStatus.Invalid);
                }
                
            }
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException("");
        }

        public static bool operator <(COPSDate a, COPSDate b)
        {
            return a.dt < b.dt;
        }
        public static bool operator >(COPSDate a, COPSDate b)
        {
            return a.dt > b.dt;
        }
        public static bool operator <=(COPSDate a, COPSDate b)
        {
            return a.dt <= b.dt;
        }
        public static bool operator >=(COPSDate a, COPSDate b)
        {
            return a.dt >= b.dt;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool IsValid()
        {
            return GetStatus() == COPSDateStatus.Valid;
        }
        public string CopyToChar()
        {
            string result = string.Empty;
            try
            {
                if (GetStatus() == COPSDateStatus.Valid)
                {
                    return Value.ToString("HHmmssddMMyy", CultureInfo.InvariantCulture);
                }
                else {
                    result = string.Empty;
                }
            }
            catch (Exception )
            {
                result = string.Empty;
            }
            return result;
        }

        public COPSDate GetMinDateEx()
        {
            return new COPSDate(0,0,0,1,1,1998);
        }
        public COPSDate GetMaxDateEx()
        {
            return new COPSDate(0,0,0,1,1,2999);
        }

        public bool IsDateOkEx(ITraceable trace) {
            trace?.Write(TraceLevel.Info, "IsDateOkEx");
            bool fnResult = true;

            try
            {
                if (!IsValid()) {
                    trace?.Write( TraceLevel.Info, "The Date is not valid");
                    return false;
                }

                if (this < GetMinDateEx()) {
                    trace?.Write(TraceLevel.Info, "The Date is not valid");
                    return false;
                }

                if (this > GetMaxDateEx())
                {
                    trace?.Write(TraceLevel.Info, "The Date is not valid");
                    return false;
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        public string fstrGetTraceString()
        {
            string strDummy = string.Empty;

            if (!IsValid())
                strDummy = "DATE INVALID";
            else
                strDummy = Value.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

            return strDummy;
        }

        public int GetDayOfWeek()
        {
            int dayOfWeek = (int)Value.DayOfWeek;
            if (dayOfWeek == 0)
            {
                dayOfWeek = 7;
            }

            return dayOfWeek;
        }

        public void SetDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            try
            {
                Value = new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception)
            {
                SetStatus(COPSDateStatus.Invalid);
            }
        }

        public void AddTimeSpan(TimeSpan timeSpan)
        {
            Value += timeSpan;
        }

        public COPSDate Copy()
        {
            return new COPSDate(this);
        }
    }
}