using System;
using System.Data;
using OPS.Components.Data;
using System.Collections;

using OTS.Framework.Collections;

namespace OPS.Components
{
	/// <summary>
	/// Contains methods to search for operations associated to vehicle, date and group
	/// </summary>
	public class CmpOperations
	{
		#region Static stuff

		// Static "constants"
		private static int OPERATIONS_DEF_PARKING;
		private static int OPERATIONS_DEF_EXTENSION;

		/// <summary>
		/// Init the static variables reading the configuration file
		/// </summary>
		static CmpOperations()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();			
			OPERATIONS_DEF_PARKING = (int)appSettings.GetValue ("OperationsDef.Parking", typeof(int));
			OPERATIONS_DEF_EXTENSION = (int)appSettings.GetValue ("OperationsDef.Extension", typeof(int));
		}

		#endregion

		/// <summary>
		/// Results codes for CmpOperations::FindOperationsResults
		/// </summary>
		public enum FindOperationsResults
		{
			/// <summary>
			/// One or more valid operations were found and max time is returned in [out] parameter
			/// </summary>
			OK = 1,
			/// <summary>
			/// Not found any operation
			/// </summary>
			NotFoundAnyOperation = 2,
			/// <summary>
			/// At least one invalid operation was found (invalid operation is one that is associated with a PDM
			/// that does has not any relation with group passed).
			/// </summary>
			OperationOnInvalidZone = 3
		}

		private DateTime _date;
		private string _vehicle;
		private int _grpid;
		private int _ampliedZoneTypeId;

		/// <summary>
		/// Constructs a new CmpOperations associated to a vehicle, group and date
		/// </summary>
		/// <param name="date">Date</param>
		/// <param name="vehicle">ID of the vehicle</param>
		/// <param name="grpid">ID of the group</param>
		/// <param name="azoneId"`>ID of the type of the amplied zone (that is a config parameter).</param>
		public CmpOperations(DateTime date, string vehicle, int grpid, int azoneId)
		{
			_date = date;
			_vehicle  = vehicle;
			_grpid = grpid;
			_ampliedZoneTypeId = azoneId;
		}


		/// <summary>
		/// Find if one (or more) operations are associated for the vehicle, date and group.
		/// If an operation is found, but in other group, the method checks if the PDM of the operation found belongs
		/// to an amplied zone containing the associated group and the PDM. If the amplied zone is found, the operation
		/// is considered valid.
		/// </summary>
		/// <param name="maxTime">If retval is FindOperationsResult.OK, the parameter is filled with the max of OPE_ENDDATE of all operations</param>
		/// <returns>true if one or more operations were found</returns>
		public FindOperationsResults FindOperations (out DateTime maxTime)
		{
			// 1. Get all operations for the current vehicleID and date.
			maxTime = DateTime.MinValue;
			CmpOperationsDB cmp = new CmpOperationsDB();
			DataTable dt = cmp.GetData (null, "OPE_INIDATE <= @OPERATIONS.OPE_INIDATE@ AND OPE_ENDDATE >= @OPERATIONS.OPE_ENDDATE@ AND OPE_VEHICLEID = @OPERATIONS.OPE_VEHICLEID@", //ORDER BY OPE_ENDDATE DESC
				new object[] { _date, _date,_vehicle});
			
			if (dt.Rows.Count == 0) return FindOperationsResults.NotFoundAnyOperation;			// No operations found...
			// Select the operations on the same group
			DataRow[] currentGroupOperations = dt.Select ("OPE_GRP_ID=" + _grpid,"OPE_ENDDATE DESC");
			if (currentGroupOperations.Length > 0)
			{
				maxTime = Convert.ToDateTime (currentGroupOperations[0]["OPE_ENDDATE"]);
				return FindOperationsResults.OK;
			}
			else
			{
				FindOperationsResults bRet = FindOperationsResults.OK;
				// Not found operations for the SAME group... let's check PDM of all operations for the vehicle...
				DateTime maxOpeDate = DateTime.MinValue;
				foreach (DataRow dr in dt.Rows)
				{
					// Check if operation is in the current group or in another group but inside the same amplied zone
					int opePdm = Convert.ToInt32(dr["OPE_UNI_ID"]);
					if (CheckForAmpliedZone (opePdm)!=-1)
					{
						// operation is good
						// NOTE: When we find the first operation that is in the amplied group, we could finish (because
						// we have the operations sorted by OPE_ENDDATE).
						// But we will continue, because we could find a 2nd operation in INVALID zone... in that case the car
						// must be fined.
						DateTime opeEnd = Convert.ToDateTime (dr["OPE_ENDDATE"]);
						if (opeEnd > maxOpeDate) maxOpeDate = opeEnd;
					}
					else
					{
						// Vehicle MUST be fined
						bRet = FindOperationsResults.OperationOnInvalidZone;
						break;
					
					}
				}
				if (bRet == FindOperationsResults.OK) maxTime = maxOpeDate;
				return bRet;
			}
		}

