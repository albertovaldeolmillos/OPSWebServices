using System;
using System.Security.Cryptography;
using System.IO;

using OPS.Comm.Common.Channel;

namespace OPS.Comm.Common.Cryptography.TripleDes
{
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

		protected byte[] Encriptar (byte[] data)
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
			while ((b=encStream.ReadByte ()) !=-1) { bw.Write ((byte)b);}
			byte[] baRetval = retval.GetBuffer();
			bw.Close();
			ms.Close();
			return baRetval;
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
