using System;
using System.Text;
using System.Data;
using System.Collections;
using OPS.Components.Data;

using OTS.Framework.Collections;


namespace OPS.Components
{
	/// <summary>
	/// Summary description for CmpStatus.
	/// </summary>
	public class CmpStatus
	{

		public class StatusTreeItem
		{
			// ID of the Unit/Group/GroupType
			public readonly int Id;
			// ID of the GROUPS_DEF of the Unit/Group/GroupType
			public readonly int IdType;
			// PHY_ORDER of the GROUPS_DEF of the Unit/Group/GroupType
			public readonly int PhyOrder;
			
			private bool _isType;
			private bool _isUnit;

			/// <summary>
			/// Builds a new StatusTreeItem object
			/// </summary>
			/// <param name="id">ID of the element (use -1 for GROUPS_DEF elements)</param>
			/// <param name="idtype">ID of the GROUPS_DEF of that element</param>
			/// <param name="phyorder">PHY_ORDER of the current GROUPS_DEF of that element</param>
			/// <param name="istype">true if is a GROUPS_DEF element</param>
			/// <param name="isunit">true if is a UNIT element</param>
			public StatusTreeItem (int id, int idtype, int phyorder, bool istype, bool isunit)
			{
				Id = id; IdType = idtype; _isType = istype; _isUnit = isunit; PhyOrder = phyorder;
			}

			/// <summary>
			/// Gets if item is a GROUPS_DEF (a type of GROUP)
			/// </summary>
			public bool IsType  { get { return _isType; } }
			/// <summary>
			/// Gets if item is a UNIT
			/// </summary>
			public bool IsUnit  { get { return _isUnit; } }
			/// <summary>
			/// Gets if item is a specific sector or zone or another GROUP
			/// </summary>
			public bool IsGroup { get { return !_isType && !_isUnit; } }
			/// <summary>
			/// Gets if item is the special "Todos" element (can be Todos or <Todos> if IsType is also true)
			/// </summary>
			public bool IsTodos 
			{ 
				get { return  _isType ? (IdType == 0) : (Id ==0); } 
			}
		}
		public CmpStatus() {}

		// That method is used to sort the list obtained by the tree... The list will be
		// sorted by the PHY_ORDER of the StatusTreeItem contained in the TreeItem objects of the
		// tree
		private int CompareStatusTreeItems (UnorderedTree.TreeItem item1, UnorderedTree.TreeItem item2)
		{
			StatusTreeItem sitem1 = (StatusTreeItem)item1.Data;
			StatusTreeItem sitem2 = (StatusTreeItem)item2.Data;

			if ((sitem1.IsUnit && sitem2.IsUnit) && (sitem1.Id == sitem2.Id)) return 0;
			else if (sitem1.PhyOrder < sitem2.PhyOrder || sitem1.IsUnit) return -1;
			else if (sitem1.PhyOrder > sitem2.PhyOrder) return 1;
			else return 0;
		}

		/// <summary>
		/// Get the list of all parent (and parent types) of the group or unit specified by groupid.
		/// NOTE: Only physical groups are searched!
		/// </summary>
		/// <param name="groupId">ID of the group</param>
		/// <param name="isUnit">true if groupId is a ID of a UNIT instead a ID of a GROUP</param>
		/// <param name="tree">Out parameter containing the tree of GROUP with groupId and ALL HIS parents</param>
		/// <returns>An ArrayList of OTS.Framework.Collection.UnorderedTree::TreeItem objects.
		/// Each item of ArrayList contains a CmpStatus::StatusTreeItem object in its Data property</returns>
		
