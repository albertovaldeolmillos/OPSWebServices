
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
    public class IMT_Status : InMemoryTable<Status>
    {
        StatusRepository repository;
        public IMT_Status(ILoggerManager loggerManager, StatusRepository repository) : base(repository.Connection)
        {
            trace = loggerManager.CreateTracer(this.GetType());
            this.repository = repository;
            this.repository.Trace = trace;
        }

        public override void LoadData()
        {
            trace?.Write(TraceLevel.Debug, "IMT_Status::LoadData");
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

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///	Searches the status for a machine. If a state is found for that specific machine, exits. While this
        /// it also searches a state defined for the entire group (but it is only used if no state is defined
        /// for the specific machine). If a state defined for the type of group it is used only if no state is
        /// defined neither for group nor type of group
        /// </summary>
        public bool GetDayBlockStatus(long unitId, long groupId, long groupTypeId, long lBlockID, long dayType, ref bool bFind, ref long lStatusAct)
        {
            trace.Write(TraceLevel.Debug, "IMT_Status::GetDayBlockStatus");
            bool fnResult = true;

            try
            {
                trace.Write(TraceLevel.Info, $"Group: {groupId}, Day: {dayType}, Block: {lBlockID}");

                lStatusAct = GlobalDefs.DEF_UNDEFINED_VALUE;

                foreach (Status status in Data)
                {
                    if ((status.STA_TIM_ID.HasValue && status.STA_TIM_ID == lBlockID) &&
                        (status.STA_DDAY_ID.HasValue && status.STA_DDAY_ID == dayType))
                    {
                        // Searches status for unit
                        if (status.STA_UNI_ID.HasValue && status.STA_UNI_ID == unitId) {
                            lStatusAct = status.STA_DSTA_ID.Value;
                            bFind = true;
                            trace.Write(TraceLevel.Info, $"We have found status {lStatusAct} for unit ({unitId}) Type Of Day({dayType})");
                            break;
                        }
                        // Searches status for group
                        else if (status.STA_GRP_ID.HasValue && status.STA_GRP_ID == groupId)
                        {
                            lStatusAct = status.STA_DSTA_ID.Value;
                            bFind = true;
                            trace.Write(TraceLevel.Info, $"We have found status {lStatusAct} for Group ({groupId}) Type Of Day({dayType})");
                        }
                        // Searches status for type of group
                        else if (status.STA_DGRP_ID.HasValue && status.STA_DGRP_ID == groupTypeId && lStatusAct == GlobalDefs.DEF_UNDEFINED_VALUE)
                        {
                            lStatusAct = status.STA_DSTA_ID.Value;
                            bFind = true;
                            trace.Write(TraceLevel.Info, $"We have found status {lStatusAct} for type of group ({unitId}) Type Of Day({dayType})");
                        }
                    }
                }
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
    }
}
