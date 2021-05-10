using System;
using System.Xml;
using System.Security.Cryptography;
using OPS.Comm.Cryptography.TripleDes;
using OPS.Comm.Channel;
using ZLibNet;
using System.IO;

namespace OPS.Comm
{
	/// <summary>
	/// That class represents a datagram of OPS
	/// Each datagram could have one or more messages and is in format:
	///		STX|HEADER|XML_INFO|CRC|ETX
	///		Where XML_INFO is a packet of 1 or more messages
	///	The methods on that class allow convertions between a OPSTelegramaFrame2 and a byte[]
	/// </summary>
	public  class OPSTelegramaFrame2: OPSTelegrama
	{
		internal const string KEY_MESSAGE_TCP_0	= "75o73K3%0=53?73*h>7*32<5";
		internal const string KEY_MESSAGE_TCP_1	= "35s03!*3!8H3j33*53)73*lf";
		internal const string KEY_MESSAGE_TCP_2	= "7*32z5$8j07!3*35f5%73(30";
		internal const string KEY_MESSAGE_TCP_3	= "*5%57*3j3!*50,73*3(65k3%";
		internal const string KEY_MESSAGE_TCP_4	= "3!*50g73*5=57*3j$8j07!3*";
		internal const string KEY_MESSAGE_TCP_5	= "j07!(*h>7*32<5y8n%=!g5/&";
		internal const string KEY_MESSAGE_TCP_6	= "!8H37t3*5*3(65k3%57*3j3!";
		internal const string KEY_MESSAGE_TCP_7	= "253)73*lf5%73(30*32z5$8j";
		
		internal const byte STX = 0x02;
		internal const byte ETX = 0x03;

		internal const int  CRC_TABLE_LEN=256;
		static	 uint [] gs_plCrc32Table=null;
		
		bool _bUseNewKey = false;
		static int _nEncryptionOption = 0;		// 0 -> Use only old key, 1 -> Use only new key, 2 -> Use both keys

		/// <summary>
		/// Header of a OPS Message
		/// </summary>
		public class Header
		{
			protected byte _startChar;
			protected int _id;
			protected int _size;
			protected bool _bIsCompressed;

			/// <summary>
			/// Constructs a header for a OPS Message
			/// That ctor is used when receiving data
			/// </summary>
			/// <param name="data">Array of bytes. Contains the header but can contain also data (data is not readed)</param>
			public Header(byte[] data)
			{
				// Reads the header and nothing more
				System.IO.BinaryReader br = new System.IO.BinaryReader (new System.IO.MemoryStream(data));
				_startChar = br.ReadByte();
				_id = br.ReadInt32 ();
				_bIsCompressed= (br.ReadByte()!=0);
				_size = br.ReadInt32();		// In that case we don't add 4 (for the CRC) because we assume we are receiving a Header.
				br.Close();
				if (_size < 0 || _size > MaxSize)
					throw new OPSTelegramaException(string.Format("Tried to create a {0} bytes datagram", _size));
			}

			/// <summary>
			/// Constructs a header for a OPS Message
			/// That ctor is used when constructing an OPSTelegramaFrame2 for sending
			/// </summary>
			/// <param name="id">Id of the header</param>
			/// <param name="size">Number of bytes of the message, EXCLUDING Header, but INCLUDING CRC</param>
			public Header (int id, int size,bool bIsCompressed)
			{
				_startChar = STX;
				_id = id;
				_bIsCompressed=bIsCompressed;
				_size = size + 4+1;				// +4+1 because the message WILL include a CRC+ETX
			}

			/// <summary>
			/// Gets an byte[] representation of the current header
			/// </summary>
			/// <returns>Array of bytes with the header. Length is Header::ByteSize bytes</returns>
			public byte[] ToByteArray()
			{
				return ToByteArray(this._startChar,this._id,this._bIsCompressed, this._size);
			}

			/// <summary>
			/// Gets the id of the Header as a Int32
			/// </summary>
			public int IdTelegrama { get { return _id; } }

