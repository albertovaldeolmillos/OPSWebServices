using System;
using System.Collections.Specialized;

namespace OPS.Comm
{
	/// <summary>
	/// That interface allows a set of functions that gets a string of data (in XML)
	/// and returns all the "messages" (as XML-Strings) that are in it.
	/// </summary>
	public interface IPacketizer
	{
		/// <summary>
		/// Gets the number of items in the string
		/// </summary>
		int PacketsCount { get; }
		/// <summary>
		/// Gets the packet specified by idx (0-based)
		/// </summary>
		string this[int idx] { get; }

		/// <summary>
		/// Gets the data of the paquet (p-tag).
		/// </summary>
		PacketData PacketInfo { get;}
	}
	
	public class PacketData
	{
		private DateTime _dtx;
		private string _src;
		private bool _hasDtx;

		public PacketData (DateTime dt, string src)
		{
			_dtx = dt;
			_src = src;
			_hasDtx = true;
		}

		public PacketData (string dtx, string src)
		{
			if (dtx!=null) 
			{
				_dtx = OPS.Comm.Dtx.StringToDtx(dtx);
				_hasDtx = true;
			}
			else 
			{
				_hasDtx = false;
			}
			_src = src;
		}


		public string SourceId
		{
			get { return _src; }
		}
		public DateTime Dtx
		{
			get { return _dtx;}
		}
		public bool HasDtx 
		{
			get { return _hasDtx; }
		}

		public string DtxToString()
		{
			return OPS.Comm.Dtx.DtxToString (_dtx);
		}
	}
}