using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Data;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;


namespace OPS.Components
{
    public class DBMapZone : DBComponent
    {

		#region Private

		private string GetBasesSQL(bool inGroup, string groupList)
		{
			StringBuilder sb = new StringBuilder();
			
			sb.Append(" SELECT b.bas_id        AS baseId");
			sb.Append("      , b.bas_name      AS baseName");
			sb.Append("      , b.bas_lat       AS basePosLat");
			sb.Append("      , b.bas_lon       AS basePosLon");
			sb.Append("      , g.grp_descshort AS baseGroup");
			sb.AppendFormat("      , {0}               AS baseInGroup", (inGroup ? "1" : "0"));
			sb.Append(" FROM map_bases b");
			sb.Append(" INNER JOIN groups g ON g.grp_id = b.bas_grp_id");
			sb.Append(" WHERE (g.grp_deleted = 0) AND (g.grp_valid <> 0)");
			sb.Append(" AND (b.bas_deleted = 0) AND (b.bas_visible <> 0)");
			
			if (!IsNullOrEmpty(groupList))
			{
				sb.AppendFormat(" AND (b.bas_grp_id {0} IN ({1}))", (inGroup ? "" : "NOT"), groupList);
			}

			return sb.ToString();
		}


		private string GetChildGroups(string parentGroupId)
		{
			string groups = "";

			StringBuilder sb = new StringBuilder();
			sb.Append(" SELECT t1.cgrpg_child AS childGroupId, t2.zon_grp_id AS zoneGroupId");
			sb.Append(" FROM groups_childs_gis t1");
			sb.Append(" LEFT JOIN map_zones t2 ON t2.zon_grp_id = t1.cgrpg_child");
			sb.Append(" WHERE (t1.cgrpg_valid != 0) AND (t1.cgrpg_deleted = 0)");
			sb.Append("   AND (t1.cgrpg_type = 'G') AND (t1.cgrpg_id = " + parentGroupId + ")");

			DataSet ds = GetDataSet(sb.ToString());

			foreach (DataRow row in ds.Tables[0].Rows)
			{
				if (!Convert.IsDBNull(row["zoneGroupId"])) groups+= "," + row["zoneGroupId"].ToString();
				groups+= GetChildGroups(row["childGroupId"].ToString());
			}

			ds.Dispose();
			ds = null;

			return groups;
		}

		
		private string GetGroups(string groupId)
		{
			return groupId + GetChildGroups(groupId);
		}

		
		private string GetParkingMetersSQL(bool inGroup, string groupList)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(" SELECT p.pkm_id        AS pkmId");
			sb.Append("      , p.pkm_uni_id    AS pkmUId");
			sb.Append("      , u.uni_descshort AS pkmSNum");
			sb.Append("      , u.uni_desclong  AS pkmName");
			sb.Append("      , p.pkm_lat       AS pkmPosLat");
			sb.Append("      , p.pkm_lon       AS pkmPosLon");
			sb.Append("      , s.st            AS pkmStatus");
			sb.Append("      , g.grp_descshort AS pkmGroup");
			sb.AppendFormat("      , {0}               AS pkmInGroup", (inGroup ? "1" : "0"));

			sb.Append(" FROM map_pkmeters p");
			sb.Append(" INNER JOIN units          u ON u.uni_id = p.pkm_uni_id");
			sb.Append(" INNER JOIN groups         g ON g.grp_id = p.pkm_grp_id");
			sb.Append(" INNER JOIN v_units_status s ON s.id     = p.pkm_uni_id");
			sb.Append(" WHERE (u.uni_deleted = 0) AND (u.uni_valid <> 0)");
			sb.Append(" AND (g.grp_deleted = 0) AND (g.grp_valid <> 0)");
			
			if (!IsNullOrEmpty(groupList))
			{
				sb.AppendFormat(" AND (p.pkm_grp_id {0} IN ({1}))", (inGroup ? "" : "NOT"), groupList);
			}

