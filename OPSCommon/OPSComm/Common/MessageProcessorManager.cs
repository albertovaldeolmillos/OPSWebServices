using System;
using System.Collections;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Summary description for MessageProcessorManager.
	/// </summary>
	public class MessageProcessorManager
	{
		#region Public API

		static MessageProcessorManager()
		{
			_mapIds = new Hashtable(5);
			_mapPolicies = new Hashtable(5);
		}
		/// <summary>
		/// Finds a MessageProcessor given its identifier
		/// </summary>
		/// <param name="id">The MessageProcessor identifier</param>
		/// <returns>The MessageProcessor object with the specified
		/// identifier. It returns null if not found</returns>
		public static MessageProcessor GetProcessor(string id)
		{
			return (MessageProcessor) _mapIds[id];
		}
		/// <summary>
		/// Finds a MessageProcessor given its retry policy
		/// </summary>
		/// <param name="id">The MessageProcessor policy</param>
		/// <returns>The MessageProcessor object with the specified
		/// policy. It returns null if not found</returns>
		public static MessageProcessor GetProcessor(RetryPolicy policy)
		{
			return (MessageProcessor) _mapPolicies[policy];
		}
		/// <summary>
		/// Includes a message processor in the set of objects
		/// managed
		/// </summary>
		/// <param name="proc">The MessageProcessor to include</param>
		/// <remarks>If when added the supplied processor doesn't have
		///  a policy assigned, it can't be retrieved later by policy</remarks>
		public static void AddProcessor(MessageProcessor proc)
		{
			if (!_mapIds.ContainsKey(proc.Id))
				_mapIds.Add(proc.Id, proc);
			if (proc.Policy != null && !_mapPolicies.ContainsKey(proc.Policy))
				_mapPolicies.Add(proc.Policy, proc);
		}

		public static void CloseProcessor(string id)
		{
			MessageProcessor p = (MessageProcessor) _mapIds[id];
			_mapIds.Remove(id);
			if (p.Policy != null)
				_mapPolicies.Remove(p.Policy);
			p.MessageArrived -= new MessageProcessor.MessageArrivedHandler(
				Receiver.GetInstance().NewMessage);
			p.Close();
		}

		/// <summary>
		/// Handler fr the Port.NewConnectionEvent
		/// </summary>
		/// <param name="newConnection"></param>
		public static void OnNewConnection(IChannel newConnection)
		{
			MessageProcessor p = new MessageProcessor(newConnection);
			AddProcessor(p);
			p.MessageArrived += new MessageProcessor.MessageArrivedHandler(
				Receiver.GetInstance().NewMessage);
		}

		public static void CloseProcessors()
		{
			IEnumerator en = _mapIds.Values.GetEnumerator();
			while (en.MoveNext())
			{
				MessageProcessor p = (MessageProcessor) en.Current;
				p.MessageArrived -= new MessageProcessor.MessageArrivedHandler(
					Receiver.GetInstance().NewMessage);
				p.Close();
			}
			_mapIds.Clear();
			_mapPolicies.Clear();
		}

		#endregion // Public API
	
		#region Private methods
	

		#endregion // Private methods

		#region Private data members

		private static Hashtable _mapIds;
		private static Hashtable _mapPolicies;

		#endregion // Private data members
	}
}
