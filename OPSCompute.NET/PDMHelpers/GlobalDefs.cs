using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDMHelpers
{
    public static class GlobalDefs
    {

        ///******** STRINGS LENGTHS ************//
        ///*************************************//
        public const int DEFAULT_ARTICLE_TYPE = 4;

        public const int INI_MSG_IN_DEV_BUS_QUEUE = 5;
        public const int DEF_MAXIMUM_COUNT = 50;
        public const int MAX_MSG_QUEUE = 50;
        public const int DEF_MAX_MSG = 99999;
        public const int MAX_TEL_ID = 5000;
        public const int MAX_TICKET_COUNT = 9999999;
        public const int DEF_UNDEFINED_VALUE = -999;

        public const int DEF_STRING_LEN = 512;
        public const int DEF_BUF_MAXLEN = 512;

        public const int DEF_DATE_LEN = 12;
        public const int DEF_SQLDATE_LEN = 24;

        public const int DEF_TELMAX_LEN = 1536;

        public const int PLATENUMBER_LEN = 20;
        public const int CURRENTDATE_LEN = 12;
        public const int EXPEDIENTE_LEN = 12;
        public const int TIPOSANCION_LEN = 10;
        public const int C_MAX_MATRICULA = 20;
        public const int ZONE_SECTOR_LEN = 40;  ///MWI!!

        public const int DDAY_CODE_LENGTH = 7;
        public const int CC_UNIT_ID = 4;

        //PDMStates
        public const int DEF_PDMOK = 0;             //PDM RUNNING NORMALLY
        public const int DEF_PDMNOSALES = 1;        //PDM RUNNING BUT NO TICKETS
        public const int DEF_PDMOUTOFSERVICE = 2;   //PDM OUT OF SERVICE
        public const int DEF_PDMSUSPENDED = 4;      //PDM SLEEPING
        public const int DEF_PDMINIT = 5;           //PDM BOOTING
        public const int DEF_PDMMAINTCE = 6;        //PDM MAINTCE
        public const int DEF_PDMTAKING = 7;         //PDM COLLECTING

        public const int DEF_MODEMCONNECTED = 5;    //MODEM MUST BE CONNECTED
        public const int DEF_MODEMDISCONNECTED = 6;	//MODEM MUST BE DISCONNECTED

    }
}
