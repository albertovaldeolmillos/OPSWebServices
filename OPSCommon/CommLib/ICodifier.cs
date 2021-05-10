using System;

namespace OPS.Comm.Common.Codify
{

	/// <summary>
	/// Class that is a generic exception occurred when codifying data
	/// </summary>
	public class CodifyException : Exception
	{
		public CodifyException() : base() { }
		public CodifyException (string msg) : base (msg) { }
	}


}
