using System;
using System.Data;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Units (UNITS) mainteinance.
	/// </summary>
	public class CmpUnit	
	{
		/// <summary>
		/// Unit empty constructor.
		/// </summary>
		public CmpUnit() {}

		/// <summary>
		/// Gets all table ordered by Unit Short description.
		/// </summary>
		/// <returns>DataTable containing all Units information.</returns>
		public DataTable GetData()
		{
			return (new CmpUnitsDB().GetData(null,null,"UNI_DESCLONG", null));
		}

	}
}
