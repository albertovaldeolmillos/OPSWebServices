using System;
using System.Data;

namespace OPS.Comm.Configuration
{
	/// <summary>
	/// The interface for objects that know how to update
	/// the MSGS and MSGS_HIS tables
	/// </summary>
	public interface IMessageUpdate
	{
		/// <summary>
		/// Updates the MSGS and MSGS tables with the contents of the
		/// supplied DataSet
		/// </summary>
		/// <param name="ds">A DataSet containing rows from the MSGS table</param>
		void Update(DataSet ds);
	}
}
