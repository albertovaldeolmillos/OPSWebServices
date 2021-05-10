using System;
using System.Data;
using OPS.Comm.Messaging;
using System.Configuration;

namespace OPS.Comm.Configuration
{
	/// <summary>
	/// Provides utility methods for creating messages from message configuration
	/// data
	/// </summary>
	public class Util
	{
		#region Public API

		/// <summary>
		/// Fills a dataset with the messages to sent given the message body and its
		/// configuration data
		/// </summary>
		/// <param name="msgBody">The body of the message. It is a complete message
		/// text but the common attibutes (priority, destination, etc.) have default
		/// values. These values are updated by the message generation process</param>
		/// <param name="configDs">Tha dataset holding the message configuration</param>
		/// <param name="msgsDs">The dataset to receive the messages. It must be
		/// an empty dataset but must be setup with the schema for the MSGS table</param>
		/// <param name="msgId">Before the method invocation it must contain
		/// the value for the identifier of the first message generated. On method
		/// return it has the last identifier assigned to a message</param>
		public static void CreateMessageRows(string msgBody, DataSet configDs, DataSet msgsDs,
			ref decimal msgId)
		{
			decimal lastMandatoryId = decimal.Zero;
			decimal relatedId = decimal.Zero;
			decimal id = msgId;
			foreach (DataRow cr in configDs.Tables[0].Rows)
			{
				if ((decimal) cr[MessageConfiguration.Mandatory] != decimal.Zero)
				{
					lastMandatoryId = id;
					relatedId = decimal.Zero;
				}
				else
				{
					relatedId = lastMandatoryId;
				}
				AddMessageRow(cr, id, msgBody, relatedId, msgsDs);
				id++;
			}
			if (msgId != id)
				msgId = id - 1;
		}
		
		#endregion //Public API

		#region Private methods

		/// <summary>
		/// Adds a row to a message dataset using a row from the configuration
		/// dataset and the specific information for the new message
		/// </summary>
		/// <param name="cfgRow">A row from the message configuration dataset</param>
		/// <param name="msgId">The message identifier</param>
		/// <param name="msgBody">The text of the message</param>
		/// <param name="msgsDs">The dataset where the new row has to be added</param>
		private static void AddMessageRow(DataRow cfgRow, decimal msgId, string msgBody, 
			decimal relatedId, DataSet msgsDs)
		{
			/// TODO: Obtain these values
			decimal version = decimal.One;
			decimal valid = decimal.One;
			decimal deleted = decimal.Zero;
			AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			double nDifHour=0;
			try
			{
				nDifHour= (double) appSettings.GetValue   ("HOUR_DIFFERENCE",typeof(double));
			}
			catch
			{
				nDifHour=0;
			}

			MessageAccess msg = new MessageAccess(msgBody);
			msg.UpdateMessageHeader(msgId.ToString(), 
				cfgRow[MessageConfiguration.DestUnitId].ToString(),
				cfgRow[MessageConfiguration.Priority].ToString());
			string xml = msg.ToString();

			msgsDs.Tables[0].Rows.Add(new object[] { 
				msgId, 
				cfgRow[MessageConfiguration.MsgId], 
				cfgRow[MessageConfiguration.MediaId], DateTime.Now.AddHours(nDifHour), 
				cfgRow[MessageConfiguration.Priority], 
				cfgRow[MessageConfiguration.Mandatory], 
				(relatedId != 0) ? (object) relatedId : (object) DBNull.Value,
				(relatedId != 0) ? (object) cfgRow[MessageConfiguration.Order] : (object) DBNull.Value,
				xml, 
				cfgRow[MessageConfiguration.DestUnitId], 
				cfgRow[MessageConfiguration.IPAdapter], 
				(DBNull.Value.Equals(cfgRow[MessageConfiguration.IPAdapter])) ? 
				cfgRow[MessageConfiguration.DestUnitPort] : 
				cfgRow[MessageConfiguration.PortAdapter],
				decimal.Zero,
				DBNull.Value, DBNull.Value, 
				cfgRow[MessageConfiguration.TotalRetries],
				cfgRow[MessageConfiguration.PartialRetries],
				cfgRow[MessageConfiguration.TotalInterval],
				cfgRow[MessageConfiguration.PartialInterval],
				cfgRow[MessageConfiguration.TotalTime],
				cfgRow[MessageConfiguration.HisMandatory],
				DBNull.Value, version, valid, deleted } );
		}
		
		#endregion // Private methods
	}
}