		/// <summary>
		/// Check if exists one amplied zone that has the PDM passed and our group associated.
		/// </summary>
		/// <param name="pdm">PDM to search for amplied zone</param>
		/// <returns>ID of the amplied zone or -1 if no amplied zone is found.</returns>
		protected int CheckForAmpliedZone (int pdm)
		{
			CmpStatus cmps = new CmpStatus();
			UnorderedTree tree = null;
			// Build the logical tree of the pdm
			ArrayList ar = cmps.GetUnitTree(pdm, true, false ,out tree);
			// Now search for ar, searching for our group. If we found and is a amplied zone that means that exists
			// an amplied zone containing both: our group, and the pdm.
			foreach (object o in ar)
			{
				CmpStatus.StatusTreeItem titem = ((CmpStatus.StatusTreeItem)((UnorderedTree.TreeItem)o).Data);
				if (titem.IsGroup && titem.Id == _grpid &&  titem.IdType == _ampliedZoneTypeId) return titem.Id;
			}
			return -1;
		}

		/// <summary>
		/// Obtains all the operations related to a parking in one register.
		/// So, if "id" is a parking, searches also for its extensions
		/// And if "id" itself is an extension, looks for its base parking and afterwards its extensions
		/// </summary>
		/// <param name="id">The parking to look for</param>
		/// <returns>A single row with the summarized data</returns>
		public DataTable GetSummarizedParking(int id)
		{
			CmpOperationsDB odb = new CmpOperationsDB();
			DataTable dt = odb.GetAllData(null, "OPE_ID = @OPERATIONS.OPE_ID@ OR OPE_OPE_ID = @OPERATIONS.OPE_OPE_ID@",
				new object[] { id, id } );
			if (dt.Rows.Count == 0)
				return null;
			else if (dt.Rows.Count == 1) // Is it a parking?
			{
				if (Convert.ToInt32(dt.Rows[0]["OPE_DOPE_ID"]) == OPERATIONS_DEF_EXTENSION)
				{
					int original = Convert.ToInt32(dt.Rows[0]["OPE_OPE_ID"]);
					dt = odb.GetAllData(null, "OPE_ID = @OPERATIONS.OPE_ID@ OR OPE_OPE_ID = @OPERATIONS.OPE_OPE_ID@",
						new object[] { original, original } );
				}
				else if (Convert.ToInt32(dt.Rows[0]["OPE_DOPE_ID"]) != OPERATIONS_DEF_PARKING)
				{
					return null;
				}
			}
			// Now just summarize the data
			foreach (DataRow row in dt.Rows)
			{
				if (Convert.ToInt32(row["OPE_ID"]) != Convert.ToInt32(dt.Rows[0]["OPE_ID"]))
				{
					// MOVDATE
					if (dt.Rows[0]["OPE_MOVDATE"] == DBNull.Value)
						dt.Rows[0]["OPE_MOVDATE"] = row["OPE_MOVDATE"];
					else if (row["OPE_MOVDATE"] != DBNull.Value)
						dt.Rows[0]["OPE_MOVDATE"] =
							MaxDateTime(Convert.ToDateTime(row["OPE_MOVDATE"]), 
							Convert.ToDateTime(dt.Rows[0]["OPE_MOVDATE"]));

					// INIDATE
					if (dt.Rows[0]["OPE_INIDATE"] == DBNull.Value)
						dt.Rows[0]["OPE_INIDATE"] = row["OPE_INIDATE"];
					else if (row["OPE_INIDATE"] != DBNull.Value)
						dt.Rows[0]["OPE_INIDATE"] = 
							MinDateTime(Convert.ToDateTime(row["OPE_INIDATE"]), 
							Convert.ToDateTime(dt.Rows[0]["OPE_INIDATE"]));

					// ENDDATE
					if (dt.Rows[0]["OPE_ENDDATE"] == DBNull.Value)
						dt.Rows[0]["OPE_ENDDATE"] = row["OPE_ENDDATE"];
					else if (row["OPE_ENDDATE"] != DBNull.Value)
						dt.Rows[0]["OPE_ENDDATE"] = 
							MaxDateTime(Convert.ToDateTime(row["OPE_ENDDATE"]), 
							Convert.ToDateTime(dt.Rows[0]["OPE_ENDDATE"]));

					// DURATION
					if (dt.Rows[0]["OPE_DURATION"] == DBNull.Value)
						dt.Rows[0]["OPE_DURATION"] = row["OPE_DURATION"];
					else if (row["OPE_DURATION"] != DBNull.Value)
						dt.Rows[0]["OPE_DURATION"] = 
							Convert.ToInt32(row["OPE_DURATION"])
							+ Convert.ToInt32(dt.Rows[0]["OPE_DURATION"]);

					// VALUE
					if (dt.Rows[0]["OPE_VALUE"] == DBNull.Value)
						dt.Rows[0]["OPE_VALUE"] = row["OPE_VALUE"];
					else if (row["OPE_VALUE"] != DBNull.Value)
						dt.Rows[0]["OPE_VALUE"] = 
							Convert.ToDouble(row["OPE_VALUE"])
							+ Convert.ToDouble(dt.Rows[0]["OPE_VALUE"]);

					row.Delete();
				}
			}
			// Leave only the first row and return
			//while (dt.Rows.Count > 2)
			//{
			//	dt.Rows[1].Delete();
			//}
			return dt;
		}

		private DateTime MaxDateTime(DateTime dt1, DateTime dt2)
		{
			if (DateTime.Compare(dt1, dt2) < 0) 
				return dt2;
			else 
				return dt1;
		}
		private DateTime MinDateTime(DateTime dt1, DateTime dt2)
		{
			if (DateTime.Compare(dt1, dt2) > 0) 
				return dt2;
			else 
				return dt1;
		}
	}
}