			return sb.ToString();
		}

		
		private bool IsNullOrEmpty(string strValue)
		{
			if (strValue == null) 
			{
				return true;
			}
			else
			{
				return (strValue.Trim() == "");
			}
		}


		#endregion // Private

		#region Public

		#region Helper Classes

		public class Coordinate
		{
			public enum TargetType {Undefined = 0, Base = 1, ParkingMeter = 2};

			private TargetType _target = TargetType.Undefined;
			private string _id = null;
			private string _lat = null;
			private string _lon = null;
			private string _sql = null;

			private bool CheckCoord(string value)
			{
				bool ok = false;
				try
				{
					decimal coord = Decimal.Parse(value,CultureInfo.InvariantCulture);
					ok = (coord != 0);
				}
				catch
				{
				}
				return ok;
			}


			public Coordinate(TargetType target, string id, string lat, string lon)
			{
				if (target == TargetType.Undefined) throw new ArgumentException();
				if (id == null) throw new ArgumentException();
				if (id.Trim() == "") throw new ArgumentException();
				if (!CheckCoord(lat)) throw new ArgumentException();
				if (!CheckCoord(lon)) throw new ArgumentException();

				_target = target;
				_id = id;
				_lat = lat;
				_lon = lon;

				switch(_target)
				{
					case TargetType.Base:
						_sql = string.Format("UPDATE map_bases SET bas_lat = {0}, bas_lon = {1} WHERE bas_id = {2}", _lat, _lon, _id);
						break;
					case TargetType.ParkingMeter:
						_sql = string.Format("UPDATE map_pkmeters SET pkm_lat = {0}, pkm_lon = {1} WHERE pkm_id = {2}", _lat, _lon, _id);
						break;
				}
			}

			
			public string GetKey()
			{
				return _target.ToString() + _id;
			}

			
			public string GetSQL()
			{
				return _sql;
			}
		}

		
		#endregion // Helper Classes

