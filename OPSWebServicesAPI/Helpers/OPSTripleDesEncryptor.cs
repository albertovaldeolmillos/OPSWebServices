using System;
using System.Security.Cryptography;

namespace OPSWebServicesAPI.Helpers
{
	/// <summary>
	/// When processing data using an IInternalSink indicates if channel is being processes in forward order
	/// or in reverse order.
	/// </summary>
	public enum InternalSinkProcessOrder
	{
		/// <summary>
		/// Channel is being processed in forward order
		/// </summary>
		forward = 1,
		/// <summary>
		/// Channel is being processed in reverse order
		/// </summary>
		reverse = 2
	}

	/// <summary>
	/// A Sink is a process that is attached to a CommChannel in order to process the data.
	/// There are two types of sinks
	///		EndPointSinks that act with another client.
	///		InternalSinks that act with another sinks
	/// </summary>
	public interface ISink
	{
	}

	/// <summary>
	/// An EndPointSink is located at the end (or beginning) of a channel, translating data
	/// that goes into the channel (byte[]) to external data (string).
	/// </summary>
	public interface IEndPointSink : ISink
	{
		/// <summary>
		/// Transforms the internal data to external representation (string)
		/// </summary>
		/// <param name="data">Internal data to transform</param>
		/// <returns>External representation of data (as a string)</returns>
		string DecodifyData(byte[] data);
		/// <summary>
		/// Transforms the part of internal data to external representation (string)
		/// </summary>
		/// <param name="data">Internal data to transform</param>
		/// <param name="offset">Offset where data to transform starts</param>
		/// <param name="len">Number of bytes to transform.</param>
		/// <returns>External representation of data (as a string)</returns>
		string DecodifyData(byte[] data, int offset, int len);

		/// <summary>
		/// Transforms external data (string) to the internal representation allowed by the channel
		/// </summary>
		/// <param name="data">Data to transform</param>
		/// <returns>Array of bytes with the internal data.</returns>
		byte[] CodifyData(string data);
	}

	/// <summary>
	/// An InternalSink is located inside the Channel, doing some process of the data.
	/// It acts with the internal data (byte[]).
	/// </summary>
	public interface IInternalSink : ISink
	{
		/// <summary>
		/// Processes the data that is passing for the Channel
		/// </summary>
		/// <param name="data">Data to process</param>
		/// <param name="order">Order which channel is processed.</param>
		/// <returns>Data after process. Can be NULL if process does not modify the data</returns>
		byte[] ProcessData(byte[] data, InternalSinkProcessOrder order);
	}


	/// <summary>
	/// That class is an adapter to implement IEndPointSink interface.
	/// Implements all overloads, making implementation of IEndPointSink easier.
	/// </summary>
	public abstract class EndPointSinkAdapter : IEndPointSink
	{
		/// <summary>
		/// Transforms external data (string) to the internal representation allowed by the channel
		/// </summary>
		/// <param name="data">Data to transform</param>
		/// <returns>Array of bytes with the internal data.</returns>
		public abstract byte[] CodifyData(string data);

		/// <summary>
		/// Decodify all data conteined in data calling IEndPointSkink::DecodifyData (data, 0, data.Length).
		/// Not virtual. If you need to override that overload, don't inherit from EndPointSinkAdapter: just implement IEndPointSink
		/// </summary>
		/// <param name="data">Internal data to transform</param>
		/// <returns>External representation of data (as a string)</returns>		
		public string DecodifyData(byte[] data)
		{
			return DecodifyData(data, 0, data.Length);
		}

		/// <summary>
		/// Transforms the part of internal data to external representation (string)
		/// </summary>
		/// <param name="data">Internal data to transform</param>
		/// <param name="offset">Offset where data to transform starts</param>
		/// <param name="len">Number of bytes to transform.</param>
		/// <returns>External representation of data (as a string)</returns>
		public abstract string DecodifyData(byte[] data, int offset, int len);
	}

	/// <summary>
	/// That class is an InternalSink that encrypts/decrypts data using TripleDES algorithm
	/// </summary>
	public class OPSTripleDesEncryptor : IInternalSink
	{
		TripleDESCryptoServiceProvider _cs;

		/// <summary>
		/// Constructs a TripleDesEncryptor with a specified secret key and initialization vector
		/// </summary><
		/// <param name="cs">Object TripleDESCryptoServiceProvider containing the key and init vector</param>
		public OPSTripleDesEncryptor(TripleDESCryptoServiceProvider cs)
		{
			_cs = cs;
		}

