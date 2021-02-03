using System;
using System.Collections;
using System.Data;
using OPS.Components.Data;

using OTS.Framework.Collections;

namespace OPS.Components
{
	/// <summary>
	/// Summary description for CmpConstraints.
	/// </summary>
	public class CmpConstraints
	{
		private ArrayList _lstGroups;
		private UnorderedTree _treeGroups;
		private int _conid;
		
		public enum ValidateTRReturnCodes { OperationNonValid = -1, Extension = 2, OnlyOk = 0}

		/// <summary>
		/// Constructs a new CmpConstraints
		/// </summary>
		/// <param name="constraintId">ID of the constraint to check</param>
		/// <param name="groups">ArrayList of all groups (sorted by PHY_ORDER)</param>
		/// <param name="treeGroups">Tree of all groups. The parameter group MUST be obtained by calling ToArrayList() method
		/// of that parameter.</param>
		public CmpConstraints(int constraintId, ArrayList groups, UnorderedTree treeGroups)
		{
			_conid = constraintId;
			_lstGroups = groups;			// ArrayList of UnorderedTree::TreeItem with CmpStatus::StatusTreeItem objects in Data property
			_treeGroups = treeGroups;		// Tree from which _lstGroups was obtained.
		}

		#region public methods

		/// <summary>
		/// Get all the values of:
		///		One Constraint
		///			for
		///		A list of group/group types
		/// </summary>
		/// <param name="tipoRestricciones">Array of 5 int with the values of table CON_DEF.
		/// tipoRestricciones[0] must be the DCON_ID of MAX_ESTANCIA (o MAX_PERMANENCIA).
		/// tipoRestricciones[1] must be the DCON_ID of MIN_REENTRADA.
		/// tipoRestricciones[2] must be the DCON_ID of TIEMPO_CORTESIA.
		/// tipoRestricciones[3] must be the DCON_ID of MAX_IMPORT.
		/// tipoRestricciones[4] must be the DCON_ID of MIN_IMPORT.
		/// </param>
		/// <returns>DataTable with all constraints for a con_id and a list of group/group_types</returns>
		public DataTable GetConstraints(int[] tipoRestricciones)
		{

			// Builds the DataTable that we will return
			DataTable dtReturn = new DataTable();
			dtReturn.Columns.Add ("Agrupacion",typeof(int));			// ID del grupo (GRP_ID o DGRP_ID)
			dtReturn.Columns.Add ("TipoAgrupacion",typeof(bool));		// Indica si el campo anterior es DGRP_ID (true) o GRP_ID
			dtReturn.Columns.Add ("MP",typeof(int));					// Tiempo maximo de permanencia
			dtReturn.Columns.Add ("DGRP_ID",typeof(int));				// DGRP_ID de la agrupación (tanto so es group o group_type).
			dtReturn.Columns.Add ("CalculoMP",typeof(int));			//MP Calculado para la agrupación y sus hijos
			dtReturn.Columns.Add ("TR",typeof(int));					// Tiempo de reentrada
			dtReturn.Columns.Add ("TC",typeof(int));					// Tiempo de cortesia
			dtReturn.Columns.Add ("PagoTC",typeof(bool));				// Si TC se paga o no
			dtReturn.Columns.Add ("Imax",typeof(double));					// Importe max
			dtReturn.Columns.Add ("Imin",typeof(double));					// Importe min

			// Get ALL the constraints from the current CON_ID
			CmpConstraintsDB cmp = new CmpConstraintsDB();
			DataTable dt = cmp.GetData (null, "CON_ID = @CONSTRAINTS.CON_ID@",new object[] {_conid});
			// Filter and stores only the constraints that exists in groups
			foreach (object o in _lstGroups)
			{
				CmpStatus.StatusTreeItem item = (CmpStatus.StatusTreeItem)((UnorderedTree.TreeItem)o).Data;
				string sfilter = null;
				if (item.IsUnit) continue;
				else if (item.IsGroup) 
				{
					sfilter = "CON_GRP_ID=" + item.Id;
				}
				else		// item.IsType
				{
					sfilter = "CON_DGRP_ID=" + item.IdType;
				}
				// Get all the constraints to be applied to that group or group type.
				// Will add a single row containing all the constraints.
				DataRow[] rows = dt.Select (sfilter);
				if (rows.Length > 0)
				{
					DataRow rowToAdd = dtReturn.NewRow();
					rowToAdd["TipoAgrupacion"] = item.IsType;
					rowToAdd["Agrupacion"] = item.IsType ? item.IdType : item.Id;
					rowToAdd["DGRP_ID"] = item.IdType;
					rowToAdd["CalculoMP"] = DBNull.Value;
					foreach (DataRow row in rows)
					{
						// foereach constraint add it in the DataTable that we have to return...
						rowToAdd["PagoTC"] =  false;			// TODO: Read from row object (is not in BBDD now)
						int tipoRestriccion = Convert.ToInt32(row["CON_DCON_ID"]);
						string scol = null;
						if (tipoRestriccion == tipoRestricciones[0]) { scol = "MP";}				// MAX_PERMANENCIA
						else if (tipoRestriccion == tipoRestricciones[1]) { scol = "TR";}			// TIEMPO_REENTRADA
						else if (tipoRestriccion == tipoRestricciones[2]) { scol = "TC";}			// TIEMPO_CORTESIA
						else if (tipoRestriccion == tipoRestricciones[3]) { scol = "Imax";}		// MAX_IMPORT
						else if (tipoRestriccion == tipoRestricciones[4]) { scol = "Imin";}		// MIN_IMPORT

						if (scol!=null) rowToAdd[scol] = row["CON_VALUE"];
					}
					dtReturn.Rows.Add (rowToAdd);
				}
			}
			dtReturn.AcceptChanges();
			return dtReturn;
		}

