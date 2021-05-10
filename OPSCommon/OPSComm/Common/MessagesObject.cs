using System;
using System.Data;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Provides helper methods and properties for querying and updating a 
	/// DataSet containing rows from the MSGS table
	/// </summary>
	public class MessagesObject
	{
		#region Public API

		/// <summary>
		/// The indexes of the columns in the MSGS table
		/// </summary>
		public enum MsgsColumns : int
		{
			Id, DmsgId, MmsgId, Date, Priority, Mandatory, MsgId, MsgOrder, Xml,
			UniId, IpAdapter, PortAdapter, Status, NumRetries, LastRetry, 
			TotalRetries, PartialRetries, TotalInterval, PartialInterval, 
			TotalTime, HisMandatory, Fid, Version, Valid, Deleted
		}

		/// <summary>
		/// The only way to construct the object is by initializing it with
		/// a DataSet
		/// </summary>
		/// <param name="ds">The DataSet containing the MSGS table rows</param>
		public MessagesObject(DataSet ds)
		{
			_msgs = ds;
			int rowCount = _msgs.Tables[0].Rows.Count;
			_msgIdToIndex = new Hashtable(rowCount);
			for (int i = 0; i < rowCount; i++)
			{
				_msgIdToIndex.Add(_msgs.Tables[0].Rows[i][(int) MsgsColumns.Id], i);
			}
		}
		/// <summary>
		/// Returns the DataSet owned by this object and releases its ownership
		/// </summary>
		/// <returns>The DataSet possibly modified through this object</returns>
		/// <remarks>After calling this method the object cannot be used
		/// anymore</remarks>
		public DataSet ReleaseDataSet()
		{
			DataSet retVal = _msgs;
			_msgIdToIndex.Clear();
			_msgs = null;
			return retVal;
		}
		/// <summary>
		/// Updates the status of a messsage
		/// </summary>
		/// <param name="msgId">The identifier of the message to update</param>
		/// <param name="msgId">The new status of the message</param>
		/// <returns>true if the message was in the table; false otherwise</returns>
		public bool UpdateMessageStatus(decimal msgId, decimal status)
		{
			bool retVal = false;
			if (_msgIdToIndex.ContainsKey(msgId))
			{
				int index = (int) _msgIdToIndex[msgId];
				lock (this)
				{
					_msgs.Tables[0].Rows[index][(int) MsgsColumns.Status] = status;
					retVal = true;
				}
			}
			return retVal;
		}
		/// <summary>
		/// Adds a retry to the message with the specified identifier
		/// </summary>
		/// <param name="msgId">The identifier of the message to update</param>
		/// <param name="retriesLeft">The number of pending retries for the message</param>
		/// <param name="lastRetryTime">The last time the message was retried</param>
		public void UpdateMessageRetries(decimal msgId, decimal retriesLeft, DateTime lastRetryTime)
		{
			if (_msgIdToIndex.ContainsKey(msgId))
			{
				int index = (int) _msgIdToIndex[msgId];
				lock (this)
				{
					DataRow row = _msgs.Tables[0].Rows[index];
					decimal partial = (decimal) row[(int) MsgsColumns.PartialRetries];
					decimal total = (decimal) row[(int) MsgsColumns.TotalRetries];
					decimal val = partial * total - retriesLeft;
					row[(int)MsgsColumns.LastRetry] = lastRetryTime;
					row[(int)MsgsColumns.NumRetries] = val;
				}
			}
		}
		/// <summary>
		/// Returns the indexes of the messages that hasn't been successfully sent
		/// yet
		/// </summary>
		/// <returns>An ArrayList of Int32 objects</returns>
		public ArrayList GetNotDoneMessageIndexes()
		{
			return null;
		}
		/// <summary>
		/// Returns the alternate messages meant to be sent when the message
		/// having the supplied id can't be sent
		/// </summary>
		/// <param name="messageId">The identifier of the message for which
		/// alternates need to be found</param>
		/// <returns>An ArrayList of MessageData objects sorted by priority</returns>
		public ArrayList GetAlternateMessages(decimal messageId)
		{
			ArrayList retVal = new ArrayList(5);
			lock (this)
			{
				if (_msgs != null)
				{
					DataView vw = _msgs.DefaultViewManager.CreateDataView(
						_msgs.Tables[0]);
					vw.RowFilter = string.Format(_alternateFilter, messageId);
					vw.Sort = _alternateSort;
					foreach (DataRow r in vw.Table.Rows)
					{
						retVal.Add(GetMessageData(r));
					}
				}
			}
			return retVal;
		}
		/// <summary>
		/// Returns the groups of mandatory messages that share a 
		/// common retry policy
		/// </summary>
		/// <returns>An ArrayList of RetryGroup objects</returns>
		public ArrayList GetRetryGroups()
		{
			ArrayList retVal = null;
			ArrayList groups = new ArrayList(5);
			lock (this)
			{
				if (_msgs != null)
				{
					DataView vw = _msgs.DefaultViewManager.CreateDataView(
						_msgs.Tables[0]);
					//vw.RowFilter = _mandatoryFilter;
					int rowCount = vw.Table.Rows.Count;
					if (rowCount > 0)
					{
						int i = 0;
						MessageData msg = GetMessageData(vw.Table.Rows[i]);
						while (i < rowCount)
						{
							RetryGroup g = new RetryGroup();
							g.Policy = new RetryPolicy(msg);
							groups.Add(g);
							while (i < rowCount && g.Policy.Equals(new RetryPolicy(msg)))
							{
								g.AddMessage(msg);
								i++;
								if (i < rowCount)
									msg = GetMessageData(vw.Table.Rows[i]);
							}
						}
						retVal = groups;
					}
				}
			}

			return retVal;
		}
		/// <summary>
		/// Marks as failed any row in the messages dataset that has not
		/// been marked as Sent
		/// </summary>
		public void FailPendingMessages()
		{
			lock (this)
			{
				int rowCount = _msgs.Tables[0].Rows.Count;
				for (int i = 0; i < rowCount; i++)
				{
					MessageStatus status = 
						(MessageStatus)(int)(decimal)_msgs.Tables[0].Rows[i][
						(int)MessagesObject.MsgsColumns.Status];
					if ( status == MessageStatus.Sending || status == MessageStatus.Pending) 
						_msgs.Tables[0].Rows[i][(int) MessagesObject.MsgsColumns.Status] = 
							(decimal)(int) MessageStatus.Failed;
				}
			}
		}

		#endregion // Public API

		#region Private methods

		private MessageData GetMessageData(DataRow row)
		{
			MessageData retVal = null;
			try
			{
				string destURI = GetDestinationUri(row);
				
				retVal = new MessageData(
					(decimal) row[(int) MsgsColumns.Id],
					(decimal) row[(int) MsgsColumns.PartialRetries],
					(decimal) row[(int) MsgsColumns.TotalRetries],
					(MessageStatus) (int) (decimal) row[(int) MsgsColumns.Status],
					(DateTime) row[(int) MsgsColumns.Date],
					(decimal) row[(int) MsgsColumns.TotalTime],
					(DBNull.Value.Equals(row[(int) MsgsColumns.Xml]))? 
						String.Empty : (string) row[(int) MsgsColumns.Xml],
					(decimal) row[(int) MsgsColumns.Mandatory],
					(DBNull.Value.Equals(row[(int) MsgsColumns.MsgOrder]))? 
						decimal.Zero : (decimal) row[(int) MsgsColumns.MsgOrder],
					(DBNull.Value.Equals(row[(int) MsgsColumns.MsgId]))? 
						decimal.Zero : (decimal) row[(int) MsgsColumns.MsgId],
					(decimal) row[(int) MsgsColumns.PartialInterval],
					(decimal) row[(int) MsgsColumns.TotalInterval],
					(decimal) row[(int) MsgsColumns.MmsgId],
					destURI,
					(decimal) row[(int) MsgsColumns.Priority],
					(DBNull.Value.Equals(row[(int) MsgsColumns.UniId]))? 
					decimal.Zero : (decimal) row[(int) MsgsColumns.UniId],
					!DBNull.Value.Equals(row[(int) MsgsColumns.IpAdapter]));
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("Exception at MessagesObject.GetMessageData: " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
			return retVal;
		}
		/// <summary>
		/// Returns the URI for the destination specified in a message row
		/// </summary>
		/// <param name="row">The row containing data for a message</param>
		/// <returns>The URI of the message destination</returns>
		private string GetDestinationUri(DataRow row)
		{
			string destURI = null;
			string addr = "0.0.0.0";
			int port = Convert.ToInt32(row[(int) MsgsColumns.PortAdapter]);
			if (!DBNull.Value.Equals(row[(int) MsgsColumns.IpAdapter]))
			{
				addr = (string) row[(int) MsgsColumns.IpAdapter];
			}
			else
			{
				addr = Configuration.AddressCache.GetAddressCache().GetUnitAddress(
					(decimal) row[(int) MsgsColumns.UniId]);
			}
			destURI = TcpIpUri.AddressPortAsUri(addr, port);

			return destURI;
		}

		#endregion // Private methods

		#region Private data members

		/// <summary>
		/// The DataSet with rows from the MSGS table
		/// </summary>
		DataSet _msgs;
		/// <summary>
		/// Maps the message identifiers found in the DataSet to their 
		/// corresponding row index
		/// </summary>
		Hashtable _msgIdToIndex;
		/// <summary>
		/// The filter to include only mandatory messages
		/// </summary>
		private const string _mandatoryFilter = "MSG_MANDATORY=1";
		/// <summary>
		/// The filter to include only alternate messages
		/// </summary>
		private const string _alternateFilter = "MSG_MANDATORY=0 AND MSG_MSG_ID={0}";
		/// <summary>
		/// The sort expression to return alternate messages by priority
		/// </summary>
		private const string _alternateSort = "MSG_MSG_ORDER";

		#endregion // Private data members
	}
}