		/*protected byte[] Encriptar (byte[] data)
		{
			int blockSizeBytes = _cs.BlockSize / 8;
			byte[] buf = new byte[blockSizeBytes];			// bytes we will encrypt each time.

			byte[] ddata = data;
			int modulus =data.Length % (blockSizeBytes); 
			if (modulus !=0) 
			{
				modulus = blockSizeBytes - modulus;
				ddata = new byte[data.Length + modulus];
				Array.Copy (data, 0, ddata, 0, data.Length);
				for (int i=0; i< modulus; i++)
				{
					ddata[ddata.Length - (i+1)] = 0;
				}
			}
			// Creates a memory stream auto-expandable...
			MemoryStream ms = new MemoryStream();
			// Get a tripleDES provider and create the CryptoStream
			CryptoStream encStream = new CryptoStream (ms,
				_cs.CreateEncryptor (_cs.Key, _cs.IV), CryptoStreamMode.Write);

			// Encode data in groups of BlockSize
			for (int i=0; i< ddata.Length; i+=buf.Length)
			{
				Array.Copy (ddata, i, buf, 0, buf.Length);
				encStream.Write (buf, 0, buf.Length);
			}

			// Gets the whole encrypted data (stored in memory stream behind the crypto stream).
			byte[] retval  = new byte[ms.Length];
			Array.Copy (ms.GetBuffer(), 0, retval, 0, ms.Length);
			ms.Close();
			return retval;
		}

		protected byte[] Desencriptar(byte[] data)
		{
			MemoryStream ms = new MemoryStream(data);
			// Get a tripleDES provider and create the CryptoStream
			CryptoStream encStream = new CryptoStream (ms,
				_cs.CreateDecryptor(_cs.Key, _cs.IV), CryptoStreamMode.Read);
			// Decrypts the whole data...
			MemoryStream retval = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(retval);
			int b;
			string temp="";
			int i=0;
			try
			{
				while (((i<(data.Length-16)&&(b=encStream.ReadByte ()) !=-1))) 
				{ 
					bw.Write ((byte)b);
					System.Text.Decoder utf8Decoder = System.Text.Encoding.UTF8.GetDecoder();
					int charCount = utf8Decoder.GetCharCount(retval.GetBuffer(), 0, retval.GetBuffer().Length);
					char[] recievedChars = new char[charCount];
					utf8Decoder.GetChars(retval.GetBuffer(), 0, retval.GetBuffer().Length , recievedChars, 0);
					temp = new String(recievedChars);
					i++;
				}
			}
			catch(Exception ex)
			{
				string e=ex.Message;
			}
			byte[] baRetval = retval.GetBuffer();
			bw.Close();
			ms.Close();
			return baRetval;
		}*/

		public byte[] Encriptar(byte[] data)
		{


			byte[] bEnc = null;
			int blockSizeBytes = _cs.BlockSize / 8;
			byte[] buf = new byte[blockSizeBytes];          // bytes we will encrypt each time.

			byte[] ddata = data;
			int modulus = data.Length % (blockSizeBytes);
			if (modulus != 0)
			{
				modulus = blockSizeBytes - modulus;
				ddata = new byte[data.Length + modulus];
				Array.Clear(ddata, 0, ddata.Length);
				Array.Copy(data, 0, ddata, 0, data.Length);
			}

			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			ICryptoTransform encrypto = _cs.CreateEncryptor();
			CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
			cs.Write(ddata, 0, ddata.Length);
			cs.FlushFinalBlock();
			bEnc = ms.GetBuffer();

			byte[] byTemp = bEnc;

			bEnc = null;
			bEnc = new byte[data.Length + modulus];
			Array.Copy(byTemp, 0, bEnc, 0, data.Length + modulus);

			return bEnc;
		}


		public byte[] Desencriptar(byte[] data)
		{
			byte[] bDec = null;

			byte[] ddata = new byte[data.Length + 16];
			Array.Clear(ddata, 0, ddata.Length);
			Array.Copy(data, 0, ddata, 0, data.Length);

			System.IO.MemoryStream ms = new System.IO.MemoryStream(ddata, 0, ddata.Length);
			bDec = new byte[data.Length];
			ICryptoTransform encrypto = _cs.CreateDecryptor();
			CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
			cs.Read(bDec, 0, bDec.Length);

			byte[] byTemp = bDec;
			/*int i = 0;
			for (i = byTemp.Length-1; i>=0; i--)
				if (byTemp[i] != 0)
					break;				
			i++;*/

			bDec = null;
			bDec = new byte[data.Length];
			Array.Copy(byTemp, 0, bDec, 0, data.Length);

			return bDec;
		}


		#region IInternalSink Members

		public byte[] ProcessData(byte[] data, InternalSinkProcessOrder order)
		{
			if (order == InternalSinkProcessOrder.forward)
			{
				return Encriptar(data);
			}
			else
			{
				return Desencriptar(data);
			}
		}

		#endregion

	}
}