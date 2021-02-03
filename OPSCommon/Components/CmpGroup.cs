using System;
using System.Data;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Groups (GROUPS) mainteinance.
	/// </summary>
	public class CmpGroup	
	{
		/// <summary>
		/// Group empty constructor.
		/// </summary>
		public CmpGroup() {}

		/// <summary>
		/// Gets all table rows ordered by Group Short description.
		/// </summary>
		/// <returns>DataTable containing all Groups information.</returns>
		public DataTable GetData()
		{
			return (new CmpGroupsDB().GetData(null,null,"GRP_DESCSHORT", null));
		}

	}
}