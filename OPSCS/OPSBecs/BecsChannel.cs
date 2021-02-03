using System;
using OPS.Comm;
using OPS.Comm.Messaging;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// A channel implementation that routes messages to a queue adapter
	/// </summary>
	public class BecsChannel : IChannel
	{
		#region Public API

		public BecsChannel(OutputQueueBecsAdapter outAdapter, 
			InputQueueBecsAdapter inAdapter)
		{
			_outAdapter = outAdapter;
			_inAdapter = inAdapter;
		}
		
		public void SendMessage(OPSTelegrama msg)
		{
			_outAdapter.SendMessage(msg, _uri);
		}

		public string Uri
		{
			get { return _uri; }
			set { _uri = value; }
		}

		public void Close()
		{
		}

		public void ReceiveMessage(OPSTelegrama msg)
		{
			if (IncomingMessage != null)
			{
				IncomingMessage(msg, this);
			}
			else
			{
				BecsMain.Logger.AddLog("Error: IncomingMessage event handler is null in BecsChannel.ReceiveMessage.",
					OPS.Comm.LoggerSeverities.Error);
			}
		}

		public event OPS.Comm.Messaging.MessageHandler OutcomingMessage;

		public event OPS.Comm.Messaging.ErrorHandler SendError;

		public event OPS.Comm.Messaging.MessageHandler IncomingMessage;

		public event OPS.Comm.Messaging.ErrorHandler ReceiveError;

		#endregion // Public API

		#region Private data members
		
		/// <summary>
		/// The adapter through which outgoing messages are routed
		/// </summary>
		private OutputQueueBecsAdapter _outAdapter;
		/// <summary>
		/// The adapter through which incoming messages are received
		/// </summary>
		private InputQueueBecsAdapter _inAdapter;
		/// <summary>
		/// The channel remote end-point
		/// </summary>
		private string _uri;
		
		#endregion // Private data members
	}
}
