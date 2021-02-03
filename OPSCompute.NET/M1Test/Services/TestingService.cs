using M1Test.Helpers;
using PDMDatabase;
using PDMDatabase.MemoryDatabase;
using PDMDatabase.Models;
using PDMHelpers;
using System.Collections.Generic;

namespace M1Test.Services
{
    class TestingService
    {
        private readonly ITraceable trace;
        private readonly ILoggerManager loggerManager;

        public TestingService(ILoggerManager loggerManager)
        {
            this.loggerManager = loggerManager;
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public void Run()
        {
            this.trace.Write(TraceLevel.Info, "OPENING DATABASE");

            Database database = new Database(this.loggerManager);
            database.Open(AppSettings.ConnectionString);

            this.trace.Write(TraceLevel.Info, "DATABASE IS OPEN");

            MemoryDatabase imd = new MemoryDatabase(this.loggerManager);
            this.trace.Write(TraceLevel.Info, "LOAD MEMORY DATABASE");
            imd.LoadData(database.Connection);
            this.trace.Write(TraceLevel.Info, "MEMORY DATABASE IS LOADED");

            SearchOperationsByVehicleId(imd, "5525CFH");
            SearchGroup(imd, 50001);

        }

        private void SearchGroup(MemoryDatabase imd, long beasainGroup)
        {
            IMT_Groups groupsTable = imd.GetTable(MemoryDatabaseTables.Groups) as IMT_Groups;
            this.trace.Write(TraceLevel.Info, $"SEARCHING FOR GROUP {beasainGroup} ");

            string descripcion = groupsTable.GetGroupDesc(beasainGroup);
            long tipo = groupsTable.GetGroupType(beasainGroup);
            long related = groupsTable.GetGroupRelated(beasainGroup);

            this.trace.Write(TraceLevel.Info, $"GROUP {beasainGroup} \\t{descripcion} \\t{tipo} \\t{related}");
        }

        private void SearchOperationsByVehicleId(MemoryDatabase imd, string vehicleIdFilter)
        {
            this.trace.Write(TraceLevel.Info, $"SEARCHING FOR VEHICLE {vehicleIdFilter} OPERATIONS");
            IMT_Operations operationsTable = imd.GetTable(MemoryDatabaseTables.Operations) as IMT_Operations;
            IEnumerable<Operations> byVehicle = operationsTable.GetVehicleOperations(vehicleIdFilter);
            this.trace.Write(TraceLevel.Info, $"VEHICLE {vehicleIdFilter} HAS {(byVehicle as IList<Operations>).Count} OPERATIONS");
        }
    }
}
