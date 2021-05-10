using System;
using System.Collections;
using System.Data;
using OPS.Components;
using OPS.Components.Data;

namespace OPS.Components.Statistics.Helper
{
	/// <summary>
	/// Summary description for CmpStatisticsHelper.
	/// </summary>
	
	
	public enum OutputType 
	{
		Count,
		CountDistinct,
		Average,
		Sum,
		StdDev,
		AverageLessStdDev,
		AveragePlusStdDev
	}
			  
	public class SqlHelper 
	{
		//private	string	_sSql = "";
		private	string	_sSelectFields;
		private string	_sFrom;
		private string	_sWhere;
		private	string	_sGroupBy;
		private string  _sOrderBy;

		public void AddSelectFieldAggregate(OutputType outType, string field) 
		{
			switch (outType)
			{
				case OutputType.Count:
					AddSelectFieldRight("COUNT(" + field + ") AS Numero", false);
					break;
				case OutputType.CountDistinct:
					AddSelectFieldRight("COUNT(distinct " + field + ") AS Numero", false);
					break;
				case OutputType.Average:
					AddSelectFieldRight("ROUND(AVG(" + field + "),2) As Media", false);
					break;
				case OutputType.Sum:
					AddSelectFieldRight("ROUND(SUM(" + field + "),2) As Suma", false);
					break;
				case OutputType.StdDev:
					AddSelectFieldRight("ROUND(STDDEV(" + field + "),2) As StdDev", false);
					break;
				case OutputType.AverageLessStdDev:
					AddSelectFieldRight("ROUND((AVG(" + field + ") - STDDEV(" + field + ")), 2) as ErrorInferior", false);
					break;
				case OutputType.AveragePlusStdDev:
					AddSelectFieldRight("ROUND((AVG(" + field + ") + STDDEV(" + field + ")), 2) as ErrorSuperior", false);
					break;
			}
		}

		public void AddSelectFieldRight(string field, bool bText)
		{
			if (_sSelectFields == null)
				_sSelectFields = field;
			else 
			{
				if (!bText) 
				{
					_sSelectFields += "," + field;
				}
				else 
				{
					_sSelectFields += "," + "TRIM(" + field + ") AS " + field;
				}
			}
		}

		public void AddSelectFieldLeft(string field, bool bText)
		{
			if (_sSelectFields == null)
				_sSelectFields = field;
			else 
			{
				if (!bText) 
				{
					_sSelectFields = field + "," + _sSelectFields;
				}
				else 
				{
					_sSelectFields = "TRIM(" + field + ") AS " + field + "," + _sSelectFields;
				}
			}
		}


		public void AddSelectFieldRight(string field, bool bText,string field_desc)
		{
			if (_sSelectFields == null)
				_sSelectFields = field;
			else 
			{
				string locFieldDesc;
				if (field_desc!=null)
					locFieldDesc=field_desc;
				else
					locFieldDesc=field;

				if (!bText) 
				{
					_sSelectFields += "," + field;
				}
				else 
				{
					_sSelectFields += "," + "TRIM(" + field + ") AS " + locFieldDesc;
				}
			}
		}

		public void AddSelectFieldLeft(string field, bool bText,string field_desc)
		{
			if (_sSelectFields == null)
				_sSelectFields = field;
			else 
			{
				string locFieldDesc;
				if (field_desc!=null)
					locFieldDesc=field_desc;
				else
					locFieldDesc=field;


				if (!bText) 
				{
					_sSelectFields = field + "," + _sSelectFields;
				}
				else 
				{
					_sSelectFields = "TRIM(" + field + ") AS " + locFieldDesc + "," + _sSelectFields;
				}
			}
		}

		public void AddFromJoinClause(string clause) 
		{
			if (_sFrom == null)
				_sFrom = clause;
			else
				_sFrom += " " + clause;
		}

		public void AddFromTableClause(string clause) 
		{
			if (_sFrom == null)
				_sFrom = clause;
			else
				_sFrom += "," + clause;
		}

		public void AddWhereAndClause(string clause)
		{
			if (_sWhere == null)
				_sWhere = clause;
			else
				_sWhere += " AND " + clause;
		}

		public void AddWhereOrClause(string clause)
		{
			if (_sWhere == null)
				_sWhere = clause;
			else
				_sWhere += " OR " + clause;
		}

		public void AddWhereStartBlock(string clause) 
		{
			if (_sWhere == null)
				_sWhere = "( " + clause;
			else
				_sWhere += " ( " + clause;
		}