		/// <summary>
		/// Filters the DataTable of constraints passed.
		/// "Propagates" all constraints of the groups_type to all his childs and delete all groups_types from
		/// the DataTable.
		/// As a result the DataTable only contains groups and not groups_types.
		/// One coherence rule asserts that only one physical treee will be referenced in the data. That means
		/// only one group for a group_type can appear (i.e. if we have a <zona> in the DataTable only one group
		/// with DGRP_ID of <zona> can appear. Otherwise the database is inconsistent).
		/// </summary>
		/// <param name="dtConstraints">DataTable to filter. The method modifies it</param>
		/// <returns>false if some error or incoherence was detected. true otherwise</returns>
		public bool FilterConstraintsTable (DataTable dtConstraints)
		{
			// Get ALL group_type of the DataTable
			DataRow[] dgtypes = dtConstraints.Select("TipoAgrupacion=true");
			foreach (DataRow drtype in dgtypes)
			{
				int dgrpid = Convert.ToInt32 (drtype["DGRP_ID"]);
				DataRow[] dgroup = dtConstraints.Select ("TipoAgrupacion=false AND DGRP_ID=" + dgrpid);
				if (dgroup!=null && dgroup.Length > 0) 
				{
					if (dgroup.Length > 1) 
					{
						return false;				// Inconsistent: more than one group for group_type
					}
					DataRow dr = dgroup[0];
					PropagateConstraints (drtype, dr);
				}
			}
			// At that point all constraints are propagated. Now we can delete group_type rows from the DataTable
			foreach (DataRow dr in dgtypes) { dtConstraints.Rows.Remove(dr); }
			// Finally check the last consistency rule: Only one  non-null value for Imax and Imin can appear.
			int imax = dtConstraints.Rows[0]["Imax"] !=DBNull.Value ? Convert.ToInt32(dtConstraints.Rows[0]["Imax"]):-1;
			int imin = dtConstraints.Rows[0]["Imin"] !=DBNull.Value ? Convert.ToInt32(dtConstraints.Rows[0]["Imin"]):-1;
			// Iterate over all rows...
			for (int i=1; i< dtConstraints.Rows.Count;i++)
			{
				// Get the imax and imin
				object omax = dtConstraints.Rows[i]["Imax"];
				object omin = dtConstraints.Rows[i]["Imin"];
				if (omax != DBNull.Value)
				{
					// If omax is not null compare it with one previous not-null value (imax).
					// (note that if imax were null, store omax in imax, since is the 1st not null value that we found)
					if (imax == -1) { imax = Convert.ToInt32 (omax); }
					else if (imax != Convert.ToInt32(omax)) { return false; }
				}
				if (omin != DBNull.Value) 
				{
					// We do for omin, exactly the same that we did for omax...
					if (imin == -1) { imin = Convert.ToInt32 (omin); }
					else if (imin != Convert.ToInt32(omin)) { return false; }
				}
			}
			// Constraints were propagated and no incoherence were detected...
			return true;
		}

