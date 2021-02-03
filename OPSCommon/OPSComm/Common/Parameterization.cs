using System;
using OPS.Comm.Media;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// A container for parameters needed across the library
	/// </summary>
	public class Parameterization
	{
		#region Public API

		/// <summary>
		/// The identifier to add to all outgoing packets
		/// </summary>
		public static string SourceId
		{
			get { return _sourceId; }
			set { _sourceId = value; }
		}
		/// <summary>
		/// The object that manages the creation of communication
		/// channels
		/// </summary>
		public static ChannelManager ChannelManager
		{
			get { return _channelMgr; }
			set { _channelMgr = value; }
		}
		/// <summary>
		/// The object that routes the responses to messages
		/// </summary>
		public static IMessageRouter MessageRouter
		{
			get { return _msgRouter; }
			set { _msgRouter = value; }
		}
		/// <summary>
		/// The object that provides information about media availability
		/// </summary>
		public static CommMediaContext MediaContext
		{
			get { return _mediaCtx; }
			set { _mediaCtx = value; }
		}

		#endregion // Public API

		#region Private data members

		private static string _sourceId = "???";
		private static ChannelManager _channelMgr;
		private static IMessageRouter _msgRouter;
		private static CommMediaContext _mediaCtx;
		
		#endregion Private data members
	}
}