		public void AddWhereEndBlock() 
		{
			if (_sWhere == null)
				_sWhere = ")";
			else
				_sWhere += " ) ";
		}


		public void AddGroupByClause(string clause) 
		{
			if (_sGroupBy == null)
				_sGroupBy = clause;
			else
				_sGroupBy += "," + clause;
		}

		public void AddOrderByClause(string clause) 
		{
			if (_sOrderBy == null)
				_sOrderBy = clause;
			else
				_sOrderBy += "," + clause;
		}

		public string SqlSelect
		{
			get 
			{
				return _sSelectFields;
			}
		}

		public string SqlFrom 
		{
			get 
			{
				return _sFrom;
			}
		}

		public string SqlWhere 
		{
			get 
			{
				return _sWhere;
			}
		}

		public string SqlGroupBy
		{
			get
			{
				return _sGroupBy;
			}
		}

		public string SqlOrderBy
		{
			get
			{
				return _sOrderBy;
			}
		}

		public string SqlFullSentence
		{
			get 
			{
				string	sql;

				sql = "SELECT " + _sSelectFields;
				sql += " FROM " + _sFrom;
				if (_sWhere != null)
					sql += " WHERE " + _sWhere;
				if (_sGroupBy != null)
					sql += " GROUP BY " + _sGroupBy;
				if (_sOrderBy != null)
					sql += " ORDER BY " + _sOrderBy;
				return sql;
			}
		}
	}

	public class SqlTimeManager 
	{
		protected	DateTime	_startDate;
		protected	DateTime	_endDate;
		protected	string		_dayDefId;
		protected	string		_timeTableId;

		// Group by
		protected	string		_timeField;
		protected	OPS.Components.Statistics.TimeGrouping	_timeGroup;


		public SqlTimeManager() 
		{
			this.Reset();
		}

		public void Reset() 
		{
			_startDate = DateTime.MinValue;
			_endDate = DateTime.MaxValue;
			_dayDefId = null;
			_timeTableId = null;
		}

		// Returns SQL group by (without GROUP BY)
		// TRUNC(OPE_INIDATE)
		public string SqlGroupBy 
		{
			get 
			{
					string	sql = null;

					switch(_timeGroup) 
					{
						case TimeGrouping.DayPrecisionDate:
							sql = "TO_CHAR(" + _timeField + ", 'DD/MM/YY')";
							break;
						case TimeGrouping.WeekNumber:
							sql = "TO_CHAR(" + _timeField + ", 'WW - YY')";
							break;
						case TimeGrouping.Month:
							sql = "TO_CHAR(" + _timeField + ", 'MM - MON - YY')";
							break;
						case TimeGrouping.Year:
							sql = "TO_CHAR(" + _timeField + ", 'YYYY')";
							break;
						default:
							sql = "TO_CHAR(" + _timeField + ", 'DD/MM/YY')";
							break;

					}
					return sql;
			}
		}

		public string SqlOrderBy 
		{
			get 
			{
				string	sql = null;

				switch(_timeGroup) 
				{
					case TimeGrouping.DayPrecisionDate:
						sql = "TO_CHAR(" + _timeField + ", 'YYYYMMDD')";
						break;
					case TimeGrouping.WeekNumber:
						sql = "TO_CHAR(" + _timeField + ", 'YYYYWW')";
						break;
					case TimeGrouping.Month:
						sql = "TO_CHAR(" + _timeField + ", 'YYYYMM')";
						break;
					case TimeGrouping.Year:
						sql = "TO_CHAR(" + _timeField + ", 'YYYY')";
						break;
					default:
						sql = "TO_CHAR(" + _timeField + ", 'YYYYMMDD')";
						break;
				}
				return sql;
			}
		}

