using System;
using System.Collections;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Contains the information needed while a message is being sent
	/// </summary>
	public class MessageData
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public MessageData()
		{
		}
		/// <summary>
		/// Constructor that initializes all of the parameters needed
		/// to send a message
		/// </summary>
		public MessageData(decimal msgId, decimal partialRetries,
			decimal totalRetries, MessageStatus status, 
			DateTime sendTime, decimal expirationTime, string xmlData,
			decimal mandatory, decimal order, decimal referredId, 
			decimal partialTimeout, decimal totalTimeout, decimal media,
			string destURI, decimal priority, decimal destUnitId, bool retryAddress)
		{
			_msgId = msgId;
			_partialRetries = partialRetries;
			_totalRetries = totalRetries;
			_pendingRetries = totalRetries * partialRetries;
			_status = status;
			_sendTime = sendTime;
			_expirationTime = expirationTime;
			_xml = xmlData;
			_mandatory = mandatory;
			_alternativeOrder = order;
			_referredMessageId = referredId;
			_partialTimeout = partialTimeout;
			_totalTimeout = totalTimeout;
			_media = media;
			_destURI = destURI;
			_priority = priority;
			_destUnitId = destUnitId;
			_retryAddress = retryAddress;
		}
		/// <summary>
		/// The message identifier
		/// </summary>
		public decimal MessageId
		{
			get { return _msgId; }
			set { _msgId = value; }
		}
		/// <summary>
		/// The maximum number of partial retries
		/// </summary>
		public decimal PartialRetries
		{
			get { return _partialRetries; }
			set { _partialRetries = value; }
		}
		/// <summary>
		/// The maximum number of total retries
		/// </summary>
		public decimal TotalRetries
		{
			get { return _totalRetries; }
			set { _totalRetries = value; }
		}
		/// <summary>
		/// The number of retries pending
		/// </summary>
		public decimal PendingRetries
		{
			get { return _pendingRetries; }
			set { _pendingRetries = value; }
		}
		/// <summary>
		/// The sending status of the message
		/// </summary>
		public MessageStatus Status
		{
			get { return _status; }
			set { _status = value; }
		}
		/// <summary>
		/// The time the message sending process was started
		/// </summary>
		public DateTime SentTime
		{
			get { return _sendTime; }
			set { _sendTime = value; }
		}
		/// <summary>
		/// The time limit after which the sending process of the message
		/// has to be cancelled
		/// </summary>
		public decimal ExpirationTime
		{
			get { return _expirationTime; }
			set { _expirationTime = value; }
		}
		/// <summary>
		/// The data to be sent
		/// </summary>
		public string XmlData
		{
			get { return _xml; }
			set { _xml = value; }
		}
		/// <summary>
		/// Indicates whether the message is mandatory (1) or an 
		/// alternative (0)
		/// </summary>
		public decimal MandatoryStatus
		{
			get { return _mandatory; }
			set { _mandatory = value; }
		}
		/// <summary>
		/// Indicates the order the message must be retried when its 
		/// MandatoryStatus indicates the message is an alternative
		/// </summary>
		public decimal AlternativeOrder
		{
			get { return _alternativeOrder; }
			set { _alternativeOrder = value; }
		}
		/// <summary>
		/// When MandatoryStatus indicates the message is not mandatory
		/// this property gives the identifier of the mandatory message
		/// this is an alternative to
		/// </summary>
		public decimal ReferredMessageId
		{
			get { return _referredMessageId; }
			set { _referredMessageId = value; }
		}
		/// <summary>
		/// The amount of time between partial retries. In milliseconds
		/// </summary>
		public decimal PartialTimeout
		{
			get { return _partialTimeout; }
			set { _partialTimeout = value; }
		}
		/// <summary>
		/// The amount of time between total retries. In milliseconds
		/// </summary>
		public decimal TotalTimeout
		{
			get { return _totalTimeout; }
			set { _totalTimeout = value; }
		}
		/// <summary>
		/// The transmission media to use wirh the message
		/// </summary>
		public decimal Media
		{
			get { return _media; }
			set { _media = value; }
		}
		/// <summary>
		/// The URI of the destination
		/// </summary>
		public string URI
		{
			get { return _destURI; }
			set { _destURI = value; }
		}
		/// <summary>
		/// The last time the messages was retried
		/// </summary>
		public DateTime TimeLastRetry
		{
			get { return _timeLastRetry; }
			set { _timeLastRetry = value; }
		}
		/// <summary>
		/// The message priority
		/// </summary>
		public decimal Priority
		{
			get { return _priority; }
			set { _priority = value; }
		}
		/// <summary>
		/// The identifier of the message destination
		/// </summary>
		public decimal DestinationUnit
		{
			get { return _destUnitId; }
			set { _destUnitId = value; }
		}
		/// <summary>
		/// Indicates whether the address of the message should be
		/// requeried before sending it
		/// </summary>
		public bool ChangeAddressOnRetry
		{
			get { return _retryAddress; }
			set { _retryAddress = value; }
		}

		private decimal _msgId;
		private decimal _partialRetries;
		private decimal _totalRetries;
		private decimal _pendingRetries;
		private MessageStatus _status;
		private DateTime _sendTime;
		private decimal _expirationTime;
		private string _xml;
		private decimal _mandatory;
		private decimal _alternativeOrder;
		private decimal _referredMessageId;
		private decimal _partialTimeout;
		private decimal _totalTimeout;
		private decimal _media;
		private string _destURI;
		private DateTime _timeLastRetry;
		private decimal _priority;
		private decimal _destUnitId;
		private bool _retryAddress;
	}

	/// <summary>
	/// A class that maps message identifiers to their status
	/// </summary>
	public class MessageDataTable
	{
		public MessageDataTable()
		{
			_mapIdsToStatus = new Hashtable(5);
		}
		/// <summary>
		/// Adds a new message status object to the table
		/// </summary>
		/// <param name="msgStatus"></param>
		public void AddMessageStatus(MessageData msgStatus)
		{
			_mapIdsToStatus.Add(msgStatus.MessageId, msgStatus);
		}
		private Hashtable _mapIdsToStatus;

		/// <summary>
		/// Read-only property that returns the status for the 
		/// specified message identifier
		/// </summary>
		public MessageData this[decimal msgId]
		{
			get { return (MessageData)_mapIdsToStatus[msgId]; }
		}
		/// <summary>
		/// Read-only property that returns the number of messages
		/// in the table
		/// </summary>
		public int Count 
		{
			get { return _mapIdsToStatus.Count; }
		}
		/// <summary>
		/// Read-only property that returns the collection of MessageData
		/// objects stored in the table
		/// </summary>
		public ICollection Values
		{
			get { return _mapIdsToStatus.Values; }
		}
	}
}
