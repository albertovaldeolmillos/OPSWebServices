using System;

namespace OPS.Comm
{
	/// <summary>
	/// Summary description for Dtx.
	/// </summary>
	public class Dtx
	{

		private Dtx() { }

		/// <summary>
		/// Pass a string (format hhmmssddmmyy) to a DateTime
		/// </summary>
		/// <param name="s">string to convert</param>
		/// <returns>DateTime with the same data as s</returns>
		public static DateTime StringToDtx (string s)
		{
			int hour = Convert.ToInt32(s.Substring(0,2));
			int minute = Convert.ToInt32(s.Substring(2,2));
			int second = Convert.ToInt32(s.Substring(4,2));
			int day = Convert.ToInt32(s.Substring(6,2));
			int month = Convert.ToInt32(s.Substring(8,2));
			int year = Convert.ToInt32(s.Substring(10,2));
			DateTime dt = new DateTime(2000 + year, month, day, hour, minute, second);
			return dt;
		}

		/// <summary>
		/// Pass a DateTime to a string in format (hhmmssddmmyy)
		/// </summary>
		/// <param name="dt">DateTime to convert</param>
		/// <returns>string in OPS-dtx format</returns>
		public static string DtxToString (DateTime dt)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append (dt.Hour.ToString("D2"));
			sb.Append (dt.Minute.ToString("D2"));
			sb.Append (dt.Second.ToString("D2"));
			sb.Append (dt.Day.ToString("D2"));	
			sb.Append (dt.Month.ToString("D2"));
			int year = dt.Year - 2000;					// We use only 2 digits.
			sb.Append (year.ToString("D2"));
			return sb.ToString();

		}

	}
}
