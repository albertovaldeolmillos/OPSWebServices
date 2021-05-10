namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for STREETS.
	/// </summary>
	public class CmpStreetsStretchsDB : CmpGenericBase
	{
		public CmpStreetsStretchsDB() 
		{
			_standardFields				= new string[] {"SS_ID",              
														"SS_GRP_ID",       
														"SS_STR_ID",         
														"SS_STR_SS_ID",       
														"SS_EVEN",            
														"SS_PARK_SPACES",     
														"SS_NUM_WORKS",       
														"SS_REMARKS",         
														"SS_VERSION",         
														"SS_VALID",           
														"SS_DELETED",  
													    "SS_STR_ID_DESDE",
														"SS_STR_ID_HASTA",
														"SS_P_EN_LINEA_PAR",      
														"SS_P_EN_LINEA_IMPAR",      
														"SS_P_EN_BATERIA_PAR",    
														"SS_P_EN_BATERIA_IMPAR",    
														"SS_P_C_D_DIA_ENTERO",
														"SS_P_C_D_MEDIODIA",
														"SS_P_PMR",           
														"SS_P_VADO_DIA_ENTERO",          
														"SS_P_VADO_MEDIODIA",          
														"SS_P_BASURA",        
														"SS_P_MOTOS",         
														"SS_EXT_ID"};
			_standardPks				= new string[] { "SS_ID" };
			_standardTableName			= "STREETS_STRETCHS";
			_standardOrderByField		= "SS_EXT_ID";
			_standardOrderByAsc			= "ASC";
		
			_standardRelationFileds		= new string[0];
			_standardRelationTables		= new string[0];
			_stValidDeleted				= new string[] {"SS_VALID", "SS_DELETED"};
		}
	}
}