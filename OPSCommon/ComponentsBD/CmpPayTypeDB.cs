//using System;
//using System.Data;
//
//
//namespace OPS.Components.Data
//{
//	/// <summary>
//	/// Database component for payment methods (PAY_TYPES).
//	/// </summary>
//	public class CmpPayTypeDB
//	{
//		/// <summary>
//		/// PayTypeDB empty constructor.
//		/// </summary>
//		public CmpPayTypeDB() {}
//
//		/// <summary>
//		/// Gets all table ordered by Paying Method description.
//		/// </summary>
//		/// <returns>DataTable containing all Paying Methods information.</returns>
//		public DataTable GetData()
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			return d.FillDataSet("DPAY_ID, DPAY_DESCSHORT",
//									"PAYTYPES_DEF",
//									"DPAY_DESCSHORT",
//									null,
//									"PayTypes",
//									-1).Tables[0];
//		}
//	}
//}
