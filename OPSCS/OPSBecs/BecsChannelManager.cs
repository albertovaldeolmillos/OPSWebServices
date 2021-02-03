using System;
using OPS.Comm.Messaging;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// A ChannelManager derivative to handle BecsChannels
	/// </summary>
	public class BecsChannelManager : ChannelManager
	{
		#region Public API

		/// <summary>
		/// Constructor receiving the adapters for the input and iutput queues
		/// </summary>
		/// <param name="outQueue">The adapter for the output queue</param>
		/// <param name="inQueue">The adapter for the input queue</param>
		public BecsChannelManager(OutputQueueBecsAdapter outQueue, 
			InputQueueBecsAdapter inQueue)
		{
			_outQueue = outQueue;
			_inQueue = inQueue;
		}
		public override IChannel OpenChannel(string uri, ChannelType type)
		{
			IChannel ch = null;
			if (type.Equals(ChannelType.BecsQueue))
			{
				ch = GetChannel(uri);
				if (ch == null)
				{
					ch = new BecsChannel(_outQueue, _inQueue);
					ch.Uri = uri;
					AddToChannels(uri, ch);
				}
			}
			return ch;
		}
		public override IChannel OpenChannel(string uri)
		{
			return OpenChannel(uri, ChannelType.BecsQueue);
		}

		#endregion // Public API
		
		#region Private data member
		
		private OutputQueueBecsAdapter _outQueue;
		private InputQueueBecsAdapter _inQueue;

		#endregion // Private data member
	}
}
