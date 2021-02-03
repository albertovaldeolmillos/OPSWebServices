using System;
using System.Collections.Specialized;
using System.Text;
using System.Data;
using System.Collections;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Summary description for CmpCallejero.
	/// </summary>
	public class CmpCallejero
	{
		
		public CmpCallejero()
		{}


		/// <summary>
		/// Gets the number of alarms for that sector
		/// </summary>
		/// <param name="zoneId">The Zone to return alarms</param>
		/// <returns>Integer with the qty of alarms</returns>
		/// 
		public static int GetAlarmsQtyByZone (int zoneId)
		{
			int qty = 0;
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();

			// STEP 1: Get the first level of sub-zones (Level 2)
			string swhere	= "CGRPG_ID = " + zoneId.ToString();
			DataTable dtzone	= cGroupChildsDb.GetData(null,swhere,null,null);
			// We made a recursive search of the tree to find nodes
			qty = SeekUnitAlarmsQty(dtzone,qty,zoneId);

			return qty;
		}

		public static int SeekUnitAlarmsQty(DataTable dtzone, int qty, int zoneId)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			string UNIT_ID = (string)appSettings.GetValue("GroupsChilds.UnitId", typeof(string));

			string swhere	= "";

			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			CmpGroupsDB			cGroupsDb	= new CmpGroupsDB();

			foreach (DataRow dr in dtzone.Rows)
			{
				// If there is not a UNIT we follow deeping in the tree...(Level 2)
				if (dr["CGRPG_TYPE"].ToString() != UNIT_ID) 
				{
					swhere	= "CGRPG_ID	= " + dr["CGRPG_CHILD"].ToString();
					DataTable dtsubzone	= cGroupChildsDb.GetData(null,swhere,null,null);
					qty	= SeekUnitAlarmsQty(dtsubzone,qty,zoneId);
				}
					// if theres a unit let's get if he has an active alarm (or more)
				else
				{
					swhere	= "ALA_UNI_ID	= " + dr["CGRPG_CHILD"].ToString();
					DataTable dtAlarms	= new CmpAlarmsDB().GetData(null,swhere,null,null);
					if (dtAlarms.Rows.Count > 0)
					{
						qty++;
					}
				}
			}
			return qty;
		}



		/// <summary>
		/// Gets All Alarms for a determined Zone, it returns the number of alarms for that sector
		/// </summary>
		/// <param name="zoneId">The Zone to return alarms</param>
		/// <returns>DataTable with the alarm_id, the zone of the alarm and the description of alarm</returns>
		/// 
		public static DataTable GetAlarmsByZone (int zoneId)
		{
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			DataTable	dtReturn		= new DataTable();

			dtReturn.Columns.Add("UNI_ID");
			dtReturn.Columns.Add("ZONE_ID");
			dtReturn.Columns.Add("DALA_LIT_ID");
			dtReturn.Columns.Add("UNI_DESCSHORT");

			// STEP 1: Get the first level of sub-zones (Level 2)
			string swhere	= "CGRPG_ID = " + zoneId.ToString();
			DataTable dtzone	= cGroupChildsDb.GetData(null,swhere,null,null);
			// We made a recursive search of the tree to find nodes
			SeekUnitAlarms(dtzone,dtReturn,zoneId);

			//			DataTable dtAlarmsbyzone	= GetAlarms();
			//			for (int aux =0; aux < dtAlarmsbyzone.Rows.Count; aux ++)
			//			{
			//				if (dtAlarmsbyzone.Rows[aux].ItemArray[1].ToString() == zoneId.ToString())
			//			}
		
			//dtReturn.Rows.Add(ovalues);
			
			return dtReturn;
		}

		public static void SeekUnitAlarms(DataTable dtzone, DataTable dtReturn, int zoneId)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			string UNIT_ID = (string)appSettings.GetValue("GroupsChilds.UnitId", typeof(string));
			string swhere	= "";
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			CmpGroupsDB			cGroupsDb	= new CmpGroupsDB();

			object[]	ovalues			= new object[4];

			foreach (DataRow dr in dtzone.Rows)
			{
				// If there is not a UNIT we follow deeping in the tree...(Level 2)
				if (dr["CGRPG_TYPE"].ToString() != UNIT_ID) 
				{
					swhere	= "CGRPG_ID	= " + dr["CGRPG_CHILD"].ToString();
					DataTable dtsubzone	= cGroupChildsDb.GetData(null,swhere,null,null);
					SeekUnitAlarms(dtsubzone,dtReturn,zoneId);
				}
					// if theres a unit let's get if he has an active alarm (or more)
				else
				{
					swhere	= "ALA_UNI_ID	= " + dr["CGRPG_CHILD"].ToString();
					//DataTable dtAlarms	= new CmpAlarmsDB().GetData(null,swhere,null,null);

					DataTable dtAlarmsZone	= new CmpAlarmsZoneDB().GetData(null,swhere,null,null);
					for (int j = 0; j < dtAlarmsZone.Rows.Count; j++)
					{
						ovalues[0]	= new object();
						ovalues[1]	= new object();
						ovalues[2]	= new object();
						ovalues[3]	= new object();
						ovalues[0]	= dtAlarmsZone.Rows[j]["ALA_UNI_ID"].ToString();
						ovalues[1]	= zoneId;
						ovalues[2]	= dtAlarmsZone.Rows[j]["DALA_LIT_ID"].ToString();
						ovalues[3]	= dtAlarmsZone.Rows[j]["UNI_DESCSHORT"].ToString();
						dtReturn.Rows.Add(ovalues);
					}
				}
			}
		}



		/// <summary>
		/// Gets All Alarms for zones
		/// </summary>
		/// <returns>DataTable with info about alarms and Phisical zones</returns>
		/// 
		public static DataTable GetAlarms ()
		{

			// Components Used
			CmpAlarmsDB			cAlarmsDb		= new CmpAlarmsDB();
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			CmpGroupsDB			cGroupsDb		= new CmpGroupsDB();
		
			// STEP 1: Get the alarms
			DataTable dtalarms	= cAlarmsDb.GetData();
			// STEP 2: Recursive search of the unit to find his zone.
			DataTable	dtReturn		= new DataTable();
			dtReturn.Columns.Add("UNI_ID");
			dtReturn.Columns.Add("ZONE_ID");
			object[]	ovalues			= new object[2];

			foreach (DataRow dr in dtalarms.Rows)
			{

				// Use a recursive function to reach zone. We pass the unit (or group) and the functions return its sector
				string id =dr["ALA_UNI_ID"].ToString();
				string sZone = "";
				sZone  = RecursiveSearchParent(id);

				ovalues[0]	= new object();
				ovalues[1]	= new object();

				if (String.Compare(sZone,"-1") != 0)
				{
					ovalues[0]	= dr["ALA_UNI_ID"];
					ovalues[1]	= sZone;

					dtReturn.Rows.Add(ovalues);
				}

			}
			
			return dtReturn;
		}

		/// <summary>
		/// This function returns the zone of a unit/group
		/// </summary>
		/// <param name="iUnitId"></param>
		/// <returns></returns>
		public static string RecursiveSearchParent(string sGroupId)
		{

			string sWhereUnit	= "CGRPG_CHILD = " + sGroupId;


			// Use a view (CmpGroupsPhy) to filter units to find his physical parent.
			DataTable dtGroupsChild	= new CmpVWGroupsPhyDB().GetData(null,sWhereUnit,null,null);
			// This view should only return one row, indicating level, parent and type, if type = 1 (Zone) finish, else continue searching
			// If no rows are returned, it means there are not phyiscal parents, the function returns -1.
			if (dtGroupsChild.Rows.Count == 0) 
			{
				return "-1";
			}

			sGroupId		= dtGroupsChild.Rows[0]["GRP_ID"].ToString();

			string sGroupChild	= dtGroupsChild.Rows[0]["CGRPG_CHILD"].ToString();
			string sGroupType	= dtGroupsChild.Rows[0]["GRP_DGRP_ID"].ToString();
			
			if ((sGroupType == "1")||(sGroupType == "14"))
			{
				return sGroupId;
			}
			else
			{

				return RecursiveSearchParent(sGroupId);
			}

			
			/*

			string sGroupParent = dtGroupsChild.Rows[0].ItemArray[dtGroupsChild.Columns["GRP_DGRP_TPYE"].Ordinal].ToString();
			string sGroupType = dtGroupsChild.Rows[0].ItemArray[dtGroupsChild.Columns["GRP_DGRP_TPYE"].Ordinal].ToString();
			string sGroupType = dtGroupsChild.Rows[0].ItemArray[dtGroupsChild.Columns["GRP_DGRP_TPYE"].Ordinal].ToString();
			iUnitParent				= Int32.Parse(dtGroupsChild.Rows[0].ItemArray[dtGroupsChild.Columns["GRP_ID"].Ordinal].ToString());
			sWhereParent			= "GRP_ID = " + iUnitParent;
			DataTable dtGroups		= new CmpGroupsDB().GetData(null,sWhereParent,null,null);

			while (dtGroups.Rows.Count > 0 && dtGroupsChild.Rows.Count > 0)
			{
				iUnitId			= Int32.Parse(dtGroups.Rows[0].ItemArray[0].ToString());
				sWhereUnit		= "CGRPG_CHILD = " + iUnitId;
				dtGroupsChild	= new CmpGroupsChildsGisDB().GetData(null,sWhereUnit,null,null);
				if (dtGroupsChild.Rows.Count > 0)
				{
					iUnitParent		= Int32.Parse(dtGroupsChild.Rows[0].ItemArray[0].ToString());
					sWhereParent	= "GRP_ID = " + iUnitParent;
					dtGroups		= new CmpGroupsDB().GetData(null,sWhereParent,null,null);
				}
			}
			return iUnitId;
			
			*/
		}



		/// <summary>
		/// Gets All Units for zones
		/// </summary>
		/// <returns>Integer with the quantity of units of this agrupation</returns>
		/// 
		public static int GetUnitsByZone(int zoneId)
		{
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			int qty = 0;
			// STEP 1: Get the first level of sub-zones (Level 2)
			string swhere	= "CGRPG_ID = " + zoneId.ToString();
			DataTable dtzone	= cGroupChildsDb.GetData(null,swhere,null,null);
			// We made a recursive search of the tree to find nodes
			qty = SeekUnits(dtzone,qty);
			return qty;
		}

		public static int GetUnits()
		{
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			int qty = 0;
			// STEP 1: Get the first level of sub-zones (Level 2)
			string swhere	= "CGRPG_ID IN (SELECT GRP_ID FROM GROUPS WHERE GRP_DGRP_ID=1)";
			DataTable dtzone	= cGroupChildsDb.GetData(null,swhere,null,null);
			// We made a recursive search of the tree to find nodes
			qty = SeekUnits(dtzone,qty);
			return qty;
		}



		public static int SeekUnits(DataTable dtzone, int qty)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			string UNIT_ID = (string)appSettings.GetValue("GroupsChilds.UnitId", typeof(string));
			string swhere	= "";
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			CmpGroupsDB			cGroupsDb	= new CmpGroupsDB();
			foreach (DataRow dr in dtzone.Rows)
			{
				// If there is not a UNIT we follow deeping in the tree...(Level 2)
				if (dr["CGRPG_TYPE"].ToString() != UNIT_ID) 
				{
					swhere	= "CGRPG_ID	= " + dr["CGRPG_CHILD"].ToString();
					DataTable dtsubzone	= cGroupChildsDb.GetData(null,swhere,null,null);
					qty = SeekUnits(dtsubzone,qty);
				}
				else
				{
					if (dr["CGRPG_CHILD"].ToString()!="")
					{

						CmpUnitsDB udb = new CmpUnitsDB();
						swhere = "UNI_ID = " + dr["CGRPG_CHILD"].ToString() + " AND UNI_DPUNI_ID IN (1,2) ";
						DataTable dtUnit = udb.GetData(null, swhere, null, null);
						if (dtUnit.Rows.Count != 0)
						{
							qty ++;
						}						
					}


					
				}
			}  
			return qty;
		}

		public static string GetUnitsStringByZone(int zoneId)
		{
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			string strRes = "";
			// STEP 1: Get the first level of sub-zones (Level 2)
			string swhere	= "CGRPG_ID = " + zoneId.ToString();
			DataTable dtzone	= cGroupChildsDb.GetData(null,swhere,null,null);
			// We made a recursive search of the tree to find nodes
			strRes = SeekUnitsString(dtzone);
			return strRes;
		}

		public static string GetUnitsString()
		{
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			string strRes = "";
			// STEP 1: Get the first level of sub-zones (Level 2)
			string swhere	= "CGRPG_ID IN (SELECT GRP_ID FROM GROUPS WHERE GRP_DGRP_ID=1)";
			DataTable dtzone	= cGroupChildsDb.GetData(null,swhere,null,null);
			// We made a recursive search of the tree to find nodes
			strRes = SeekUnitsString(dtzone);
			return strRes;
		}

		public static string SeekUnitsString(DataTable dtzone)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			string UNIT_ID = (string)appSettings.GetValue("GroupsChilds.UnitId", typeof(string));
			string swhere	= "";
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			CmpGroupsDB			cGroupsDb	= new CmpGroupsDB();
			string strRes="";
			string strTemp="";
			foreach (DataRow dr in dtzone.Rows)
			{
				// If there is not a UNIT we follow deeping in the tree...(Level 2)
				if (dr["CGRPG_TYPE"].ToString() != UNIT_ID) 
				{
					swhere	= "CGRPG_ID	= " + dr["CGRPG_CHILD"].ToString();
					DataTable dtsubzone	= cGroupChildsDb.GetData(null,swhere,null,null);
					strTemp = SeekUnitsString(dtsubzone);
					if (strTemp!="")
					{
						if (strRes=="")
						{
							strRes=strTemp;
						}
						else
						{
							strRes+=","+strTemp;
						}
					}

				}
				else
				{
					if (dr["CGRPG_CHILD"].ToString()!="")
					{

						CmpUnitsDB udb = new CmpUnitsDB();
						swhere = "UNI_ID = " + dr["CGRPG_CHILD"].ToString() + " AND UNI_DPUNI_ID IN (1,2) ";
						DataTable dtUnit = udb.GetData(null, swhere, null, null);
						if (dtUnit.Rows.Count != 0)
						{
							if (strRes=="")
							{
								strRes=dr["CGRPG_CHILD"].ToString();
							}
							else
							{
								strRes+=","+dr["CGRPG_CHILD"].ToString();
							}
						}
						
					}
				}
			}  
			return strRes;
		}

		/// <summary>
		/// Gets All Units for zones
		/// </summary>
		/// <returns>Integer with the quantity of units of this agrupation</returns>
		/// 
		public static int GetActiveUnitsByZone(int zoneId)
		{
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			int qty = 0;
			// STEP 1: Get the first level of sub-zones (Level 2)
			string swhere	= "CGRPG_ID = " + zoneId.ToString();
			DataTable dtzone	= cGroupChildsDb.GetData(null,swhere,null,null);
			// We made a recursive search of the tree to find nodes
			qty = SeekActiveUnits(dtzone,qty);
			return qty;
		}

		public static int SeekActiveUnits(DataTable dtzone, int qty)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			string UNIT_ID = (string)appSettings.GetValue("GroupsChilds.UnitId", typeof(string));

			string swhere	= "";
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			CmpGroupsDB			cGroupsDb	= new CmpGroupsDB();
			foreach (DataRow dr in dtzone.Rows)
			{
				// If there is not a UNIT we follow deeping in the tree...(Level 2)
				if (dr["CGRPG_TYPE"].ToString() != UNIT_ID) 
				{
					swhere	= "CGRPG_ID	= " + dr["CGRPG_CHILD"].ToString();
					DataTable dtsubzone	= cGroupChildsDb.GetData(null,swhere,null,null);
					qty = SeekActiveUnits(dtsubzone,qty);
				}
				else
				{
					CmpUnitsDB udb = new CmpUnitsDB();
					swhere = "UNI_ID = " + dr["CGRPG_CHILD"].ToString() + " AND UNI_IP IS NOT NULL AND UNI_DPUNI_ID IN (1,2) ";
					DataTable dtUnit = udb.GetData(null, swhere, null, null);
					if (dtUnit.Rows.Count != 0)
						qty++;
				}
			}  
			return qty;
		}

		///////// Cuencas
		/// <summary>
		/// Gets All Units for zones
		/// </summary>
		/// <returns>Integer with the quantity of units of this agrupation</returns>
		/// 
		public static int GetActiveUnitsByCuenca(int zoneId)
		{
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			int qty = 0;
			// STEP 1: Get the first level of sub-zones (Level 2)
			string swhere	= "CGRPG_ID = " + zoneId.ToString();
			DataTable dtzone	= cGroupChildsDb.GetData(null,swhere,null,null);
			// We made a recursive search of the tree to find nodes
			qty = SeekActiveUnitsCuenca(dtzone,qty);
			return qty;
		}

		public static int SeekActiveUnitsCuenca(DataTable dtzone, int qty)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			string UNIT_ID = (string)appSettings.GetValue("GroupsChilds.UnitId", typeof(string));

			string swhere	= "";
			CmpGroupsChildsGisDB	cGroupChildsDb	= new CmpGroupsChildsGisDB();
			CmpGroupsDB			cGroupsDb	= new CmpGroupsDB();
			foreach (DataRow dr in dtzone.Rows)
			{
				// If there is not a UNIT we follow deeping in the tree...(Level 2)
				if (dr["CGRPG_TYPE"].ToString() != UNIT_ID) 
				{
					swhere	= "CGRPG_ID	= " + dr["CGRPG_CHILD"].ToString();
					DataTable dtsubzone	= cGroupChildsDb.GetData(null,swhere,null,null);
					qty = SeekActiveUnits(dtsubzone,qty);
				}
				else
				{
					CmpUnitsDB udb = new CmpUnitsDB();
					swhere = "UNI_ID = " + dr["CGRPG_CHILD"].ToString() + " AND UNI_IP IS NOT NULL AND UNI_DSTA_ID IN (0,1,2)";
					DataTable dtUnit = udb.GetData(null, swhere, null, null);
					if (dtUnit.Rows.Count != 0)
						qty++;
				}
			}  
			return qty;
		}
	}
}