			/// <summary>
			/// Gets the size (in bytes) of the message (excluding the size of the Header, but including the CRC)
			/// </summary>
			public int Size { get { return _size; }  set { _size = value; }}
			public bool IsCompressed { get { return _bIsCompressed; } set { _bIsCompressed = value; }}			
			/// <summary>
			/// Gets the size (in bytes) of the header
			/// </summary>
			public static int ByteSize { get { return 1+4+1+4; } }		// int32 + int32 size

			/// <summary>
			/// Gets an byte[] representation of a header
			/// </summary>
			/// <param name="id">Id of the header</param>
			/// <param name="size">Size of the header</param>
			/// <returns>Array of bytes with the header.</returns>
			public static byte[] ToByteArray(byte startChar,int id, bool bIsCompressed, int size)
			{
				byte[] hdata = new byte[Header.ByteSize];
				System.IO.BinaryWriter bw  =new System.IO.BinaryWriter (new System.IO.MemoryStream(hdata));
				bw.Write(startChar);
				bw.Write (id);
				bw.Write(bIsCompressed);
				bw.Write (size);
				return hdata;
			}
		}
		protected Header _header;						// Header of the message
		protected byte _endChar;
		protected bool _bIsEncrypted;

		/// <summary>
		/// Contructs a new OPSTelegramaFrame2 with initial data. Depending of the data
		/// passed the OPSTelegramaFrame2 may be complete or incomplete
		/// </summary>
		/// <param name="data">Initial data. MUST contain the header and part of (or all) body
		/// Even thought data, can contain MORE data than the full message. In that case
		/// the method GetLastExtraBytes() will return the number of extra bytes.
		/// </param>
		public OPSTelegramaFrame2(byte[] data): base(data)
		{
			_bIsEncrypted=true;
			_header = null;
			// Gets the header...
			_header = new Header (data);
			// ... and gets the rest of data
			_body = new byte[_header.Size];
			_endChar= ETX;
			_bCorrect=false;
			// Now copies the (part) of data received to the body
			AddData (data, Header.ByteSize);
			
		}

        public OPSTelegramaFrame2(byte[] data,int dataLength, int nEncryptionOption)
            : base(data)
        {
            _bIsEncrypted = true;
            _header = null;
            // Gets the header...
            _header = new Header(data);
            // ... and gets the rest of data
            _body = new byte[_header.Size];
            _endChar = ETX;
            _bCorrect = false;
			_nEncryptionOption = nEncryptionOption;
            // Now copies the (part) of data received to the body
            AddData(data, Header.ByteSize,dataLength);

        }

		public OPSTelegramaFrame2(byte[] data,int dataLength, bool bUseNewKey)
			: base(data)
		{
			_bIsEncrypted = true;
			_header = null;
			// Gets the header...
			_header = new Header(data);
			// ... and gets the rest of data
			_body = new byte[_header.Size];
			_endChar = ETX;
			_bCorrect = false;
			_bUseNewKey = bUseNewKey;
			// Now copies the (part) of data received to the body
			AddData(data, Header.ByteSize,dataLength);

		}

