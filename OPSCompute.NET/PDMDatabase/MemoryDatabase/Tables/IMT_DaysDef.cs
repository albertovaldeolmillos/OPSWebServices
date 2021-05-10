
using PDMDatabase.Commands;
using PDMDatabase.Models;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using PDMDatabase.Repositories;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_DaysDef : InMemoryTable<DaysDef>
    {
        DaysDefRepository repository;
        public IMT_DaysDef(ILoggerManager loggerManager, DaysDefRepository repository) : base(repository.Connection)
        {
         
            trace = loggerManager.CreateTracer(this.GetType());
            this.repository = repository;
            this.repository.Trace = trace;
        }

        public override void LoadData()
        {
            trace?.Write(TraceLevel.Debug, "LoadData");
            try
            {
                Data = repository.GetAll();
                IsLoaded = true;
            }
            catch (System.Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                IsLoaded = false;
            }
        }

        public List<long> GetListTypeOfDays(COPSDate date) {
            trace?.Write(TraceLevel.Debug, "GetListTypeOfDays");
            List<long> fnResult = null;

            try
            {
                int dayOfWeek = date.GetDayOfWeek();

                trace?.Write(TraceLevel.Info, "QUERY : Which are the days to apply?");

                IEnumerable<DaysDef> dayTypes = Data.Where(w => w.DDAY_CODE[dayOfWeek - 1] == '1');
                fnResult = dayTypes.Select(s => s.DDAY_ID.Value).ToList();
                
                dayTypes.ToList().ForEach(t => LogDayResponse(t));
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                throw;
            }

            return fnResult;
        }

        private void LogDayResponse(DaysDef day)
        {
            trace?.Write(TraceLevel.Info, $"RESPONSE: Day Id({day.DDAY_ID}) Day Code ({day.DDAY_CODE}) Day Desc({day.DDAY_DESC})");
        }
    }
}
