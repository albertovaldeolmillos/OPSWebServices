using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMDatabase
{
    public class CTariffM1
    {
        public const int TARIFFM1_NEXTBLOCK_NOT_ALLOWED = 0;
        public const int TARIFFM1_NEXTBLOCK_ALLOWED = 1;
        public const int TARIFFM1_NEXTBLOCK_LESS_ALLOWED = 2;
        public const int TARIFFM1_NEXTBLOCK_LESS_OR_EQUAL_ALLOWED = 3;
        public const int TARIFFM1_NEXTBLOCK_GREATER_ALLOWED = 4;
        public const int TARIFFM1_NEXTBLOCK_GREATER_OR_EQUAL_ALLOWED = 5;

        public const int TARIFFM1_NEXTBLOCK_ALLOWED_IF_NO_FREE_TIME = 11;
        public const int TARIFFM1_NEXTBLOCK_NO_FREE_TIME_AND_LESS_ALLOWED = 12;
        public const int TARIFFM1_NEXTBLOCK_NO_FREE_TIME_AND_LESS_OR_EQUAL_ALLOWED = 13;
        public const int TARIFFM1_NEXTBLOCK_NO_FREE_TIME_AND_GREATER_ALLOWED = 14;
        public const int TARIFFM1_NEXTBLOCK_NO_FREE_TIME_AND_GREATER_OR_EQUAL_ALLOWED = 15;

        public bool m_bNextDay;
        public int m_iNextBlock;
        public int m_iNextBlockConditionalValue;
        public int m_iMaxTimeForNotApplyReentry;
        public bool m_bResetNextBlockTime;
        public bool m_bResetNextBlockInt;
        public bool m_bResetNextDayInterval;
        public bool m_bResetNextDayTime;
        public bool m_bRoundEndOfDay;
        public int m_nTarID;
        public int m_nNumber;

        public CTariffM1()
        {
        }

    }
}
