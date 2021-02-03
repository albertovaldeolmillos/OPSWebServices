
using PDMDatabase.Commands;
using PDMDatabase.Models;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_Days : InMemoryTable<Days>
    {
        public IMT_Days(ILoggerManager loggerManager,  IDbConnection connection) : base(connection) {
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public override void LoadData()
        {
            trace?.Write(TraceLevel.Debug, "IMT_Days::LoadData");
            try
            {
                DaysSelectCommand command = new DaysSelectCommand(Connection, this.trace);
                Data = command.Execute();

                IsLoaded = true;
            }
            catch (System.Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                IsLoaded = false;
            }
        }

        public Days IsSpecialDay(COPSDate pdtDate)
        {
            trace?.Write(TraceLevel.Debug, $"IMT_Days::IsSpecialDay");
            Days fnResult = null;
            try
            {
                LoadIfIsNeeded();
                IEnumerable<Days> filteredDays = FilterByDate(pdtDate);

                string dateString = pdtDate.Value.ToShortDateString();
                trace?.Write(TraceLevel.Info, $"IsSpecialDay - QUERY : Is the Day ({dateString}) an special day ?  (QUERY TABLE DAYS)");

                fnResult = filteredDays.Any() ? filteredDays.ElementAt(0) : null;
                LogResponse(fnResult, dateString);
            }
            catch (System.Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = null;
            }

            return fnResult;
        }

        private IEnumerable<Days> FilterByDate(COPSDate date)
        {
            if (date == null) throw new InvalidOperationException("Invalida date");
            if (!date.IsDateOkEx(trace)) throw new InvalidOperationException($"Error : Date is not Correct {date.fstrGetTraceString()}");

            return Data.Where(
                    d => d.DAY_DATE.IsValid() &&
                         d.DAY_DATE.Value.Year == date.Value.Year &&
                         d.DAY_DATE.Value.Month == date.Value.Month &&
                         d.DAY_DATE.Value.Day == date.Value.Day);
        }
        private void LogResponse(Days fnResult, string dateString)
        {
            if (fnResult != null)
            {
                trace?.Write(TraceLevel.Info, $"IsSpecialDay - RESPONSE: ({dateString}) IS the special Day {fnResult.DAY_ID} of type {fnResult.DAY_DDAY_ID}");
            }
            else
            {
                trace?.Write(TraceLevel.Info, $"IsSpecialDay - RESPONSE: ({dateString}) IS NOT a special Day ");
            }
        }
    }
}
