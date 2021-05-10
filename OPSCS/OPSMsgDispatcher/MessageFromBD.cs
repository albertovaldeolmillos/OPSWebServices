using System;
using System.Data;
using System.Messaging;
using System.Configuration;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Builds a message from the table MSGS
	/// The message can be send to a MSMQ Queue
	/// The BECS engine uses that class to send pending messages in MSGS table to the FECS queue
	/// </summary>
	public class MessageFromBD
	{
		DataRow _dr;
		/// <summary>
		/// Builds a new MessageFromBD with the data contained in the DataRow specified
		/// </summary>
		/// <param name="data">DataRow with a register of MSGS containing the data</param>
		public MessageFromBD(DataRow data)
		{
			_dr = data;
		}

		/// <summary>
		/// Updates the data on the DataRow containing info about the message
		/// Data updated includes but is not limited to, number of retries, date of last retry, status ...
		/// Data is updated in the internal DataRow (the same DataRow passed to ctor when the object was constructed)
		/// </summary>
		public void UpdateData()
		{
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
		
		
			_dr["MSG_STATUS"] = Convert.ToInt32(OPS.Components.Data.CmpMessagesDB.MsgsStatus.Sending);
			_dr["MSG_NUMRETRIES"] = Convert.ToInt32 (_dr["MSG_NUMRETRIES"]) + 1;
			_dr["MSG_LASTRETRY"] = DateTime.Now.AddHours(nDifHour);
		}

		/// <summary>
		/// Puts the message in the specified MSMQ Queue
		/// </summary>
		/// <param name="queue">queue to be used</param>
		public void Send (MessageQueue queue)
		{
			Message msg = new Message();
			string header = null;
			// Step 1: Build the Header (label of MSMQ Message)
			msg.Label = header;
			// Step 2: Build the body (body of MSMQ Message)
			msg.Body = _dr["MSG_XML"].ToString();
			// Step 3: Set additional attributes to the message
			int priority = Convert.ToInt32(_dr["MSG_PRIORITY"]);
			switch (priority)
			{
				case 0:
					msg.Priority = System.Messaging.MessagePriority.Lowest;
					break;
				case 1:
					msg.Priority = System.Messaging.MessagePriority.VeryLow;
					break;
				case 2:
					msg.Priority = System.Messaging.MessagePriority.Low;
					break;
				case 3:
					msg.Priority = System.Messaging.MessagePriority.Normal;
					break;
				case 4:
					msg.Priority = System.Messaging.MessagePriority.AboveNormal;
					break;
				case 5:
					msg.Priority = System.Messaging.MessagePriority.High;
					break;
				case 6:
					msg.Priority = System.Messaging.MessagePriority.VeryHigh;
					break;
				case 7:
					msg.Priority = System.Messaging.MessagePriority.Highest;
					break;
			}
			// Step 4: Send the message to MSMQ
			lock (queue)
			{
				if (queue == null) return;
				queue.Send (msg);
			}
		}
	}
}
