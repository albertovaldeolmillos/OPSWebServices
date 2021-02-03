using System;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using OPS.Comm.Becs.Messages;
using OPS.Comm.Messaging;
using OPS.Comm;
using OPS.Components.Data;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// A generic handler for incoming messages
	/// </summary>
	public class BecsMessageHandler : IMessageHandler
	{
		#region Public API

		public BecsMessageHandler()
		{
		}

		public void HandleMessage(MessageAccess msg, string replyToId, string srcId)
		{
			if (msg.ToString().Substring(0,2).Equals("<s"))
				ProcessSystemMessage(msg, replyToId);
			else
				ProcessUserMessage(msg, replyToId, srcId);
		}

		#endregion // Public API

		#region Private methods

		public void ProcessSystemMessage(MessageAccess msgAccess, string replyToId)
		{
			string body = msgAccess.ToString();
			BecsMain.Logger.AddLog("[BecsMessageHandler]:Received system message (body): " + body, LoggerSeverities.Debug);

			try 
			{
				SystemMessages sysmsgs = new SystemMessages();
				sysmsgs.Process(body);
			} 
			catch(Exception)
			{
				return;
			}
		}

		/// <summary>
		/// Just processes the message, delegating the real work on the corresponding class
		/// To do so, just creates the message class and calls it's Process method
		/// </summary>
		/// <param name="msgAccess">An accessor to the received message</param>
		/// <param name="replyToId">The identifier of the object to reply to</param>
		/// <param name="srcId">Source Unit Id</param>
		public void ProcessUserMessage(MessageAccess msgAccess, string replyToId, string srcId)
		{
			// Delegate message processing to the subclass that
			// really handles the process, and sends back the
			// response to MSMQ. TODO: Save response in MSGS
			string body = msgAccess.ToString();
			BecsMain.Logger.AddLog("[BecsMessageHandler]:Received message(body): " + body, LoggerSeverities.Debug);
			BecsMain.Logger.AddLog("[BecsMessageHandler]:# Starting message processing ", LoggerSeverities.Debug);

			IRecvMessage msg = null;
			try 
			{
				// Gets an instance of the class that processes the message
				msg = MessageFactory.GetReceivedMessage(body);
				msg.Session = BecsEngine.Session.MessagesSession;
			} 
			catch(Exception)
			{
				// This exception means that message has not been created
				// This is probably because parameters are incorrect
				StringCollection nack = new StringCollection();
				nack.Add(new NackMessage(MessageFactory.GetIdFromMessage(body), 
					NackMessage.NackTypes.NACK_SEMANTIC).ToString());
				SendResponsesBack(nack, replyToId, srcId);
				LogMsgDB(srcId, msgAccess.GetMessageName(), body, nack, replyToId);

				return;
			}

			try 
			{
				BecsMain.Logger.AddLog("[BecsMessageHandler]:# Process Message", LoggerSeverities.Debug);
				BecsMain.Logger.AddLog("[BecsMessageHandler]ProcessUserMessage " + msg.MsgId.ToString() ,
																				LoggerSeverities.Debug);
				BecsMain.Logger.AddLog("[BecsMessageHandler]From UnitId: " + srcId,
																	LoggerSeverities.Debug);
				// Execute the real processing of the message
				System.Collections.Specialized.StringCollection sc = msg.Process();
				
				// Send back all responses
				SendResponsesBack(sc, replyToId, srcId);

				LogMsgDB(srcId, msgAccess.GetMessageName(), body, sc, replyToId);
			} 
			catch (System.Threading.ThreadAbortException) 
			{
				// Thread was cancelled by user (stopping service, etc)
				// Must send a nack to the client with code "FE"
				BecsMain.Logger.AddLog("[BecsMessageHandler]:Thread " + System.Threading.Thread.CurrentThread.GetHashCode() + " aborted.", LoggerSeverities.Info);
				StringCollection nack = new StringCollection();
				nack.Add(new NackMessage(msg.MsgId, NackMessage.NackTypes.NACK_ERROR_BECS, 0xFE).ToString());
				SendResponsesBack(nack, replyToId, srcId);

				LogMsgDB(srcId, msgAccess.GetMessageName(), body, nack, replyToId);
			}
			catch (System.Exception ex)
			{
				// An unknown exception has happened
				// Must send a nack with code "FF"
				BecsMain.Logger.AddLog(ex);
				StringCollection nack = new StringCollection();
				nack.Add(new NackMessage(msg.MsgId, NackMessage.NackTypes.NACK_ERROR_BECS, 0xFF).ToString());
				SendResponsesBack(nack, replyToId, srcId);

				LogMsgDB(srcId, msgAccess.GetMessageName(), body, nack, replyToId);
			}
		}

		/// <summary>
		/// Send the responses back to the client
		/// </summary>
		/// <param name="sc">The responses (one to many)</param>
		/// <param name="replyToId">The identifier of the object to reply to</param>
		/// <param name="srcId">Source Unit Id</param>
		private void SendResponsesBack(StringCollection sc, string replyToId, string srcId)
		{
			MessageProcessor respProc = MessageProcessorManager.GetProcessor(replyToId);
			if (respProc != null)
			{
				foreach (string s in sc)
				{
					respProc.OneWaySend(s);
				}
			}
		}

		/// <summary>
		/// Logs msg into the Database
		/// </summary>
		/// <param name="srcId">Source Unit Id</param>
		/// <param name="type">Message Name</param>
		/// <param name="body">Source Message Body</param>
		/// <param name="scAnsw">Answer</param>
		/// <param name="replyToId">The identifier of the object to reply to</param>
		private void LogMsgDB(string srcId, string type, string body, StringCollection scAnsw, string replyToId)
		{
			try
			{
				StringBuilder sbAnsw = new StringBuilder();
				int iSrcId = 0;

				if (scAnsw != null)
				{
					foreach (string str in scAnsw)
					{
						sbAnsw.Append(str);
					}
				}
				
				if( srcId != null )
				{
					iSrcId = int.Parse(srcId);
				}


				AppSettingsReader appSettings = new AppSettingsReader();
				string unitId = (string) appSettings.GetValue("UnitID", typeof(string));

				CmpMsgsLogDB oMsgLogDB = new CmpMsgsLogDB();
				oMsgLogDB.InsertMsg( iSrcId, int.Parse(unitId), type, body, sbAnsw.ToString());
			}
			catch(Exception e)
			{
				// CFE - No permito lanzar excepciones fuera porque influyen en el tratamiento de los 
				// mensajes
				e.ToString();
			}
		}


		#endregion // Private methods
	}
}
