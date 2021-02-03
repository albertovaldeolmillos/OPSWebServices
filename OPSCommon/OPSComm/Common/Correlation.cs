using System;
using System.Threading;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Manages the generation of packet identifiers
	/// </summary>
	public class Correlation
	{
		#region Public API

		/// <summary>
		/// Read-only property that returns the next packet identifier
		/// </summary>
		public static int NextId
		{
			get
			{
				return Interlocked.Increment(ref _correlationNumber);
			}
		}
	
		#endregion  // Public API

		#region Private data members

		private static int _correlationNumber = 0;
			
		#endregion  // Private data members
	}
}
