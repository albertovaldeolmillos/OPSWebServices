using System;
using System.Data;

using OPS.Comm;



namespace OPS.Components.Data
{
	/// <summary>
	/// 
	/// </summary>
	public class CmpCollectingsCrdDB : CmpGenericBase
	{
		public const int C_RES_OK = 1;
		public CmpCollectingsCrdDB()
		{
			 
			_standardFields		= new string[] {"COR_DATE", "COR_TYPE", "COR_OPNUM", 
												"COR_TOTAL", "COR_TYPE_DATA",
												"COR_DATE_RCV","COR_UNI_ID"};
			_standardPks		= new string[] {"COR_DATE","COR_TYPE"};
			_standardTableName	= "COLLECTINGS_CARD";
			_standardOrderByField	= "COR_DATE";
			_standardOrderByAsc		= "DESC";
	
			_standardRelationFileds	= new string[0]; //{"UNI_STR_ID","UNI_DPUNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpStreetsDB,ComponentsBD","OPS.Components.Data.CmpUnitsPhyDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0]; // {"UNI_VALID", "UNI_DELETED"};
		}

	}
}
