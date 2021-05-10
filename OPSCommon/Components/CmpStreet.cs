using System;
using System.Data;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Streets (STREETS) mainteinance.
	/// </summary>
	public class CmpStreet	
	{
		/// <summary>
		/// Streets empty constructor.
		/// </summary>
		public CmpStreet() {}

		/// <summary>
		/// Gets all table rows ordered by Street description.
		/// </summary>
		/// <returns>DataTable containing all Street information.</returns>
		public DataTable GetData()
		{
			return (new CmpStreetsDB().GetData(null, null, "STR_DESC", null));
		}

	}
}