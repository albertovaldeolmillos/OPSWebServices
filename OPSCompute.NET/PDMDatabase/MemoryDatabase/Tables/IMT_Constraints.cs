
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
    public class IMT_Constraints : InMemoryTable<Constraints>
    {
        ConstraintsRepository repository;
        public IMT_Constraints(ILoggerManager loggerManager, ConstraintsRepository repository) : base(repository.Connection)
        {
            trace = loggerManager.CreateTracer(this.GetType());
            this.repository = repository;
            this.repository.Trace = trace;

        }

        public override void LoadData()
        {
            trace?.Write(TraceLevel.Debug, "IMT_Constraints::LoadData");
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

        public List<Constraints> GetConstraints(long constraintId )
        {
            trace?.Write(TraceLevel.Debug, "IMT_Constraints::GetConstraints");
            List<Constraints> fnResult = null;

            try
            {
                fnResult = Data
                                .Where(c => c.CON_ID == constraintId)
                                .Select(c =>
                                {
                                    c.CON_GRP_ID = (c.CON_GRP_ID.HasValue) ? c.CON_GRP_ID : GlobalDefs.DEF_UNDEFINED_VALUE;
                                    c.CON_DGRP_ID = (c.CON_DGRP_ID.HasValue) ? c.CON_DGRP_ID : GlobalDefs.DEF_UNDEFINED_VALUE;
                                    return c;
                                })
                                .ToList();
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                throw new InvalidOperationException("FAILED call to GetConstraints");
            }

            return fnResult;

        }
    }
}
