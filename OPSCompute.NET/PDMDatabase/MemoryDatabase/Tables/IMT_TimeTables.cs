
using PDMDatabase.Commands;
using PDMDatabase.Models;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_TimeTables : InMemoryTable<Timetables>
    {
        public IMT_TimeTables(ILoggerManager loggerManager, IDbConnection connection) : base(connection) {
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public override void LoadData()
        {
            trace?.Write(TraceLevel.Debug, "IMT_TimeTables::GetTimFromDate");

            try
            {
                TimeTablesSelectCommand command = new TimeTablesSelectCommand(Connection, this.trace);
                Data = command.Execute();
                IsLoaded = true;
            }
            catch (System.Exception error)
            {
                IsLoaded = false;
            }
        }

        public Timetables GetIniEndFromTimId(long timId) {
            trace?.Write(TraceLevel.Debug, "IMT_TimeTables::GetIniEndFromTimId");
            Timetables returnValue = null;

            try
            {
                returnValue = Data.Single(w => w.TIM_ID == timId);
                trace?.Write(TraceLevel.Info, $"RESPONSE : TimId({returnValue.TIM_ID}) [{returnValue.TIM_INI}, {returnValue.TIM_END}]");
            }
            catch (System.Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                returnValue = null;
            }

            return returnValue;
        }

        public List<long> GetTimFromDate(long minutes)
        {
            trace?.Write(TraceLevel.Debug, "IMT_TimeTables::GetTimFromDate");
            List<long> returnValue = null;

            try
            {
                trace?.Write(TraceLevel.Info, $"GetTimFromDate - Evaluating minutes: {minutes}");

                IEnumerable<Timetables> foundTimetables = Data
                    .Where(w => w.TIM_INI <= minutes && minutes < w.TIM_END);

                foreach (Timetables timetable in foundTimetables)
                {
                    trace?.Write(TraceLevel.Info, $"GetTimFromDate - Found block {timetable.TIM_ID}: [{timetable.TIM_INI}, {timetable.TIM_END}]");
                }

                returnValue = foundTimetables
                        .Select(s => s.TIM_ID)
                    .ToList();

                
            }
            catch (System.Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                returnValue = null;
            }

            return returnValue;
        }
    }
}
