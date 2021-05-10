using System;
using System.Data;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Interface for message dispatchers
	/// </summary>
	public interface IMessageDispatcher
	{
		/// <summary>
		/// Dispatchs a single message
		/// </summary>
		/// <param name="xmlData">The body of the message</param>
		/// <param name="replyToId">The identifier of the object that can handle the 
		/// response. It is null if the message is a response to a previously sent
		/// message</param>
		/// <param name="srcId">Source unit id</param>
		void DispatchMessage(string xmlData, string replyToId, string srcId);
		/// <summary>
		/// Dispatchs a set of messages
		/// </summary>
		/// <param name="messages">A dataset contining the messages
		/// to be dispatched</param>
		void DispatchMessages(DataSet messages);
	}
}
