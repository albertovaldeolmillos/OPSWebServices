using System;
using System.Diagnostics;
using System.Collections;
using System.Data;
using System.Threading;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Summary description for RequestWait.
	/// </summary>
	public class RequestWait
	{
		#region Public API

		/// <summary>
		/// Sends the messages contained in a DataSet through the
		/// specified Sender and awaits the responses coming from
		/// the supplied MessageRouter
		/// </summary>
		/// <param name="s">The object that sends the messages</param>
		/// <param name="router">The object that receives the responses</param>
		/// <param name="messages">The messages to send. After</param>
		public RequestWait(Sender sender, IMessageRouter router)
		{
			_router = router;
			_sender = sender;
			_sender.ResponseRouter = router;
			_evtReceived = new AutoResetEvent(false);
			_msgsResponses = new Hashtable(5);
		}
		/// <summary>
		/// Waits for the completion of the sending process of the 
		/// single message in the dataset and returns the response
		/// to this message
		/// </summary>
		/// <param name="msgId">The identifier of the message to wait for</param>
		/// <param name="messages">The dataset containing the message</param>
		/// <param name="response">Receives the response to the message</param>
		public void WaitOne(decimal msgId, ref DataSet messages, out string response)
		{
			_msgsResponses.Clear();
			_msgsResponses[msgId] = null;
			_router.ResponsesReceived += 
				new ResponsesReceivedHandler(OnResponsesReceived);
			_router.MessageReceived += 
				new MessageReceivedHandler(OnMessageReceived);
			
			try 
			{
				_sender.Send(messages);
				_evtReceived.WaitOne(); 
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("RequestWait.WaitOne error - " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
			
			_router.ResponsesReceived -= 
				new ResponsesReceivedHandler(OnResponsesReceived);
			_router.MessageReceived -= 
				new MessageReceivedHandler(OnMessageReceived);
			messages = _responsesDataSet;
			response = _lastResponse;
		}
		/// <summary>
		/// Waits for the completion of the sending process of all the 
		/// messages in the dataset
		/// </summary>
		/// <param name="messages">The messages to send</param>
		public void WaitAll(ref DataSet messages)
		{
			_msgsResponses.Clear();
			StoreMessageIds(messages);
			_router.ResponsesReceived += 
				new ResponsesReceivedHandler(OnResponsesReceived);
			_router.MessageReceived += 
				new MessageReceivedHandler(OnMessageReceived);
			
			try 
			{
				_sender.Send(messages);
				_evtReceived.WaitOne(); 
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("RequestWait.WaitAll error - " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
			
			_router.ResponsesReceived -= 
				new ResponsesReceivedHandler(OnResponsesReceived);
			_router.MessageReceived -= 
				new MessageReceivedHandler(OnMessageReceived);
			messages = _responsesDataSet;
		}
		/// <summary>
		/// Read-only property that returns the responses received. The hashtable
		/// maps message identifiers to the bodies of the responses. If a
		/// message didn't received a response, the value is null
		/// </summary>
		public Hashtable Responses
		{
			get { return _msgsResponses; }
		}

		#endregion // Public API
		
		#region Private methods

		/// <summary>
		/// Handler for the SimpleMessageRouter.ResponsesReceivedHandler
		/// event
		/// </summary>
		/// <param name="ds">The dataset containing the messages updated after
		/// their sending process</param>
		private void OnResponsesReceived(DataSet ds)
		{
			_responsesDataSet = ds;
			try 
			{
				_evtReceived.Set();
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("RequestWait.OnResponsesReceived error - " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
		}
		/// <summary>
		/// Handler for the SimpleMessageRouter.MessageReceived
		/// event
		/// </summary>
		/// <param name="msg">The received message</param>
		private void OnMessageReceived(ReceivedMessage msg)
		{
			if (_msgsResponses.Count > 0 && 
				_msgsResponses.ContainsKey(msg.AckMessageId))
			{
				_lastResponse = msg.Message;
				_msgsResponses[msg.AckMessageId] = msg.Message;
				//_evtReceived.Set();
			}
		}
		/// <summary>
		/// Stores the identifiers of the messages to wait for
		/// </summary>
		/// <param name="ds"></param>
		private void StoreMessageIds(DataSet ds)
		{
			if (ds != null && ds.Tables.Count > 0)
			{
				for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
				{
					string xml = 
						(string) ds.Tables[0].Rows[i][(int) MessagesObject.MsgsColumns.Xml];
					MessageAccess msg = new MessageAccess(xml);
					decimal id = decimal.Parse(msg.GetMessageId());
					_msgsResponses[id] = null;
				}
			}
		}

		#endregion // Private methods

		#region Private data members

		/// <summary>
		/// The event that signals the arrival of the responses
		/// </summary>
		private AutoResetEvent _evtReceived;
		/// <summary>
		/// The dataset containing the updated messages state after
		/// the sending process
		/// </summary>
		private DataSet _responsesDataSet;
		/// <summary>
		/// The last message received
		/// </summary>
		private string _lastResponse;
		/// <summary>
		/// The object that sends messages
		/// </summary>
		private Sender _sender;
		/// <summary>
		/// The object that provides the responses to sent
		/// messages
		/// </summary>
		private IMessageRouter _router;
		/// <summary>
		/// Maps the identifiers of the messages to wait for to the
		/// responses received
		/// </summary>
		private Hashtable _msgsResponses;

		#endregion // Private data members
	}
}
