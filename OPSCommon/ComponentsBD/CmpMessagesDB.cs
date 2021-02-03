using System;
using System.Data;



namespace OPS.Components.Data
{
	/// <summary>
	/// Contains database functions to modify MSGS table
	/// </summary>
	public class CmpMessagesDB
	{
		
		/// <summary>
		/// Status of a message (field MGS_STATUS)
		/// </summary>
		public enum MsgsStatus
		{
			/// <summary>
			/// Message has not been processed
			/// </summary>
			Pending = 0,
			/// <summary>
			/// Message is in process
			/// </summary>
			Sending = 1,
			/// <summary>
			/// Message has been processed succesfully
			/// </summary>
			Sended = 2,
			/// <summary>
			/// Message has been processed, and the process has failed
			/// </summary>
			Failed = 3
		}

		public CmpMessagesDB() {}

		/// <summary>
		/// Gets ALL pending messages ordered by date (oldest first)
		/// </summary>
		/// <param name="getSendingAlso">If true get the Messages with STATUS 0 (Pending) and 1(Sending). If false only STATUS 0 is retrieved</param>
		/// <returns>DataSet with data. Returns a DataSet instead of a DataTable because includes a relation</returns>
		public DataSet GetPendingMessages(bool getSendingAlso)
		{
			Database d = DatabaseFactory.GetDatabase();
			string swhere = "MSG_STATUS = @MSGS.MSG_STATUS@";
			if (getSendingAlso) swhere = swhere + " OR MSG_STATUS = @MSGS.MSG_STATUS@";
			object[] whereValues = null;
			whereValues = getSendingAlso ? new object[2] :  new object [1];
			whereValues[0] = Convert.ToString(Convert.ToInt32 (MsgsStatus.Pending));
			if (getSendingAlso) 
			{
				whereValues[1] = Convert.ToString(Convert.ToInt32(MsgsStatus.Sending));
			}
			DataTable dt = d.FillDataTable ("MSG_ID, MSG_DATE, MSG_PRIORITY, MSG_MEDIAPRIORITY, MSG_MSG_ID, MSG_XML, MSG_DESTINATION, MSG_NUMRETRIES, MSG_LASTRETRY, MSG_TOTALRETRIES, MSG_PARCIALRETRIES, MSG_TOTALINTERVAL, MSG_PARCIALINTERVAL",
				"MSGS", "MSG_DATE DESC", "MSG_STATUS = @MSGS.MSG_STATUS@","MSGS",-1, whereValues);
			// Add a Relation to the table to itself (it can be done????)
			DataSet ds = new DataSet();
			ds.Tables.Add (dt);
			ds.Relations.Add(dt.Columns["MSG_ID"],dt.Columns["MSG_MSG_ID"]);
			return ds;
		}

		/// <summary>
		/// Transfers the data containing on the dataset (related to MSGS table) to the database
		/// </summary>
		/// <param name="data">DataSet with updated MSGS info</param>
		public void UpdateMessages (DataSet data)
		{
			Database d =DatabaseFactory.GetDatabase();
			IDbDataAdapter da = d.GetDataAdapter ();
			da.UpdateCommand = d.PrepareCommand ("UPDATE MSGS SET MSG_STATUS = @MSGS.MSG_STATUS@, MSG_NUMRETRIES = @MSGS.MSG_NUMRETRIES@, MSG_LASTRETRY = @MSGS.MSG_LASTRETRY@");
			d.UpdateDataSet (da, data.Tables[0]);
		}

	}
}
