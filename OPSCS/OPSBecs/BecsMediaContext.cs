using System;
using OPS.Comm.Media;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// Summary description for BecsMediaContext.
	/// </summary>
	public class BecsMediaContext : CommMediaContext
	{
		#region Public API

		public BecsMediaContext()
		{
		}

		public bool IsMediaAvailable(MediaType mediaType)
		{
			// TODO:  Add BecsMediaContext.IsMediaAvailable implementation
			return true;
		}

		public event MediaAvailabilityHandler MediaAvailabilityChange;

		#endregion // Public API
	} 
}
