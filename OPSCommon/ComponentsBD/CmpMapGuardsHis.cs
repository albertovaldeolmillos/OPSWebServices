using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpMapGuardsHisDB : CmpGenericBase
	{
		public CmpMapGuardsHisDB() 
		{
			_standardFields		= new string[] 
				{										                    
					"GUAH_ID",         
					"GUAH_INS_DATE",   
					"GUAH_USR_ID",     
					"GUAH_NAME",       
					"GUAH_SURNAME1",   
					"GUAH_SURNAME2",   
					"GUAH_PHOTO",      
					"GUAH_UNI_ID",     
					"GUAH_IPDIR",      
					"GUAH_IPDATE",     
					"GUAH_POSLAT",     
					"GUAH_POSLON",     
					"GUAH_POSDATE",    
					"GUAH_DELETED",  
					"GUAH_STATUS"   
				};
			_standardPks		= new string[] {"GUAH_ID"};
			_standardTableName	= "MAP_GUARDS_HIS";
			_standardOrderByField	= "GUAH_INS_DATE";
			_standardOrderByAsc		= "DESC";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}
