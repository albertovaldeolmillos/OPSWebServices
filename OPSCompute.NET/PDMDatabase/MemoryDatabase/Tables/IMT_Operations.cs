using PDMDatabase.Models;
using PDMDatabase.Repositories;
using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PDMDatabase.MemoryDatabase
{
    public class IMT_Operations : InMemoryTable<Operations>
    {
        OperationsRepository repository;
        public IMT_Operations(PDMHelpers.ILoggerManager loggerManager, OperationsRepository repository) : base(repository.Connection) {
            trace = loggerManager.CreateTracer(GetType());
            this.repository = repository;
            this.repository.Trace = trace;
        }

        public override void LoadData()
        {
            try
            {
                Data = repository.GetAll();
                IsLoaded = true;
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                IsLoaded = false;
            }
            
        }

        public IEnumerable<Operations> GetAllOperations()
        {
            if (!IsLoaded)
            { 
                LoadData();
            }

            return Data;
        }

        public IEnumerable<Operations> GetVehicleOperations(DateTime date)
        {
            IEnumerable<Operations> result = null;

            if (IsLoaded)
            {
                result = Data.Where(o => o.OPE_MOVDATE.Equals(date)).AsEnumerable();
            }

            return result;
        }

        public IEnumerable<Operations> GetVehicleOperations(string vehicleId) {
            IEnumerable<Operations> result = null;

            if (IsLoaded)
            {
                result = Data.Where(o => o.OPE_VEHICLEID.Equals(vehicleId)).ToList();
            }

            return result;
        }

        public IEnumerable<Operations> GetVehicleOperations(string vehicleId, long articleId, bool mustSort = false)
        {
            trace.Write(TraceLevel.Debug, "IMT_Operations::GetVehicleOperations");

            IEnumerable<Operations> result = null;

            if (IsLoaded)
            {
                result = Data
                        .Where(o => o.OPE_VEHICLEID.Equals(vehicleId) &&
                                       o.OPE_DART_ID.Equals(articleId) &&
                                       o.OPE_DELETED == 0)
                        .AsEnumerable();

                if (result != null && mustSort)
                {
                    result = result.OrderBy(o => o.OPE_MOVDATE.Value).AsEnumerable();
                }
            }

            trace.Write(TraceLevel.Info, $"Found {result?.Count() ?? 0} operations for vehicle {vehicleId}");

            return result;
        }

        public long GetActualPayedQuantity(long operationId) {
            trace.Write(TraceLevel.Debug, "IMT_Operations::GetActualPayedQuantity");
            bool fnResult = true;
            long returnValue = GlobalDefs.DEF_UNDEFINED_VALUE;

            try
            {
                LoadIfIsNeeded();
                returnValue = Data.First(o => o.OPE_DELETED == 0 &&
                                                o.OPE_VALID == 1 &&
                                                o.OPE_ID.HasValue && o.OPE_ID.Value == operationId)
                                    .OPE_VALUE_VIS.GetValueOrDefault();
            }
            catch (Exception error)
            {
                trace.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            if (!fnResult) {
                returnValue = GlobalDefs.DEF_UNDEFINED_VALUE;
            }

            return returnValue;

        }
    }
}
