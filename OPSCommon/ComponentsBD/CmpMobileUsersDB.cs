using System;
using System.Text;
using System.Data;


namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for OPERATIONS.
	/// </summary>
	public class CmpMobileUsersDB : CmpGenericBase
	{
		public CmpMobileUsersDB() 
		{
			_standardFields		= new string[] 
				{
					"MU_ID",                     
					"MU_NAME",                                        
					"MU_SURNAME1",                                    
					"MU_SURNAME2",                                    
					"MU_DNI",                                         
					"MU_EMAIL",                                       
					"MU_ADDR_STREET",                                 
					"MU_ADDR_NUMBER",                                 
					"MU_ADDR_LEVEL",                                  
					"MU_ADDR_STAIR",                                  
					"MU_ADDR_LETTER",                                 
					"MU_ADDR_POSTAL_CODE",                            
					"MU_ADDR_CITY",                                   
					"MU_ADDR_PROVINCE",                               
					"MU_ADDR_COUNTRY",                                
					"MU_NUM_CREDIT_CARD",                             
					"MU_NAME_CARD",                                   
					"MU_NUM_CC_EXPIRATION_DATE",                      
					"MU_MOBILE_TELEPHONE",                            
					"MU_MOBILE_COMPANY",                              
					"MU_PAY_PROFILE",                                 
					"MU_PENDING_PAYMENTS",                            
					"MU_LOGIN",                                       
					"MU_PASSWORD"                                    
				};
			_standardPks		= new string[] {"MU_ID"};
			_standardTableName	= "MOBILE_USERS";
			_standardOrderByField	= "MU_ID";
			_standardOrderByAsc		= "ASC";
		

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"OPE_DOPE_ID","OPE_ART_ID","OPE_GRP_ID","OPE_UNI_ID","OPE_DPAY_ID"} ;
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpOperationsDefDB,ComponentsBD","OPS.Components.Data.CmpArticlesDB,ComponentsBD","OPS.Components.Data.CmpGroupsDB,ComponentsBD","OPS.Components.Data.CmpUnitsDB,ComponentsBD","OPS.Components.Data.CmpPaytypesDefDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}
