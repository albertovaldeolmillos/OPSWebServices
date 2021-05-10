//using System;
//using System.Data;
//
//using System.Text;
//
//namespace OPS.Components.Data
//{
//	/// <summary>
//	/// Summary description for CmpOperationsViewDB.
//	/// </summary>
//	/// 
//	public class CmpOperationsViewDB: CmpGenericBase
//	{
//		public CmpOperationsViewDB() 
//		{
//
//
//
//			_standardFields		= new string[] {"VOPE_ID","VOPE_DOPE_DESCSHORT","VOPE_DART_DESCLONG","VOPE_UNI_DESCSHORT","VOPE_GRP_DESCSHORT","VOPE_VEHICLEID","VOPE_INIDATE","VOPE_ENDDATE","VOPE_VALUE","VOPE_DPAY_DESCSHORT"};
//			_standardPks		= new string[] {"VOPE_ID"};
//			_standardTableName	= "OPERATIONS_VIEW";
//			_standardOrderByField	= "VOPE_ENDDATE";		
//			_standardOrderByAsc		= "DESC";			
//		
//			// Field of main table whois foreign Key of table
//			_standardRelationFileds		= new string[0];
//			_standardRelationTables		= new string[0];
//			_stValidDeleted			= new string[0];
//
//
//
//			/*
//			
//			
//			_standardFields		= new string[] {"OPE_ID", "OPE_DOPE_ID","OPE_ART_ID", "OPE_GRP_ID",	"OPE_UNI_ID", "OPE_DPAY_ID", "OPE_INIDATE" , "OPE_ENDDATE" , "OPE_DURATION" , "OPE_VALUE" , "OPE_VEHICLEID"};
//			_standardPks		= new string[] {"OPE_ID"};
//			_standardTableName	= "OPERATIONS";
//			_standardOrderByField	= "OPE_ENDDATE";		
//			_standardOrderByAsc		= "DESC";	
//		
//*/
///*
// * 
// *   FROM operations, articles_def, operations_def, paytypes_def, GROUPS, units
// WHERE operations.ope_dope_id = operations_def.dope_id
//   AND operations.ope_dpay_id = paytypes_def.dpay_id
//   AND operations.ope_grp_id = GROUPS.grp_id
//   AND operations.ope_uni_id = units.uni_id
//   AND operations.ope_art_id = articles_def.dart_id;
// * */
///*
//
//			// Field of main table whois foreign Key of table
//			_standardRelationFileds		= new string[] {"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
//			_standardRelationTables		= new string[] {"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
//			_stValidDeleted				= new string[] {"OPE_VALID","OPE_DELETED"};
//
//			*/
//		}
//	}
//}
