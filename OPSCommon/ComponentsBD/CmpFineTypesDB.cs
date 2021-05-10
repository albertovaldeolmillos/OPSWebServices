//using System;
//using System.Data;
//
//
//namespace OPS.Components.Data
//{
//	/// <summary>
//	/// Database component for Fine Types (FINES_DEF).
//	/// </summary>
//	public class CmpFineTypesDB
//	{
//		/// <summary>
//		/// CmpFineTypesDB empty constructor.
//		/// </summary>
//		public CmpFineTypesDB() {}
//
//		/// <summary>
//		/// Gets most used columns ordered by long description.
//		/// </summary>
//		/// <returns>DataTable containing all FineTypes information.</returns>
//		public DataSet GetData()
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			return d.FillDataSet("DFIN_ID, DFIN_COD_ID, DFIN_DESCSHORT, DFIN_DESCLONG, DFIN_VALUE",
//				"FINES_DEF",
//				"DFIN_DESCLONG",
//				null,
//				"FINES_DEF",
//				-1);
//		}
//	}
//}
