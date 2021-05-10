using PDMDatabase.Repositories;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Data;

namespace PDMDatabase.MemoryDatabase
{
    public enum MemoryDatabaseTables
    {
        DB_VERSION = 0,
        ArticlesDef = DB_VERSION + 1,
        ArticlesRules = DB_VERSION + 2,
        Constraints = DB_VERSION + 3,

        Days = DB_VERSION + 4,
        DaysDef = DB_VERSION + 5,
        Groups  = DB_VERSION + 6,
        GroupsChilds = DB_VERSION + 7,
        Intervals =	DB_VERSION + 8,
        Status = DB_VERSION + 9,
        Tariffs	= DB_VERSION + 10,
        TimeTables = DB_VERSION + 11,
        Operations	= DB_VERSION + 12,

        Articles = DB_VERSION + 13,
        StatusModem = DB_VERSION + 14,
        ShowArticlesRules = DB_VERSION + 15,
        Platforms = DB_VERSION + 16,
        SpaceSensors = DB_VERSION + 17,
        UMDMStationsSlots = DB_VERSION + 18,
        UMDMTimeSlots = DB_VERSION + 19,
        ElecChargers = DB_VERSION + 20 ,

        NotFoundTable = 99
    }

    public class MemoryDatabase
    {
        private readonly ILoggerManager loggerManager;
        private readonly ITraceable trace;
        private Dictionary<MemoryDatabaseTables, IInMemoryTable> inMemoryTables;

        public MemoryDatabase(ILoggerManager loggerManager)
        {
            this.loggerManager = loggerManager;
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public void SetTracerEnabled(bool enabled)
        {
            if (trace != null)
            {
                trace.Enabled = enabled;
            }
        }

        public void LoadData(IDbConnection connection)
        {
            trace.Write(TraceLevel.Info, "LoadData");

            inMemoryTables = new Dictionary<MemoryDatabaseTables, IInMemoryTable>
            {
                { MemoryDatabaseTables.DB_VERSION, new IMT_TablesVersion(loggerManager, connection) },
                { MemoryDatabaseTables.Groups, new IMT_Groups(loggerManager, connection) },
                { MemoryDatabaseTables.GroupsChilds, new IMT_GroupsChilds(loggerManager, connection) },
                { MemoryDatabaseTables.Articles, new IMT_Articles(loggerManager, connection) },
                { MemoryDatabaseTables.ArticlesRules, new IMT_ArticlesRules(loggerManager, connection) },
                { MemoryDatabaseTables.Days, new IMT_Days(loggerManager, connection) },
                { MemoryDatabaseTables.DaysDef, new IMT_DaysDef(loggerManager, new DaysDefRepository(connection)) },
                { MemoryDatabaseTables.TimeTables, new IMT_TimeTables(loggerManager, connection) },
                { MemoryDatabaseTables.Constraints, new IMT_Constraints(loggerManager, new ConstraintsRepository(connection)) },
                { MemoryDatabaseTables.Tariffs, new IMT_Tariffs(loggerManager, new TariffsRepository(connection)) },
                { MemoryDatabaseTables.Status, new IMT_Status(loggerManager, new StatusRepository(connection)) },
                { MemoryDatabaseTables.Intervals, new IMT_Intervals(loggerManager, new IntervalsRepository(connection)) }
            };

            foreach (KeyValuePair<MemoryDatabaseTables, IInMemoryTable> table in inMemoryTables)
            {
                if (table.Key != MemoryDatabaseTables.DB_VERSION) {
                    table.Value.Version = GetTable<IMT_TablesVersion>(MemoryDatabaseTables.DB_VERSION).GetTableVersion((long)table.Key);
                }
                table.Value.LoadData();
                table.Value.SetTracerEnabled(trace.Enabled);

                trace.Write(TraceLevel.Info, $"Loaded table {Enum.GetName(typeof(MemoryDatabaseTables), table.Key)} Version {table.Value.Version} Count {table.Value.GetNum()}");
            }

            trace.Write(TraceLevel.Info, "LoadData finished");
        }

        /// <exception cref="MemoryDatabaseTableNotFoundException"></exception>
        public IInMemoryTable GetTable(MemoryDatabaseTables table)
        {
            trace.Write(TraceLevel.Debug, "GetTable");

            if (!inMemoryTables.ContainsKey(table)) {
                trace.Write(TraceLevel.Error, $"Table name {Enum.GetName(typeof(MemoryDatabaseTables), table)} not found.");

                throw new MemoryDatabaseTableNotFoundException(table);
            }

            trace.Write(TraceLevel.Debug, $"Get table name {Enum.GetName(typeof(MemoryDatabaseTables), table)}");

            return inMemoryTables[table];
        }

        /// <exception cref="MemoryDatabaseTableNotFoundException"></exception>
        public T GetTable<T>(MemoryDatabaseTables table) where T : IInMemoryTable {
            T requestedTable = (T)GetTable(table);

            return requestedTable;
        }

        public bool DropTables() {
            trace.Write(TraceLevel.Debug, "DropTables");
            bool bRdo = true;

            try
            {
                inMemoryTables.Clear();
                bRdo = true;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                bRdo = false;
            }

            return bRdo;
        }

        public bool ReloadTariffs()
        {
            throw new NotImplementedException();
        }
    }
}
