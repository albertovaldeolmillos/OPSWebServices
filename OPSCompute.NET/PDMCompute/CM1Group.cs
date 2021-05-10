using PDMHelpers;
using System;
using System.Collections.Generic;

namespace PDMCompute
{
    public enum M1GroupState {
        GRP_ON,
        GRP_REE,
        GRP_STOP,
        GRP_STATE_MAX
    }

    public class CM1Group
    {
        public const int GRP_UNDEFINED = -1;

        long m_lId;             // Group Id
        long m_lTypeId;         // GroupDef Id (Type of group)
        COPSDate m_dtLast = new COPSDate();          // Last operation
        CM1Constraint[] m_Cnstr = new CM1Constraint[CM1Constraint.CNSTR_NUM]; // Constraints (Alert!: CNSTR_BASE = 1)
        ITraceable trace;
        M1GroupState m_lState;          // Internal state of the group: STOP, REE, AMP
        
        //M1ComputeEx0 Attributes
        long m_lAccumMoney;     // Accumulated money due to in group operations
        long m_lAccumMinutes;   // Accumulated minutes due to in group operations
        
        //M1ComputeEx1 Atrributes
        long m_lEfecAccumMoney;     // Accumulated money due to in group operations
        long m_lEfecAccumMinutes;   // Accumulated minutes due to in group operations
        long m_lRealAccumMoney;     // Accumulated money due to in group operations
        long m_lRealAccumMinutes;   // Accumulated minutes due to in group operations

        List<CM1Group> m_lstChildren;		// List of children groups

        public CM1Group()
        {
            Init();
        }
        public CM1Group(long id, long typeId)
        {
            Init();
        }

        public void Init()
        {
            m_lId = GRP_UNDEFINED;
            m_lTypeId = GRP_UNDEFINED;
            m_dtLast.SetStatus(COPSDateStatus.Null);
            trace = null;
            m_lState = M1GroupState.GRP_ON;

            ResetTime();
            ResetMoney();

            for (int i = 0; i < CM1Constraint.CNSTR_NUM; i++)
            {
                m_Cnstr[i] = new CM1Constraint();
                m_Cnstr[i].Set(CM1Constraint.CNSTR_UNDEFINED, CM1Constraint.CNSTR_UNDEFINED);
            }
        }

