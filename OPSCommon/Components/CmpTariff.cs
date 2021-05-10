using System;
using System.Data;
using System.Collections;
using OPS.Components.Data;
using System.Text;

namespace OPS.Components
{
	/// <summary>
	/// Functions for operate with tariffs
	/// </summary>
	public class CmpTariff
	{

		#region Nested classes

		public class Tariff
		{
			/// <summary>
			/// Constructs a Tariff with the data contained in the DataRow (datarow of TARIFF table)
			/// </summary>
			/// <param name="drTariff">DataRow with data.</param>
			
			public readonly int TAR_ID;
			public readonly int TAR_NUMBER;
			public readonly string TAR_DESCSHORT;
			public readonly string TAR_DESCLONG;
			public readonly int TAR_TAR_ID;
			public readonly double TAR_DISCOUNT; 
			public readonly int TAR_STAR_ID;
			public readonly int TAR_DDAY_ID;
			public readonly int TAR_DAY_ID;
			public readonly int TAR_TIM_ID;
			public readonly DateTime TAR_INIDATE;
			public readonly DateTime TAR_ENDDATE;
			public readonly bool TAR_NEXTDAY;
			public readonly bool TAR_NEXTBLOCK;
			public readonly bool TAR_RNEXTBLOCKTIME;
			public readonly bool TAR_RNEXTBLOCKINT;
			public readonly bool TAR_RNEXTDAYINT;
			public readonly bool TAR_RNEXTDAYTIME;
			public readonly bool TAR_ADDFREETIME;

			/// <summary>
			/// Constructs a new Tariff object with the typed info of a TARIFF DataRow passed
			/// </summary>
			/// <param name="drTariff">Row with info about the TARIFF to be built</param>
			internal Tariff (DataRow drTariff)
			{
				// Store the row in the public readonly vars
				TAR_ID = Convert.ToInt32 (drTariff["TAR_ID"]);
				TAR_NUMBER = Convert.ToInt32 (drTariff["TAR_NUMBER"]);
				TAR_DESCSHORT = drTariff["TAR_DESCSHORT"] != DBNull.Value ? Convert.ToString (drTariff["TAR_DESCSHORT"]) : string.Empty;
				TAR_DESCLONG = drTariff["TAR_DESCLONG"]   != DBNull.Value ? Convert.ToString (drTariff["TAR_DESCLONG"]) : string.Empty;
				TAR_TAR_ID = drTariff["TAR_TAR_ID"] != DBNull.Value ? Convert.ToInt32 (drTariff["TAR_TAR_ID"]) : -1 ;
				TAR_DISCOUNT = drTariff["TAR_DISCOUNT"] !=DBNull.Value ? Convert.ToDouble (drTariff["TAR_DISCOUNT"]) : 0.0;
				TAR_STAR_ID = drTariff["TAR_STAR_ID"] != DBNull.Value ? Convert.ToInt32 (drTariff["TAR_STAR_ID"]) : -1;
				TAR_DDAY_ID = drTariff["TAR_DDAY_ID"] != DBNull.Value ? Convert.ToInt32 (drTariff["TAR_DDAY_ID"]) : -1;
				TAR_DAY_ID = drTariff["TAR_DAY_ID"] != DBNull.Value ? Convert.ToInt32 (drTariff["TAR_DAY_ID"]) : -1;
				TAR_TIM_ID = drTariff["TAR_TIM_ID"] != DBNull.Value ? Convert.ToInt32 (drTariff["TAR_TIM_ID"]) : -1;
				TAR_INIDATE = drTariff["TAR_INIDATE"] !=DBNull.Value ? Convert.ToDateTime (drTariff["TAR_INIDATE"]) : DateTime.MinValue ;
				TAR_ENDDATE = drTariff["TAR_ENDDATE"] != DBNull.Value ? Convert.ToDateTime (drTariff["TAR_ENDDATE"]) : DateTime.MaxValue;
				TAR_NEXTDAY = drTariff["TAR_NEXTDAY"] != DBNull.Value ? Convert.ToBoolean (drTariff["TAR_NEXTDAY"]) : false;
				TAR_NEXTBLOCK = drTariff["TAR_NEXTDAY"] != DBNull.Value ? Convert.ToBoolean (drTariff["TAR_NEXTDAY"]) : false;
				TAR_RNEXTBLOCKTIME = drTariff["TAR_RNEXTBLOCKTIME"] != DBNull.Value ? Convert.ToBoolean (drTariff["TAR_RNEXTBLOCKTIME"]) : false;
				TAR_RNEXTBLOCKINT = drTariff["TAR_RNEXTBLOCKINT"] != DBNull.Value ? Convert.ToBoolean (drTariff["TAR_RNEXTBLOCKINT"]) : false;
				TAR_RNEXTDAYINT	 = drTariff["TAR_RNEXTDAYINT"] != DBNull.Value ? Convert.ToBoolean (drTariff["TAR_RNEXTDAYINT"]) : false;
				TAR_RNEXTDAYTIME  = drTariff["TAR_RNEXTDAYTIME"] != DBNull.Value ? Convert.ToBoolean (drTariff["TAR_RNEXTDAYTIME"]) : false;
				TAR_ADDFREETIME = drTariff["TAR_ADDFREETIME"] != DBNull.Value ? Convert.ToBoolean(drTariff["TAR_ADDFREETIME"]) : false;
			}
		}

