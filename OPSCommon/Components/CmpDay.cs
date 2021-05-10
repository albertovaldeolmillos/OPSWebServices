using System;
using System.Data;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Contains info about a Day. Info about a Day is all his IDs and (optionally)
	/// all his DDAY_IDs
	/// All processing of DDAY_ID is delegated on CmpDayDef object, which can be created
	/// automatically for that class
	/// </summary>
	public class CmpDay
	{
		private CmpDaysDef _daysdef;				// DaysDef info
		private DateTime _day;						// date processed
	
		private int _id;							// DAY ID
		private string _desc;						// DAY_DESC
		/// <summary>
		/// Creates a new CmpDay object associated to a specific day
		/// </summary>
		/// <param name="day">DateTime specifing the day</param>
		/// <param name="getDefInfo">If true a CmpDaysDef object with DAYS_DEF info will be created</param>
		public CmpDay(DateTime day, bool getDefInfo)
		{
			_id = -1;
			_desc = null;
			_day = day;
			if (getDefInfo) _daysdef = new CmpDaysDef (day);
		}

		/// <summary>
		/// Explicity Loads the info about that day (and optionally its DAY_DEF info if created) from the Database.
		/// That method is automatically called when needed, but it can be called explicity too.
		/// </summary>
		public void Load()
		{
			if (_id!=-1) return;				// Call UnLoad explicity first

			// Load data from days
			CmpDaysDB cmpdays = new CmpDaysDB();
			DataTable days = cmpdays.GetData (null, "DAY_DATE=@DAYS.DAY_DATE@",new object[] {_day});
			if (days.Rows.Count > 0)
			{
				if (days.Rows.Count > 1) 
				{
					// TODO: Log the incoherence.
				}
				_id = Convert.ToInt32(days.Rows[0]["DAY_ID"]);
				_desc = Convert.ToString(days.Rows[0]["DAY_DESC"]);
			}
			
			// Load data from daysdef
			if (_daysdef!=null) _daysdef.Load();
		}

		/// <summary>
		/// Unload data previously loaded. If Load() is called two times, second call will have no
		/// efect: is needed to call UnLoad first
		/// </summary>
		public void UnLoad () 
		{ 
			_id= -1;
			_desc = null;
			if (_daysdef != null) { _daysdef.UnLoad();}
		}

		/// <summary>
		/// Gets the number of DAYS rows that contains the current date (note that is 0 or 1).
		/// Note that if you call Count before calling Load result will be 0
		/// </summary>
		public int Count
		{
			get 
			{ 
				return _id!=-1 ? 1 : 0;
			}
		}

		/// <summary>
		/// Gets and Sets the new day to process
		/// </summary>
		public DateTime Date
		{
			set 
			{
				if (value!=_day) 
				{
					_day = value;
					_id =-1;			
					_desc = null;
					Load();
					if (_daysdef!=null) { _daysdef.Date = value; }
				}
			}
			get { return _day; }
		}

		/// <summary>
		/// Gets the info about DAYS_DEF of the current date (CmpDaysDef object)
		/// Note that can be null (will be null if getDefInfo parameter was false in the ctor).
		/// </summary>
		public CmpDaysDef DaysDef
		{
			get { return _daysdef; }
		}

		/// <summary>
		/// Gets the DAY_ID of the current Day (or -1 if no DAY_ID)
		/// </summary>
		/// <returns>DAY_ID of specific register</returns>
		public int Id 
		{
			get { return _id;}
		}

		/// <summary>
		/// Gets the DAY_DESC of the current Day (or null if no DAY_DESC)
		/// </summary>
		/// <param name="idx">Index of day info to retrieve</param>
		/// <returns>DAY_DESC of specific register</returns>
		public string Desc
		{
			get { return _desc;}
		}
	}
}
