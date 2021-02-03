
using PDMDatabase.Commands;
using PDMDatabase.Models;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_TablesVersion : InMemoryTable<TablesVersion>
    {
        public IMT_TablesVersion(ILoggerManager loggerManager, IDbConnection connection) : base(connection) {
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public override void LoadData()
        {
            trace?.Write(TraceLevel.Debug, "IMT_TablesVersion::LoadData");

            try
            {
                TablesVersionSelectCommand command = new TablesVersionSelectCommand(Connection, this.trace);
                Data = command.Execute();
                IsLoaded = true;
            }
            catch (System.Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                IsLoaded = false;
            }
        }

        public long GetTableVersion(long tableID) {
            trace?.Write(TraceLevel.Debug, "IMT_TablesVersion::GetTableVersion");
            long returnValue = (tableID >= (long)MemoryDatabaseTables.ShowArticlesRules) ? 0 : 1;

            try
            {
                returnValue = Data.FirstOrDefault(w => w.TBV_ID == tableID)?.TBV_VERSION ?? returnValue;
                
                trace?.Write(TraceLevel.Info, $"The Version of the Table({tableID}) is ({returnValue})");
            }
            catch (System.Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
            }

            return returnValue;
        }

        public void SetTracerEnabled(bool enabled)
        {
            if (trace != null)
            {
                trace.Enabled = enabled;
            }
        }

    }
}
