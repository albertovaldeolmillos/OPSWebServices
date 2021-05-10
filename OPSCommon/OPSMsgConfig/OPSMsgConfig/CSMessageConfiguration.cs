using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using OPS.Components.Data;


namespace OPS.Comm.Configuration
{
	/// <summary>
	/// Summary description for CSMessageConfiguration.
	/// </summary>
	public class CSMessageConfiguration : MessageConfiguration
	{
		#region Public API

		public CSMessageConfiguration()
		{
			_msgCfgCache = new Hashtable(25);
			_maxPacketMessages = 10;
		}
		public override PriorityDestinationGroupList GetPriorityGroups()
		{
			PriorityDestinationGroupList list = new PriorityDestinationGroupList();
			IDataReader rd = null;
			try
			{
				Database db = DatabaseFactory.GetDatabase();
				rd = db.ExecQuery(_selectPriorityDest, _connection, null);
				while (rd.Read())
				{
					PriorityDestinationGroup g = new PriorityDestinationGroup(
						rd.GetDecimal(0), rd.GetDecimal(1));
					list.Add(g);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("CSMessageConfiguration.GetPriorityGroups error - " + ex.Message);
			}
			finally
			{
				if (rd != null)
					rd.Close();
			}

			return list;
		}
		public override DataSet GetPriorityGroupMessages(PriorityDestinationGroup g)
		{
			DataSet retVal = null;
			try
			{
				Database db = DatabaseFactory.GetDatabase();
				retVal = db.FillDataSet(_selectMsgsByPriorityDest, "MSGS", g.Priority, g.Destination);
				int rowCount = retVal.Tables[0].Rows.Count;
				if (rowCount > _maxPacketMessages)
				{
					for (int i = _maxPacketMessages; i < rowCount; i++)
					{
						retVal.Tables[0].Rows[i].Delete();
					}
					retVal.AcceptChanges();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("CSMessageConfiguration.GetPriorityGroupMessages error - " + ex.Message);
				Debug.WriteLine(ex.StackTrace);
			}
			return retVal;
		}
		public int MaxPacketMessages
		{
			get { return _maxPacketMessages; }
			set { _maxPacketMessages = value; }
		}

		#endregion // Public API

		#region Private methods
		
		protected override DataSet LoadConfiguration(string msgTag, 
			decimal srcUnitId, decimal dstUnitId)
		{
			DataSet retVal = null;
			IDbDataAdapter da = null;
			IDbCommand cmd = null;
			try
			{
				Database db = DatabaseFactory.GetDatabase();
				cmd = db.PrepareCommand(_selectMsgCfgCmdText, msgTag, srcUnitId, dstUnitId);
				da = db.GetDataAdapter();
				da.SelectCommand = cmd;
				retVal = new DataSet();
				int rowCount = da.Fill(retVal);
				if (rowCount == 0)
					retVal = null;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("CSMessageConfiguration.LoadConfiguration error - " + ex.Message);
			}
			return retVal;
		}

		#endregion // Private methods
		
		#region Private data members

		private string _selectMsgCfgCmdText = 
			"SELECT MSGS_CONFIG.CMSG_DESCSHORT AS CMSG_DESCSHORT, MSGS_DEF.DMSG_DESCSHORT AS DMSG_DESCSHORT, " +
			"MSGS_DEF.DMSG_PROCESSTIME AS DMSG_PROCESSTIME, MSGS_DEF.DMSG_USERTIME AS DMSG_USERTIME, " +
			"MSGS_DEF.DMSG_ID AS MSG_DMSG_ID, MSGS_DEF.DMSG_HISMANDATORY AS MSG_HISMANDATORY, " +
			"MSGS_DEF.DMSG_PRIORITY AS MSG_PRIORITY, MSGS_RULES.RMSG_MMSG_ID AS MSG_MMSG_ID, " +
			"MSGS_RULES.RMSG_MANDATORY AS MSG_MANDATORY, MSGS_RULES.RMSG_PRIORITY AS MSG_ORDER, " + 
			"MSGS_RULES.RMSG_IPADAPTER AS MSG_IPADAPTER, MSGS_RULES.RMSG_PORTADAPTER AS MSG_PORTADAPTER, " +
			"MSGS_RETRIES_DEF.DRMSG_TOTALRETRIES AS MSG_TOTALRETRIES, " +
			"MSGS_RETRIES_DEF.DRMSG_PARCIALRETRIES AS MSG_PARCIALRETRIES, " +
			"MSGS_RETRIES_DEF.DRMSG_TOTALINTERVAL AS MSG_TOTALINTERVAL, " +
			"MSGS_RETRIES_DEF.DRMSG_PARCIALINTERVAL AS MSG_PARCIALINTERVAL, " +
			"MSGS_RETRIES_DEF.DRMSG_TOTALTIME AS MSG_TOTALTIME, " +
			"DEST_UNIT.DLUNI_ID AS MSG_UNI_ID, DEST_UNIT.DLUNI_PORT AS MSG_UNI_PORT, " +
			"MSGS_FMT.FMSG_XSL AS MSG_XSL " +
			"FROM MSGS_CONFIG, MSGS_DEF, MSGS_RULES, MSGS_RETRIES_DEF, MSGS_FMT, " +
			"UNITS_LOG_DEF SRC_UNIT, UNITS_LOG_DEF DEST_UNIT " +
			"WHERE MSGS_CONFIG.CMSG_DESCSHORT = @MSGS_CONFIG.CMSG_DESCSHORT@ AND " +
			"MSGS_RULES.RMSG_DLUNI_SOURCE = @MSGS_RULES.RMSG_DLUNI_SOURCE@ AND " +
			"MSGS_RULES.RMSG_DLUNI_DESTINATION = @MSGS_RULES.RMSG_DLUNI_DESTINATION@ AND " +
			"MSGS_CONFIG.CMSG_DMSG_ID = MSGS_DEF.DMSG_ID AND " +
			"MSGS_RULES.RMSG_DMSG_ID = MSGS_DEF.DMSG_ID  AND " +
			"MSGS_RETRIES_DEF.DRMSG_DMSG_ID = MSGS_DEF.DMSG_ID AND " +
			"MSGS_RULES.RMSG_MMSG_ID = MSGS_RETRIES_DEF.DRMSG_MMSG_ID AND " +
			"MSGS_RULES.RMSG_DLUNI_SOURCE = SRC_UNIT.DLUNI_ID AND " +
			"MSGS_RULES.RMSG_DLUNI_DESTINATION = DEST_UNIT.DLUNI_ID AND " +
			"MSGS_RULES.RMSG_FMSG_ID = MSGS_FMT.FMSG_ID AND " +
			"MSGS_DEF.DMSG_VALID = 1 AND MSGS_DEF.DMSG_DELETED = 0 AND " +
			"MSGS_RULES.RMSG_VALID = 1 AND MSGS_RULES.RMSG_DELETED = 0 AND " +
			"MSGS_RETRIES_DEF.DRMSG_VALID = 1 AND MSGS_RETRIES_DEF.DRMSG_DELETED = 0 AND " +
			"SRC_UNIT.DLUNI_VALID = 1 AND SRC_UNIT.DLUNI_DELETED = 0 AND " +
			"DEST_UNIT.DLUNI_VALID = 1 AND DEST_UNIT.DLUNI_DELETED = 0 " +
			"ORDER BY MSG_MANDATORY ASC, MSG_PRIORITY DESC, MSG_ORDER DESC";

		private string _selectPriorityDest = 
			"SELECT DISTINCT MSGS.MSG_PRIORITY, MSGS.MSG_UNI_ID " +
			"FROM MSGS WHERE MSGS.MSG_STATUS = 0 " + 
			"ORDER BY MSGS.MSG_PRIORITY DESC, MSGS.MSG_UNI_ID DESC";
		private string _selectMsgsByPriorityDest = 
			"SELECT MSG_ID, MSG_DMSG_ID, MSG_MMSG_ID, MSG_DATE, MSG_PRIORITY, " +
			"MSG_MANDATORY, MSG_MSG_ID, MSG_MSG_ORDER, MSG_XML, MSG_UNI_ID, " +
			"MSG_IPADAPTER, MSG_PORTADAPTER, MSG_STATUS, MSG_NUMRETRIES, " +
			"MSG_LASTRETRY, MSG_TOTALRETRIES, MSG_PARCIALRETRIES, MSG_TOTALINTERVAL, " +
			"MSG_PARCIALINTERVAL, MSG_TOTALTIME, MSG_HISMANDATORY, MSG_FID " +
			"FROM MSGS " +
			"WHERE MSGS.MSG_STATUS=0 AND " +
			"MSGS.MSG_PRIORITY=@MSGS.MSG_PRIORITY@ AND MSGS.MSG_UNI_ID=@MSGS.MSG_UNI_ID@ " +
			"ORDER BY MSGS.MSG_PRIORITY DESC, MSGS.MSG_UNI_ID DESC, "+
			"MSGS.MSG_TOTALRETRIES, MSGS.MSG_PARCIALRETRIES, " +
			"MSGS.MSG_TOTALINTERVAL, MSGS.MSG_PARCIALINTERVAL" ;
		
		private int _maxPacketMessages;

		#endregion // Private data members
	}
}
