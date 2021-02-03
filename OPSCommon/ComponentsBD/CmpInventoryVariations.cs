namespace OPS.Components.Data
{
	/// <summary>
	/// Virtual-filter-compatible executant class for GROUPS.
	/// </summary>
	public class CmpInventoryVariations: CmpGenericBase
	{
		public CmpInventoryVariations() 
		{
			_standardFields		= new string[] {"SSIV_ID","SSIV_SS_ID", "SSIV_INI_DATE", "SSIV_END_DATE", "SSIV_P_EN_LINEA_PAR", "SSIV_P_EN_LINEA_IMPAR", "SSIV_P_EN_BATERIA_PAR", "SSIV_P_EN_BATERIA_IMPAR",  "SSIV_P_C_D_DIA_ENTERO", "SSIV_P_C_D_MEDIODIA", "SSIV_P_PMR", "SSIV_P_VADO_DIA_ENTERO", "SSIV_P_VADO_MEDIODIA", "SSIV_P_BASURA", "SSIV_P_MOTOS", "SSIV_P_WORKS"};
			_standardPks		= new string[] {"SSIV_ID"};
			_standardTableName	= "INVENTORY_VARIATIONS";
			_standardOrderByField	= "";
			_standardOrderByAsc		= "";

			// Field of main table whois foreign Key of table
			_standardRelationFileds	= new string[0]; //{"GRP_DGRP_ID","GRP_RELATED"};
			_standardRelationTables	= new string[0]; //{"OPS.Components.Data.CmpGroupsDefDB,ComponentsBD","OPS.Components.Data.CmpVWGroupsDB,ComponentsBD"};
			_stValidDeleted			= new string[0];
		}
	}
}