		/// <summary>
		/// Find the only non-null value of Imax or Imin
		/// </summary>
		/// <param name="dtConstraints">Constraints Table filtered (after CmpConstraints::FilterConstraintsTable)</param>
		/// <param name="bMax">If true will find Imax, otherwise Imin will be returned</param>
		/// <returns>Value of Imax or Imin</returns>
		public double FindImport (DataTable dtConstraints, bool bMax)
		{
			string sfield = bMax ? "Imax": "Imin";
			foreach (DataRow dr in dtConstraints.Rows)
			{
				if (dr[sfield]!=DBNull.Value) return Convert.ToDouble(dr[sfield]);
			}
			return -1.0;			// Not found (was NULL)!
		}

		/// <summary>
		/// Validates the TR (tiempo de reentrada) constraints for a given table of constraints and groups
		/// That method is called after get the constraints (CmpConstraints::GetConstraints) and
		/// after filtering the constraints obtained (CmpConstraints::FilterConstraintsTable)
		/// </summary>
		/// <param name="dtConstraints">DataTable with constraints and groups (not groups_types) to process.</param>
		/// <param name="pInDate">DateTime when te operation was done (usually MOV_OPDATE of M01 message)</param>
		/// <param name="artid">ID of the ARTICLE (-1 for NULL)</param>
		/// <param name="vehicleid">ID of the VEHICLE (CANNOT be null)</param>
		/// <param name="operationId">OUT parameter with value of DATE of previous Operation (only used if return code is PreviousDate)</param>
		/// <returns>A ValidateTRReturnCodes specifying the result of validation (non valid, ok with previous date, ok)</returns>
		public ValidateTRReturnCodes ValidateTR(DataTable dtConstraints, DateTime pInDate, 
			int artid, string vehicleid, out int operationId)
		{
			//previousDate = DateTime.MinValue;
			operationId = -1;
			CmpOperationsDB cmp = new CmpOperationsDB();
			foreach (DataRow group in dtConstraints.Rows)
			{
				int tr = group["TR"] != DBNull.Value ? Convert.ToInt32 (group["TR"]) : 0;
				int tc = group["TC"] != DBNull.Value ? Convert.ToInt32 (group["TC"]) : 0;
				DateTime opend = pInDate.Subtract(new TimeSpan (0,tr,0));
				DataTable dtOperations;
				if (artid != -1)
				{
					dtOperations = cmp.GetAllData(null, "OPE_ENDDATE >= @OPERATIONS.OPE_ENDDATE@ "
						+ "AND OPE_VEHICLEID = @OPERATIONS.OPE_VEHICLEID@ "
						+ "AND OPE_ART_ID = @OPERATIONS.OPE_ART_ID@",
						"OPE_ENDDATE DESC", new object[] {opend, vehicleid, artid} );
				}
				else
				{
					dtOperations = cmp.GetAllData(null, "OPE_ENDDATE >= @OPERATIONS.OPE_ENDDATE@ "
						+ "AND OPE_VEHICLEID = @OPERATIONS.OPE_VEHICLEID@",
						"OPE_ENDDATE DESC", new object[] {opend, vehicleid} );
				}
				if (dtOperations.Rows.Count > 0)
				{
					// Ok.. At that point we have:
					//		ALL operations of the current vehicle and current article for ALL zones in the TR period of time.
					//		We must filter that table for having only the operations for the CURRENT zone and ALL HIS CHILDS.
					//		The rest of operations are discarded at that point (but, of course, we could find them again when
					//		processing another zone
					UnorderedTree.TreeItem titem = _treeGroups.FindItem (new UnorderedTree.FindItemDelegate (FindItemById), group);
					// Delete from dtOperations table ALL operations that are not associated by titem or any childs of titem.
					FilterOperationsTableByGroup (dtOperations, titem);
				}
				if (dtOperations.Rows.Count > 0)
				{
//					return ValidateTRReturnCodes.OperationNonValid;
					// At that point we have only the operations related with the current group (or any of his childs).
					DataRow operation = dtOperations.Rows[0];
					DateTime opendtc = Convert.ToDateTime(operation["OPE_ENDDATE"]).AddMinutes(tc);
					if (pInDate > opendtc)
					{
						// Operation not valid. At the first non-valid operation we return
						return ValidateTRReturnCodes.OperationNonValid;
					}
					else // An ampliation of previous operation is possible
					{
						// Find the group of the current operation
						int currentGroup = -1;
						foreach (object o in _lstGroups)
						{
							CmpStatus.StatusTreeItem item = (CmpStatus.StatusTreeItem)((UnorderedTree.TreeItem)o).Data;
							if (item.IsUnit) continue;
							else if (item.IsGroup) 
							{
								currentGroup = item.Id;
								break;
							}
						}

						// Let's check if the groups are the same.
						// If not, exit as an extension is not possible.
						int operationGroup = Convert.ToInt32(operation["OPE_GRP_ID"]);
						if (currentGroup != operationGroup)
						{
							return ValidateTRReturnCodes.OperationNonValid;
						}

						operationId = Convert.ToInt32(operation["OPE_ID"]);
						return ValidateTRReturnCodes.Extension;
						/////////////////////////////////////////////////////////
						//int groupid = Convert.ToInt32 (group["Agrupacion"]);
						//if (opeid == groupid && Convert.ToBoolean (group["PagoTC"])) 
						//{
						//	// An ampliation is allowed.
						//	previousDate = Convert.ToDateTime (operation["OPE_ENDDATE"]);
						//	return ValidateTRReturnCodes.PreviousDate;
						//}
					}
				}
			}
			return ValidateTRReturnCodes.OnlyOk;
		}