		public string SqlWhere(string dateFieldTable, string dateFieldName, ArrayList paramValues) 
		{
			string	sql = "";
			string	pField = "@" + dateFieldTable + "." + dateFieldName + "@";
			
			
			if (_startDate != DateTime.MinValue) 
			{
				sql += "TRUNC(" + dateFieldName + ") >= TRUNC(" + pField + ")";
				paramValues.Add(_startDate);
			}

			if (_endDate != DateTime.MaxValue) 
			{
				if (sql != "")
					sql += " AND ";

				sql += "TRUNC(" + dateFieldName + ") < TRUNC(" + pField + ")";
				paramValues.Add(_endDate);
			}

			if (_dayDefId != null) 
			{
				// Load info
				CmpDaysDef	cDayDef = new CmpDaysDef();
				cDayDef.Load(_dayDefId);
							
				int		howManyDays = 0;
				string	sqlDayDefs = "";
				DayOfWeek[]	dow = new DayOfWeek[7] {
													  DayOfWeek.Monday, 
													  DayOfWeek.Tuesday,
													  DayOfWeek.Wednesday, 
													  DayOfWeek.Thursday, 
													  DayOfWeek.Friday, 
													  DayOfWeek.Saturday, 
													  DayOfWeek.Sunday
												  };
				
				for(int i = 1; i <= 7; i++) // Oracle days of week: 1 = Mon, 7 = Sun
				{
					bool		isIncluded = cDayDef.IsDayIncluded(dow[i-1]);
					if (isIncluded) 
					{
						if (howManyDays > 0)
							sqlDayDefs += " OR ";
						sqlDayDefs += "TO_CHAR(" + dateFieldName + ", 'D') = " + i.ToString();
						howManyDays++;
					}
				}

				if(howManyDays > 0)
					sql += " AND (" + sqlDayDefs + ") ";

			}

			if (_timeTableId != null) 
			{
				if (sql != "")
					sql += " AND ";

				CmpTimetablesDB cTimetable = new CmpTimetablesDB();
				DataTable	dtTd = cTimetable.GetData(null, "TIM_ID = @TIMETABLES.TIM_ID@", null, new object[] {_timeTableId});

				int start = Convert.ToInt32(dtTd.Rows[0]["TIM_INI"]);
				int end = Convert.ToInt32(dtTd.Rows[0]["TIM_END"]);

				sql += " (  (TO_NUMBER(TO_CHAR(" + dateFieldName + ", 'HH24'))*60+ TO_NUMBER(TO_CHAR(" + dateFieldName + ", 'MI'))) >= " + start.ToString();
				sql += " AND (TO_NUMBER(TO_CHAR(" + dateFieldName + ", 'HH24'))*60+ TO_NUMBER(TO_CHAR(" + dateFieldName + ", 'MI'))) <= " + end.ToString() + ")";

			}

			if (sql != "")
				sql = "(" + sql + ")";

			return sql;
		}

		public string SqlWhereWithNVLSysdate(string dateFieldTable, string dateFieldName, ArrayList paramValues) 
		{
			string	sql = "";
			string	pField = "@" + dateFieldTable + "." + dateFieldName + "@";
			
			
			if (_startDate != DateTime.MinValue) 
			{
				sql += "TRUNC(NVL(" + dateFieldName + ",SYSDATE)) >= TRUNC(" + pField + ")";
				paramValues.Add(_startDate);
			}

			if (_endDate != DateTime.MaxValue) 
			{
				if (sql != "")
					sql += " AND ";

				sql += "TRUNC(NVL(" + dateFieldName + ",SYSDATE)) < TRUNC(" + pField + ")";
				paramValues.Add(_endDate);
			}

			if (_dayDefId != null) 
			{
				// Load info
				CmpDaysDef	cDayDef = new CmpDaysDef();
				cDayDef.Load(_dayDefId);
							
				int		howManyDays = 0;
				string	sqlDayDefs = "";
				DayOfWeek[]	dow = new DayOfWeek[7] {
													   DayOfWeek.Monday, 
													   DayOfWeek.Tuesday,
													   DayOfWeek.Wednesday, 
													   DayOfWeek.Thursday, 
													   DayOfWeek.Friday, 
													   DayOfWeek.Saturday, 
													   DayOfWeek.Sunday
												   };
				
				for(int i = 1; i <= 7; i++) // Oracle days of week: 1 = Mon, 7 = Sun
				{
					bool		isIncluded = cDayDef.IsDayIncluded(dow[i-1]);
					if (isIncluded) 
					{
						if (howManyDays > 0)
							sqlDayDefs += " OR ";
						sqlDayDefs += "TO_CHAR(NVL(" + dateFieldName + ",SYSDATE), 'D') = " + i.ToString();
						howManyDays++;
					}
				}

				if(howManyDays > 0)
					sql += " AND (" + sqlDayDefs + ") ";

			}

			if (_timeTableId != null) 
			{
				if (sql != "")
					sql += " AND ";

				CmpTimetablesDB cTimetable = new CmpTimetablesDB();
				DataTable	dtTd = cTimetable.GetData(null, "TIM_ID = @TIMETABLES.TIM_ID@", null, new object[] {_timeTableId});

				int start = Convert.ToInt32(dtTd.Rows[0]["TIM_INI"]);
				int end = Convert.ToInt32(dtTd.Rows[0]["TIM_END"]);

				sql += " (  (TO_NUMBER(TO_CHAR(NVL(" + dateFieldName + ",SYSDATE), 'HH24'))*60+ TO_NUMBER(TO_CHAR(NVL(" + dateFieldName + ",SYSDATE), 'MI'))) >= " + start.ToString();
				sql += " AND (TO_NUMBER(TO_CHAR(NVL(" + dateFieldName + ",SYSDATE), 'HH24'))*60+ TO_NUMBER(TO_CHAR(NVL(" + dateFieldName + ",SYSDATE), 'MI'))) <= " + end.ToString() + ")";

			}

			if (sql != "")
				sql = "(" + sql + ")";

			return sql;
		}