        public void ConfirmAlarms(string csvIdValues)
        {
            StringBuilder sb = new StringBuilder();

			sb.Append(" UPDATE map_alarms");
            sb.Append(" SET alx_als_id = 1");
            sb.AppendFormat(" WHERE alx_id IN ({0})", csvIdValues);

			ExecuteCommand(sb.ToString());
        }

        
		public DataSet GetAlarms(string groupId, string guardId, string typeId, string levelId, string statusId, string lastHours, string records, string langId)
        {
			StringBuilder sb = new StringBuilder();

			sb.Append(" SELECT   t1.alx_id        AS alarmId");
            sb.Append("        , t1.alx_timestamp AS alarmTimeStamp");
			sb.Append("        , t1.alx_ald_id    AS alarmTypeId");
			sb.Append("        , t1.alx_all_id    AS alarmLevelId");
			sb.Append("        , t1.alx_als_id    AS alarmStatusId");
			sb.Append("        , t1.alx_gua_id    AS alarmGuardId");
            sb.Append("        , NVL(t5.lit_desclong, t2.ald_name) AS alarmTypeName");
			sb.Append("        , NVL(t8.lit_desclong, t6.all_name) AS alarmLevelName");
			sb.Append("        , NVL(t9.lit_desclong, t7.als_name) AS alarmStatusName");
			sb.Append("        , TRIM(t3.gua_surname1) || NVL2(t3.gua_surname2,' ' || TRIM(t3.gua_surname2),'') || ', ' || TRIM(t3.gua_name) AS alarmGuardName");
            sb.Append(" FROM map_alarms t1");
			sb.Append(" INNER JOIN map_alarms_def    t2 ON (t2.ald_id = t1.alx_ald_id)");
			sb.Append(" INNER JOIN map_alarms_level  t6 ON (t6.all_id = t1.alx_all_id)");
			sb.Append(" INNER JOIN map_alarms_status t7 ON (t7.als_id = t1.alx_als_id)");
            sb.Append(" INNER JOIN map_guards        t3 ON (t3.gua_id = t1.alx_gua_id)");
			sb.Append(" INNER JOIN groups            t4 ON (t4.grp_id = t3.gua_grp_id)");
			sb.AppendFormat(" LEFT JOIN literals t5 ON (t5.lit_id = t2.ald_name_lit_id) AND (t5.lit_lan_id = {0})", langId);
			sb.AppendFormat(" LEFT JOIN literals t8 ON (t8.lit_id = t6.all_name_lit_id) AND (t8.lit_lan_id = {0})", langId);
			sb.AppendFormat(" LEFT JOIN literals t9 ON (t9.lit_id = t7.als_name_lit_id) AND (t9.lit_lan_id = {0})", langId);

			sb.Append(" WHERE (t4.grp_deleted = 0) AND (t4.grp_valid <> 0) AND (t3.gua_deleted = 0)");

            if (!IsNullOrEmpty(guardId))
            {
                sb.AppendFormat(" AND (t1.alx_gua_id = {0})", guardId);
            }
            else if (!IsNullOrEmpty(groupId))
            {
                sb.AppendFormat(" AND (t4.grp_id IN ({0}))", GetGroups(groupId));
            }

			if (!IsNullOrEmpty(typeId))
			{
				sb.AppendFormat(" AND (t1.alx_ald_id = {0})", typeId);
			}

			if (!IsNullOrEmpty(levelId))
			{
				sb.AppendFormat(" AND (t1.alx_all_id = {0})", levelId);
			}

            if (!IsNullOrEmpty(statusId))
            {
                sb.AppendFormat(" AND (t1.alx_als_id = {0})", statusId);
            }
            else if (IsNullOrEmpty(guardId))
            {
                sb.Append(" AND (t1.alx_als_id = 0)");
            }

			if (!IsNullOrEmpty(lastHours))
			{
				sb.AppendFormat(" AND (t1.alx_timestamp BETWEEN (SYSDATE - ({0} * 1/24)) AND SYSDATE)", lastHours);
			}

			if (!IsNullOrEmpty(records))
			{
				sb.AppendFormat(" AND (rownum <= {0})", records);
			}

            sb.Append(" ORDER BY t1.alx_timestamp DESC");

            return GetDataSet(sb.ToString());
        }


		public DataSet GetAlarmsHist(string groupId, string guardId, string typeId, string levelId, string statusId, string lastHours, string records, string langId)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(" SELECT   t1.alxh_id            AS alarmId");
			sb.Append("        , t1.alxh_timestamp_ini AS alarmTimeStamp");
			sb.Append("        , t1.alxh_ald_id        AS alarmTypeId");
			sb.Append("        , t1.alxh_all_id        AS alarmLevelId");
			sb.Append("        , t1.alxh_als_id        AS alarmStatusId");
			sb.Append("        , t1.alxh_gua_id        AS alarmGuardId");
			sb.Append("        , NVL(t5.lit_desclong, t2.ald_name) AS alarmTypeName");
			sb.Append("        , NVL(t8.lit_desclong, t6.all_name) AS alarmLevelName");
			sb.Append("        , NVL(t9.lit_desclong, t7.als_name) AS alarmStatusName");
			sb.Append("        , TRIM(t3.gua_surname1) || NVL2(t3.gua_surname2,' ' || TRIM(t3.gua_surname2),'') || ', ' || TRIM(t3.gua_name) AS alarmGuardName");
			sb.Append(" FROM map_alarms_his t1");
			sb.Append(" INNER JOIN map_alarms_def    t2 ON (t2.ald_id = t1.alxh_ald_id)");
			sb.Append(" INNER JOIN map_alarms_level  t6 ON (t6.all_id = t1.alxh_all_id)");
			sb.Append(" INNER JOIN map_alarms_status t7 ON (t7.als_id = t1.alxh_als_id)");
			sb.Append(" INNER JOIN map_guards        t3 ON (t3.gua_id = t1.alxh_gua_id)");
			sb.Append(" INNER JOIN groups            t4 ON (t4.grp_id = t3.gua_grp_id)");
			sb.AppendFormat(" LEFT JOIN literals t5 ON (t5.lit_id = t2.ald_name_lit_id) AND (t5.lit_lan_id = {0})", langId);
			sb.AppendFormat(" LEFT JOIN literals t8 ON (t8.lit_id = t6.all_name_lit_id) AND (t8.lit_lan_id = {0})", langId);
			sb.AppendFormat(" LEFT JOIN literals t9 ON (t9.lit_id = t7.als_name_lit_id) AND (t9.lit_lan_id = {0})", langId);

