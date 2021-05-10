using PDMHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMCompute
{
    public enum TariffCalculatorsTag
    {
        OTS,
        OTS_0,
        OTS_1,
        MAX_TARIFFS
    }

    public static class TariffCalculatorFactory
    {
        public static TariffCalculator CreateTariffCalculator(ILoggerManager loggerManager, int type) {
            TariffCalculator instance = null;

            switch ((TariffCalculatorsTag)type) {
                case TariffCalculatorsTag.OTS:
                    instance = new M1ComputeEx0(loggerManager, type);
                    break;
                case TariffCalculatorsTag.OTS_0:
                    instance = new M1ComputeEx0(loggerManager, type);
                    break;
                case TariffCalculatorsTag.OTS_1:
                    instance = new M1ComputeEx1(loggerManager, type);
                    break;
            }

            return instance;
        }

        public static TariffCalculator CreateTariffCalculator(ILoggerManager loggerManager, string type)
        {
            if (String.IsNullOrEmpty(type))
                return null;

            int index = Array.IndexOf(Enum.GetNames(typeof(TariffCalculatorsTag)), type);
            return CreateTariffCalculator(loggerManager,index);
        }
    }
}
