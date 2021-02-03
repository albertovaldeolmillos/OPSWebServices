using System;
using System.Collections;

namespace OPS.Comm.Configuration
{
	/// <summary>
	/// Maps unit identifiers to their last known address
	/// </summary>
	public class AddressCache
	{
		#region Public API

		/// <summary>
		/// Returns the only instance of the AddressCache type
		/// </summary>
		/// <returns>An instance of the AddressCache type</returns>
		public static AddressCache GetAddressCache()
		{
			if (_instance == null)
			{
				_instance = new AddressCache();
			}
			return _instance;
		}
		/// <summary>
		/// Stores the address associated to an unit
		/// </summary>
		/// <param name="unitId">The unit identifier</param>
		/// <param name="address">The unit's adress</param>
		public void CacheUnitAddress(decimal unitId, string address)
		{
			_unitAdresses[unitId] = address;
		}
		/// <summary>
		/// Obtains the address of an unit
		/// </summary>
		/// <param name="unitId">The unit identifier</param>
		/// <returns>The unit address</returns>
		public string GetUnitAddress(decimal unitId)
		{
			return (string) _unitAdresses[unitId];
		}

		#endregion // Public API

		#region Private methods

		/// <summary>
		/// The only way to obtain an instance is through GetAddressCache
		/// </summary>
		private AddressCache()
		{
			OperatingSystem os = System.Environment.OSVersion;
			int initSize = (os.Platform == PlatformID.WinCE) ? 5 : 50;
			_unitAdresses = new Hashtable(initSize);
		}
		
		#endregion // Private methods

		#region Private data members

		/// <summary>
		/// Maps unit ids to adresses
		/// </summary>
		private Hashtable _unitAdresses;
		/// <summary>
		/// The only instance of AddressCache
		/// </summary>
		private static AddressCache _instance;

		#endregion // Private data members
	}
}