			sb.Append(" WHERE (t4.grp_deleted = 0) AND (t4.grp_valid <> 0) AND (t3.gua_deleted = 0)");

			if (!IsNullOrEmpty(guardId))
			{
				sb.AppendFormat(" AND (t1.alxh_gua_id = {0})", guardId);
			}
			else if (!IsNullOrEmpty(groupId))
			{
				sb.AppendFormat(" AND (t4.grp_id IN ({0}))", GetGroups(groupId));
			}

			if (!IsNullOrEmpty(typeId))
			{
				sb.AppendFormat(" AND (t1.alxh_ald_id = {0})", typeId);
			}

			if (!IsNullOrEmpty(levelId))
			{
				sb.AppendFormat(" AND (t1.alxh_all_id = {0})", levelId);
			}

			if (!IsNullOrEmpty(statusId))
			{
				sb.AppendFormat(" AND (t1.alxh_als_id = {0})", statusId);
			}
			else if (IsNullOrEmpty(guardId))
			{
				sb.Append(" AND (t1.alxh_als_id = 0)");
			}

			if (!IsNullOrEmpty(lastHours))
			{
				sb.AppendFormat(" AND (t1.alxh_timestamp_ini BETWEEN (SYSDATE - ({0} * 1/24)) AND SYSDATE)", lastHours);
			}

			if (!IsNullOrEmpty(records))
			{
				sb.AppendFormat(" AND (rownum <= {0})", records);
			}

			sb.Append(" ORDER BY t1.alxh_timestamp_ini DESC");

