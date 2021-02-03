using System;
using System.Diagnostics;
using System.Data;

using OPS.Comm.Configuration;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Contains the logic associated to message generation and sending
	/// </summary>
	public class CmpMessages : IMessageUpdate
	{
		public const int IdCol = 0;
		public const int DmsgIdCol = 1;
		public const int MmsgIdCol = 2;
		public const int DateCol = 3;
		public const int PriorityCol = 4;
		public const int MandatoryCol = 5;
		public const int MsgIdCol = 6;
		public const int MsgOrderCol = 7;
		public const int XmlCol = 8;
		public const int UniIdCol = 9;
		public const int IpAdapterCol = 10;
		public const int PortAdapterCol = 11;
		public const int StatusCol = 12;
		public const int NumRetriesCol = 13;
		public const int LastRetryCol = 14; 
		public const int TotalRetriesCol = 15;
		public const int PartialRetriesCol = 16;
		public const int TotalIntervalCol = 17;
		public const int PartialIntervalCol = 18;
		public const int TotalTimeCol = 19;
		public const int HisMandatoryCol = 20;
		public const int FidCol = 21;
		public const int VersionCol = 22;
		public const int ValidCol = 23;
		public const int DeletedCol = 24;

		public CmpMessages()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		/// <summary>
		/// Creates the necessary rows for a message in the MSGS table
		/// </summary>
		/// <param name="msgTag">The message name</param>
		/// <param name="srcUnitId">The source identifier</param>
		/// <param name="dstUnitId">The destination identifier</param>
		/// <param name="configDs">The message configuration</param>
		/// <param name="msgBody">The text of the message</param>
		public void CreateFromConfig(string msgTag, decimal srcUnitId, decimal dstUnitId,
			DataSet configDs, string msgBody)
		{
			if (configDs != null && configDs.Tables.Count > 0 &&
				configDs.Tables[0].Rows != null && 
				configDs.Tables[0].Rows.Count > 0)
			{
				DataSet msgsDs = null;
				IDbDataAdapter adapter = null;
				try
				{
					/// Create messages with configuration values
					adapter = GetAdapter();
					msgsDs = new DataSet();
					adapter.FillSchema(msgsDs, SchemaType.Mapped);
					int count = configDs.Tables[0].Rows.Count;
					CmpMsgsDB msgDb = new CmpMsgsDB();
					decimal id = msgDb.LastPKValue + 1;
					Util.CreateMessageRows(msgBody, configDs, msgsDs, ref id);
					adapter.Update(msgsDs);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Messages.CreateMessages - " + ex.Message);
					throw;
				}
			}
		}
		#region IMessageUpdate Members

		public void Update(DataSet ds)
		{
			try
			{
				// Accept updates made by the sending process (status, retries, ...)
				ds.AcceptChanges();
				IDbDataAdapter adapter = GetAdapter();
				Database d = DatabaseFactory.GetDatabase();
				foreach (DataRow r in ds.Tables[0].Rows)
				{
					if ((decimal) r[HisMandatoryCol] != decimal.Zero)
					{
						object[] pars = GetHistoryInsertParameters(r);
						d.ExecuteNonQuery(_historyInsertCmdText, pars);
					}
					Debug.WriteLine("Messages.Update - Deleting: " + 
						(string) r[XmlCol]);
					d.ExecuteNonQuery(_deleteCmdText, r[IdCol]);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Messages.Update - " + ex.Message);
				throw;
			}
		}

		#endregion

		private IDbDataAdapter GetAdapter()
		{
			Database d = DatabaseFactory.GetDatabase();
			IDbDataAdapter adapter = d.GetDataAdapter();
			adapter.SelectCommand = d.PrepareCommand(_selectCmdText);
			adapter.InsertCommand = d.PrepareCommand(_insertCmdText);
			adapter.UpdateCommand = d.PrepareCommand(_updateCmdText);
			adapter.DeleteCommand = d.PrepareCommand(_deleteCmdText);
			adapter.TableMappings.Add("MSG_ID", "MSG_ID");
			adapter.TableMappings.Add("MSG_DMSG_ID", "MSG_DMSG_ID");
			adapter.TableMappings.Add("MSG_MMSG_ID", "MSG_MMSG_ID");
			adapter.TableMappings.Add("MSG_DATE", "MSG_DATE");
			adapter.TableMappings.Add("MSG_PRIORITY", "MSG_PRIORITY");
			adapter.TableMappings.Add("MSG_MANDATORY", "MSG_MANDATORY");
			adapter.TableMappings.Add("MSG_MSG_ID", "MSG_MSG_ID");
			adapter.TableMappings.Add("MSG_MSG_ORDER", "MSG_MSG_ORDER");
			adapter.TableMappings.Add("MSG_XML", "MSG_XML");
			adapter.TableMappings.Add("MSG_UNI_ID", "MSG_UNI_ID");
			adapter.TableMappings.Add("MSG_IPADAPTER", "MSG_IPADAPTER");
			adapter.TableMappings.Add("MSG_PORTADAPTER", "MSG_PORTADAPTER");
			adapter.TableMappings.Add("MSG_STATUS", "MSG_STATUS");
			adapter.TableMappings.Add("MSG_NUMRETRIES", "MSG_NUMRETRIES");
			adapter.TableMappings.Add("MSG_LASTRETRY", "MSG_LASTRETRY");
			adapter.TableMappings.Add("MSG_TOTALRETRIES", "MSG_TOTALRETRIES");
			adapter.TableMappings.Add("MSG_PARCIALRETRIES", "MSG_PARCIALRETRIES");
			adapter.TableMappings.Add("MSG_TOTALINTERVAL", "MSG_TOTALINTERVAL");
			adapter.TableMappings.Add("MSG_PARCIALINTERVAL", "MSG_PARCIALINTERVAL");
			adapter.TableMappings.Add("MSG_TOTALTIME", "MSG_TOTALTIME");
			adapter.TableMappings.Add("MSG_HISMANDATORY", "MSG_HISMANDATORY");
			adapter.TableMappings.Add("MSG_FID", "MSG_FID");
			adapter.TableMappings.Add("MSG_VERSION", "MSG_VERSION");
			adapter.TableMappings.Add("MSG_VALID", "MSG_VALID");
			adapter.TableMappings.Add("MSG_DELETED", "MSG_DELETED");

			return adapter;
		}
		private object[] GetHistoryInsertParameters(DataRow r)
		{
			object[] par = new object[20];
			par[0] = r[IdCol];
			par[1] = r[DmsgIdCol];
			par[2] = r[MmsgIdCol];
			par[3] = r[DateCol];
			par[4] = r[PriorityCol];
			par[5] = r[MandatoryCol];
			par[6] = r[MsgIdCol];
			par[7] = r[MsgOrderCol];
			par[8] = r[XmlCol];
			par[9] = r[UniIdCol];
			par[10] = r[IpAdapterCol];
			par[11] = r[PortAdapterCol];
			par[12] = r[StatusCol];
			par[13] = r[NumRetriesCol];
			par[14] = r[LastRetryCol];
			par[15] = r[TotalRetriesCol];
			par[16] = r[PartialRetriesCol];
			par[17] = r[TotalIntervalCol];
			par[18] = r[PartialIntervalCol];
			par[19] = r[TotalTimeCol];
			return par;
		}


		private static string _selectCmdText = 
			"SELECT MSG_ID, MSG_DMSG_ID, MSG_MMSG_ID, MSG_DATE, MSG_PRIORITY, " +
			"MSG_MANDATORY, MSG_MSG_ID, MSG_MSG_ORDER, MSG_XML, MSG_UNI_ID, " +
			"MSG_IPADAPTER, MSG_PORTADAPTER, MSG_STATUS, MSG_NUMRETRIES, " +
			"MSG_LASTRETRY, MSG_TOTALRETRIES, MSG_PARCIALRETRIES, MSG_TOTALINTERVAL, " +
			"MSG_PARCIALINTERVAL, MSG_TOTALTIME, MSG_HISMANDATORY, MSG_FID, MSG_VERSION, " +
			"MSG_VALID, MSG_DELETED " +
			"FROM MSGS";
		private static string _insertCmdText = 
			"INSERT INTO MSGS(MSG_ID, MSG_DMSG_ID, MSG_MMSG_ID, MSG_DATE, MSG_PRIORITY, " +
			"MSG_MANDATORY, MSG_MSG_ID, MSG_MSG_ORDER, MSG_XML, MSG_UNI_ID, MSG_IPADAPTER, " +
			"MSG_PORTADAPTER, MSG_STATUS, MSG_NUMRETRIES, MSG_LASTRETRY, MSG_TOTALRETRIES, " +
			"MSG_PARCIALRETRIES, MSG_TOTALINTERVAL, MSG_PARCIALINTERVAL, MSG_TOTALTIME, " +
			"MSG_HISMANDATORY, MSG_FID, MSG_VERSION, MSG_VALID, MSG_DELETED) " + 
			"VALUES (@MSGS.MSG_ID@, @MSGS.MSG_DMSG_ID@, @MSGS.MSG_MMSG_ID@, @MSGS.MSG_DATE@, "+ 
			"@MSGS.MSG_PRIORITY@, @MSGS.MSG_MANDATORY@, @MSGS.MSG_MSG_ID@, @MSGS.MSG_MSG_ORDER@, " +
			"@MSGS.MSG_XML@, @MSGS.MSG_UNI_ID@, @MSGS.MSG_IPADAPTER@, @MSGS.MSG_PORTADAPTER@, " +
			"@MSGS.MSG_STATUS@, @MSGS.MSG_NUMRETRIES@, @MSGS.MSG_LASTRETRY@, @MSGS.MSG_TOTALRETRIES@, " +
			"@MSGS.MSG_PARCIALRETRIES@, @MSGS.MSG_TOTALINTERVAL@, @MSGS.MSG_PARCIALINTERVAL@, " + 
			"@MSGS.MSG_TOTALTIME@, @MSGS.MSG_HISMANDATORY@, @MSGS.MSG_FID@, @MSGS.MSG_VERSION@, " + 
			"@MSGS.MSG_VALID@, @MSGS.MSG_DELETED@)";
		private static string _updateCmdText = 
			"UPDATE MSGS SET MSG_PRIORITY = @MSGS.MSG_PRIORITY@, MSG_XML = @MSGS.MSG_XML@, " +
			"MSG_STATUS = @MSGS.MSG_STATUS@, MSG_NUMRETRIES = @MSGS.MSG_NUMRETRIES@, " +
			"MSG_LASTRETRY = @MSGS.MSG_LASTRETRY@ " +
			"WHERE MSG_ID = @MSGS.MSG_ID@";
		private static string _deleteCmdText = 
			"DELETE MSGS WHERE MSG_ID = @MSGS.MSG_ID@";
		private static string _historyInsertCmdText = 
			"INSERT INTO MSGS_HIS(HMSG_ID, HMSG_DHMSG_ID, HMSG_MMSG_ID, HMSG_DATE, HMSG_PRIORITY, " +
			"HMSG_MANDATORY, HMSG_HMSG_ID, HMSG_HMSG_ORDER, HMSG_XML, HMSG_UNI_ID, HMSG_IPADAPTER, " +
			"HMSG_PORTADAPTER, HMSG_STATUS, HMSG_NUMRETRIES, HMSG_LASTRETRY, HMSG_TOTALRETRIES, " +
			"HMSG_PARCIALRETRIES, HMSG_TOTALINTERVAL, HMSG_PARCIALINTERVAL, HMSG_TOTALTIME) " + 
			"VALUES (@MSGS_HIS.HMSG_ID@, @MSGS_HIS.HMSG_DHMSG_ID@, @MSGS_HIS.HMSG_MMSG_ID@, " +
			"@MSGS_HIS.HMSG_DATE@, @MSGS_HIS.HMSG_PRIORITY@, @MSGS_HIS.HMSG_MANDATORY@, " +
			"@MSGS_HIS.HMSG_HMSG_ID@, @MSGS_HIS.HMSG_HMSG_ORDER@, @MSGS_HIS.HMSG_XML@, " +
			"@MSGS_HIS.HMSG_UNI_ID@, @MSGS_HIS.HMSG_IPADAPTER@, @MSGS_HIS.HMSG_PORTADAPTER@, "+
			"@MSGS_HIS.HMSG_STATUS@, @MSGS_HIS.HMSG_NUMRETRIES@, @MSGS_HIS.HMSG_LASTRETRY@, " +
			"@MSGS_HIS.HMSG_TOTALRETRIES@, @MSGS_HIS.HMSG_PARCIALRETRIES@, " +
			"@MSGS_HIS.HMSG_TOTALINTERVAL@, @MSGS_HIS.HMSG_PARCIALINTERVAL@, @MSGS_HIS.HMSG_TOTALTIME@)";
	}
}
