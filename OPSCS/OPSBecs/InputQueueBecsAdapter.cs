using System;
using System.Threading;
using System.Text;
using OPS.Comm;
using OPS.Comm.Messaging;
using OPS.Components.Data;
//using OTS.Data;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// An interface adapter between MSMQ input wrappers and communication channels
	/// </summary>
	public class InputQueueBecsAdapter
	{
		#region Public API

		/// <summary>
		/// Constructor receiving the incoming messages queue wrapper anf the
		/// channel factory
		/// </summary>
		/// <param name="inQueue">The object that receives incoming messages from a queue</param>
		/// <param name="channelMgr">The channel factory</param>
		public InputQueueBecsAdapter(FecsBecsInputWrapper inQueue)
		{
			_packetId = 0;
			_inQueue = inQueue;
			_inQueue.ReceiveCompleted += 
				new FecsBecsInputWrapper.ReceiveCompletedHandler(OnInQueueReceiveCompleted);
		}
		public IChannelManager ChannelManager
		{
			get { return _channelMgr; }
			set { _channelMgr = value; }
		}

		#endregion // Public API

		#region Private methods

		private void OnInQueueReceiveCompleted(FecsBecsHeader header, string body)
		{
			if (_channelMgr != null)
			{
				string uri = header.ID;
				IChannel channel = _channelMgr.GetChannel(uri);
				if (channel == null)
				{
					channel = _channelMgr.OpenChannel(uri, ChannelType.BecsQueue);
					if (channel != null)
					{
						MessageProcessorManager.OnNewConnection(channel);
					}
				}
				if (channel != null)
				{
					BecsChannel bc = (BecsChannel) channel;
					StringBuilder packet = new StringBuilder(256);
					packet.AppendFormat(null, "<{0} {1}=\"{2}\" {3}=\"{4}\">{5}</{0}>",
						Tags.Packet, Tags.PacketSrcAttr, header.PacketInfo.SourceId,
						Tags.PacketDateAttr, Dtx.DtxToString(header.PacketInfo.Dtx), body);
					string xml = packet.ToString();
					ILogger localLogger = null;
					try
					{
						Database d = null;
						CmpUnitsDB  cmpUnits = new CmpUnitsDB();
						//d = DatabaseFactory.GetDatabase();
						localLogger = DatabaseFactory.Logger;

						if(localLogger != null)
							localLogger.AddLog("[OnInQueueReceiveCompleted]: Updating ID: " + header.PacketInfo.SourceId + " IP :" + header.IP,LoggerSeverities.Debug);
						if(Convert.ToInt32(header.PacketInfo.SourceId) != -1)
						{
							if(cmpUnits.UpdateIP(Convert.ToInt32(header.PacketInfo.SourceId),header.IP) != 1)
							{	
								if(localLogger != null)
									localLogger.AddLog("[OnInQueueReceiveCompleted]: Error Updating IP",LoggerSeverities.Debug);
							}
							else
							{
								if(localLogger != null)
									localLogger.AddLog("[OnInQueueReceiveCompleted]: Update OK",LoggerSeverities.Debug);
							}
						}
					}
					catch
					{
						// Do not do nothing??
						// Si tenemos un error en realizar un update de la ip 
						// no hacemos nada
						if(localLogger != null)
							localLogger.AddLog("[OnInQueueReceiveCompleted]: Error Updating IP",LoggerSeverities.Debug);

					}
					Interlocked.Increment(ref _packetId);
					OPSTelegrama opsTel = OPSTelegramaFactory.CreateOPSTelegrama(_packetId, xml);
					bc.ReceiveMessage(opsTel);
				}
			}
		}

		#endregion // Private methods

		#region Private data members
	
		/// <summary>
		/// The object that receives incoming messages from a queue
		/// </summary>
		private FecsBecsInputWrapper _inQueue;
		/// <summary>
		/// The channel factory
		/// </summary>
		private IChannelManager _channelMgr;
		/// <summary>
		/// The last packet id
		/// </summary>
		private int _packetId;

		#endregion // Private data members
	}
}