		public ArrayList GetUnitTree (int groupId, bool isUnit,	out UnorderedTree tree)
		{
			return GetUnitTree (groupId, isUnit, true, out tree);

		}
		/// <summary>
		/// Get the list of all parent (and parent types) of the group or unit specified by groupid
		/// </summary>
		/// <param name="groupId">ID of the group</param>
		/// <param name="isUnit">true if groupId is a ID of a UNIT instead a ID of a GROUP</param>
		/// <param name="searchPhyGroups">If true, only physical groups (DGRP_PHYORDER NOT NULL) will be searched</param>
		/// <param name="tree">Out parameter containing the tree of GROUP with groupId and ALL HIS parents</param>
		/// <returns>An ArrayList of OTS.Framework.Collection.UnorderedTree::TreeItem objects.
		/// Each item of ArrayList contains a CmpStatus::StatusTreeItem object in its Data property</returns>
		public ArrayList GetUnitTree (int groupId, bool isUnit, bool searchPhyGroups, out UnorderedTree tree)
		{
			tree= null;

			// Find the group type and physical order for the passed groupId
			int groupDefId = -1;
			int groupDefPhyorder = -1;
			CmpGroupsDefDB gddb = new CmpGroupsDefDB();
			DataTable dtgd = gddb.GetGroupDefByGroup(groupId);
			if (dtgd.Rows.Count > 0)
			{
				groupDefId = Convert.ToInt32(dtgd.Rows[0]["DGRP_ID"]);
				groupDefPhyorder = Convert.ToInt32(dtgd.Rows[0]["DGRP_PHYORDER"]);
			}

			StatusTreeItem item = new StatusTreeItem(groupId, groupDefId, groupDefPhyorder, false, isUnit);
			GetUnitTree (item, null, ref tree,searchPhyGroups);
			ArrayList list = tree.ToArrayList(new UnorderedTree.OrderItemDelegate(CompareStatusTreeItems));
			// At that point we have the list the groupId and ALL his parents. Now we have to put the GROUPS_DEF
			// items in the list (such as <sectores> or <zonas>,... (the list must have at least two elements: groupId and
			// his parent)
			StatusTreeItem itemAnt = null;
			StatusTreeItem itemAct = null;
			if (list.Count > 1)
			{
				itemAnt = (StatusTreeItem)((UnorderedTree.TreeItem)list[0]).Data;
				for (int i=1; i< list.Count;i++)
				{
					itemAct = (StatusTreeItem)((UnorderedTree.TreeItem)list[i]).Data;
					if (itemAnt.IdType != itemAct.IdType && !itemAnt.IsUnit)
					{
						// We must insert a new item AFTER itemAnt
						// (for example list[i-1] is a Zona and list[i] is a Sector, so we have to add
						// a <Zona> element in middle of both
						StatusTreeItem toInsert = new StatusTreeItem (-1, itemAnt.IdType,itemAnt.PhyOrder,true,false);
						// In order for consistency insert the StatusTreeItem in the tree. The item is inserted
						// as an Orphan item, because there is not really a parent-child relation...
						UnorderedTree.TreeItem titem =  tree.Add (toInsert);
						// For consistency again: Insert a UnorderedTree::TreeItem in the list
						// instead of a StatusTreeItem. Of course tha UnorderedTree::TreeItem inserted
						// will have only a StatusTreeItem object (in its data property)
						list.Insert (i, titem);
						i++;		// just skip the new element 
					}
					itemAnt = (StatusTreeItem)((UnorderedTree.TreeItem)list[i]).Data;
				}
			}
			// Add the final GROUPS_DEF item (the GROUPS_DEF item corresponding to the last element)
			item = (StatusTreeItem)((UnorderedTree.TreeItem)list[list.Count-1]).Data;
			if (item.IsGroup)			// item must be a group, nor a type or unit
			{
				StatusTreeItem toInsert =  new StatusTreeItem (-1, item.IdType,item.PhyOrder,true,false);
				// Insert the last item in the tree AND in the list
				UnorderedTree.TreeItem  titem = tree.Add (toInsert);
				list.Add ( titem);
			}

			return list;
		}



		/// <summary>
		/// Gets the parent of the ID specified
		/// </summary>
		/// <param name="itemToAdd">Item to add in the list</param>
		/// <param name="child">Child of the current item to add (NULL if no child - at the 1st call)</param>
		/// <param name="currentTree">Tree where to store the items</param>
		/// <param name="searchPhyGroups">If true, only physical groups (DGRP_PHYORDER NOT NULL) will be searched</param>
		protected void GetUnitTree (StatusTreeItem itemToAdd, OTS.Framework.Collections.UnorderedTree.TreeItem child,
									ref OTS.Framework.Collections.UnorderedTree currentTree, bool searchPhyGroups)
		{
			CmpGroupsChildsGisDB cmp = new CmpGroupsChildsGisDB();
			if (currentTree== null) 
			{
				currentTree = new OTS.Framework.Collections.UnorderedTree();
			}

			// Add the current element, find the parent
			OTS.Framework.Collections.UnorderedTree.TreeItem titem = currentTree.Add (itemToAdd);
			if (child != null) titem.AddChild (child);
			//currentList.Add (itemToAdd);
			
			DataTable dtParents =  cmp.GetParentsGroup (itemToAdd.Id, itemToAdd.IsUnit, searchPhyGroups);

			// If the parents exists ==> recursivity
			foreach (DataRow row in dtParents.Rows)
			{
				// NOTE: Although we pass _isUnit that parameter is not longer used (its only used in the 1st iteration)
				int parentId = Convert.ToInt32(row["CGRPG_ID"]);
				int dgrpid = Convert.ToInt32(row["DGRP_ID"]);
				int phyorder = Convert.ToInt32 (row["DGRP_PHYORDER"]);

				// Next item that will be added (in next recurisvity call).
				// Note that item is a GROUP (4th and 5th ctor parameters are false) nor a UNIT, nor a GROUPS_DEF because:
				//		a) We don't add GROUPS_DEF in that phase
				//		b) UNITs cannot be parents of anything!
				StatusTreeItem item = new StatusTreeItem (parentId,dgrpid,phyorder,false,false);
				GetUnitTree ( item, titem, ref currentTree, searchPhyGroups);
			}
		}



