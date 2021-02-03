using System;
using System.Xml;

namespace OPS.Comm
{
	/// <summary>
	/// That class represents a datagram of OPS
	/// Each datagram could have one or more messages and is in format:
	///		HEADER|XML_INFO|CRC
	///		Where XML_INFO is a packet of 1 or more messages
	///	The methods on that class allow convertions between a OPSTelegramaFrame1 and a byte[]
	/// </summary>
	public  class OPSTelegramaFrame1: OPSTelegrama
	{
		/// <summary>
		/// Header of a OPS Message
		/// </summary>
		public  class Header
		{
			protected int _id;
			protected int _size;
			/// <summary>
			/// Constructs a header for a OPS Message
			/// That ctor is used when receiving data
			/// </summary>
			/// <param name="data">Array of bytes. Contains the header but can contain also data (data is not readed)</param>
			public Header(byte[] data)
			{
				// Reads the header and nothing more
				System.IO.BinaryReader br = new System.IO.BinaryReader (new System.IO.MemoryStream(data));
				_id = br.ReadInt32 ();
				_size = br.ReadInt32();		// In that case we don't add 4 (for the CRC) because we assume we are receiving a Header.
				br.Close();
				if (_size < 0 || _size > MaxSize)
					throw new OPSTelegramaException(string.Format("Tried to create a {0} bytes datagram", _size));
			}

			/// <summary>
			/// Constructs a header for a OPS Message
			/// That ctor is used when constructing an OPSTelegramaFrame1 for sending
			/// </summary>
			/// <param name="id">Id of the header</param>
			/// <param name="size">Number of bytes of the message, EXCLUDING Header, but INCLUDING CRC</param>
			public Header (int id, int size)
			{
				_id = id;
				_size = size + 4;				// +4 because the message WILL include a CRC
			}

			/// <summary>
			/// Gets an byte[] representation of the current header
			/// </summary>
			/// <returns>Array of bytes with the header. Length is Header::ByteSize bytes</returns>
			public byte[] ToByteArray()
			{
				return ToByteArray(this._id, this._size);
			}

			/// <summary>
			/// Gets the id of the Header as a Int32
			/// </summary>
			public int IdTelegrama { get { return _id; } }

			/// <summary>
			/// Gets the size (in bytes) of the message (excluding the size of the Header, but including the CRC)
			/// </summary>
			public int Size { get { return _size; } }
			
			/// <summary>
			/// Gets the size (in bytes) of the header
			/// </summary>
			public static int ByteSize { get { return 4+4; } }		// int32 + int32 size

			/// <summary>
			/// Gets an byte[] representation of a header
			/// </summary>
			/// <param name="id">Id of the header</param>
			/// <param name="size">Size of the header</param>
			/// <returns>Array of bytes with the header.</returns>
			public static byte[] ToByteArray(int id, int size)
			{
				byte[] hdata = new byte[Header.ByteSize];
				System.IO.BinaryWriter bw  =new System.IO.BinaryWriter (new System.IO.MemoryStream(hdata));
				bw.Write (id);
				bw.Write (size);
				return hdata;
			}
		}
		protected Header _header;						// Header of the message
		/// <summary>
		/// Contructs a new OPSTelegramaFrame1 with initial data. Depending of the data
		/// passed the OPSTelegramaFrame1 may be complete or incomplete
		/// </summary>
		/// <param name="data">Initial data. MUST contain the header and part of (or all) body
		/// Even thought data, can contain MORE data than the full message. In that case
		/// the method GetLastExtraBytes() will return the number of extra bytes.
		/// </param>
		public OPSTelegramaFrame1(byte[] data): base(data)
		{
			_header = null;
			// Gets the header...
			_header = new Header (data);
			// ... and gets the rest of data
			_body = new byte[_header.Size];
			// Now copies the (part) of data received to the body
			AddData (data, Header.ByteSize);
		}

        public OPSTelegramaFrame1(byte[] data,int dataLength)
            : base(data)
        {
            _header = null;
            // Gets the header...
            _header = new Header(data);
            // ... and gets the rest of data
            _body = new byte[_header.Size];
            // Now copies the (part) of data received to the body
            AddData(data, Header.ByteSize,dataLength);
        }

		/// <summary>
		/// Constructs a COMPLETE OPSTelegramaFrame1
		/// </summary>
		/// <param name="id">Id of the new OPSTelegramaFrame1</param>
		/// <param name="data">ALL data of the OPSTelegramaFrame1 (as a string)</param>
		public OPSTelegramaFrame1 (int id, string data): base(id,data)
		{
			int size = System.Text.Encoding.UTF8.GetByteCount (data);
			_header = new Header(id, size);
			// Copy the body as byte[]
			_body = new byte[System.Text.Encoding.UTF8.GetByteCount(data) + 4];		// 4 extra bytes for CRC
			System.Text.Encoding.UTF8.GetBytes(data,0, data.Length,_body, 0);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(_body, _body.Length - 4,4)).Write (_crc);;
		}
	