			return GetDataSet(sb.ToString());
		}

		
		public DataSet GetBases(string groupId)
        {
			StringBuilder sb = new StringBuilder();
			if (IsNullOrEmpty(groupId))
			{
				sb.Append(GetBasesSQL(true, null));
			}
			else
			{
				string groups = GetGroups(groupId);
				sb.Append(GetBasesSQL(true, groups));
				sb.Append(" UNION");
				sb.Append(GetBasesSQL(false, groups));
			}
			sb.Append(" ORDER BY baseGroup, baseName");
			return GetDataSet(sb.ToString());
		}

        
		public DataSet GetGuards(string groupId, string alarmsLastHours, string alarmsEnabledTypes)
        {
            StringBuilder sb = new StringBuilder();

			sb.Append(" SELECT x.gua_id      AS guardId");
            sb.Append("      , TRIM(x.gua_surname1) || NVL2(x.gua_surname2,' ' || TRIM(x.gua_surname2),'') || ', ' || TRIM(x.gua_name) AS guardName");
            sb.Append("      , x.gua_ipdir   AS guardIP");
            sb.Append("      , x.gua_ipdate  AS guardIPDate");
            sb.Append("      , x.gua_poslat  AS guardPosLat");
            sb.Append("      , x.gua_poslon  AS guardPosLon");
            sb.Append("      , x.gua_posdate AS guardPosDate");
            sb.Append("      , x.gua_photo   AS guardImgProfile");
			sb.Append("      , x.gua_status  AS guardStatus");

            if (IsNullOrEmpty(alarmsLastHours) || IsNullOrEmpty(alarmsEnabledTypes))
            {
                sb.Append("      , -1 AS guardWarningCount");
                sb.Append("      , -1 AS guardAlarmCount");
            }
            else
            {
                sb.Append("      , ( SELECT COUNT(*) FROM map_alarms a1");
                sb.Append("          WHERE (a1.alx_gua_id = x.gua_id)");
				sb.Append("            AND (a1.alx_als_id = 0)");
                sb.Append("            AND (a1.alx_all_id = 2)");
                sb.AppendFormat("      AND (a1.alx_timestamp  BETWEEN (SYSDATE - ({0} * 1/24)) AND SYSDATE)", alarmsLastHours);
                sb.AppendFormat("      AND (a1.alx_ald_id IN ({0}))", alarmsEnabledTypes);
                sb.Append("        ) AS guardWarningCount");

				sb.Append("      , ( SELECT COUNT(*) FROM map_alarms a2");
				sb.Append("          WHERE (a2.alx_gua_id = x.gua_id)");
				sb.Append("            AND (a2.alx_als_id = 0)");
				sb.Append("            AND (a2.alx_all_id = 1)");
				sb.AppendFormat("      AND (a2.alx_timestamp  BETWEEN (SYSDATE - ({0} * 1/24)) AND SYSDATE)", alarmsLastHours);
				sb.AppendFormat("      AND (a2.alx_ald_id IN ({0}))", alarmsEnabledTypes);
				sb.Append("        ) AS guardAlarmCount");
            }

            sb.Append(" FROM map_guards x");
			sb.Append(" INNER JOIN groups g ON g.grp_id = x.gua_grp_id");
			sb.Append(" WHERE (g.grp_deleted = 0) AND (g.grp_valid <> 0)");
			sb.Append(" AND (x.gua_deleted = 0)");

            if (!IsNullOrEmpty(groupId))
            {
				sb.AppendFormat(" AND (x.gua_grp_id IN ({0}))", GetGroups(groupId));
            }
            sb.Append(" ORDER BY x.gua_surname1, x.gua_surname2, x.gua_name");

            return GetDataSet(sb.ToString());
        }

        
		public DataSet GetMapPoint(string groupId)
        {
			StringBuilder sb = new StringBuilder();

			sb.Append(" SELECT z.zon_lat  AS zone_lat");
			sb.Append("      , z.zon_lon  AS zone_lon");
			sb.Append("      , z.zon_zoom AS zone_zoom");
			sb.Append(" FROM map_zones z");
			sb.Append(" INNER JOIN groups g ON g.grp_id = z.zon_grp_id");
			sb.Append(" WHERE (g.grp_deleted = 0) AND (g.grp_valid <> 0)");
			sb.AppendFormat(" AND (z.zon_grp_id = {0})", groupId);

            return GetDataSet(sb.ToString());
        }

        
		public DataSet GetParkingMeters(string groupId)
        {
			StringBuilder sb = new StringBuilder();
			if (IsNullOrEmpty(groupId))
			{
				sb.Append(GetParkingMetersSQL(true, null));
			}
			else
			{
				string groups = GetGroups(groupId);
				sb.Append(GetParkingMetersSQL(true, groups));
				sb.Append(" UNION");
				sb.Append(GetParkingMetersSQL(false, groups));
			}
			sb.Append(" ORDER BY pkmGroup, pkmName");
			return GetDataSet(sb.ToString());
        }

        
		public void UpdateCoordinates(SortedList list)
		{
			OracleTransaction dbTran = GetTransaction();;
			long numRowsAffected = 0;
			try
			{
				foreach(DBMapZone.Coordinate coord in list.Values)
				{
					numRowsAffected = ExecuteCommand(coord.GetSQL(), dbTran);
				}
				dbTran.Commit();
			}
			catch (Exception)
			{
				dbTran.Rollback();
			}
			finally
			{
				if (dbTran != null)
				{
					dbTran.Dispose();
					dbTran = null;
				}
			}
		}


		#endregion // Public

    }
}
