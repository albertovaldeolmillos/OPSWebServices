using System;
using System.Data;

using OPS.Comm;
using System.Configuration;



namespace OPS.Components.Data
{
	/// <summary>
	/// 
	/// </summary>
	public class CmpCollectingsDB : CmpGenericBase
	{
		public CmpCollectingsDB()
		{
			 
			_standardFields		= new string[] {"COL_ID",
												   "COL_UNI_ID",
												   "COL_NUM",
												   "COL_DATE",
												   "COL_INIDATE",
												   "COL_ENDDATE",
												   "COL_COIN_SYMBOL",
												   "COL_BACK_COL_TOTAL",
												   "COL_CASH_PARK",
												   "COL_CASH_FINE",
												   "COL_CASH_RECHARGE",
												   "COL_CASH_TOTAL",
												   "COL_CASH_PARK_OPS",
												   "COL_CASH_FINE_OPS",
												   "COL_CASH_RECHARGE_OPS",
												   "COL_CASH_TOTAL_OPS",
												   "COL_CASH_COINS_V1",
												   "COL_CASH_COINS_Q1",
												   "COL_CASH_COINS_V2",
												   "COL_CASH_COINS_Q2",
												   "COL_CASH_COINS_V3",
												   "COL_CASH_COINS_Q3",
												   "COL_CASH_COINS_V4",
												   "COL_CASH_COINS_Q4",
												   "COL_CASH_COINS_V5",
												   "COL_CASH_COINS_Q5",
												   "COL_CASH_COINS_V6",
												   "COL_CASH_COINS_Q6",
												   "COL_CASH_COINS_V7",
												   "COL_CASH_COINS_Q7",
												   "COL_CASH_COINS_V8",
												   "COL_CASH_COINS_Q8",
												   "COL_CASH_COINS_V9",
												   "COL_CASH_COINS_Q9",
												   "COL_CASH_COINS_V10",
												   "COL_CASH_COINS_Q10",
												   "COL_CASH_COINS_V11",
												   "COL_CASH_COINS_Q11",
												   "COL_CASH_COINS_V12",
												   "COL_CASH_COINS_Q12",
												   "COL_CASH_COINS_V13",
												   "COL_CASH_COINS_Q13",
												   "COL_CASH_COINS_V14",
												   "COL_CASH_COINS_Q14",
												   "COL_CASH_COINS_V15",
												   "COL_CASH_COINS_Q15",
												   "COL_CRCARD_PARK",
												   "COL_CRCARD_FINE",
												   "COL_CRCARD_RECHARGE",
												   "COL_CRCARD_TOTAL",
												   "COL_CRCARD_PARK_OPS",
												   "COL_CRCARD_FINE_OPS",
												   "COL_CRCARD_RECHARGE_OPS",
												   "COL_CRCARD_TOTAL_OPS",
												   "COL_CHCARD_PARK",
												   "COL_CHCARD_FINE",
												   "COL_CHCARD_RETURN",
												   "COL_CHCARD_TOTAL",
												   "COL_CHCARD_PARK_OPS",
												   "COL_CHCARD_FINE_OPS",
												   "COL_CHCARD_RETURN_OPS",
												   "COL_CHCARD_TOTAL_OPS",
												   "COL_CMI_050",
												   "COL_CMI_050_075",
												   "COL_CMI_075_100",
												   "COL_CMI_100_075",
												   "COL_CMI_150_075",
												   "COL_CMI_200_075",
												   "COL_CMI_250_075",
												   "COL_CMI_300_075",
												   "COL_CMI_400_075",
												   "COL_CMI_500",
												   "COL_CTI_1000",
												   "COL_CTI_1000_1200",
												   "COL_CTI_1200_1400",
												   "COL_CTI_1400_1600",
												   "COL_CTI_1600_1800",
												   "COL_CTI_1800_2000",
												   "COL_CTI_2000_2200",
												   "COL_CTI_2200_0000"};
											   
											   
			_standardPks		= new string[] {"COL_ID"};
			_standardTableName	= "V_COLLECTINGS";
			_standardOrderByField	= "COL_ID";
			_standardOrderByAsc		= "DESC";
	
			_standardRelationFileds	= new string[0]; //{"UNI_STR_ID","UNI_DPUNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpStreetsDB,ComponentsBD","OPS.Components.Data.CmpUnitsPhyDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0]; // {"UNI_VALID", "UNI_DELETED"};
		}
	}
}

