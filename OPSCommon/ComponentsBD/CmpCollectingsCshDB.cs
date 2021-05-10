using System;
using System.Data;

using OPS.Comm;



namespace OPS.Components.Data
{
	/// <summary>
	/// 
	/// </summary>
	public class CmpCollectingsCshDB : CmpGenericBase
	{
		public const int C_RES_OK = 1;
		public CmpCollectingsCshDB()
		{
			 
			_standardFields		= new string[] {"COS_DATE", "COS_TYPE", "COS_005", 
											"COS_010", "COS_020","COS_050",
											"COS_100","COS_200","COS_TYPE_DATA",
											"COS_DATE_RCV","COS_UNI_ID"};

			_standardPks		= new string[] {"COS_DATE","COS_TYPE"};
			_standardTableName	= "COLLECTINGS_CASH";
			_standardOrderByField	= "COS_DATE";
			_standardOrderByAsc		= "DESC";
	
			_standardRelationFileds	= new string[0]; //{"UNI_STR_ID","UNI_DPUNI_ID"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpStreetsDB,ComponentsBD","OPS.Components.Data.CmpUnitsPhyDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0]; // {"UNI_VALID", "UNI_DELETED"};
		}
	
	}
}
