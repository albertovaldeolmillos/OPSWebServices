using PDMDatabase.Commands;
using PDMDatabase.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_Groups : InMemoryTable<Groups>
    {
        public IMT_Groups(PDMHelpers.ILoggerManager loggerManager, IDbConnection connection) : base(connection) {
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public override void LoadData()
        {
            try
            {
                GroupsSelectCommand command = new GroupsSelectCommand(Connection, this.trace);
                Data = command.Execute();
                IsLoaded = true;
            }
            catch (System.Exception error)
            {
                IsLoaded = false;
            }
        }

        public IEnumerable<Groups> GetAllGroups()
        {
            LoadIfIsNeeded();
            return Data.Where(w => w.GRP_DGRP_ID != 5);
        }


        public long GetGroupType(long groupID)
        {
            LoadIfIsNeeded();
            return Data.FirstOrDefault(o => o.GRP_ID.Equals(groupID))?.GRP_DGRP_ID ?? -1;
        }

        public string GetGroupDesc(long groupID)
        {
            LoadIfIsNeeded();
            return Data.FirstOrDefault(o => o.GRP_ID.Equals(groupID))?.GRP_DESCLONG ?? null;
        }

        public long GetGroupRelated(long groupID)
        {
            LoadIfIsNeeded();
            return Data.FirstOrDefault(o => o.GRP_ID.Equals(groupID))?.GRP_RELATED ?? -1;
        }
    }
}
