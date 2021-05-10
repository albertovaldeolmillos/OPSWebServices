using System;
using OPS.Comm;
using OPS.Comm.Messaging;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// An interface adaptation between MSMQ queue input wrappers and
	/// communication channels
	/// </summary>
	public class InputQueueFecsAdapter
	{
		#region Public API
		/// <summary>
		/// Constructor taking the object to use as interface to an inbound
		/// message queue and a channel factory
		/// </summary>
		/// <param name="queue">The inbound message queue</param>
		/// <param name="channelMgr">The channel wrapper</param>
		public InputQueueFecsAdapter(BecsFecsInputWrapper queue, IChannelManager channelMgr)
		{
			_inQueue = queue;
			_channelMgr = channelMgr;
			queue.ReceiveCompleted += new BecsFecsInputWrapper.ReceiveCompletedHandler(
				OnQueueReceiveCompleted);
		}
		/// <summary>
		/// Handler for the inbound message queue ReceiveCompleted event
		/// </summary>
		/// <param name="header">The header of the incoming message</param>
		/// <param name="body">The body of the incoming message</param>
		public void OnQueueReceiveCompleted(BecsFecsHeader header, string body)
		{
			/// JLB 2004.07.21 - time sending a message
			/// 
			Org.Mentalis.Utilities.StopWatch sw = new Org.Mentalis.Utilities.StopWatch();

			IChannel channel = _channelMgr.GetChannel(header.Fid);

			long d = sw.Peek();

			if (channel == null)
			{
				try
				{
					//>> LDRTEST0
					//channel = _channelMgr.OpenChannel(header.Fid, ChannelType.FecsQueue);
					//<< LDRTEST0
				}
				catch (Exception ex)
				{
					FecsMain.Logger.AddLog(
						string.Format("Couldn't open channel to send to {0}.", header.Fid), 
						LoggerSeverities.Error);
					FecsMain.Logger.AddLog(ex);
				}
				if (channel != null)
				{
					SetupChannel(channel);
				}
			}
			if (channel != null)
			{
				if (d > 100)
				{
					FecsMain.Logger.AddLog(string.Format("Delay for GetChannel({1}) {0} ms",
						d, channel.Uri), LoggerSeverities.Debug);
				}

				try
				{
					OPSTelegrama t = OPSTelegramaFactory.CreateOPSTelegrama(Correlation.NextId, body);
					channel.SendMessage(t);
					FecsMain.Logger.AddLog(
						string.Format("Message sent to {0} [{1}].", channel.Uri, t.XmlData), 
						LoggerSeverities.Info);
				}
				catch (Exception ex)
				{
					_channelMgr.CloseChannel(header.Fid);
					FecsMain.Logger.AddLog(
						string.Format("Couldn't send to {0}. Closing channel.", header.Fid), 
						LoggerSeverities.Error);
					FecsMain.Logger.AddLog(ex);
				}
			}
			else
			{
				FecsMain.Logger.AddLog(string.Format("Delay for GetChannel({1}) {0} ms",
					d, header.Fid), LoggerSeverities.Debug);
				FecsMain.Logger.AddLog(
					string.Format("Message NOT sent to {0} [{1}].", header.Fid, body), 
					LoggerSeverities.Error);
			}
		}
		/// <summary>
		/// Binds itself and the output adapter to the channel events
		/// </summary>
		/// <param name="channel">The channel to bind to</param>
		public void SetupChannel(IChannel channel)
		{
			//>> LDR 2004.07.16
			//channel.SendError += new ErrorHandler(OnChannelSendError);
			channel.SendError += new ErrorHandler(_outAdapter.OnChannelSendError);
			//<< LDR 2004.07.16
			channel.IncomingMessage += 
				new MessageHandler(_outAdapter.OnChannelIncomingMessage);
			channel.ReceiveError += 
				new ErrorHandler(_outAdapter.OnChannelReceiveError);
			FecsMain.Logger.AddLog(string.Format("Client connected from {0}", channel.Uri), 
				LoggerSeverities.Info);
		}

		//>> LDRTEST2
//		public void EndChannel(IChannel channel)
//		{
//			channel.SendError -= new ErrorHandler(OnChannelSendError);
//			channel.IncomingMessage -= 
//				new MessageHandler(_outAdapter.OnChannelIncomingMessage);
//			channel.ReceiveError -= 
//				new ErrorHandler(_outAdapter.OnChannelReceiveError);
//			FecsMain.Logger.AddLog(string.Format("Client disconnected from {0}", channel.Uri), 
//				LoggerSeverities.Info);
//		}
		//<< LDRTEST2

		/// <summary>
		/// The Adapter that handles responses to messages
		/// </summary>
		public OutputQueueFecsAdapter OutputAdapter
		{
			get { return _outAdapter; }
			set { _outAdapter = value; }
		}
		#endregion // Public API

		//>> LDR 2004.07.16
//		#region Private methods
//		/// <summary>
//		/// Handler for the IChannel.SendError event
//		/// </summary>
//		/// <param name="error">The error code</param>
//		/// <param name="message">The error message</param>
//		/// <param name="channel">The channel indicating the error</param>
//		private void OnChannelSendError(int error, string message, IChannel channel)
//		{
//			string uri = channel.Uri;
//			if (error == (int) SocketChannel.SocketErrors.RemoteClosed)
//			{
//				_channelMgr.CloseChannel(uri);
//				FecsMain.Logger.AddLog(string.Format("Client {0} disconnected (inqueue)", uri), 
//					LoggerSeverities.Debug);
//			}
//			else if (_channelMgr.GetChannel(uri) != null)
//			{
//				FecsMain.Logger.AddLog(string.Format("Error sending to {0}: {1} ({2})",
//					channel.Uri, error, message), LoggerSeverities.Error);
//			}
//		}
//		#endregion // Private methods
		//<< LDR 2004.07.16

		#region Private data members
		/// <summary>
		/// The inbound message queue
		/// </summary>
		private BecsFecsInputWrapper _inQueue;
		/// <summary>
		/// The channel wrapper
		/// </summary>
		private IChannelManager _channelMgr;
		/// <summary>
		/// The optional adapter that handles responses to messages
		/// </summary>
		private OutputQueueFecsAdapter _outAdapter;
		#endregion // Private data members
	}
}
