using System;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Interface that represents an ACK or a NACK
	/// </summary>
	public interface IAcknowledgeMessage
	{
		bool IsAck { get; }
	}

	/// <summary>
	/// Class representing an ACK message.
	/// </summary>
	public class AckMessage : IAcknowledgeMessage
	{
		public enum AckTypes
		{
			ACK_OK = 0,
			ACK_ERROR = 1,
			ACK_PROCESSED = 2,
			ACK_JAMMED = 3
		}

		private AckTypes _type;
		private long _id;
		string _data;

		/// <summary>
		/// Builds an Ack message to acknowledge a message
		/// </summary>
		/// <param name="id">ID of the message to acknowledge</param>
		/// <param name="type">Type of the acknowledge message</param>
		public AckMessage(long id, AckTypes type)
		{
			_id = id;
			_type = type;
			_data = null;
		}

		/// <summary>
		/// Builds an ACK_PROCESSED message with additional data (data can be null)
		/// </summary>
		/// <param name="id">ID of the message to acknowledge</param>
		/// <param name="data">data (xml string) to be inserted in the ack</param>
		public AckMessage (long id, string data)
		{
			_type = AckTypes.ACK_PROCESSED;
			_id = id;
			_data = data;
		}

		/// <summary>
		/// Returns an string representation of the ACK
		/// </summary>
		/// <returns>XML with the ACK data</returns>
		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			string stag = null;
			sb.Append ('<');
			switch (_type) 
			{
				case AckTypes.ACK_ERROR:
					stag = "ae";
					break;
				case AckTypes.ACK_JAMMED:
					stag = "aj";
					break;
				case AckTypes.ACK_OK:
					stag = "ao";
					break;
				case AckTypes.ACK_PROCESSED:
					stag = "ap";
					break;
			}
			sb.Append (stag);
			sb.Append (' ');
			sb.Append ("id=\"");
			sb.Append (Convert.ToString(_id));
			sb.Append ('"');
			if (_data != null) 
			{
				sb.Append ('>');
				sb.Append(_data);
				sb.Append ("</");
				sb.Append (stag);
				sb.Append ('>');
			}
			else 
			{
				sb.Append ("/>");
			}
			return sb.ToString();
		}

		#region IAcknowledgeMessage Members

		public bool IsAck
		{
			get { return true; }
		}

		#endregion
	}


	/// <summary>
	/// Class that represent a NACK 
	/// </summary>
	public class NackMessage : IAcknowledgeMessage
	{
		public enum NackTypes
		{
			NACK_SEMANTIC = 0,
			NACK_ERROR_BECS = 1
		}

		private NackTypes _type;
		private long _id;
		private int _errorCode;

		/// <summary>
		/// Builds an NACK message to acknowledge a message with a negative response
		/// </summary>
		/// <param name="id">ID of the message to acknowledge</param>
		/// <param name="type">Type of the not-acknowledge message</param>
		public NackMessage(long id, NackTypes type)
		{
			_id = id;
			_type = type;
			_errorCode = Int32.MinValue;
		}

		public NackMessage (long id, NackTypes type, int errorcode)
		{
			_id = id;
			_type = type;
			_errorCode = errorcode;
		}

		/// <summary>
		/// Returns an string representation of the ACK
		/// </summary>
		/// <returns>XML with the ACK data</returns>
		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			string stag = null;
			sb.Append ('<');
			switch (_type) 
			{
				case NackTypes.NACK_ERROR_BECS:
					stag = "nb";
					break;
				case NackTypes.NACK_SEMANTIC:
					stag = "ne";
					break;
			}
			sb.Append (stag);
			sb.Append (' ');
			sb.Append ("id=\"");
			sb.Append (Convert.ToString(_id));
			sb.Append ('"');

			if (_errorCode != Int32.MinValue)	// if we have error add a <error> tag inside
			{
				sb.Append ("><error>");
				sb.Append (Convert.ToString(_errorCode));
				sb.Append ("</error></");
				sb.Append (stag);
				sb.Append ('>');
			}
			else 
			{
				sb.Append ("/>");
			}
			return sb.ToString();
		}

		#region IAcknowledgeMessage Members

		public bool IsAck
		{
			get { return false; }
		}

		#endregion
	}
}
