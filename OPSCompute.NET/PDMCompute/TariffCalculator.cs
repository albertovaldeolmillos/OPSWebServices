using PDMDatabase;
using PDMHelpers;
using System;
using System.Collections;

namespace PDMCompute
{
    public class TariffCalculator
    {
        public const int M1_RESIDENT = 5;    // Residente
        public const int M1_SIN_LIMITE = 4; // Sin limitaciones de importe o tiempo
        public const int M1_VIP = 3;    // Persona VIP
        public const int M1_DATE_AMP = 2;   // Fecha anterior (pOUT_FechaOperacion por ampliación de operación )
        public const int M1_OP_OK = 1;// Operación válida

        public const int M1_OP_NOK = -1;    // Operación no válida de forma genérica
        public const int M1_OP_ARTNOK = -2;// Tipo de operación no válida por los datos que se tienen del artículo
        public const int M1_BLACKVEH = -3;// Vehículo en lista negra
        public const int M1_BLACKUSU = -4;  // Usuario en lista negra
        public const int M1_SYS_OFF = -5;// Sistema fuera de servicio
        public const int M1_SYS_NOSALES = -6;// Sistema fuera de ventas
        public const int M1_OP_TPERM = -7;// Se ha superado el tiempo de permanencia
        public const int M1_OP_TREENT = -8;// No se ha superado el tiempo mínimo de reentrada
        public const int M1_ART_KO = -9;// No se ha encontrado el artículo
        public const int M1_ART_OLD = -10;// Artículo no vigente
        public const int M1_AMP_NO_OK = -11; // Ampliación no es válida
        public const int M1_NO_TARIFF = -12; // Tarifa no encontrada
        public const int M1_NO_RETURN = -13; // No tiene derecho a devolución
        public const int M1_CARD_ALREADY_USED = -14; // User Card Already used in a current parking
        public const int M1_NO_ROOM_FOR_PARK = -15; // In a controlled space of parking there is no room for another car
        public const int M1_PLATE_NOT_FOUND = -16; // Plate not found in table of scanned plates
        public const int M1_ARTICLE_DEF_RESTRICTED = -17; // Article type restricted to white list
        public const int M1_ARTICLE_DEF_NOT_APPLYABLE = -18; // SHOW_ARTICLES_RULES doesn't allow the use of the article def

        public const int M1_RESULT_MAX = M1_RESIDENT;
        public const int M1_RESULT_MIN = M1_ARTICLE_DEF_NOT_APPLYABLE;

        public const int STEP_CALCULATION_DEFAULT_VALUE = 5;

        public const int MAX_TARIFF_PERIODS = 2;
        public const int DAYS_IN_WEEK = 7;

        public const int SHOW_ARTICLES_RULES_LIST_BEHAVIOR_DEFAULT = 0; //RESIDENTS AND VIPS CAN CHANGE CURRENT ARTICLE DEF (THIS IS THE DEFAULT)
        public const int SHOW_ARTICLES_RULES_LIST_BEHAVIOR_WL_RESIDENTS = 1;//ARTICLE DEF IS RESTRICTED TO PLATES WITH THE SAME ARTICLE DEF IN TABLE RESIDENTS
        public const int SHOW_ARTICLES_RULES_LIST_BEHAVIOR_WL_VIPS = 2;//ARTICLE DEF IS RESTRICTED TO PLATES WITH THE SAME ARTICLE DEF IN TABLE VIPS
        public const int SHOW_ARTICLES_RULES_LIST_BEHAVIOR_NO_CHECK = 3; //ARTICLED DEF DOESN'T ALLOW TO CHECK RESIDENTS OR VIPS TABLE TO CHANGE CURRENT ARTICLE DEF
        public const int SHOW_ARTICLES_RULES_LIST_BEHAVIOR_NO_CHECK_VIPS = 4; //RESIDENTS TABLE CAN CHANGE CURRENT ARTICLE_DEF BUT WE DON'T WANT TO CHECK VIPS TABLE
        public const int SHOW_ARTICLES_RULES_LIST_BEHAVIOR_NO_CHECK_RESIDENTS = 5; //VIPS TABLE CAN CHANGE CURRENT ARTICLE_DEF BUT WE DON'T WANT TO CHECK RESIDENTS TABLE

        protected OPSPDMDatabase m_pDBB;
        protected ITraceable trace;
        protected int m_iSelectedSubtariff;
        protected bool m_bPlateIDRequired;
        protected long m_lMinCoinValue;

        public int Type { get; set; }

        public TariffCalculator(int type)
        {
            this.Type = type;
        }
        public void SetTrace(ITraceable pTrace)
        {
            trace = pTrace;
        }

