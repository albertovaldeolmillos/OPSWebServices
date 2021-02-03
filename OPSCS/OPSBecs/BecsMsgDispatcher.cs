using System;
using System.Collections;
using System.Data;
using System.Threading;
using System.Diagnostics;
using OPS.Comm.Messaging;
using OPS.Comm;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// An implementation of the IMessageDispatcher interface to route messages
	/// to the objects that know how to handle them
	/// </summary> 
	public class BecsMsgDispatcher : IMessageDispatcher
	{
		#region Public API

		public BecsMsgDispatcher()
		{
			_msgHandlers = new Hashtable(5);
			_handlerType = Type.GetType("OPS.Comm.Becs.IMessageHandler");
		}
		/// <summary>
		/// Dispatchs a single message
		/// </summary>
		/// <param name="xmlData">The body of the message</param>
		/// <param name="replyToId">The identifier of the object to respond to
		/// Might be null if the message is already a response</param>
		/// <param name="srcId">Source unit id</param>
		public void DispatchMessage(string xmlData, string replyToId, string srcId)
		{
			try
			{
				MessageAccess msg = new MessageAccess(xmlData);
				String msgName = msg.GetMessageName();
				Type handlerType = (Type) _msgHandlers[msgName];
				if (handlerType != null)
				{
					object o = Activator.CreateInstance(handlerType);
					IMessageHandler handler = (IMessageHandler) o;
					HandlerData p = new HandlerData(handler, msg, replyToId, srcId);
					ThreadPool.QueueUserWorkItem(new WaitCallback(FireMessageHandler), p);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("BecsMsgDispatcher.DispatchMessage error - " + ex.Message);
				BecsMain.Logger.AddLog(ex);
			}
		}
		/// <summary>
		/// Dispatchs a set of messages
		/// </summary>
		/// <param name="messages">A dataset contining the messages
		/// to be dispatched</param>
		public void DispatchMessages(System.Data.DataSet messages)
		{
			if (messages != null && messages.Tables.Count > 0 &&
				messages.Tables[0].Rows.Count > 0)
			{
				foreach (DataRow r in messages.Tables[0].Rows)
				{
					MessageStatus status = (MessageStatus)(int)(decimal)
						r[(int) MessagesObject.MsgsColumns.Status];
					if (MessageStatus.Sent == status)
					{
						DispatchMessage(r[(int) MessagesObject.MsgsColumns.Xml].ToString(), null, null);
					}
				}
			}
		}
		/// <summary>
		/// Registers the type of objects to handle a kind of messages
		/// </summary>
		/// <param name="msgName">The name of the message to handle</param>
		/// <param name="type">The type of the handler</param>
		public void RegisterMessageHandler(string msgName, Type type)
		{
			Type[] itfs = type.GetInterfaces();
			bool isHandlerType = false;
			if (itfs != null && itfs.Length > 0)
			{
				for (int i = 0; i < itfs.Length && !isHandlerType; i++)
				{
					isHandlerType = _handlerType.Equals(itfs[i]);
				}
			}
			if (isHandlerType)
			{
				_msgHandlers[msgName] = type;
				Debug.WriteLine(string.Format(
					"BecsMsgDispatcher.RegisterMessageHandler: {0} - {1}", 
					msgName, type.Name));
				BecsMain.Logger.AddLog(string.Format(
					"BecsMsgDispatcher.RegisterMessageHandler: {0} - {1}", 
					msgName, type.Name), LoggerSeverities.Error);
			}
			else
			{
				Debug.WriteLine(string.Format(
					"BecsMsgDispatcher.RegisterMessageHandler - Invalid type: {0}", 
					type.Name));
				BecsMain.Logger.AddLog(string.Format(
					"BecsMsgDispatcher.RegisterMessageHandler - Invalid type: {0}", 
					type.Name), LoggerSeverities.Error);
			}
		}
		/// <summary>
		/// Handler for the IMessageRouter.ResponsesReceived event
		/// </summary>
		/// <param name="ds"></param>
		public void OnResponsesReceived(DataSet ds)
		{
			DispatchMessages(ds);
		}
		/// <summary>
		/// Handler for the IMessageRouter.MessageReceived event
		/// </summary>
		/// <param name="msg">The message received</param>
		public void OnMessageReceived(ReceivedMessage msg)
		{
			DispatchMessage(msg.Message, msg.ReplyToId, msg.SrcId);
		}


		#endregion // Public API
		
		#region Private methods

		/// <summary>
		/// Runs a handler for a message
		/// </summary>
		/// <param name="state"></param>
		private void FireMessageHandler(object state)
		{
			HandlerData p = (HandlerData) state;
			p._handler.HandleMessage(p._message, p._replyId, p._srcId);
		}

		#endregion // Private methods

		#region Private data members

		private Hashtable _msgHandlers;
		private Type _handlerType;

		internal class HandlerData
		{
			internal HandlerData(IMessageHandler handler, MessageAccess msg, string replyId, string srcId)
			{
				_handler = handler;
				_message = msg;
				_replyId = replyId;
				_srcId	 = srcId;
			}
			internal IMessageHandler _handler;
			internal MessageAccess _message;
			internal string _replyId;
			internal string _srcId;
		}

		#endregion // Private data members
	}

	/// <summary>
	/// The interface that the reponse handlers must implement
	/// </summary>
	/// <param name="msg">The body of the message</param>
	/// <param name="replyToId">The identifier of the object that can handle the 
	/// response. It is null if the message is a response to a previously sent
	/// message</param>
	/// <param name="srcId">Source Unit Id</param>
	public interface IMessageHandler
	{
		void HandleMessage(MessageAccess msg, string replyToId, string srcId);
	}
}
