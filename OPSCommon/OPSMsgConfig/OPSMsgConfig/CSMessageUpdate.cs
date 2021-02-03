using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using OPS.Components.Data;

using OPS.Comm.Messaging;

namespace OPS.Comm.Configuration
{
	/// <summary>
	/// 
	/// </summary>
	public class CSMessageUpdate : IMessageUpdate
	{
		#region Public API

		public CSMessageUpdate()
		{

		}
		/// <summary>
		/// Implementation of the IMessageUpdate.Update method. Updates
		/// the MSGS_HIS with the data in a MSGS dataset if needed. Deletes
		/// the messages in the dataset from the MSGS table
		/// </summary>
		/// <param name="ds">A dataset containing rows from the MSGS table</param>
		public void Update(DataSet ds)
		{
			try
			{
				Database db = DatabaseFactory.GetDatabase();
				foreach (DataRow r in ds.Tables[0].Rows)
				{
					if ((decimal) r[(int)MessagesObject.MsgsColumns.HisMandatory] != decimal.Zero)
					{
						MessageToHistory(db, r);
					}
					Debug.WriteLine("CSMessageUpdate.Update - Deleting: " + 
						(string) r[(int)MessagesObject.MsgsColumns.Xml]);
					db.ExecuteNonQuery(_deleteCmdText, r[(int) MessagesObject.MsgsColumns.Id]);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("CSMessageUpdate.Update error - " + ex.Message);
			}

		}

		#endregion // Public API

		#region Private methods

		private void MessageToHistory(Database db, DataRow r)
		{
			object[] args = GetHistoryInsertParameters(r);
			db.ExecuteNonQuery(_historyInsertCmdText, args);
		}
		private object[] GetHistoryInsertParameters(DataRow r)
		{
			object[] par = new object[20];
			par[0] = r[(int) MessagesObject.MsgsColumns.Id];
			par[1] = r[(int) MessagesObject.MsgsColumns.DmsgId];
			par[2] = r[(int) MessagesObject.MsgsColumns.MmsgId];
			par[3] = r[(int) MessagesObject.MsgsColumns.Date];
			par[4] = r[(int) MessagesObject.MsgsColumns.Priority];
			par[5] = r[(int) MessagesObject.MsgsColumns.Mandatory];
			par[6] = r[(int) MessagesObject.MsgsColumns.MsgId];
			par[7] = r[(int) MessagesObject.MsgsColumns.MsgOrder];
			par[8] = r[(int) MessagesObject.MsgsColumns.Xml];
			par[9] = r[(int) MessagesObject.MsgsColumns.UniId];
			par[10] = r[(int) MessagesObject.MsgsColumns.IpAdapter];
			par[11] = r[(int) MessagesObject.MsgsColumns.PortAdapter];
			par[12] = r[(int) MessagesObject.MsgsColumns.Status];
			par[13] = r[(int) MessagesObject.MsgsColumns.NumRetries];
			par[14] = r[(int) MessagesObject.MsgsColumns.LastRetry];
			par[15] = r[(int) MessagesObject.MsgsColumns.TotalRetries];
			par[16] = r[(int) MessagesObject.MsgsColumns.PartialRetries];
			par[17] = r[(int) MessagesObject.MsgsColumns.TotalInterval];
			par[18] = r[(int) MessagesObject.MsgsColumns.PartialInterval];
			par[19] = r[(int) MessagesObject.MsgsColumns.TotalTime];
			return par;
		}

		#endregion // Private methods
		
		#region Private data members

		private static string _deleteCmdText = 
			"DELETE MSGS WHERE MSGS.MSG_ID = @MSGS.MSG_ID@";
		private static string _historyInsertCmdText = 
			"INSERT INTO MSGS_HIS(HMSG_ID, HMSG_DHMSG_ID, HMSG_MMSG_ID, HMSG_DATE, HMSG_PRIORITY, " +
			"HMSG_MANDATORY, HMSG_HMSG_ID, HMSG_HMSG_ORDER, HMSG_XML, HMSG_UNI_ID, HMSG_IPADAPTER, " +
			"HMSG_PORTADAPTER, HMSG_STATUS, HMSG_NUMRETRIES, HMSG_LASTRETRY, HMSG_TOTALRETRIES, " +
			"HMSG_PARCIALRETRIES, HMSG_TOTALINTERVAL, HMSG_PARCIALINTERVAL, HMSG_TOTALTIME) " + 
			"VALUES (@MSGS_HIS.HMSG_ID@, @MSGS_HIS.HMSG_DHMSG_ID@, @MSGS_HIS.HMSG_MMSG_ID@, @MSGS_HIS.HMSG_DATE@, @MSGS_HIS.HMSG_PRIORITY@, " +
			"@MSGS_HIS.HMSG_MANDATORY@, @MSGS_HIS.HMSG_HMSG_ID@, @MSGS_HIS.HMSG_HMSG_ORDER@, @MSGS_HIS.HMSG_XML@, @MSGS_HIS.HMSG_UNI_ID@, @MSGS_HIS.HMSG_IPADAPTER@, " +
			"@MSGS_HIS.HMSG_PORTADAPTER@, @MSGS_HIS.HMSG_STATUS@, @MSGS_HIS.HMSG_NUMRETRIES@,  @MSGS_HIS.HMSG_LASTRETRY@, @MSGS_HIS.HMSG_TOTALRETRIES@, " +
			"@MSGS_HIS.HMSG_PARCIALRETRIES@, @MSGS_HIS.HMSG_TOTALINTERVAL@, @MSGS_HIS.HMSG_PARCIALINTERVAL@, @MSGS_HIS.HMSG_TOTALTIME@)";

		#endregion // Private data members
	}
}