		/// <summary>
		/// Computes the minimum MP.
		/// Foreach group in the table of groups:
		///		Get all operations of the group (for article and vehicle passed)
		///		Sums all OPE_DURATION times
		///		Set CalculoMP of the group to the value MP - Sum(OPE_DURATION)
		/// The dtConstraints DataTable is modified (CalculoMP field is set).
		/// </summary>
		/// <param name="dtConstraints">DataTable of constraints (obtained with CmpConstraints::GetConstraints())</param>
		/// <param name="artid">ID of the article (use -1 for NULL)</param>
		/// <param name="vehid">ID of the vehicle (cannot be NULL)</param>
		/// <param name="pInDate">DateTime of the new intended operation (OPE_INIDATE or OPE_MOVDATE if passed in the message).</param>
		public void CalculateMP (DataTable dtConstraints, DateTime pInDate,int artid, string vehid)
		{
			CmpOperationsDB cmp = new CmpOperationsDB();
			_treeGroups.EvalHandler = new OTS.Framework.Collections.UnorderedTree.EvalItem(TreeItemToId);
			foreach (DataRow drGroup in dtConstraints.Rows)
			{
				// Can use Agrupación, we don't have GROUP_TYPEs at that step
				// (were all removed in FilterConstraintsTable)
				int group = Convert.ToInt32(drGroup["Agrupacion"]);			
				UnorderedTree.TreeItem titem = _treeGroups.FindItem(new UnorderedTree.FindItemDelegate (FindItemById), drGroup);
				// Get an arraylist with the IDs of all descendants of the current group.
				ArrayList groupChilds = titem.MapcarDescendants ();
				// Get all operations.
				int mp = drGroup["MP"] != DBNull.Value ? Convert.ToInt32(drGroup["MP"]) : 0;
				DateTime opend = pInDate;
				if (mp > 0) { opend = opend.Subtract(new TimeSpan (0,mp,0)); }
				DataTable dtOperations = cmp.GetData(null, "OPE_ENDDATE > @OPERATIONS.OPE_ENDDATE@ AND OPE_ART_ID = @OPERATIONS.OPE_ART_ID@ AND OPE_VEHICLEID = @OPERATIONS.OPE_VEHICLEID@",
					"OPE_ENDDATE DESC", new object[] {opend, artid!=-1 ? (object)artid : (object)DBNull.Value, vehid } );
				// Iterates throught the operations DataTable. We will exit when we found an OPERATION that:
				//	a) Were not related to the group we are processing (group variable)
				//	b) AND were not related to any of the group_childs of the group...
				int sumOpeDuration = 0;
				foreach (DataRow drOperation in dtOperations.Rows)
				{
					int opeGroup = Convert.ToInt32(drOperation["OPE_GRP_ID"]);
					// Check if opeGroup (group of the operation) is the group we are processing (group) or any
					// descendant of the group we are processing..
					if (!groupChilds.Contains(opeGroup)) break;		// finish the iteration over operations table
					sumOpeDuration += Convert.ToInt32(drOperation["OPE_DURATION"]);
				}
				// At that point we have the sumOpeDuration for the current group, so store it in the CalculoMP row
				drGroup["CalculoMP"] = mp-sumOpeDuration;
			}
			// Now we have "CalculoMP" computed by all operations.
			_treeGroups.EvalHandler = null;
		}