		public class TimeTable
		{
			public readonly int TIM_ID;
			public readonly string TIM_DESC;
			public readonly double TIM_INI;
			public readonly double TIM_END;
			//public readonly int TIM_INI_Hours;
			//public readonly int TIM_INI_Minutes;
			//public readonly int TIM_END_Hours;
			//public readonly int TIM_END_Minutes;
			public readonly int TIM_Delta_Minutes;

			/// <summary>
			/// Constructs a new TimeTable based on data contained on DataRow (register of TIMETABLE table) passed
			/// </summary>
			/// <param name="dr">DataRow with data used to construct the TimeTable object</param>
			internal TimeTable (DataRow dr)
			{
				TIM_ID = Convert.ToInt32 (dr["TIM_ID"]);
				TIM_DESC = Convert.ToString (dr["TIM_DESC"]);
				//TIM_INI = dr["TIM_INI"]!=DBNull.Value ? Convert.ToInt32 (dr["TIM_INI"]) :0;
				TIM_INI = dr["TIM_INI"]!=DBNull.Value ? Convert.ToDouble(dr["TIM_INI"]) :0.0;
				//TIM_END = dr["TIM_END"]!=DBNull.Value ? Convert.ToInt32(dr["TIM_END"]) : 24;
				TIM_END = dr["TIM_END"]!=DBNull.Value ? Convert.ToDouble(dr["TIM_END"]) : 24.0;
				//TIM_INI_Hours = Convert.ToInt32(TIM_INI);
				//TIM_INI_Minutes = Convert.ToInt32( (TIM_INI-TIM_INI_Hours)*100 );
				//TIM_END_Hours = Convert.ToInt32(TIM_END);
				//TIM_END_Minutes = Convert.ToInt32( (TIM_END-TIM_END_Hours)*100 );
				TIM_Delta_Minutes = (Convert.ToInt32(TIM_END) - Convert.ToInt32(TIM_INI))*60 +
					Convert.ToInt32( (TIM_END-Convert.ToInt32(TIM_END))*100 ) - 
					Convert.ToInt32( (TIM_INI-Convert.ToInt32(TIM_INI))*100 );
			}
		}

		#endregion

		private int _tarId;
		private DateTime _pInDate;
		/// <summary>
		/// Constructs a CmpTariff associated to a specified Tariff and date
		/// </summary>
		/// <param name="tarid">ID of the tariff</param>
		/// <param name="inDate">DateTime of the tariff</param>
		public CmpTariff(int tarid, DateTime inDate) { _tarId = tarid; _pInDate = inDate;}