        public virtual void SetTracerEnabled(bool enabled)
        {
            if (trace != null)
            {
                trace.Enabled = enabled;
            }
        }

        public virtual bool GetM1(CDatM1 pM1, bool bM1Plus, int nMaxMoney = 0, bool bApplyHistory = true)
        {
            throw new NotImplementedException("Code in .cpp");
        }
        public virtual bool CalculateTariff(CDatM1 pDatM1, IEnumerable plstTariffs, IEnumerable plstFestives)
        {
            throw new NotImplementedException("Code in .cpp");
        }
        public virtual bool CalculateParking(CDatM1 pDatM1, IEnumerable plstTariffs, IEnumerable plstFestives)
        {
            throw new NotImplementedException("Code in .cpp");
        }
        public virtual bool IterateTariffs(CDatM1 pDatM1, IEnumerable plstTariffs, IEnumerable plstFestives, long lMaxChangeDays, bool bDayChangeInInterval, bool bM1Plus)
        {
            throw new NotImplementedException("Code in .cpp");
        }
        public virtual bool IterateDayTariff(CDatM1 pDatM1, stHOURLYBLOCK pstDayTariff, COPSDate pdtWork, long plWorkMoney, long plDaysChanges, bool bChangeDayInOutOfTariff, bool bDayChangeInInterval, bool bM1Plus, bool pbDayChange)
        {
            throw new NotImplementedException("");
        }
        public virtual bool IterateInterval(CDatM1 pDatM1, long plWorkMinutes, stHOURLYBLOCK pstDayTariff, long plWorkMoney, bool bDayChangeInInterval, bool bM1Plus, bool pbDayChange)
        {
            throw new NotImplementedException("");
        }

        public bool IsFestive(COPSDate pdtWork, IEnumerable plstFestives, bool pbFestive) {
            throw new NotImplementedException("");
        }
        public bool Interpolate(int iMode, long x0, long fx0, long x1, long fx1, long x, double pfx) {
            throw new NotImplementedException("");
        }

        public virtual bool SelectSubtariff(int iSubtariff)
        {
            m_iSelectedSubtariff = iSubtariff;
            return true;
        } 
        public virtual bool SelectSubtariff(string szSubtariff) { return false; }
        public virtual bool GetApplyVehicleHistory() { return false; }
        public virtual long GetRealCurrMinutes() { return 0; }

        public virtual IEnumerable GetTariff() { return null; }
        public virtual IEnumerable GetFestives() { return null; }
        public virtual bool FillTree() { return false; }
        public virtual CM1GroupsTree GetTree() { return null; }

        public virtual bool SetDBB(OPSPDMDatabase pDBB) {
            m_pDBB = pDBB;
            return true;
        }

        // TODO: ESTAS FUNCIONES SE TENDRIAN QUE PASAR A OPSPDMDatabase
        // DB ACCESS
        public bool GetGroupDesc(long lGroup, string szGroup, int iBufferSize) {
            throw new NotImplementedException("");
        }
        public bool GetGroupFromUnit(long lUnit, long plGroup, long plTypeOfGroup) {
            throw new NotImplementedException("");
        }
        public bool GetGroupsFromGroup(int idGroup, CGroupSet lstGroup) {
            throw new NotImplementedException("");
        }
        public bool GetGroupsFromGroupEx(int idGroup, CGroupSet lstGroup)
        {
            throw new NotImplementedException("");
        }
	    public bool GetGroupDescFromUnit(long lUnit, string szGroup, int iBufferSize)
        {
            throw new NotImplementedException("");
        }
        public void SetPlateIsRequired(bool bRequired) {
            m_bPlateIDRequired = bRequired;
        }
        public bool GetNumberTariff(int iTariff, COPSDate pdtInDate, long plOutTimTabIni, long plOutTimTabEnd,long plStarID, bool pbFind, CTariffM1 pobjTariff)
        {
            throw new NotImplementedException("");
        }
        public bool DeleteOperations(COPSDate pdt, int iMinutes)
        {
            throw new NotImplementedException("");
        }
        public virtual bool GetArticleDesc(long lArticleDef, long plArticle)
        {
            throw new NotImplementedException("");
        }
        public virtual bool GetArticlesDef(long pplArticlesDef, long plArticlesDefCount) { return false; }
        // TODO: ESTAS FUNCIONES SE TENDRIAN QUE PASAR A OPSPDMDatabase
        public void SetMinCoinValue(long lMinCoinValue)
        {
            m_lMinCoinValue = lMinCoinValue;
        }
        public long GetMinCoinValue()
        {
            return m_lMinCoinValue;
        }
    }


    public class CGroupSet
    {
    }
    public class stHOURLYBLOCK
    {
    }
}