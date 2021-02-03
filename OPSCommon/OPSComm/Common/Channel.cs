using System;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// The signature for the channel's registered message handlers
	/// </summary>
	public delegate void MessageHandler(OPSTelegrama msg, IChannel channel);
	/// <summary>
	/// The signature for the channel's registered error handlers
	/// </summary>
	public delegate void ErrorHandler(int error, string message, IChannel channel);

	/// <summary>
	/// The abstraction of a two-way communication channel.
	/// </summary>
	public interface IChannel
	{
		/// <summary>
		/// Initiates the process of sending a message
		/// </summary>
		/// <param name="msg">The message to be sent</param>
		void SendMessage(OPSTelegrama msg);
		/// <summary>
		/// Releases the resources used by the channel
		/// </summary>
		void Close();
		/// <summary>
		/// A string representation of the channel endpoint
		/// </summary>
		String Uri
		{
			get;
			set;
		}
		/// <summary>
		/// Event fired when a message has been sent
		/// </summary>
		event MessageHandler OutcomingMessage;
		/// <summary>
		/// Event fired when a message has been received
		/// </summary>
		event MessageHandler IncomingMessage;
		/// <summary>
		/// Event fired when a error occurs sending a message
		/// </summary>
		event ErrorHandler SendError;
		/// <summary>
		/// Event fired when an error occurs trying to receive a message
		/// </summary>
		event ErrorHandler ReceiveError;
	}
}
