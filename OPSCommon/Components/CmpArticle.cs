using System;
using System.Data;

using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Summary description for CmpArticle.
	/// </summary>
	public class CmpArticle
	{

		/// <summary>
		/// Class that represents a set of rules to be applied to an article (ARTICLES_RULES entry)
		/// </summary>
		public class ArticleRules
		{
			
			private Article _article;

			public readonly int RulId;
			public readonly int GrpId;
			public readonly int DgrpId;
			public readonly int TarifaId;
			public readonly int ConstraintId;
			public readonly DateTime IniDate;
			public readonly DateTime EndDate;
			private bool _hasIniDate;
			private bool _hasEndDate;
			/// <summary>
			/// Constructs a new ArticleRules object.
			/// </summary>
			/// <param name="drData">DataRow containing the rules (ARTICLES_RULES row)</param>
			/// <param name="article">Article for which the rules are applied</param>
			public ArticleRules (DataRow drData, Article article)
			{
				_article = article;
				RulId = Convert.ToInt32(drData["RUL_ID"]);
				GrpId = drData["RUL_GRP_ID"] != DBNull.Value ? Convert.ToInt32(drData["RUL_GRP_ID"]) : -1;
				DgrpId = drData["RUL_DGRP_ID"] != DBNull.Value ? Convert.ToInt32(drData["RUL_DGRP_ID"]) : -1;
				IniDate = (_hasIniDate = ( drData["RUL_INIDATE"] != DBNull.Value)) ? Convert.ToDateTime(drData["RUL_INIDATE"]) : DateTime.MinValue;
				EndDate = (_hasEndDate = ( drData["RUL_ENDDATE"] != DBNull.Value )) ? Convert.ToDateTime(drData["RUL_ENDDATE"]) : DateTime.MaxValue;
				TarifaId = Convert.ToInt32(drData["RUL_TAR_ID"]);
				ConstraintId = Convert.ToInt32(drData["RUL_CON_ID"]);
			}
			public bool HasIniDate { get { return _hasIniDate; } }
			public bool HasEndDate { get { return _hasEndDate; } } 
			public CmpArticle.Article Article { get { return _article; } } 
		}

		/// <summary>
		/// Class that represents an Article
		/// </summary>
		public class Article 
		{
			public readonly int Id;
			public readonly int DartId;
			public readonly int CusId;
			public readonly DateTime IniDate;
			public readonly DateTime EndDate;
			public readonly bool HasIniDate;
			public readonly bool HasEndDate;
			public readonly string VehicleId;

			/// <summary>
			/// Builds a new Article
			/// </summary>
			/// <param name="dr">DataRow containing all info except VehicleID</param>
			/// <param name="vehicleid">VehicleID associated to the article</param>
			public Article (DataRow dr, string vehicleid)
			{
				Id = Convert.ToInt32 (dr["ART_ID"]);
				DartId = Convert.ToInt32 (dr["ART_DART_ID"]);
				CusId =  dr["ART_CUS_ID"] != DBNull.Value ? Convert.ToInt32 (dr["ART_CUS_ID"]) : -1 ;
				HasIniDate = dr["ART_INIDATE"] != DBNull.Value; 
				if (HasIniDate) 
				{
					IniDate = Convert.ToDateTime (dr["ART_INIDATE"]);
				}
				HasEndDate = dr["ART_ENDDATE"] != DBNull.Value;
				if (HasEndDate)
				{
					EndDate = Convert.ToDateTime (dr["ART_ENDDATE"]);
				}

				VehicleId = vehicleid;
			}
		}

		public CmpArticle() {}

		/// <summary>
		/// Given a Vehicle and a DateTime obtains the Article assigned to that Vehicle ID
		/// </summary>
		/// <param name="vehicleID">Vehicle to use</param>
		/// <param name="date">DateTime to check</param>
		/// <param name="OkValue">Value of ART_STATUS that is considered to be 'Ok'</param>
		/// <returns>A Article with all info</returns>
		public static Article GetArticle (string vehicleID, DateTime date, int OkValue)
		{
			DataTable dt = new CmpArticlesDB().GetArticle (vehicleID, date, OkValue);
			if (dt.Rows.Count == 0) return null;
			// Get the first row (contains the article that we want).
			DataRow dr = dt.Rows[0];
			return new Article (dr, vehicleID);
		}

		/// <summary>
		/// Returns if an article
		/// </summary>
		/// <param name="arttype">Article Type</param>
		/// <returns>true if NOT artcicle is requiered for the specified article type</returns>
		public static bool ArticleCanBeNull (int arttype)
		{
			return new CmpArticlesDB().ArticleCanBeNull (arttype);
		}

		/// <summary>
		/// Get the rules for a Article given one group/or group type (GROUP or GROUPS_DEF item)
		/// </summary>
		/// <param name="art">Article to use</param>
		/// <param name="item">Group or Group type to use</param>
		/// <returns>An ArticleRules object with data</returns>
		public static ArticleRules GetArticleRules (Article art, int artDefId, CmpStatus.StatusTreeItem item, DateTime time)
		{
			string swhere = null;
			object[] whereValues = null;
			if (item.IsGroup) 
			{
				swhere = "RUL_GRP_ID = @ARTICLES_RULES.RUL_GRP_ID@";
				whereValues	 = new object[] {item.Id, null, null, null};
			}
			else if (item.IsType)
			{
				swhere = "RUL_DGRP_ID = @ARTICLES_RULES.RUL_DGRP_ID@";
				whereValues = new object[] {item.IdType, null, null, null};
			}
			else 
			{
				// We have only ARTICLES_RULES entries for GROUPS or GROUPS_DEF but not for UNITS!
				// TODO: Log the error.
				return null;
			}
			swhere = swhere + " AND (RUL_ENDDATE >= @ARTICLES_RULES.RUL_ENDDATE@ OR RUL_ENDDATE IS NULL) AND (RUL_INIDATE <= @ARTICLES_RULES.RUL_INIDATE@ OR RUL_INIDATE IS NULL)";
			whereValues[1] = time;
			whereValues[2] = time;

			// Now we add the filter by article type
			swhere += " AND (RUL_DART_ID = @ARTICLES_RULES.RUL_DART_ID@)";
			whereValues[3] = (art != null ? art.DartId : artDefId);

			CmpArticlesRulesDB cmp = new CmpArticlesRulesDB();
			DataTable dt =  cmp.GetData (null, swhere,whereValues);
			if (dt.Rows.Count > 1)
			{
				// TODO: Log the error [We must find at most one register]
				return null;
			}
			else if (dt.Rows.Count == 0)
			{
				// This is no error
				return null;
			}
			// Return the first row of the DataTable
			return new ArticleRules (dt.Rows[0], art);
		}
	}
}