		#endregion

		#region protected methods
		/// <summary>
		/// "Propagate" the constraints from the group_type to a group
		/// </summary>
		/// <param name="grouptype"></param>
		/// <param name="group"></param>
		protected void PropagateConstraints (DataRow grouptype, DataRow group)
		{
			// 1: Set the minimum MP at group
			int valueg = group["MP"]!= DBNull.Value ? Convert.ToInt32(group["MP"]): Int32.MaxValue;
			int valuegt = grouptype["MP"] != DBNull.Value ? Convert.ToInt32(grouptype["MP"]): Int32.MaxValue;
			valueg = Math.Min(valueg, valuegt);
			group["MP"] = valueg!=Int32.MaxValue ? (object)valueg:(object)DBNull.Value;
			// 2. Set the minimum TR at group
			valueg = group["TR"]!= DBNull.Value ? Convert.ToInt32(group["TR"]):Int32.MaxValue;
			valuegt = grouptype["TR"] != DBNull.Value ? Convert.ToInt32(grouptype["TR"]): Int32.MaxValue;
			valueg = Math.Min(valueg, valuegt);
			group["TR"] = valueg!=Int32.MaxValue ? (object)valueg:(object)DBNull.Value;
			//3: Set the minimum TC at group
			valueg = group["TC"]!= DBNull.Value ? Convert.ToInt32(group["TC"]): Int32.MaxValue;
			valuegt = grouptype["TC"] != DBNull.Value ? Convert.ToInt32(grouptype["TC"]): Int32.MaxValue;
			valueg = Math.Min(valueg, valuegt);
			group["TC"] = valueg!=Int32.MaxValue ? (object)valueg:(object)DBNull.Value;
			// 4: Propagate pagoTC 
			if (group["PagoTC"] == DBNull.Value)
			{
				group["PagoTC"] =  grouptype["PagoTC"];
			}
			// 5: Set the minimum at Imax
			valueg = group["Imax"]!= DBNull.Value ? Convert.ToInt32(group["Imax"]): Int32.MaxValue;
			valuegt = grouptype["Imax"] != DBNull.Value ? Convert.ToInt32(grouptype["Imax"]): Int32.MaxValue;
			valueg = Math.Min(valueg, valuegt);
			group["Imax"] = valueg!=Int32.MaxValue? (object)valueg:(object)DBNull.Value;
			// 6: Set the minimum at Imin
			valueg = group["Imin"]!= DBNull.Value ? Convert.ToInt32(group["Imin"]): Int32.MaxValue;
			valuegt = grouptype["Imin"] != DBNull.Value ? Convert.ToInt32(grouptype["Imin"]): Int32.MaxValue;
			valueg = Math.Min(valueg, valuegt);
			group["Imin"] = valueg!=Int32.MaxValue ? (object)valueg:(object)DBNull.Value;
			// Done... all constraints were propagated
		}

