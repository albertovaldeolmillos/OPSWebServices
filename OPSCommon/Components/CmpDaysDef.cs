using System;
using System.Data;
using OPS.Components.Data;

namespace OPS.Components
{
	/// <summary>
	/// Contains helper functions to access DAYS_DEF table based on simple business rules.
	/// </summary>
	public class CmpDaysDef
	{
		private DateTime _now;				// Current date
		private DataTable _dtdaysdef;		// DataTable with all days_def associated to _now day
		/// <summary>
		/// Constructs a new CmpDaysDef associated to a specified date
		/// </summary>
		/// <param name="now">date associated to the new CmpDaysDef object created</param>
		public CmpDaysDef(DateTime now)
		{
			_now = now;
		}

		/// <summary>
		/// Constructs a new CmpDaysDef
		/// </summary>
		public CmpDaysDef()
		{
		}

		/// <summary>
		/// Loads all table days_def and stores all days_def that contains the date asasociated to that object, in its mask (dday_code)
		/// That method is automatically called when needed, but can be explicity called, too
		/// </summary>
		public void Load()
		{
			if (_dtdaysdef != null) return;				// Call UnLoad first
			CmpDaysDefDB cmp = new CmpDaysDefDB();
			_dtdaysdef = cmp.GetData ();
			foreach (DataRow daydef in _dtdaysdef.Rows)
			{
				// Get the row iif have one '1' on the position mask.
				DayOfWeek dw=  _now.DayOfWeek;
				int iw = -1;
				switch (dw)
				{
					case DayOfWeek.Monday:
						iw=0; break;
					case DayOfWeek.Tuesday:
						iw=1; break;
					case DayOfWeek.Wednesday:
						iw=2; break;
					case DayOfWeek.Thursday:
						iw=3; break;
					case DayOfWeek.Friday:
						iw=4; break;
					case DayOfWeek.Saturday:
						iw=5; break;
					case DayOfWeek.Sunday:
						iw=6; break;
				}
				string sdaydefmask = Convert.ToString(daydef["DDAY_CODE"]);
				if (sdaydefmask[iw] != '1') 
				{
					// The current day_def does not includes the _now date... so delete them.
					daydef.Delete();
				}
			}
			_dtdaysdef.AcceptChanges();
		}

		/// <summary>
		/// Loads the day definition with the specified Id
		/// </summary>
		public void Load(string	dayDefId)
		{
			if (_dtdaysdef != null) return;				// Call UnLoad first
			CmpDaysDefDB cmp = new CmpDaysDefDB();
			_dtdaysdef = cmp.GetData(null,"DDAY_ID = @DAYS_DEF.DDAY_ID@", new object[] {dayDefId});
		}


		/// <summary>
		/// Return whether the given day is included in this day definition contained in the current
		/// def collection (if there is a collection of day definitions, the function uses the first element).
		/// </summary>
		public bool IsDayIncluded(DayOfWeek	day) 
		{
			return IsDayIncluded(day, 0);
		}


		/// <summary>
		/// Return whether the given day is included in this day definition contained in the given
		/// position of the day def collection.
		/// </summary>
		public bool IsDayIncluded(DayOfWeek	day, int ndx) 
		{
			string	sMask = GetDDayCode(ndx);
			int		iw = -1;

			switch (day)
			{
				case DayOfWeek.Monday:
					iw=0; break;
				case DayOfWeek.Tuesday:
					iw=1; break;
				case DayOfWeek.Wednesday:
					iw=2; break;
				case DayOfWeek.Thursday:
					iw=3; break;
				case DayOfWeek.Friday:
					iw=4; break;
				case DayOfWeek.Saturday:
					iw=5; break;
				case DayOfWeek.Sunday:
					iw=6; break;
			}
			
			return sMask[iw] == '1';
		}

		/// <summary>
		/// Unload data previously loaded. If Load() is called two times, second call will have no
		/// efect: is needed to call UnLoad first
		/// </summary>
		public void UnLoad () { _dtdaysdef = null; }

		/// <summary>
		/// Gets the number of days_def that contains the current date
		/// </summary>
		public int Count
		{
			get 
			{ 
				if (_dtdaysdef == null) { Load(); }
				return _dtdaysdef.Rows.Count;
			}
		}

		/// <summary>
		/// Gets and Sets the new day to process
		/// </summary>
		public DateTime Date
		{
			set 
			{
				if (value!=_now) 
				{
					_now = value;
					_dtdaysdef = null;			// Data will be reloaded on next query
				}
			}
			get { return _now; }
		}

		/// <summary>
		/// Returns the ID of idx-th the DAYS_DEF
		/// </summary>
		/// <param name="idx">Index of the DAYS_DEF to retrieve (use CmpDaysDef::Count to get the current number of days_def)</param>
		/// <returns>ID of the idx-th DAYS_DEF</returns>
		public int GetDDayId (int idx)
		{
			if (_dtdaysdef == null) Load();
			return Convert.ToInt32 (_dtdaysdef.Rows[idx]["DDAY_ID"]);
		}

		/// <summary>
		/// Returns the description of idx-th the DAYS_DEF
		/// </summary>
		/// <param name="idx">Index of the DAYS_DEF to retrieve (use CmpDaysDef::Count to get the current number of days_def)</param>
		/// <returns>description of the idx-th DAYS_DEF</returns>
		public string GetDDayDesc (int idx)
		{
			if (_dtdaysdef == null) Load();
			return Convert.ToString (_dtdaysdef.Rows[idx]["DDAY_DESCSHORT"]);
		}

		/// <summary>
		/// Returns the code (mask) of idx-th the DAYS_DEF
		/// </summary>
		/// <param name="idx">Index of the DAYS_DEF to retrieve (use CmpDaysDef::Count to get the current number of days_def)</param>
		/// <returns>code (mask) of the idx-th DAYS_DEF</returns>
		public string GetDDayCode (int idx)
		{
			if (_dtdaysdef == null) Load();
			return Convert.ToString (_dtdaysdef.Rows[idx]["DDAY_CODE"]);
		}

		/// <summary>
		/// Returns true if the specified DAYS_DEF ID is contained in the current data
		/// </summary>
		/// <param name="id">ID to check</param>
		/// <returns>true if ID was found, false otherwise</returns>
		public bool ContainsId (int id)
		{
			if (_dtdaysdef == null) Load();
			for (int i=0; i< _dtdaysdef.Rows.Count; i++)
			{
				if (Convert.ToInt32 (_dtdaysdef.Rows[i]["DDAY_ID"]) == id)  return true;
			}
			return false;
		}

		/// <summary>
		/// Gets if the method Load() was called. The Load() method should be called before accessing other properties
		/// of the object, but if we forget to call Load() the object will do it for us.
		/// </summary>
		public bool IsLoaded { get { return _dtdaysdef!=null; } }

	}

}