		/// <summary>
		/// Gets an IPacketizer that allows to get individual messages of a packet of messages
		/// </summary>
		public override IPacketizer Packetizer
		{
			get 
			{
				if (_completed)
				{
					if (_packetizer == null) _packetizer = new OPSPacketizer (XmlData);
					return _packetizer;
				}
				return null;
			}
		}

        public override int IdTelegrama { get { return _header.IdTelegrama; } }

		/// <summary>
		/// Gets data as string (returns null if message is not completed);
		/// </summary>
		public override string XmlData
		{
			get 
			{
				if (!_completed)  return null;
				if (_xmlData == null) 
				{
					_xmlData = GetDataAsString(_body, 4);
				}
				return _xmlData;
			}
		}

		/// <summary>
		/// Gets the FULL data (Header + body + CRC) as a byte[] array
		/// Returns null if message is NOT completed.
		/// </summary>
		public override byte[] FullData
		{
			get 
			{
				byte[] fullData = null;
				if (_completed) 
				{
					fullData = new byte[_body.Length + Header.ByteSize];
					byte[] headerBytes = _header.ToByteArray();
					for (int i=0; i< Header.ByteSize; i++) { fullData[i] = headerBytes[i]; }
					Array.Copy (_body,0, fullData,Header.ByteSize, _body.Length);
				}
				return fullData;
			}
		}

		/// <summary>
		/// Gets body (only body, not header nor CRC) as a byte[] array
		/// Returns null if message is NOT complete
		/// </summary>
		public override byte[] Data
		{
			get 
			{
				byte[] body = null;
				if (_completed)
				{
					body = new byte[_body.Length - 4];
					//Array.Copy (_body,body,_body.Length - 4);
					Array.Copy (_body, 0, body, 0,_body.Length - 4);
				}
				return body;
			}
		}


		/// <summary>
		/// Adds the data contained in the array to the body of the message
		/// </summary>
		/// <param name="data">Data to be added</param>
		/// <param name="offset">0-based index where data to be added starts in array</param>
		/// <returns>The number of unread bytes (bytes which belongs to an another message)</returns>
		protected override int AddData (byte[] data, int offset, int pDataLength)
		{
            int dataLength = pDataLength - offset;
			int length = -1;
			_modulus = 0;				
			if (dataLength + _count > _header.Size)	
			{
				length = _header.Size - _count;
				_modulus = dataLength - length;
			}
			else 
			{
				length = dataLength;
				if (dataLength + _count == _header.Size)
				{
					// The telegram is complete by now
					_modulus = 0;
				}
				else
				{
					// The telegram is not complete yet
					length = dataLength;
					_modulus = -1;
				}
			}
			Array.Copy (data,offset,_body, _count, length);
			_count+=length;
			if (_modulus >= 0) 
			{
				// Message is completed
				_completed = true;
			}
			return _modulus;
		}


		/// <summary>
		/// Converts the data passed as an array of bytes to string.
		/// Assumes the data is UTF8 encoded
		/// </summary>
		/// <param name="data">Array of bytes received</param>
		/// <param name="ignoreLastBytes">Number of bytes AT THE END of data, to be ignored. 
		/// This parameter is needed because at the end of data could be non-readable info (as CRC)</param>
		protected override String GetDataAsString(byte[] data, int ignoreLastBytes)
		{
			if (data.Length < ignoreLastBytes)
				return "";

			System.Text.Decoder utf8Decoder = System.Text.Encoding.UTF8.GetDecoder();
			int charCount = utf8Decoder.GetCharCount(data, 0, (data.Length  - ignoreLastBytes));
			char[] recievedChars = new char[charCount];
			utf8Decoder.GetChars(data, 0, data.Length - ignoreLastBytes, recievedChars, 0);
			String recievedString = new String(recievedChars);
			return recievedString;
		}

		/// <summary>
		/// Gets the ids of the messages contained in the packet.
		/// </summary>
		/// <returns>A string array with the ids.</returns>
		public override string[] GetMessagesId()
		{
			// Is it a XML document?
			XmlDocument doc = new XmlDocument();
			string[] msgsId = null;
			try
			{
				doc.LoadXml(XmlData);
			}
			catch
			{
				msgsId = new string[1];
				msgsId[0] = "0";
				return msgsId;
			}

			// If so, get the Ids
			msgsId = new string[doc.DocumentElement.ChildNodes.Count];
			int msgsCount = 0;
			foreach(XmlNode item in doc.DocumentElement.ChildNodes)
			{
				try
				{
					msgsId[msgsCount] = ((XmlElement)item).GetAttribute("id");
				}
				catch
				{
					msgsId[msgsCount] = "-1";
				}
				msgsCount++;
			}
			return msgsId;
		}
	}

}