		/// <summary>
		/// Function used to Find a TreeItem with the specified id
		/// </summary>
		/// <param name="item">Item to check if has the correct id</param>
		/// <param name="data">DataRow containing the value of the group (contains id, dgrp_id, and so on).</param>
		/// <returns>true of item has the correctid</returns>
		protected bool FindItemById (UnorderedTree.TreeItem item, object data)
		{
			CmpStatus.StatusTreeItem stitem = (CmpStatus.StatusTreeItem)item.Data;
			DataRow group = (DataRow)data;
			bool bType = Convert.ToBoolean (group["TipoAgrupacion"]);
			int id = Convert.ToInt32 (group["Agrupacion"]);
			bool bRet = bType ? stitem.IdType ==id : stitem.Id == id;
			return bRet;
		}


		/// <summary>
		/// Filters the OPERATIONS DataTable
		/// </summary>
		/// <param name="dtOperations">DataTable to filter</param>
		/// <param name="titem">Item which represents the group filtered by</param>
		protected void FilterOperationsTableByGroup (DataTable dtOperations, UnorderedTree.TreeItem titem)
		{
			
			if (dtOperations.Columns["TMP_MANTAIN"] == null)
			{
				DataColumn dc =  dtOperations.Columns.Add ("TMP_MANTAIN", typeof (bool));
			}
			DoFilterOperationsTableByGroup (dtOperations, titem);
			// Remove all rows that have TMP_MANTAIN distinct of true
			foreach (DataRow dr in dtOperations.Rows)
			{
				if (dr["TMP_MANTAIN"]==DBNull.Value || Convert.ToBoolean(dr["TMP_MANTAIN"])!=true)
				{
					dr.Delete();
				}
			}
			// Remove the temporal column used...
			dtOperations.Columns.Remove("TMP_MANTAIN");
			dtOperations.AcceptChanges();
		}
		/// <summary>
		/// Marks with true all the rows of their OPE_ID is equal to the ID of titem.
		/// Do a recursive call to check all childs of the titem.
		/// </summary>
		/// <param name="dtOperations">DataTable to filter</param>
		/// <param name="titem">UnorderedTree::TreeItem which the group to filter by.</param>
		protected void DoFilterOperationsTableByGroup (DataTable dtOperations, UnorderedTree.TreeItem titem)
		{
			// Filter by the childs of stitem.
			if (titem.ChildsCount > 0) 
			{
				foreach (object o in titem.Childs)
				{
					//FilterOperationsTableByGroup (dtOperations, (UnorderedTree.TreeItem)o);
					DoFilterOperationsTableByGroup(dtOperations, (UnorderedTree.TreeItem)o);
				}
			}
			// Filter by stitem...
			foreach (DataRow dr in dtOperations.Rows)
			{
				int grpid = Convert.ToInt32(dr["OPE_GRP_ID"]);
				if (grpid == ((CmpStatus.StatusTreeItem)titem.Data).Id) 
				{
					dr["TMP_MANTAIN"] = true;
				}
			}	
		}


		#endregion

		#region private helper methods

		/// <summary>
		/// Method used to evaluate a TreeItem, and returning the id of the group contained by that item.
		/// </summary>
		/// <param name="item">Item to evaluate</param>
		/// <returns>Int32 with the Id or null if item is not a group (is Unit or Type)</returns>
		private object TreeItemToId (UnorderedTree.TreeItem item)
		{
			CmpStatus.StatusTreeItem stitem = (CmpStatus.StatusTreeItem)item.Data;
			return stitem.IsGroup ? (object)stitem.Id : (object)null;
		}

		#endregion
	}
}
