using System;
using System.Collections;
using System.Threading;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using OPS.Comm.Messaging;
using OPS.Comm.Media;

namespace OPS.Comm.Configuration
{
	/// <summary>
	/// The class fpr the object that periodiclly scans the MSGS tables
	/// and sends pending messages.
	/// </summary>
	public class MessageSender
	{
		#region Public API

		/// <summary>
		/// Constructor that takes a message configuration object
		/// </summary>
		/// <param name="cfg">The object that provides the message
		/// configuration</param>
		public MessageSender(MessageConfiguration cfg)
		{
			_msgCfg = cfg;
			_timer = new Timer(new TimerCallback(OnPeriodExpired), this,
				Timeout.Infinite, Timeout.Infinite);
			_evtWakeup = new ManualResetEvent(false);
			_stopping = false;
		}
		/// <summary>
		/// Start the sending process
		/// </summary>
		/// <param name="period">The amount of time in milliseconds to wait between
		/// sending attempts</param>
		public void Start(int period)
		{
			_period = period;
			_stopping = false;
			_evtWakeup.Reset();
			if (_msgCfg != null)
			{
				_sendThread = new Thread(new ThreadStart(SendThreadProc));
				_sendThread.Start();
			}
		}
		/// <summary>
		/// Stops the sending process
		/// </summary>
		public void Stop()
		{
			_stopping = true;
			_evtWakeup.Set();
			_sendThread = null;
		}
		/// <summary>
		/// The object that updates the messages status after being sent
		/// </summary>
		public IMessageUpdate MessageUpdater
		{
			get { return _msgUpdate; }
			set { _msgUpdate = value; }
		}

		#endregion // Public API
		
		
		#region Overridables

		/// <summary>
		/// The callback for the timer event
		/// </summary>
		/// <param name="state"></param>
		protected virtual void OnPeriodExpired(object state)
		{
			_evtWakeup.Set();
		}
		/// <summary>
		/// The thread procedure controlling the periodic sending process
		/// </summary>
		protected virtual void SendThreadProc()
		{
			Debug.WriteLine("MessageSender sender thread started");

			_timer.Change(_period, _period);
			while (!_stopping)
			{
				_evtWakeup.WaitOne();
				if (!_stopping)
				{
					_evtWakeup.Reset();
						
//					if (Parameterization.MediaContext != null &&
//						Parameterization.MediaContext.IsMediaAvailable(MediaType.CcBase))
//					{
						PriorityDestinationGroupList groups = 
							_msgCfg.GetPriorityGroups();
						if (groups.Count > 0)
						{
							IEnumerator en = groups.GetEnumerator();
							while (en.MoveNext())
							{
								PriorityDestinationGroup g =
									(PriorityDestinationGroup) en.Current;
								DataSet ds = _msgCfg.GetPriorityGroupMessages(g);
								if (ds != null)
								{
									Debug.WriteLine(String.Format("MessageSender - issuing msgs <{0}, {1}>",
										g.Destination, g.Priority));
									FilterByMediaAvailablility(ds);
									if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
									{
										ThreadPool.QueueUserWorkItem(new WaitCallback(SendOneThreadProc), ds);
									}
									else
									{
										Debug.WriteLine("MessageSender - messages not tried (media unavailable?)");
									}
								}
								else
								{
									Debug.WriteLine(String.Format("MessageSender - No messages found for <{0}, {1}>",
										g.Destination, g.Priority));
								}
							}
						}
//					}
				
					Thread.Sleep(0);
				}
			}
			_timer.Change(Timeout.Infinite, Timeout.Infinite);

			Debug.WriteLine("MessageSender sender thread stopped");
		}
		/// <summary>
		/// The procedure in charge of the sending of a dataset
		/// </summary>
		/// <param name="state">Is a DataSet object containing rows from the
		/// MSGS table</param>
		protected virtual void SendOneThreadProc(object state)
		{
			DataSet ds = (DataSet) state;
			Sender s = new Sender(Parameterization.ChannelManager);
			RequestWait r = new RequestWait(s, Parameterization.MessageRouter);
			try
			{
				r.WaitAll(ref ds);
				if (_msgUpdate != null)
					_msgUpdate.Update(ds);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("MessageSender.SendOneThreadProc error - " + ex.Message);
			}
		}
		/// <summary>
		/// Removes from the dataset the messages not having an available media
		/// </summary>
		/// <param name="ds">The messages dataset</param>
		protected virtual void FilterByMediaAvailablility(DataSet ds)
		{
			CommMediaContext ctx = Parameterization.MediaContext;
			if (ctx != null && ds.Tables.Count > 0)
			{
				DataRowCollection rows = ds.Tables[0].Rows;
				int count = rows.Count;
				for (int i = 0; i < count; i++)
				{
					MediaType media = 
						(MediaType) Convert.ToInt32((decimal)(rows[i][2]));
					if (!ctx.IsMediaAvailable(media))
						rows[i].Delete();
				}
				if (ds.HasChanges())
					ds.AcceptChanges();
			}
		}

		#endregion // Overridables

		#region Private methods



		#endregion // Private methods

		#region Private data members
		
		/// <summary>
		/// The object that provides the message configuration
		/// </summary>
		protected MessageConfiguration _msgCfg;
		/// <summary>
		/// The timer that controls the period for checking for new 
		/// messages
		/// </summary>
		protected Timer _timer;
		/// <summary>
		/// The amount of time in milliseconds to wait between
		/// sending attempts
		/// </summary>
		protected int _period;
		/// <summary>
		/// An event object that awakes the sending thread when the period
		/// expires or the process should finish
		/// </summary>
		protected ManualResetEvent _evtWakeup;
		/// <summary>
		/// The thread that controls the sending process
		/// </summary>
		protected Thread _sendThread;
		/// <summary>
		/// Set to true when the sending process should finish
		/// </summary>
		protected bool _stopping;
		/// <summary>
		/// The object that updates the messages status after being sent
		/// </summary>
		protected IMessageUpdate _msgUpdate;
		
		#endregion // Private data members
	}
}
