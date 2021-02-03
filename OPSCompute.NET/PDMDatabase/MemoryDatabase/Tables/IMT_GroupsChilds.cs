using PDMDatabase.Commands;
using PDMDatabase.Models;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_GroupsChilds : InMemoryTable<GroupsChilds>
    {
        public IMT_GroupsChilds(ILoggerManager loggerManager, IDbConnection connection) : base(connection) {
            trace = loggerManager.CreateTracer(this.GetType());
        }

        public override void LoadData()
        {
            try
            {
                GroupsChildsSelectCommand command = new GroupsChildsSelectCommand(Connection, this.trace);
                Data = command.Execute();
                IsLoaded = true;
            }
            catch (System.Exception error)
            {
                IsLoaded = false;
            }
        }

        public bool GetGroupFromUnit(long lUnit, ref long lGroup, ref long lTypeOfGroup, IMT_Groups pGroups)
        {
            bool fnReturn = false;
            lGroup = GlobalDefs.DEF_UNDEFINED_VALUE;
            lTypeOfGroup = GlobalDefs.DEF_UNDEFINED_VALUE;

            try
            {
                IEnumerable<GroupsChilds> groupChilds = Data.Where(w => w.CGRP_CHILD == lUnit && w.CGRP_TYPE == "U");
                foreach (GroupsChilds child in groupChilds)
                {
                    lTypeOfGroup = pGroups.GetGroupType(child.CGRP_ID);
                    if (lTypeOfGroup == -1) {
                        throw new InvalidOperationException($"ERROR: GetGroupType {child.CGRP_ID}");
                    }

                    if (lTypeOfGroup != GlobalDefs.DEF_UNDEFINED_VALUE) {
                        lGroup = child.CGRP_ID;
                        fnReturn = true;
                        break;
                    }

                }
            }
            catch (Exception)
            {
                fnReturn = false;
            }

            return fnReturn;
        }

        public IEnumerable<GroupsChilds> GetAllGroupsChilds()
        {
            //trace.Write(TraceLevel.Info, "GetAllGroupsChilds");
            IEnumerable<GroupsChilds> fnResult = null;

            try
            {
                LoadIfIsNeeded();
                fnResult = Data.Where(w => w.CGRP_TYPE.ToUpper().ElementAt(0) != 'S');
            }
            catch (Exception)
            {
                //trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = null;
            }

            return fnResult;
        }
    }
}
