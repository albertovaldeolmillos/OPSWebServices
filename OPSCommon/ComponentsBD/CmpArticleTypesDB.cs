//using System;
//using System.Data;
//
//
//namespace OPS.Components.Data
//{
//	/// <summary>
//	/// Database component for article types (ARTICLE_DEF).
//	/// </summary>
//	public class CmpArticleTypesDB
//	{
//		/// <summary>
//		/// CmpArticleTypesDB empty constructor.
//		/// </summary>
//		public CmpArticleTypesDB() {}
//
//		/// <summary>
//		/// Gets all table ordered by Article Types long description.
//		/// </summary>
//		/// <returns>DataTable containing all Article Types information.</returns>
//		public DataTable GetData()
//		{
//			Database d = DatabaseFactory.GetDatabase();
//			return d.FillDataSet(
//				"DART_ID, DART_DESCSHORT, DART_DESCLONG, DART_TAX_ID",
//				"ARTICLES_DEF",
//				"DART_DESCLONG",
//				null,
//				"ArticleTypes",
//				-1).Tables[0];
//		}
//	}
//}