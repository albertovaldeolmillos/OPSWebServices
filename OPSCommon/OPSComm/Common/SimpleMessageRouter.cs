using System;
using System.Data;
using System.Threading;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// A message router that fires an event each time it receives messages
	/// to route
	/// </summary>
	public class SimpleMessageRouter : OPS.Comm.Messaging.IMessageRouter
	{
		#region Public API

		public SimpleMessageRouter()
		{
		}
		public void RouteIncomingMessage(ReceivedMessage msg)
		{
			if (MessageReceived != null)
			{
				ThreadPool.QueueUserWorkItem(
					new WaitCallback(FireMessageHandlersThreadProc),
					msg);
			}
		}
		public void RouteResponses(System.Data.DataSet ds)
		{
			if (ResponsesReceived != null)
			{
				ThreadPool.QueueUserWorkItem(
					new WaitCallback(FireResponseHandlersThreadProc),
					ds);
			}
		}
		public void RouteAck(decimal msgId, string response)
		{
			if (MessageReceived != null)
			{
				ThreadPool.QueueUserWorkItem(
					new WaitCallback(FireMessageHandlersThreadProc),
					new ReceivedMessage(msgId, response));
			}
		}
		/// <summary>
		/// Handler for the Receiver.MessagesAvailable event
		/// </summary>
		public void OnMessagesAvailable()
		{
			int msgCount = Receiver.GetInstance().PendingMessages;
			for (int i = 0; i < msgCount; i++)
			{
				ReceivedMessage msg = Receiver.GetInstance().NextMessage;
				RouteIncomingMessage(msg);
			}
		}

		public event ResponsesReceivedHandler ResponsesReceived;
		public event MessageReceivedHandler MessageReceived;

		#endregion // Public API

		#region Private methods

		/// <summary>
		/// Calls back the response handlers
		/// </summary>
		/// <param name="state">It is a DataSet object</param>
		private void FireResponseHandlersThreadProc(object state)
		{
			ResponsesReceived((DataSet) state);
		}
		/// <summary>
		/// Calls back the incoming message handlers
		/// </summary>
		/// <param name="state">It is a ReceivedMessage object</param>
		private void FireMessageHandlersThreadProc(object state)
		{
			MessageReceived((ReceivedMessage) state);
		}
		
		#endregion // Private methods
	}
}
