//using System;
//using System.Data;
//
//using System.Text;
//
//namespace OPS.Components.Data
//{
//	/// <summary>
//	/// Summary description for CmpGroupsLevelDB.
//	/// </summary>
//	public class CmpGroupsLevelDB: CmpGenericBase
//	{
//		public CmpGroupsLevelDB() 
//		{
//			_standardFields		= new string[] {"OGRP_ID","OGRP_GRP_ID","OGRP_LEVEL","OGRP_INIDATE","OGRP_ENDDATE"};
//			_standardPks		= new string[] {"OGRP_ID","OGRP_GRP_ID"};
//			_standardTableName	= "GROUPS_OCCUPATION";
//			_standardOrderByField	= "";		
//			_standardOrderByAsc		= "";		
//		
//			// Field of main table whois foreign Key of table
//			_standardRelationFileds	= new string[] {"OGRP_GRP_ID"};
//			_standardRelationTables		= new string[] {"GROUPS"};
//			_stValidDeleted			= new string[0];
//		}
//	}
//}
