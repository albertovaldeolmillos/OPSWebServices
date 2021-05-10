using System;
using System.Security.Cryptography;
using System.IO;

using OPS.Comm.Channel;

namespace OPS.Comm.Cryptography.TripleDes
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

		public byte[] Encriptar (byte[] data)
		{


			byte [] bEnc=null;
			int blockSizeBytes = _cs.BlockSize / 8;
			byte[] buf = new byte[blockSizeBytes];			// bytes we will encrypt each time.

			byte[] ddata = data;
			int modulus =data.Length % (blockSizeBytes); 
			if (modulus !=0) 
			{
				modulus = blockSizeBytes - modulus;
				ddata = new byte[data.Length + modulus];
				Array.Clear(ddata,0,ddata.Length);
				Array.Copy (data, 0, ddata, 0, data.Length);
			}
			
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			ICryptoTransform encrypto = _cs.CreateEncryptor();
			CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
			cs.Write(ddata, 0, ddata.Length);
			cs.FlushFinalBlock();	
			bEnc=ms.GetBuffer();

			byte [] byTemp = bEnc;

			bEnc=null;
			bEnc=new byte[data.Length + modulus];
			Array.Copy (byTemp,0, bEnc,0, data.Length + modulus);
		
			return bEnc;
		}


		public byte[] Desencriptar(byte[] data)
		{
			byte [] bDec=null;

			byte [] ddata= new byte[data.Length+16];
			Array.Clear(ddata,0,ddata.Length);
			Array.Copy(data,0,ddata,0,data.Length);

			System.IO.MemoryStream ms = new System.IO.MemoryStream(ddata,0,ddata.Length);
			bDec= new byte[data.Length];
			ICryptoTransform encrypto = _cs.CreateDecryptor();
			CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
			cs.Read(bDec,0,bDec.Length);	
		
			byte [] byTemp = bDec;
			/*int i = 0;
			for (i = byTemp.Length-1; i>=0; i--)
				if (byTemp[i] != 0)
					break;				
			i++;*/

			bDec=null;
			bDec=new byte[data.Length];
			Array.Copy (byTemp,0, bDec,0, data.Length);
		
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
