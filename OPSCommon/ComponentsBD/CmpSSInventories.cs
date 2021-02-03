namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpSSInventories: CmpGenericBase
	{
		public CmpSSInventories() 
		{
			_standardFields		= new string[] {"SSI_ID","SSI_SS_ID", "SSI_INI_DATE", "SSI_END_DATE", "SSI_USR_ID", "SSI_UNI_ID", "SSI_P_EN_LINEA_PAR", "SSI_P_EN_LINEA_IMPAR", "SSI_P_EN_BATERIA_PAR", "SSI_P_EN_BATERIA_IMPAR",  "SSI_P_C_D_DIA_ENTERO", "SSI_P_C_D_MEDIODIA", "SSI_P_PMR", "SSI_P_VADO_DIA_ENTERO", "SSI_P_VADO_MEDIODIA", "SSI_P_BASURA", "SSI_P_MOTOS", "SSI_ZONE_ID", "SSI_SECTOR_ID", "SSI_ROUTE_ID"};
			_standardPks		= new string[] {"SSI_ID"};
			_standardTableName	= "V_SS_INVENTORIES";
			_standardOrderByField	= "SSI_INI_DATE";
			_standardOrderByAsc		= "DESC";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];

		}
	}
}