		/// <summary>
		/// Obtains the tariff associated to the ID and day (or day_type) specified.
		/// </summary>
		/// <param name="cmpDaysDef">CmpDaysDef with the info about days_def assigned to the current Date.</param>
		/// <returns>ArrayList of Tarifs objects</returns>
		public ArrayList ObtainTariff(CmpDay cmpday)
		{
			ArrayList tariffs = new ArrayList();
			CmpDaysDef cmpdaysdef = cmpday.DaysDef;
						
			// Filter the TARIFF table by TAR_ID. As a result we will found a set of registers
			//	Foreach row that the TAR_DAY_ID is NOT NULL check if TAR_DAY_ID is a DAY_ID of the pInDate day.
			//  If it is we have found a tariff.
			//  If none of the registers have TAR_DAY_ID not NULL or we have not found the tariff yet, we get all
			//  registers with a TAR_DDAY_ID NOT NULL, and find the 1st that his DDAY_ID has the current day.
			
			CmpTariffsDB cmp = new CmpTariffsDB();
			string swhere = "TAR_ID = @TARIFFS.TAR_ID@ AND (TAR_INIDATE <=@TARIFFS.TAR_INIDATE@ OR TAR_INIDATE IS NULL) AND (TAR_ENDDATE>= @TARIFFS.TAR_ENDDATE@ OR TAR_ENDDATE IS NULL)";
			DataTable dtTariffs = cmp.GetData (null, swhere, new object[] {_tarId, _pInDate, _pInDate});

			// Step 1: Check all TAR_DAY_ID not null
			DataRow [] tarDayId = dtTariffs.Select ("TAR_DAY_ID IS NOT NULL");
			if (tarDayId.Length > 0) 
			{
				// Get all DAY_ID that correspond to _pInDate (can be more than one).
				if (cmpday.Count > 0) 
				{
					// Finally checks every DAY_ID of the TARIFFS found to IDs stored in daysid array
					foreach (DataRow drTariff in tarDayId)
					{
						int dayid = Convert.ToInt32(drTariff["TAR_DAY_ID"]);
						// Check if the DAY with day_id is the same day of _pInDate. If it is we have found the tariff (a part
						// of the tarif, because remember that we can have more than one register).
						if (dayid == cmpday.Id) 
						{	
							// We have found it!!! We have found it!!
							tariffs.Add (new Tariff (drTariff));

						}
					}
				}
			}

			// Search now by day_def and acumulate the results.
			DataRow[] tarDdayId = dtTariffs.Select ("TAR_DDAY_ID IS NOT NULL");
			if (tarDdayId.Length > 0) 
			{
				// Get al DDAY_ID that contains the _pInDate (that is, have a 1 in the mask for the _pInDate day) and store
				// them all (CmpDaysDef will store them for us ;))
				if (!cmpdaysdef.IsLoaded) { cmpdaysdef.Load(); }
				if (cmpdaysdef.Count > 0)
				{
					// If we have found at least one DDAY_ID that contains the pInDate process the tariffs...
					foreach (DataRow drTariff in tarDdayId)
					{
						int ddayid = Convert.ToInt32(drTariff["TAR_DDAY_ID"]);
						// Check if DDAY_ID is a day_def including the day _pInDate. If it is we have found de tariff
						if (cmpdaysdef.ContainsId (ddayid)) 
						{
							// We have found it!!! We have found it!!
							tariffs.Add (new Tariff (drTariff));
						}
					}
				}
			}
			return tariffs;				 // If no tariff was found will have Count == 0
		}


		/// <summary>
		/// Get the timetables associated to a tar_id.
		/// </summary>
		/// <param name="tariffs">ArrayList with CmpTariff.Tariff objects (obtained by CmpTariff::ObtainTariff())</param>
		/// <returns>ArrayList of CmpTariff.TimeTable objects with all TimeTables contained by the Tarif passed and for the Date (and hour) specified</returns>
		public ArrayList GetTimeTables(ArrayList tariffs)
		{
			ArrayList arTimeTables = new ArrayList();
			// Foreach element of tariffs arraylist get the TIM_ID and check if the TIM_ID is for a TimeTable
			// that contains _pInDate or is later (in time).
			
			// First of all get all the aplicable timetables (that is all the timetables thaT apply for the current _pInDate)
			CmpTimetablesDB cmpTimetablesDB = new CmpTimetablesDB();
			DataTable dtTimetables =  cmpTimetablesDB.GetData (null, "TIM_END > @TIMETABLES.TIM_END@ OR TIM_END IS NULL", "TIM_END ASC", new object[] {_pInDate.Hour });
			// Ok... now do de iteration over tariffs elements
			foreach (object o in tariffs)
			{
				// Get the TIM_ID specified on the tariff
				int timid = ((Tariff)o).TAR_TIM_ID;
				// And if it is found in dtTimeTables is added to the array
				for (int i=0;i<dtTimetables.Rows.Count;i++) 
				{
					if (Convert.ToInt32(dtTimetables.Rows[i]["TIM_ID"]) == timid) 
					{
						// Add the current timetable to the array of timetables specified...
						arTimeTables.Add (new TimeTable (dtTimetables.Rows[i]));
					}
				}
			}
			return arTimeTables;
		}

	}
}