		/// <summary>
		/// Validates the status of the SISTEM.
		/// Foreach element of the list of parents, searches a record in table STATUS (filtering by DAY_ID or DDAY_ID).
		/// Returns when the 1st element of the table STATUS was found.
		/// </summary>
		/// <param name="statusTreeList">List of parents (aka Physical trees). Obtained by CmpStatus::GetUnitTree()</param>
		/// <param name="cmpDay">CmpDay with info about the Day AND DaysDef</param>
		/// <param name="timid">TIM_ID (timetable) for the status is searched</param>
		/// <returns>Status of the sistem (STA_DSTA_ID of the register found) or -1 if no register is found in STATUS</returns>
		public int ValidateStatus (ArrayList statusTreeList, CmpDay cmpDay, int timid)
		{
			CmpStatusDB cmpdb = new CmpStatusDB();
			string swhere0 = null;
			string swhere1 ="STA_TIM_ID=@STATUS.STA_TIM_ID@ ";
			string sformat = "SELECT STA_DSTA_ID from STATUS WHERE {0} and {1} and {2} and STA_DAY_ID = @STATUS.STA_DAY_ID@";
			DataTable dtStatus = null;
			object[] whereValues = new object[] { null, timid, cmpDay.Id};			// null will be filled later
			bool compareByDayId = cmpDay.Count > 0;
			StringBuilder sb = null;
			string [] fields = new string[] {"STA_DSTA_ID", "STA_DDAY_ID"};


			foreach (object o in statusTreeList)
			{
				sb = new StringBuilder();
				StatusTreeItem item = ((StatusTreeItem)((UnorderedTree.TreeItem)o).Data);

				// Builds specific sql string depending upon item is unit, or group or group_type
				if (item.IsUnit)
				{
					swhere0 = "STA_UNI_ID=@STATUS.STA_UNI_ID@";
					whereValues[0] = item.Id;
				}
				else if (item.IsGroup)
				{
					swhere0 = "STA_GRP_ID=@STATUS.STA_GRP_ID@";
					whereValues[0] = item.Id;
				}
				else	// item.IsType
				{
					swhere0 = "STA_DGRP_ID=@STATUS.STA_DGRP_ID@";
					whereValues[0] = item.IdType;
				}

				if (compareByDayId)
				{
					sb.AppendFormat (sformat, swhere0, swhere1, "STA_DAY_ID", "STA_DAY_ID");

					// Get rows from STATUS for current unit/group/group_day, timetable and DAY
					dtStatus = cmpdb.GetData (sb.ToString(), whereValues);
					if (dtStatus.Rows.Count >1)
					{
						// Log the incoherence and return the 1st register
						// TODO: Log
						return Convert.ToInt32 (dtStatus.Rows[0]["STA_DSTA_ID"]);
					}
					else if (dtStatus.Rows.Count == 1) 
					{
						// We have found it.
						return Convert.ToInt32 (dtStatus.Rows[0]["STA_DSTA_ID"]);
					}
				}
				// If we reached that point means one of the following:
				//		No STATUS register found when searching by day_id
				//		There was no DAY corresponding to the currend date
				// in  both cases we have to do the same thing: search by dday_id
				sb.AppendFormat ("{0} AND {1}", swhere0, swhere1);
				dtStatus = cmpdb.GetData (fields, sb.ToString(), whereValues);			// Although whereValues has 3 values, only 2 first will be used
				// In that point we have all STATUS registers that are for current unit/group/group_type and
				// for all days/days_def. Check if one of the registers are contained in DAYS_DEF of current day
				foreach (DataRow dr in dtStatus.Rows)
				{
					if (dr["STA_DDAY_ID"]!=DBNull.Value)
					{
						int daydef = Convert.ToInt32(dr["STA_DDAY_ID"]);
						if (cmpDay.DaysDef.ContainsId(daydef)) 
						{
							// We have found the register.
							return Convert.ToInt32(dr["STA_DSTA_ID"]);
						}
					}
				}
				// No register was found, searching for day or day_def for the current item... go to next item
			}

			// We have no found any register...
			return -1;
		}

