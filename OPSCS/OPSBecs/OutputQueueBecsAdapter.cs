using System;
using OPS.Comm;
using OPS.Comm.Messaging;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// An interface adapter between communication channels and MSQMQ
	/// output wrappers
	/// </summary>
	public class OutputQueueBecsAdapter
	{
		#region Public API

		/// <summary>
		/// Cosntructor taking the object needed to send messages to the
		/// output queue
		/// </summary>
		/// <param name="outQueue"></param>
		public OutputQueueBecsAdapter(BecsFecsOutputWrapper outQueue)
		{
			_outQueue = outQueue;
		}
		/// <summary>
		/// Sends a packet to the queue
		/// </summary>
		/// <param name="packet">The packet to send</param>
		/// <param name="fromUri">The originating channel URI</param>
		public void SendMessage(OPSTelegrama packet, string fromUri)
		{
			BecsFecsHeader hd = new BecsFecsHeader("0", "0", fromUri, 0, 1, true);
			_outQueue.Send(hd, packet.XmlData);
		}
		
		#endregion // Public API
		
		#region // Private data members

		private BecsFecsOutputWrapper _outQueue;

		#endregion // Private data members
	}
}