		/// <summary>
		/// Constructs a COMPLETE OPSTelegramaFrame2
		/// </summary>
		/// <param name="id">Id of the new OPSTelegramaFrame2</param>
		/// <param name="data">ALL data of the OPSTelegramaFrame2 (as a string)</param>
		public OPSTelegramaFrame2 (int id, string data): base(id,data)
		{
			_bIsEncrypted=false;
			int size = System.Text.Encoding.UTF8.GetByteCount (data);
			_header = new Header(id, size,false);
			_endChar= ETX;
			// Copy the body as byte[]
			_body = new byte[System.Text.Encoding.UTF8.GetByteCount(data) + 4+1];		// 4+1 extra bytes for CRC+ETX
			System.Text.Encoding.UTF8.GetBytes(data,0, data.Length,_body, 0);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(_body, _body.Length - 5,4)).Write (_crc);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(_body, _body.Length - 1,1)).Write (_endChar);

		}

		public OPSTelegramaFrame2 (int id, string data, bool bUseNewKey): base(id,data)
		{
			_bIsEncrypted=false;
			int size = System.Text.Encoding.UTF8.GetByteCount (data);
			_header = new Header(id, size,false);
			_endChar= ETX;
			_bUseNewKey = bUseNewKey;
			// Copy the body as byte[]
			_body = new byte[System.Text.Encoding.UTF8.GetByteCount(data) + 4+1];		// 4+1 extra bytes for CRC+ETX
			System.Text.Encoding.UTF8.GetBytes(data,0, data.Length,_body, 0);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(_body, _body.Length - 5,4)).Write (_crc);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(_body, _body.Length - 1,1)).Write (_endChar);

		}

		public OPSTelegramaFrame2(byte[] data,int dataLength)
			: base(data)
		{
			_bIsEncrypted = true;
			_header = null;
			// Gets the header...
			_header = new Header(data);
			// ... and gets the rest of data
			_body = new byte[_header.Size];
			_endChar = ETX;
			_bCorrect = false;
			// Now copies the (part) of data received to the body
			AddData(data, Header.ByteSize,dataLength);

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
					_xmlData = GetDataAsString(_body, 5);
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
					bool bIsCompressed=false;
					byte [] byRes=TransformBodyToSend(ref bIsCompressed);

					
					fullData = new byte[byRes.Length + Header.ByteSize];
					_header.Size=byRes.Length;
					_header.IsCompressed=bIsCompressed;
					byte[] headerBytes = _header.ToByteArray();
					for (int i=0; i< Header.ByteSize; i++) { fullData[i] = headerBytes[i]; }					
					Array.Copy (byRes,0, fullData,Header.ByteSize, byRes.Length);
				}
				return fullData;
			}
		}

		protected byte [] TransformBodyToSend(ref bool bIsCompressed)
		{

			byte [] body= new byte[_body.Length-5];
			Array.Clear(body,0,body.Length);
			Array.Copy(_body,0,body,0,_body.Length-5);

			body=DropUnWantedChars(body);
			body=CompressBodyToSend(body,ref bIsCompressed);
			body=EncryptBodyToSend(body);
			_crc=CalculateCRC32(body,body.Length);

			byte [] byTemp=new byte[body.Length+5];
			Array.Copy(body,0,byTemp,0,body.Length);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(byTemp, byTemp.Length - 5,4)).Write (_crc);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(byTemp, byTemp.Length - 1,1)).Write (_endChar);

			return byTemp;
		}

		


		protected byte [] DropUnWantedChars(byte [] body)
		{

			byte [] byRes=null;
			byte [] byTemp=body;

			int iNewLen=0;
			byte temp;
			for (int i=0;i<byTemp.Length;i++)
			{
				if ((byTemp[i]!=10)&&(byTemp[i]!=13))
				{
					temp=byTemp[i];
					byTemp[iNewLen++]=temp;
				}
			}
			


			byRes=null;
			byRes=new byte[iNewLen];
			Array.Copy(byTemp,0,byRes,0,iNewLen);
			return byRes;
		}

		protected byte [] EncryptBodyToSend(byte [] body)
		{
			string strKey = KEY_MESSAGE_TCP_5;
			if ( _bUseNewKey )
				strKey = "k0XVqt/$lCgEw3FV75$84wjZ";
			TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
			//int sizeKey = System.Text.Encoding.UTF8.GetByteCount (KEY_MESSAGE_TCP_5);
			int sizeKey = System.Text.Encoding.UTF8.GetByteCount (strKey);
			byte [] byKey;
			byKey = new byte[sizeKey];	
			//System.Text.Encoding.UTF8.GetBytes(KEY_MESSAGE_TCP_5,0, KEY_MESSAGE_TCP_5.Length,byKey, 0);
			System.Text.Encoding.UTF8.GetBytes(strKey,0, strKey.Length,byKey, 0);
			TripleDesProvider.Mode=CipherMode.ECB;
			TripleDesProvider.Key=byKey;
			Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
					
			OPSTripleDesEncryptor OPSTripleDesEnc= new OPSTripleDesEncryptor(TripleDesProvider);
			byte [] byRes=null;
			byte [] byTemp;

			byTemp=OPSTripleDesEnc.ProcessData(body,InternalSinkProcessOrder.forward);

			byRes=null;
			byRes=new byte[byTemp.Length];
			Array.Copy(byTemp,0,byRes,0,byTemp.Length);
			return byRes;
		}

		protected byte [] CompressBodyToSend(byte [] body, ref bool bIsCompressed)
		{
            byte[] byRes = null;
            byte[] byTemp = null;
            MemoryStream inMemStream = new MemoryStream(body, 0, body.Length);
            MemoryStream outMemStream = new MemoryStream();

            try
            {
                ZLibStream zipStream = new ZLibStream(outMemStream, CompressionMode.Compress, CompressionLevel.Level9);

                BinaryReader dataReader;
                BinaryWriter dataWriter;
                dataReader = new BinaryReader(inMemStream);
                dataWriter = new BinaryWriter(zipStream);

                CopyStream(dataReader, dataWriter);
                dataWriter.Flush();
                byTemp = outMemStream.GetBuffer();

                if (body.Length > outMemStream.Length)
                {
                    byRes = new byte[outMemStream.Length];
                    Array.Copy(byTemp, 0, byRes, 0, outMemStream.Length);
                    bIsCompressed = true;
                }
                else
                {
                    byRes = new byte[body.Length];
                    Array.Copy(body, 0, byRes, 0, body.Length);
                    bIsCompressed = false;
                }

                dataReader.Close();
                dataWriter.Close();


            }
            finally
            {
                outMemStream.Close();
                inMemStream.Close();
            }

            return byRes;

        }		

		public static void CopyStream(System.IO.Stream input, System.IO.BinaryWriter dataWriter)
		{
			byte[] buffer = new byte[2048];
			int len;
			while ((len = input.Read(buffer, 0, 2048)) > 0)
			{
				dataWriter.Write(buffer, 0, len);
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
					body = new byte[_body.Length - 5];
					//Array.Copy (_body,body,_body.Length - 4);
					Array.Copy (_body, 0, body, 0,_body.Length - 5);
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
        protected override int AddData(byte[] data, int offset, int pDataLength)
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
            Array.Copy(data, offset, _body, _count, length);
            _count += length;
            if (_modulus >= 0)
            {
                // Message is completed
                _completed = true;
            }

            if (_completed)
            {

                System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.MemoryStream(_body, _body.Length - 5, 5));
                _crc = br.ReadUInt32();
                _endChar = br.ReadByte();
                br.Close();

                uint uiCalcCRC = CalculateCRC32(_body, _body.Length - 5);

                if (uiCalcCRC == _crc)
                {
                    _bCorrect = true;
                    try
                    {
						// 27/09/2016
//						if ( _nEncryptionOption == 1 )
//							_bUseNewKey = true;
//						else
//							_bUseNewKey = false;
						TransformReceivedBody();
                    }
                    catch
                    {
						// 27/09/2016
//						if ( _nEncryptionOption == 2 )
//						{
//							try
//							{
//								_bUseNewKey = true;
//								TransformReceivedBody();
//							}
//							catch
//							{
//								_bCorrect = false;
//							}
//						}
//						else
							_bCorrect = false;
                    }
                }
            }

            return _modulus;
        }



		protected void TransformReceivedBody()
		{


			byte [] byTemp= new byte[_body.Length-5];
			Array.Clear(byTemp,0,byTemp.Length);
			Array.Copy(_body,0,byTemp,0,_body.Length-5);
			_body=null;
			_body=new byte[byTemp.Length];
			Array.Copy(byTemp,0,_body,0,byTemp.Length);

	

			if (_bIsEncrypted)
			{
				_body=DecryptReceivedBody(_body);
			}

			if (_header.IsCompressed)
			{
				_body=UnCompressReceivedBody(_body);
			}

			// 27/09/2016
//			if ( _body == null )
//			{
//				_body=new byte[byTemp.Length];
//				Array.Copy(byTemp,0,_body,0,byTemp.Length);
//
//				_bUseNewKey = !_bUseNewKey;
//				if (_bIsEncrypted)
//				{
//					_body=DecryptReceivedBody(_body);
//				}
//
//				if (_header.IsCompressed)
//				{
//					_body=UnCompressReceivedBody(_body);
//				}
//			}

			byTemp = _body;
			int i = 0;
			for (i = byTemp.Length-1; i>=0; i--)
				if (byTemp[i] != 0)
					break;

			i++;
			_body=null;
			_body=new byte[i+5];
			Array.Copy(byTemp,0,_body,0,i);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(_body, _body.Length - 5,4)).Write (_crc);
			new System.IO.BinaryWriter (new System.IO.MemoryStream(_body, _body.Length - 1,1)).Write (_endChar);


		}

		
		protected byte [] DecryptReceivedBody(byte [] body)
		{
			string strKey = KEY_MESSAGE_TCP_5;
			if ( _bUseNewKey )
				strKey = "k0XVqt/$lCgEw3FV75$84wjZ";
			TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
			//int sizeKey = System.Text.Encoding.UTF8.GetByteCount (KEY_MESSAGE_TCP_5);
			int sizeKey = System.Text.Encoding.UTF8.GetByteCount (strKey);
			byte [] byKey;
			byKey = new byte[sizeKey];	
			//System.Text.Encoding.UTF8.GetBytes(KEY_MESSAGE_TCP_5,0, KEY_MESSAGE_TCP_5.Length,byKey, 0);
			System.Text.Encoding.UTF8.GetBytes(strKey,0, strKey.Length,byKey, 0);
			TripleDesProvider.Mode=CipherMode.ECB;
			TripleDesProvider.Key=byKey;
			Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
					
			OPSTripleDesEncryptor OPSTripleDesEnc= new OPSTripleDesEncryptor(TripleDesProvider);
			byte [] byRes=null;
			byte [] byTemp;

			byTemp=OPSTripleDesEnc.ProcessData(body,InternalSinkProcessOrder.reverse);

			byRes=null;
			byRes=new byte[byTemp.Length];
			Array.Copy(byTemp,0,byRes,0,byTemp.Length);
			return byRes;
		}

		protected byte [] UnCompressReceivedBody(byte [] body)
        {
            byte[] byRes = null;
            byte[] byTemp = null;
            MemoryStream inMemStream = new MemoryStream(body, 0, body.Length);
            MemoryStream outMemStream = new MemoryStream();

            try
            {
                System.IO.BinaryWriter dataWriter;
                System.IO.BinaryReader dataReader;

                ZLibStream zipStream = new ZLibStream(inMemStream, CompressionMode.Decompress, CompressionLevel.Level9);

                dataReader = new BinaryReader(zipStream);
                dataWriter = new BinaryWriter(outMemStream);
                CopyStream(dataReader, dataWriter);
                dataWriter.Flush();
                byTemp = outMemStream.GetBuffer();

                byRes = new byte[outMemStream.Length];
                Array.Copy(byTemp, 0, byRes, 0, outMemStream.Length);

                dataWriter.Close();
                dataReader.Close();
            }
            catch (Exception e)
            {
                byRes = null;
            }
            finally
            {
                outMemStream.Close();
                inMemStream.Close();
            }

            return byRes;

        }		


		protected void InitCRC32Table()
		{
			gs_plCrc32Table=new uint[CRC_TABLE_LEN];
			Array.Clear(gs_plCrc32Table,0,gs_plCrc32Table.Length);
			uint i, j;
			uint	uiCrc;
			for(i = 0; i < CRC_TABLE_LEN; i++)
			{
				uiCrc = i;
				for(j = 8; j > 0; j--)
				{
					if((uiCrc & 1)!=0)
						uiCrc = (uiCrc >> 1) ^ 3988292384;
					else
						uiCrc >>= 1;
				}
				gs_plCrc32Table[i] = uiCrc;
			}
		}

		protected uint CalculateCRC32(byte [] body, int iLengthForCalc)
		{
			uint uiRes=0;
			if (gs_plCrc32Table==null)
			{	
				InitCRC32Table();
			}

			for( uint i = 0; i < iLengthForCalc; i++ )
			{
				uiRes=CalculateCRC32( body[i], uiRes );
			}

			return uiRes;
		}

		protected uint CalculateCRC32(byte by,uint uiCurrCRC)
		{
			uint uiRes;

			uiRes =(uiCurrCRC >> 8) ^ gs_plCrc32Table[(by) ^ (uiCurrCRC & 0x000000FF)];

			return uiRes;
		}




        protected static void CopyStream(System.IO.BinaryReader dataReader, System.IO.BinaryWriter dataWriter)
        {
            byte[] buffer = new byte[2048];
            int len;
            while ((len = dataReader.Read(buffer, 0, 2048)) > 0)
            {
                dataWriter.Write(buffer, 0, len);
            }
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