		/// <summary>
		/// Retrieves all the hierarchichal tree information from a determined Group, to get all the nodes from down to up.
		/// </summary>
		/// <param name="groupId">The node to search</param>
		/// <param name="groupType">The type (Unit,Sector,Zone) to search</param>
		/// <returns>An object with pairs of values, indicating identifier and type of agrupation</returns>
		public object[] GetUnitTree(string groupId, string groupType)
		{

			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			string GROUP_TYPE = (string)appSettings.GetValue("Groups.GroupType", typeof(string));

			// Vars to use, with a max of 7 objects ( unit, sector, <sector>, zone, <zone>, all, <all>)
			object[] obvalues	= new object[7];
			int obvaluesindex	= 0;
			// The level is defined as 0,1. This is the maxvalue
			int level			= Int32.MaxValue;
			int level2			= Int32.MaxValue;

			// The object to Return
			obvalues[obvaluesindex] = new object[] {groupId, groupType};
            obvaluesindex ++;

			// Getting Status from Elements, variable to control if there are parent nodes
			bool fi_recursiu = false;

			while (!fi_recursiu)
			{
				// Search the parent of current node
				string swhere = "CGRPG_CHILD = @GROUPS_CHILDS_GIS.CGRPG_CHILD@ AND CGRPG_TYPE = @GROUPS_CHILDS_GIS.CGRPG_TYPE@ AND CGRPG_ORDER > @GROUPS_CHILDS_GIS.CGRPG_ORDER@";
				DataTable dtElement	= new OPS.Components.Data.CmpGroupsChildsGisDB().GetData(null,swhere,null, new object[] {groupId, groupType, 0});
				if (dtElement.Rows.Count > 0)
				{
					level = Int32.Parse(dtElement.Rows[0]["CGRPG_ORDER"].ToString());
					// To prevent infinite loops every loop the number must be smaller.
					if (level >= level2) 
					{ 
						fi_recursiu = true;
					}
					level2	= level;

					string sParentGroup		= dtElement.Rows[0]["CGRPG_ID"].ToString();
					swhere					= "GRP_ID ='" + sParentGroup + "'";
					DataTable dtInfo		= new OPS.Components.Data.CmpGroupsDB().GetData(null,swhere,null,null );
					if (dtInfo.Rows.Count > 0)
					{
						groupId		= dtInfo.Rows[0]["GRP_ID"].ToString();
						groupType	= dtInfo.Rows[0]["GRP_DGRP_ID"].ToString();
						groupType	= GROUP_TYPE;
						obvalues[obvaluesindex] = new object[] {groupId, groupType};
						obvaluesindex ++;
					}
					else { fi_recursiu = true; }
				}
				else { fi_recursiu = true; }
			}
			return obvalues;
		}

		/// <summary>
		/// Retrieves all the hierarchichal tree information from a determined Group, to get all the nodes from down to up.
		/// </summary>
		/// <param name="list">Object[] with a list of strings [{string_node,string_node_type},{string_node,string_node_type},..]</param>
		/// <returns>A DataSet with all the Status of the given List</returns>
		public DataTable GetStatusFromList(object[] list)
		{
			// "G" Denotes the type of group, we put as a Application Setting to avoid HardCoded Items
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			string GROUP_TYPE = (string)appSettings.GetValue("Groups.GroupType", typeof(string));

			string swhere	= "(STA_GRP_ID = -1";
			foreach (object[] s in list)
			{
				if (s != null)
				{
					foreach (string ss in s)
					{
						if (ss != GROUP_TYPE) 
						{
							swhere	= swhere + " OR STA_GRP_ID ='" + ss + "'";
						}
					}
				}
			}
			swhere	= swhere + ")";

			DataTable dt	= new OPS.Components.Data.CmpStatusDB().GetData(null,swhere,null,null);
			return dt;
		}


	}
}
