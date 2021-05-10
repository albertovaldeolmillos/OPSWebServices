using System;
using System.Data;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	public delegate void ResponsesReceivedHandler(DataSet ds);
	public delegate void MessageReceivedHandler(ReceivedMessage msg);

	/// <summary>
	/// The interface all messages routers must implement
	/// </summary>
	public interface IMessageRouter
	{
		/// <summary>
		/// Routes an incoming message
		/// </summary>
		/// <param name="msg">The received message to process</param>
		void RouteIncomingMessage(ReceivedMessage msg);
		/// <summary>
		/// Routes the responses to outocoming messages
		/// </summary>
		/// <param name="ds">A dataset containing rows in the MSGS
		/// table updated by the messaging subsystem after their
		/// sending process</param>
		void RouteResponses(DataSet ds);
		/// <summary>
		/// Routes an acknowledgment to a message
		/// </summary>
		/// <param name="msgId">The indentifier of the message been
		/// acknowledged</param>
		/// <param name="response">The payload of tha ACK</param>
		void RouteAck(decimal msgId, string response);

		event ResponsesReceivedHandler ResponsesReceived;
		event MessageReceivedHandler MessageReceived;
	}
}
