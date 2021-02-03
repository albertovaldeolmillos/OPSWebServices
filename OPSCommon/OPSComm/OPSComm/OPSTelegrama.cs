using System;
using System.Xml;

namespace OPS.Comm
{
	/// <summary>
	/// That class represents a datagram of OPS
	/// Each datagram could have one or more messages and is in format:
	///		HEADER|XML_INFO|CRC
	///		Where XML_INFO is a packet of 1 or more messages
	///	The methods on that class allow convertions between a OPSTelegrama and a byte[]
	/// </summary>
	public abstract class OPSTelegrama
	{
		internal const int MaxSize = 1000 * 1024;

		protected string _xmlData;					// data as xml
		protected byte[] _body;						// data as byte[]. The LAST four bytes are the CRC!!!!
		protected int _count;							// # bytes received 
		protected bool _completed;					// true if msg is completed
		protected int _modulus;						// extra bytes
		protected uint _crc;							// 4 byte CRC (appended after message)
		protected bool _bCorrect;
		protected IPacketizer _packetizer;			// packetizer of messages
		/// <summary>
		/// Contructs a new OPSTelegrama with initial data. Depending of the data
		/// passed the OPSTelegrama may be complete or incomplete
		/// </summary>
		/// <param name="data">Initial data. MUST contain the header and part of (or all) body
		/// Even thought data, can contain MORE data than the full message. In that case
		/// the method GetLastExtraBytes() will return the number of extra bytes.
		/// </param>
		public OPSTelegrama(byte[] data) 
		{
			_xmlData = null;
			_completed = false;
			_count  = 0;
			_modulus = 0;
			_crc = 0;	
			_bCorrect=true;
		}

        public OPSTelegrama(byte[] data,int dataLength)
        {
            _xmlData = null;
            _completed = false;
            _count = 0;
            _modulus = 0;
            _crc = 0;
            _bCorrect = true;
        }

		/// <summary>
		/// Constructs a COMPLETE OPSTelegrama
		/// </summary>
		/// <param name="id">Id of the new OPSTelegrama</param>
		/// <param name="data">ALL data of the OPSTelegrama (as a string)</param>
		public OPSTelegrama (int id, string data)
		{
			_xmlData = null;
			_completed = true;
			_count  = 0;
			_modulus = 0;
			_crc = 0;	
			_bCorrect=true;

		}
	
		/// <summary>
		/// Gets if OPSTelegrama is completed.
		/// </summary>
		public bool Completed { get { return _completed;} }
		public bool Correct { get { return _bCorrect;} }

		/// <summary>
		/// Gets an IPacketizer that allows to get individual messages of a packet of messages
		/// </summary>
		public abstract IPacketizer Packetizer
		{
			get;
		}

		/// <summary>
		/// Gets data as string (returns null if message is not completed);
		/// </summary>
		public abstract string XmlData
		{
			get;
		}

		/// <summary>
		/// Gets the FULL data (Header + body + CRC) as a byte[] array
		/// Returns null if message is NOT completed.
		/// </summary>
		public abstract byte[] FullData
		{
			get;
		}
		
		/// <summary>
		/// Gets body (only body, not header nor CRC) as a byte[] array
		/// Returns null if message is NOT complete
		/// </summary>
		public abstract byte[] Data
		{
			get;
		}

        public abstract int IdTelegrama { get; }


		/// <summary>
		/// Adds the data contained in the array to the body of the message
		/// The param 'data' can contain MORE data than the remaining data of the OPSTelegrama.
		/// In that case only needed data is used, and method GetLastExtraBytes can be used to
		/// know how many bytes belongs to another message (also can be used the return value for
		/// the same puporse)
		/// </summary>
		/// <param name="data">Data to be added</param>
		/// <returns>The number of unread bytes (bytes which belongs to an another message)</returns>
		public int AddData (byte[] data)
		{
			return AddData (data, 0);
		}
		/// <summary>
		/// Adds the data contained in the array to the body of the message
		/// </summary>
		/// <param name="data">Data to be added</param>
		/// <param name="offset">0-based index where data to be added starts in array</param>
		/// <returns>The number of unread bytes (bytes which belongs to an another message)</returns>
		
		protected  int AddData (byte[] data, int offset)
		{
            return AddData(data, offset, data.Length);
		}

        public int AddData(int dataLength, byte[] data)
        {
            return AddData(data, 0, dataLength);
        }

        protected abstract int AddData(byte[] data, int offset, int pDataLength);
        

		/// <summary>
		/// Returns the last extra bytes. Not implemented as a property in order to emphatize that is
		/// a "special" method:
		///		Result of that method is only valid after one call to AddData or ctor.
		/// </summary>
		/// <returns></returns>
		public int GetLastExtraBytes ()
		{
			return _modulus;
		}

		/// <summary>
		/// Converts the data passed as an array of bytes to string.
		/// Assumes the data is UTF8 encoded
		/// </summary>
		/// <param name="data">Array of bytes received</param>
		/// <param name="ignoreLastBytes">Number of bytes AT THE END of data, to be ignored. 
		/// This parameter is needed because at the end of data could be non-readable info (as CRC)</param>
		protected abstract String GetDataAsString(byte[] data, int ignoreLastBytes);

		/// <summary>
		/// Gets the ids of the messages contained in the packet.
		/// </summary>
		/// <returns>A string array with the ids.</returns>
		public abstract string[] GetMessagesId();
	}

	public class OPSTelegramaException : Exception
	{
		public OPSTelegramaException()
		{
		}
		public OPSTelegramaException(string message) : base(message)
		{
		}
	}
}
