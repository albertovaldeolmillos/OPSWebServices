using System;
using System.Collections;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// A group of messages with the same retry policy
	/// </summary>
	public class RetryGroup
	{
		#region Public API

		public RetryGroup()
		{
			_messages = new MessageDataTable();
		}
		/// <summary>
		/// Adds a message to the group
		/// </summary>
		/// <param name="msg">The message to add</param>
		/// <remarks>The first message added determines the group's
		/// retry policy if it's no set yet</remarks>
		public void AddMessage(MessageData msg)
		{
			_messages.AddMessageStatus(msg);
			if (_policy == null)
				_policy = new RetryPolicy(msg);
		}
		/// <summary>
		/// Read-only property that returns the retry policy of
		/// the group
		/// </summary>
		public RetryPolicy Policy
		{
			get { return _policy; }
			set { _policy = value; }
		}
		/// <summary>
		/// Read-only property that returns a list of messages with 
		/// the same retry policy
		/// </summary>
		public MessageDataTable Messages
		{
			get { return _messages; }
		}
		
		#endregion //Public API
		
		#region Private data members

		MessageDataTable _messages;
		RetryPolicy _policy;

		#endregion //Private data members

	}

	/// <summary>
	/// The set of data that represents a message retry polcy
	/// </summary>
	public class RetryPolicy
	{
		#region Public API

		public RetryPolicy(MessageData msg)
		{
			_totalRetries = msg.TotalRetries;
			_totalInterval = msg.TotalTimeout;
			_partialRetries = msg.PartialRetries;
			_partialInterval = msg.PartialTimeout;
			_destinationURI = msg.URI;
			_priority = msg.Priority;
		}
		/// <summary>
		/// Compares two policies for equality
		/// </summary>
		/// <param name="policy">The policy to compare to</param>
		/// <returns>true if the two policies are equal, false otherwise</returns>
		public override bool Equals(object obj)
		{
			RetryPolicy policy = (RetryPolicy) obj;
			return 
				_totalInterval.Equals(policy._totalInterval) &&
				_totalRetries.Equals(policy._totalRetries) &&
				_partialInterval.Equals(policy._partialInterval) &&
				_partialRetries.Equals(policy._partialRetries) &&
				_destinationURI.Equals(policy._destinationURI) &&
				_priority.Equals(policy._priority);
		}
		public string URI
		{
			get { return _destinationURI; }
		}
		public override int GetHashCode()
		{
			string str = String.Format("{0}-{1}-{2}-{3}-{4}-{5}",
				_totalRetries, _totalInterval, 
				_partialRetries, _partialInterval,
				_destinationURI, _priority);
			return str.GetHashCode();
		}


		#endregion // Public API

		#region Private data members

		private decimal _totalRetries;
		private decimal _totalInterval;
		private decimal _partialRetries;
		private decimal _partialInterval;
		private string _destinationURI;
		private decimal _priority;

		#endregion // Private data members
	}
}