        public bool Copy(CM1Group pSrcGroup) {
            trace?.Write(TraceLevel.Debug, "CM1Group::Copy");
            bool fnResult = true;

            try
            {
                trace = pSrcGroup.trace;
                m_lId = pSrcGroup.m_lId;
                m_lTypeId = pSrcGroup.m_lTypeId;
                m_dtLast = pSrcGroup.m_dtLast.Copy();
                m_lState = pSrcGroup.m_lState;
                //M1ComputeEx0 Attributes
                m_lAccumMoney = pSrcGroup.m_lAccumMoney;
                m_lAccumMinutes = pSrcGroup.m_lAccumMinutes;
                //M1ComputeEx1 Attributes
                m_lEfecAccumMoney = pSrcGroup.m_lEfecAccumMoney;
                m_lEfecAccumMinutes = pSrcGroup.m_lEfecAccumMinutes;
                m_lRealAccumMoney = pSrcGroup.m_lRealAccumMoney;
                m_lRealAccumMinutes = pSrcGroup.m_lRealAccumMinutes;

                int i = 0;
                for (i = 0; i < CM1Constraint.CNSTR_NUM; i++)
                {
                    m_Cnstr[i].Copy(pSrcGroup.m_Cnstr[i]);
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }

        // Group helpers
        public long GetGrpId() { return m_lId; }
        public long GetGrpTypeId() { return m_lTypeId; }
        public void SetGrpId(long lId) { m_lId = lId; }
        public void SetGrpTypeId(long lTypeId) { m_lTypeId = lTypeId; }
        public void Set(long lId, long lTypeId)
        {
            m_lId = lId;
            m_lTypeId = lTypeId;
        }

        // State helpers
        public long GetState() { return (long)m_lState; }
        public bool SetState(long lState)
        {
            if (lState < 0 || lState >= (long)M1GroupState.GRP_STATE_MAX)
                return false;

            m_lState = (M1GroupState)lState;

            return true;
        }

        // Date helpers
        public COPSDate GetLastDate() { return m_dtLast; }
        public void SetLastDate(COPSDate pdt) { m_dtLast = pdt.Copy(); }
        // Group accumulations

        //M1ComputeEx0 Functions
        public void AddTime(long lMinutes) { m_lAccumMinutes += lMinutes; }
        public bool AddTime(TimeSpan? pdtSpan) {

            trace?.Write(TraceLevel.Debug, "CM1Group::AddTime");
            bool fnResult = true;

            try
            {
                if (pdtSpan == null)
                    throw new ArgumentException("Invalid input parameter", nameof(pdtSpan));

                trace?.Write(TraceLevel.Info, $"Previous minutes: {m_lAccumMinutes}, Adding: {pdtSpan.Value.TotalMinutes}");
                m_lAccumMinutes += (long)pdtSpan.Value.TotalMinutes;

                trace?.Write(TraceLevel.Info, $"Current minutes: {m_lAccumMinutes}");
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;

        }
        public long GetAccMinutes() { return m_lAccumMinutes; }
        public void AddMoney(long lMoney) { m_lAccumMoney += lMoney; }
        public long GetAccMoney() { return m_lAccumMoney; }

        //M1ComputeEx1 Functions
        public void ResetTime()
        {
            m_lAccumMinutes = 0;
            m_lEfecAccumMinutes = 0;
            m_lRealAccumMinutes = 0;
        }
        public void ResetMoney()
        {
            m_lAccumMoney = 0;
            m_lEfecAccumMoney = 0;
            m_lRealAccumMoney = 0;
        }
        public void AddEfecTime(long lMinutes) { m_lEfecAccumMinutes += lMinutes; }
        public bool AddEfecTime(TimeSpan? pdtSpan)
        {
            trace?.Write(TraceLevel.Debug, "CM1Group::AddEfecTime");
            bool fnResult = true;

            try
            {
                if (pdtSpan == null)
                    throw new ArgumentException("Invalid input parameter", nameof(pdtSpan));

                trace?.Write(TraceLevel.Info, $"Previous Efec minutes: {m_lEfecAccumMinutes}, Adding: {pdtSpan.Value.TotalMinutes}");
                m_lEfecAccumMinutes += (long)pdtSpan.Value.TotalMinutes;

                trace?.Write(TraceLevel.Info, $"Current minutes: {m_lEfecAccumMinutes}");
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public long GetEfecAccMinutes() { return m_lEfecAccumMinutes; }
        public void AddEfecMoney(long lMoney) { m_lEfecAccumMoney += lMoney; }
        public long GetEfecAccMoney() { return m_lEfecAccumMoney; }
        public void AddRealTime(long lMinutes) { m_lRealAccumMinutes += lMinutes; }
        public bool AddRealTime(TimeSpan? pdtSpan)
        {
            trace?.Write(TraceLevel.Debug, "CM1Group::AddRealTime");
            bool fnResult = true;

            try
            {
                if (pdtSpan == null)
                    throw new ArgumentException("Invalid input parameter", nameof(pdtSpan));

                trace?.Write(TraceLevel.Info, $"Previous Real minutes: {m_lRealAccumMinutes}, Adding: {pdtSpan.Value.TotalMinutes}");
                m_lRealAccumMinutes += (long)pdtSpan.Value.TotalMinutes;

                trace?.Write(TraceLevel.Info, $"Current minutes: {m_lRealAccumMinutes}");
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public long GetRealAccMinutes() { return m_lRealAccumMinutes; }
        public void AddRealMoney(long lMoney) { m_lRealAccumMoney += lMoney; }
        public long GetRealAccMoney() { return m_lRealAccumMoney; }

        public long GetMaxMoney() {
            trace?.Write(TraceLevel.Debug, "CM1Group::GetMaxMoney");
            
            float constraintValue = 0;

            try
            {
                GetConstraint(CM1Constraint.CNSTR_MAX_IMPORT, ref constraintValue);
                if (constraintValue == CM1Constraint.CNSTR_UNDEFINED) {
                    constraintValue = GlobalDefs.DEF_UNDEFINED_VALUE;
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
            }

            return (long)constraintValue;

        }
        public long GetMaxMinutes() {
            trace?.Write(TraceLevel.Debug, "CM1Group::GetMaxMinutes");
            float constraintValue = 0;

            try
            {
                GetConstraint(CM1Constraint.CNSTR_MAX_ESTANCIA,ref constraintValue);
                if (constraintValue == CM1Constraint.CNSTR_UNDEFINED)
                {
                    constraintValue = GlobalDefs.DEF_UNDEFINED_VALUE;
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
            }

            return (long)constraintValue;

        }
        public long GetMinMoney() {
            trace?.Write(TraceLevel.Debug, "CM1Group::GetMinMoney");
            float constraintValue = 0;

            try
            {
                GetConstraint(CM1Constraint.CNSTR_MIN_IMPORT, ref constraintValue);
                if (constraintValue == CM1Constraint.CNSTR_UNDEFINED)
                {
                    constraintValue = GlobalDefs.DEF_UNDEFINED_VALUE;
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
            }

            return (long)constraintValue;
        }
        public long GetMinValueToChargeInRefund()
        {
            trace?.Write(TraceLevel.Debug, "CM1Group::GetMinValueToChargeInRefund");

            float constraintValue = 0;

            try
            {
                GetConstraint(CM1Constraint.CNSTR_REFUND_MINIMUM_VALUE_TO_CHARGE, ref constraintValue);
                if (constraintValue == CM1Constraint.CNSTR_UNDEFINED)
                {
                    constraintValue = GetMinMoney();
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
            }

            return (long)constraintValue;
        }
        public long GetMaxInterdateReentry() {
            trace?.Write(TraceLevel.Debug, "CM1Group::GetMaxInterdateReentry");
            float constraintValue = 0;

            try
            {
                GetConstraint(CM1Constraint.CNSTR_MAX_INTERDATE_REE, ref constraintValue);
                if (constraintValue == CM1Constraint.CNSTR_UNDEFINED)
                {
                    constraintValue = GlobalDefs.DEF_UNDEFINED_VALUE;
                }
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
            }

            return (long)constraintValue;
        }

        // Constraint helpers
        public bool AddConstraint(long lCnstrTypeId, float fCnstrValue)
        {
            trace?.Write(TraceLevel.Info, "CM1Group:AddConstraint");
            bool fnResult = true;

            try
            {
                // Constraints (Alert!: CNSTR_BASE = 1)
                if (!CM1Constraint.IsValidTypeId(lCnstrTypeId))
                    throw new ArgumentOutOfRangeException(nameof(lCnstrTypeId), "lCnstrTypeId out of range");

                m_Cnstr[lCnstrTypeId - 1].Set(lCnstrTypeId, fCnstrValue);
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public bool GetConstraint(long lCnstrTypeId, ref float constraintValue)
        {
            trace?.Write(TraceLevel.Info, "CM1Group:GetConstraint");
            bool fnResult = true;

            try
            {
                // Constraints (Alert!: CNSTR_BASE = 1)
                if (!CM1Constraint.IsValidTypeId(lCnstrTypeId))
                    throw new ArgumentOutOfRangeException(nameof(lCnstrTypeId), "lCnstrTypeId out of range");

                constraintValue = m_Cnstr[lCnstrTypeId - 1].Value;
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public bool MergeConstraint(long lCnstrTypeId, float fCnstrValue,ref float? pfCnstrValueOut) {

            trace?.Write(TraceLevel.Debug, "CM1Group::MergeConstraint");
            bool fnResult = true;

            try
            {
                if (!CM1Constraint.IsValidTypeId(lCnstrTypeId))
                    throw new ArgumentOutOfRangeException(nameof(lCnstrTypeId), "Invalid input parameter");

                fnResult = m_Cnstr[lCnstrTypeId - 1].Merge(fCnstrValue, ref pfCnstrValueOut);
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public bool MergeConstraint(CM1Constraint pCnstr, float? pfCnstrValueOut)
        {
            trace?.Write(TraceLevel.Debug, "CM1Group::MergeConstraint");
            bool fnResult = true;

            try
            {
                if (pCnstr == null)
                    throw new ArgumentNullException(nameof(pCnstr), "Invalid input parameter");

                fnResult = m_Cnstr[pCnstr.TypeId - 1].Merge(pCnstr, ref pfCnstrValueOut);
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public CM1Constraint[] GetConstraints() { return m_Cnstr; }

        // Trace
        public void SetTrace(ITraceable pTrace)
        {
            trace = pTrace;
        }
        public bool TraceM1ComputeEx0() {
            trace?.Write(TraceLevel.Debug, "CM1Group::TraceM1ComputeEx0");
            bool fnResult = true;

            try
            {
                trace?.Write(TraceLevel.Info, $"---------- Group % {GetGrpId(), 6}----------");
                trace?.Write(TraceLevel.Info, $"---------- Constraints ----------");
                foreach (CM1Constraint constraint in m_Cnstr)
                {
                    trace?.Write(TraceLevel.Info, $"\t\t{constraint.TypeId}: {constraint.Value}");
                }
                trace?.Write(TraceLevel.Info, $"\t ACC. MINUTES: {this.GetAccMinutes(), 10}");
                trace?.Write(TraceLevel.Info, $"\t ACC. MONEY  : {this.GetAccMoney(), 10}");
                switch (this.GetState())
                {
                    case (long)M1GroupState.GRP_ON:
                        trace?.Write(TraceLevel.Info, $"\t STATE       : ON");
                        break;
                    case (long)M1GroupState.GRP_REE:
                        trace?.Write(TraceLevel.Info, $"\t STATE       : REE");
                        break;
                    case (long)M1GroupState.GRP_STOP:
                        trace?.Write(TraceLevel.Info, $"\t STATE       : STOP");
                        break;
                }

                trace?.Write(TraceLevel.Info, "----------------------------------");
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
        public bool TraceM1ComputeEx1()
        {
            trace?.Write(TraceLevel.Debug, "CM1Group::TraceM1ComputeEx1");
            bool fnResult = true;

            try
            {
                trace?.Write(TraceLevel.Info, $"---------- Group % {GetGrpId(),6}----------");
                trace?.Write(TraceLevel.Info, $"---------- Constraints ----------");
                foreach (CM1Constraint constraint in m_Cnstr)
                {
                    trace?.Write(TraceLevel.Info, $"\t\t{constraint.TypeId}: {constraint.Value}");
                }
                trace?.Write(TraceLevel.Info, $"\t EFEC ACC. MINUTES: {this.GetEfecAccMinutes(),10}");
                trace?.Write(TraceLevel.Info, $"\t EFEC ACC. MONEY  : {this.GetEfecAccMoney(),10}");
                trace?.Write(TraceLevel.Info, $"\t REAL ACC. MINUTES: {this.GetRealAccMinutes(),10}");
                trace?.Write(TraceLevel.Info, $"\t REAL ACC. MONEY  : {this.GetRealAccMoney(),10}");
                
                switch (this.GetState())
                {
                    case (long)M1GroupState.GRP_ON:
                        trace?.Write(TraceLevel.Info, $"\t STATE       : ON");
                        break;
                    case (long)M1GroupState.GRP_REE:
                        trace?.Write(TraceLevel.Info, $"\t STATE       : REE");
                        break;
                    case (long)M1GroupState.GRP_STOP:
                        trace?.Write(TraceLevel.Info, $"\t STATE       : STOP");
                        break;
                }

                trace?.Write(TraceLevel.Info, "----------------------------------");
            }
            catch (Exception error)
            {
                trace?.Write(TraceLevel.Error, error.ToLogString());
                fnResult = false;
            }

            return fnResult;
        }
    }
}