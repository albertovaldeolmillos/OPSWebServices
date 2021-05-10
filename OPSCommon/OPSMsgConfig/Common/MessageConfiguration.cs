using System;
using System.Collections;
using System.Data;

namespace OPS.Comm.Configuration
{
	/// <summary>
	/// A class to access the configuration data for messages
	/// </summary>
	public class MessageConfiguration
	{
		#region Public API

		/// <summary>
		/// Constants for accessing message configuration properties
		/// </summary>
		public const int MsgName = 0;
		public const int MsgTypeDesc = 1;
		public const int ProcessTime = 2;
		public const int UserTime = 3;
		public const int MsgId = 4;
		public const int HisMandatory = 5;
		public const int Priority = 6;
		public const int MediaId = 7;
		public const int Mandatory = 8;
		public const int Order = 9;
		public const int IPAdapter = 10;
		public const int PortAdapter = 11;
		public const int TotalRetries = 12;
		public const int PartialRetries = 13;
		public const int TotalInterval = 14;
		public const int PartialInterval = 15;
		public const int TotalTime = 16;
		public const int DestUnitId = 17;
		public const int DestUnitPort = 18;
		public const int Xsl = 19;

		public MessageConfiguration()
		{
		}
		/// <summary>
		/// The connection to use to access the database
		/// </summary>
		public IDbConnection Connection
		{
			get { return _connection; }
			set { _connection = value; }
		}
		/// <summary>
		/// Obtains the configuration for a message to be sent from
		/// specifics source and destination
		/// </summary>
		/// <param name="msgTag">The name of the message to be sent</param>
		/// <param name="srcUnitId">The identifier of the sender</param>
		/// <param name="dstUnitId">The identifier of the destination</param>
		/// <returns>A DataSet containing the configuration of the
		/// desired message. The DataSet has the following structure:
		///	DMSG_PROCESSTIME	NUMERIC, 
		///	DMSG_USERTIME		NUMERIC,  
		///	MSG_DMSG_ID			NUMERIC, 
		///	MSG_HISMANDATORY	NUMERIC,  
		///	MSG_PRIORITY		NUMERIC, 
		///	MSG_MMSG_ID			NUMERIC, 
		///	MSG_MANDATORY		NUMERIC, 
		///	MSG_ORDER			NUMERIC, 
		///	MSG_IPADAPTER		NVARCHAR(20), 
		///	MSG_PORTADAPTER		NUMERIC, 
		///	MSG_TOTALRETRIES	NUMERIC, 
		///	MSG_PARCIALRETRIES	NUMERIC, 
		///	MSG_TOTALINTERVAL	NUMERIC, 
		///	MSG_PARCIALINTERVAL	NUMERIC, 
		///	MSG_TOTALTIME		NUMERIC,  
		///	MSG_UNI_ID			NUMERIC, 
		///	MSG_UNI_PORT		NUMERIC,
		///	MSG_XSL				NVARCHAR(255)
		/// </returns>
		/// <remarks>The results are ordered by:
		/// MSG_MANDATORY ASC, MSG_PRIORITY DESC, MSG_ORDER DESC</remarks>
		public DataSet GetConfiguration(
			string msgTag, decimal srcUnitId, decimal dstUnitId)
		{
			DataSet retVal = null;
			MsgConfigurationKey key = new MsgConfigurationKey(msgTag, srcUnitId, dstUnitId);
			if (_msgCfgCache.ContainsKey(key))
			{
				retVal = (DataSet) _msgCfgCache[key];
			}
			else
			{
				retVal = LoadConfiguration(msgTag, srcUnitId, dstUnitId);
				_msgCfgCache.Add(key, retVal);
			}
			return retVal;
		}
		
		/// <summary>
		/// Returns the list of PriorityDestinationGroup of the 
		/// pending messages in the MSGS table
		/// </summary>
		/// <returns>A list of PriorityDestinationGroup objects</returns>
		public virtual PriorityDestinationGroupList GetPriorityGroups()
		{
			return null;
		}
		/// <summary>
		/// Returns the messages belonging to a priority and destination group
		/// </summary>
		/// <param name="g">The group of the requested messages</param>
		/// <returns>A dataset containing the messages having the same
		/// priority and destination. They are ordered by the values of their
		/// retry policy</returns>
		public virtual DataSet GetPriorityGroupMessages(PriorityDestinationGroup g)
		{
			return null;
		}
		
		#endregion //Public API

		#region Private methods

		/// <summary>
		/// Loads the configuration of a message from the database
		/// </summary>
		/// <param name="msgTag">The name of the message</param>
		/// <param name="srcUnitId">The identifier of the sender</param>
		/// <param name="dstUnitId">The identifier of the destination</param>
		/// <returns></returns>
		protected virtual DataSet LoadConfiguration(string msgTag, 
			decimal srcUnitId, decimal dstUnitId)
		{
			return null;
		}

		#endregion // Private methods
		
		#region Private data members

		/// <summary>
		/// The cache for message configuration data
		/// </summary>
		protected Hashtable _msgCfgCache;
		/// <summary>
		/// The connection to use to access the database
		/// </summary>
		protected IDbConnection _connection;

		#endregion // Private data members
	}

	/// <summary>
	/// The key for searching the message configuration cache
	/// </summary>
	internal class MsgConfigurationKey
	{
		internal MsgConfigurationKey(string msgTag, decimal srcId, decimal dstId)
		{
			_msgTag = msgTag;
			_scrId = srcId;
			_dstId = dstId;
		}
		public override int GetHashCode()
		{
			string str = String.Format("{0}-{1}-{2}", 
				_msgTag, _scrId, _dstId);
			return str.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			bool retVal = false;
			if (obj is MsgConfigurationKey)
			{
				MsgConfigurationKey key = (MsgConfigurationKey) obj;
				retVal = _msgTag.Equals(key._msgTag) && 
					_scrId.Equals(key._scrId) && _dstId.Equals(key._dstId);
			}
			return retVal;
		}


		private string _msgTag;
		private decimal _scrId;
		private decimal _dstId;
	}
}