		public string SqlWhereWithoutTrunc(string dateFieldTable, string dateFieldName, ArrayList paramValues) 
		{
			string	sql = "";
			string	pField = "@" + dateFieldTable + "." + dateFieldName + "@";
			
			
			if (_startDate != DateTime.MinValue) 
			{
				sql +=  dateFieldName + " >= TRUNC(" + pField + ")";
				paramValues.Add(_startDate);
			}

			if (_endDate != DateTime.MaxValue) 
			{
				if (sql != "")
					sql += " AND ";

				sql += dateFieldName + " < TRUNC(" + pField + ")";
				paramValues.Add(_endDate);
			}

			if (_dayDefId != null) 
			{
				// Load info
				CmpDaysDef	cDayDef = new CmpDaysDef();
				cDayDef.Load(_dayDefId);
							
				int		howManyDays = 0;
				string	sqlDayDefs = "";
				DayOfWeek[]	dow = new DayOfWeek[7] {
													   DayOfWeek.Monday, 
													   DayOfWeek.Tuesday,
													   DayOfWeek.Wednesday, 
													   DayOfWeek.Thursday, 
													   DayOfWeek.Friday, 
													   DayOfWeek.Saturday, 
													   DayOfWeek.Sunday
												   };
				
				for(int i = 1; i <= 7; i++) // Oracle days of week: 1 = Mon, 7 = Sun
				{
					bool		isIncluded = cDayDef.IsDayIncluded(dow[i-1]);
					if (isIncluded) 
					{
						if (howManyDays > 0)
							sqlDayDefs += " OR ";
						sqlDayDefs += "TO_CHAR(" + dateFieldName + ", 'D') = " + i.ToString();
						howManyDays++;
					}
				}

				if(howManyDays > 0)
					sql += " AND (" + sqlDayDefs + ") ";

			}

			if (_timeTableId != null) 
			{
				if (sql != "")
					sql += " AND ";

				CmpTimetablesDB cTimetable = new CmpTimetablesDB();
				DataTable	dtTd = cTimetable.GetData(null, "TIM_ID = @TIMETABLES.TIM_ID@", null, new object[] {_timeTableId});

				int start = Convert.ToInt32(dtTd.Rows[0]["TIM_INI"]);
				int end = Convert.ToInt32(dtTd.Rows[0]["TIM_END"]);

				sql += " (  (TO_NUMBER(TO_CHAR(" + dateFieldName + ", 'HH24'))*60+ TO_NUMBER(TO_CHAR(" + dateFieldName + ", 'MI'))) >= " + start.ToString();
				sql += " AND (TO_NUMBER(TO_CHAR(" + dateFieldName + ", 'HH24'))*60+ TO_NUMBER(TO_CHAR(" + dateFieldName + ", 'MI'))) <= " + end.ToString() + ")";

			}

			if (sql != "")
				sql = "(" + sql + ")";

			return sql;
		}


		public DateTime StartDate 
		{
			set 
			{
				_startDate = value;
			}
			get 
			{
				return _startDate;
			}
		}

		public DateTime EndDate 
		{
			set 
			{
				_endDate = value;
			}
			get 
			{
				return _endDate;
			}
		}

		public string DayDefId 
		{
			set 
			{
				_dayDefId = value;
			}
			get 
			{
				return _dayDefId;
			}
		}

		public string TimeTableId 
		{
			set 
			{
				_timeTableId = value;
			}
			get 
			{
				return _timeTableId;
			}
		}

		public string TimeField 
		{
			set 
			{
				_timeField = value;
			}
			get 
			{
				return _timeField;
			}
		}

		public OPS.Components.Statistics.TimeGrouping	TimeGroup 
		{
			set 
			{
				_timeGroup = value;
			}
			get 
			{
				return _timeGroup;
			}
		}

	}
}
