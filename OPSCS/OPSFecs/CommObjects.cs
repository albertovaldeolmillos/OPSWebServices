using OPS.Comm.Messaging;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// A class that binds together the communications objects
	/// </summary>
	public class CommObjects
	{
		#region Public API

		/// <summary>
		/// Sets up and starts communications
		/// </summary>
		/// <param name="localURI">The IP address and port the FECS will
		/// listen to for connections</param>
		/// <param name="schemaPath">The location of schemas for validating messages</param>
		public static void Initialize(int port, string schemaPath, 
			BecsFecsInputWrapper inputQueue, FecsBecsOutputWrapper outputQueue)
		{
			_port = new Port(System.Net.IPAddress.Any, port);
			_channelMgr = new FecsChannelManager();
			_port.ChannelMgr = _channelMgr;
			_port.Open();
			_msgValidator = new FecsMessageValidator(schemaPath);
			_inputAdapter = new InputQueueFecsAdapter(inputQueue, _channelMgr);
			_outputAdapter = new OutputQueueFecsAdapter(outputQueue, _msgValidator);
			_inputAdapter.OutputAdapter = _outputAdapter;
			_port.NewConnection += new ConnectionHandler(OnPortNewConnection);
		}
		/// <summary>
		/// Closes the local end-point and all established incoming or
		/// outgoing channels
		/// </summary>
		public static void Shutdown()
		{
			if (_port != null)
			{
				_port.Close();
				_port = null;
			}
			if (_channelMgr != null)
			{
				_channelMgr.CloseAll();
				_channelMgr = null;
			}
		}
		/// <summary>
		/// Read-only property to access the local endpoint
		/// object
		/// </summary>
		public static Port Port
		{
			get { return _port; }
		}
		/// <summary>
		/// Read-only property to access the channel factory
		/// </summary>
		public static IChannelManager ChannelManager
		{
			get { return _channelMgr; }
		}

		public static void OnPortNewConnection(IChannel channel)
		{
			_inputAdapter.SetupChannel(channel);
		}

		#endregion // Public API
	
		#region Private data members

		/// <summary>
		/// The socket communications end-point
		/// </summary>
		private static Port _port;
		/// <summary>
		/// The channel factory
		/// </summary>
		private static FecsChannelManager _channelMgr;
		/// <summary>
		/// The object that knows how to validate incoming messages
		/// </summary>
		private static FecsMessageValidator _msgValidator;
		/// <summary>
		/// The object that receives messages from the input queue
		/// and routes them to an output channel
		/// </summary>
		private static InputQueueFecsAdapter _inputAdapter;
		/// <summary>
		/// The object that receives messages from channels and routes them
		/// to the output queue
		/// </summary>
		private static OutputQueueFecsAdapter _outputAdapter;

		#endregion // Private data members
	}
}
