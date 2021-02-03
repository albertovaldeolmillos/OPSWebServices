using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpOperationsCCDB : CmpGenericBase
	{
		public CmpOperationsCCDB() 
		{
			_standardFields		= new string[] 
				{
						"OPE_ID", "OPE_DOPE_ID", "OPE_ART_ID", "OPE_GRP_ID", "OPE_UNI_ID", 
					"OPE_DPAY_ID", "OPE_INIDATE", "OPE_ENDDATE", "OPE_DURATION", "OPE_VALUE", "OPE_VEHICLEID", "OPE_MOBI_USER_ID", "OPE_DOPE_ID_VIS", "CC_NUMBER", "OPE_TICKETNUM"};
			_standardPks		= new string[0];
			_standardTableName	= "VW_OPERATIONS_CC";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}
