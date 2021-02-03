using System;
using OPS.Comm.Messaging;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// A ChannelManager derivative that wraps channels with FecsChannels
	/// </summary>
	public class FecsChannelManager : ChannelManager
	{
		#region Public API 

		public override IChannel OpenChannel(string uri, ChannelType type)
		{
			IChannel channel = null;
			switch (type)
			{
				case ChannelType.FecsQueue:
				{
					/// Make the base class add a SocketChannel, it will be
					/// wrapped by the AddToChannels override
					channel = base.OpenChannel(uri, ChannelType.Socket);
					break;
				}
			}
			return channel;
		}
		public override void AddChannel(string uri, IChannel channel)
		{
			AddToChannels(uri, channel);
		}

		#endregion // Public API 
		
		#region Protected methods

		protected override void AddToChannels(string uri, IChannel channel)
		{
			/// Wrap the supplied channel in a FecsChannel and do 
			/// as the base class does
			IChannel fecsChannel = new FecsChannel(channel);
			fecsChannel.Uri = uri;
			base.AddToChannels(uri, fecsChannel);
		}

		#endregion // Protected methods
	}
}
