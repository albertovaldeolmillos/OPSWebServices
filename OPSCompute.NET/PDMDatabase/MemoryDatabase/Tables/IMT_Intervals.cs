
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
    public class IMT_Intervals : InMemoryTable<Intervals>
    {
        IntervalsRepository repository;

        public IMT_Intervals(ILoggerManager loggerManager, IntervalsRepository repository) : base(repository.Connection) {
            trace = loggerManager.CreateTracer(this.GetType());
            this.repository = repository;
            this.repository.Trace = trace;
        }

        public override void LoadData()
        {
            trace?.Write(TraceLevel.Debug, "IMT_Intervals::LoadData");

            try
            {
                Data = repository.GetAll(Version);
                IsLoaded = true;
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                IsLoaded = false;
            }
        }

        public bool GetIntervals(long lSubTariff, ref long lIntervalSize, ref List<long> plIntervalMinutes, ref List<long> plIntervalMoney, ref List<int> piIntervalIntermediateValuesPossible)
        {
            trace.Write(TraceLevel.Debug, "IMT_Intervals::GetIntervals");
            bool fnResult = true;

            try
            {
                foreach (Intervals interval in Data)
                {
                    if (interval.INT_STAR_ID == lSubTariff)
                    {
                        plIntervalMinutes.Add(interval.INT_MINUTES.Value);
                        plIntervalMoney.Add(interval.INT_VALUE.Value);
                        piIntervalIntermediateValuesPossible.Add((int)interval.INT_VALID_INTERMEDIATE_POINTS.Value);
                        lIntervalSize++;
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
