using System;

namespace OPS.Comm.Media
{
	/// <summary>
	/// The known media identifiers
	/// </summary>
	public enum MediaType : int
	{
		CcBase = 1, CcWireless, PdaPdmIrda, PdmCc, CcSmsAdapter, CcFaxAdapter,
		
		LastMedia
	}
	/// <summary>
	/// The signature for media availability event handlers
	/// </summary>
	/// <param name="mediaType">The medium whose availability has changed</param>
	/// <param name="available">Indicates whether the medium is available or
	/// not</param>
	public delegate void MediaAvailabilityHandler(MediaType mediaType, 
		bool available);
	/// <summary>
	/// An interface that specifies methods to know the availability
	/// of communication interfaces
	/// </summary>
	public interface CommMediaContext
	{
		/// <summary>
		/// Returns whether a specific medium is availablity or not
		/// </summary>
		/// <param name="mediaType">The medium whose availability needs to be 
		/// known</param>
		/// <returns>true if the medium is available, false otherwise</returns>
		bool IsMediaAvailable(MediaType mediaType);
		/// <summary>
		/// The event fired when the availability of any medium changes
		/// </summary>
		event MediaAvailabilityHandler MediaAvailabilityChange;
	}
}
