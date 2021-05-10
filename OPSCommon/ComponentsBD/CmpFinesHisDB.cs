using System;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpFinesHisDB.
	/// </summary>
	public class CmpFinesHisDB: CmpGenericBase
	{
		public CmpFinesHisDB()
		{
			_standardFields		= new string[] {
				"HFIN_ID","HFIN_DFIN_ID","HFIN_NUMBER","HFIN_VEHICLEID","HFIN_MODEL",
				"HFIN_MANUFACTURER","HFIN_COLOUR","HFIN_GRP_ID","HFIN_STR_ID","HFIN_STRNUMBER",
				"HFIN_DATE","HFIN_COMMENTS","HFIN_USR_ID","HFIN_UNI_ID","HFIN_DPAY_ID","HFIN_COD_ID","HFIN_POLICENUMBER","HFIN_CONFIRM_DATE"};
			_standardPks		= new string[] {"HFIN_ID"};
			_standardTableName	= "FINES_HIS";
			_standardOrderByField	= "";		
			_standardOrderByAsc		= "";		
		
			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[] {
				"HFIN_DFIN_ID","HFIN_DPAY_ID",
				"HFIN_GRP_ID","HFIN_STR_ID",
				"HFIN_UNI_ID","HFIN_USR_ID"};
			_standardRelationTables	= new string[] {
				"OPS.Components.Data.CmpFinesDefDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD",
				"OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpStreetsDB,ComponentsBD",
				"OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpUsuarioDB,ComponentsBD"};
			_stValidDeleted	= new string[0];
		}
	}
}
