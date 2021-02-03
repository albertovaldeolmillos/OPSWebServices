using System;
using System.Configuration;
using OPS.Comm;
using OPS.Comm.Messaging;
using OPS.Comm.Media;
using OPS.Comm.Configuration;


namespace OPS.Comm.Becs
{
	/// <summary>
	/// Summary description for CommObjects.
	/// </summary>
	public class CommObjects
	{
		#region Public API

		public CommObjects()
		{
		}
		/// <summary>
		/// Binds together the instances of communication objects and starts
		/// whatever process are necessary
		/// </summary>
		/// <param name="inQueue">The input queue wrapper</param>
		/// <param name="outQueue">The output queue wrapper</param>
		public void Initialize(FecsBecsInputWrapper inQueue, 
			BecsFecsOutputWrapper outQueue)
		{
			AppSettingsReader appSettings = new AppSettingsReader();
			_unitId = (string) appSettings.GetValue("UnitID", typeof(string));

			Parameterization.SourceId = _unitId;
			_inAdapter = new InputQueueBecsAdapter(inQueue);
			_outAdapter = new OutputQueueBecsAdapter(outQueue);
			_channelMgr = new BecsChannelManager(_outAdapter, _inAdapter);
			_inAdapter.ChannelManager = _channelMgr;
			Parameterization.ChannelManager = _channelMgr;
			_mediaCtx = new BecsMediaContext();
			Parameterization.MediaContext = _mediaCtx;
			_msgRouter = new SimpleMessageRouter();
			Parameterization.MessageRouter = _msgRouter;
			Receiver.GetInstance().MessagesAvailable += 
				new Receiver.MessagesAvailableHandler(_msgRouter.OnMessagesAvailable);
			_dispatcher = new BecsMsgDispatcher();
			_msgRouter.MessageReceived += new MessageReceivedHandler(_dispatcher.OnMessageReceived);
			_msgRouter.ResponsesReceived += new ResponsesReceivedHandler(_dispatcher.OnResponsesReceived);

			_msgConfig = new CSMessageConfiguration();
			_sender = new MessageSender(_msgConfig);

			RegisterMessageHandlers();

			int period = (int) appSettings.GetValue("SendPeriod", typeof(int));
			_sender.Start(period);
		}
		/// <summary>
		/// Tears down the communication objects
		/// </summary>
		public void Shutdown()
		{
			if (_sender != null)
			{
				_sender.Stop();
			}
			if (_channelMgr != null)
			{
				_channelMgr.CloseAll();
			}
		}
		public void RegisterMessageHandlers()
		{
			_dispatcher.RegisterMessageHandler(Tags.TimeSyncMsg, 
				Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler(Tags.QueryVehicleMsg, 
				Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler(Tags.PunishVehicleMsg, 
				Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m1", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m2", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m3", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m4", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m5", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m6", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m7", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m8", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m9", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			//>> LDR 2004.05.05
			_dispatcher.RegisterMessageHandler("s1", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			//<< LDR 2004.05.05
			// CFE - Añado mensaje m12
			_dispatcher.RegisterMessageHandler("m12", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			//--IGU--> 170305: M14 message added
			_dispatcher.RegisterMessageHandler("m14", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			//--IGU--> 170305: M14 message added
			// ORC - Añado mensaje m20
			_dispatcher.RegisterMessageHandler("m20", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m50", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m51", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m52", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m53", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m54", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m55", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m56", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m57", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m59", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			_dispatcher.RegisterMessageHandler("m61", Type.GetType("OPS.Comm.Becs.BecsMessageHandler"));
			
		}

		/// <summary>
		/// The server unit identifier
		/// </summary>
		public string UnitId
		{
			get { return _unitId; }
		}
		/// <summary>
		/// The process channel factory
		/// </summary>
		public IChannelManager ChannelManager
		{
			get { return _channelMgr; }
		}
		/// <summary>
		/// The process media aware object
		/// </summary>
		public CommMediaContext MediaContext
		{
			get { return _mediaCtx; }
		}
		/// <summary>
		/// The process-wide message router
		/// </summary>
		public IMessageRouter MessageRouter
		{
			get { return _msgRouter; }
		}
		/// <summary>
		/// The accessor to message configuration
		/// </summary>
		public CSMessageConfiguration MessageConfiguration
		{
			get { return _msgConfig; }
		}
		/// <summary>
		/// The object that periodically sends messages stored in
		/// the database
		/// </summary>
		public MessageSender Sender
		{
			get { return _sender; }
		}

		#endregion // Public API

		#region Private data members

		private BecsChannelManager _channelMgr;
		private BecsMediaContext _mediaCtx;
		private SimpleMessageRouter _msgRouter;
		private string _unitId;
		private OutputQueueBecsAdapter _outAdapter;
		private InputQueueBecsAdapter _inAdapter;
		private BecsMsgDispatcher _dispatcher;
		private MessageSender _sender;
		private CSMessageConfiguration _msgConfig;

		#endregion // Private data members
	}
}
