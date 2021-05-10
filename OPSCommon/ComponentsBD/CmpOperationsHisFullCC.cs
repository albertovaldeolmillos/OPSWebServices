using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpOperationsHisFullCCDB : CmpGenericBase
	{
		public CmpOperationsHisFullCCDB() 
		{
			_standardFields		= new string[] 
				{"HOPE_ID","HOPE_DOPE_ID","HOPE_HOPE_ID","HOPE_ART_ID","HOPE_GRP_ID","HOPE_UNI_ID","HOPE_DPAY_ID",
				 "HOPE_MOVDATE","HOPE_INIDATE","HOPE_ENDDATE","HOPE_DURATION","HOPE_VALUE","HOPE_VEHICLEID","HOPE_DART_ID","HOPE_MOBI_USER_ID","HOPE_POST_PAY","HOPE_CHIPCARD_ID","HOPE_CHIPCARD_CREDIT","HOPE_FIN_ID","HOPE_FIN_DFIN_ID","HOPE_REALDURATION","HOPE_BASE_HOPE_ID","HOPE_DOPE_ID_VIS","HCC_ID","HCC_INS_DATE","HCC_OPE_ID","HCC_NUMBER","HCC_NAME","HCC_EXPRTN_DATE","HCC_STATE","HOPE_TICKETNUM"};
			_standardPks		= new string[0];
			_standardTableName	= "VW_OPERATIONS_HIS_FULL_CC";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}
