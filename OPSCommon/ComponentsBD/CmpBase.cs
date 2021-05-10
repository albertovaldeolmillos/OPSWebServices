using System;
using System.Data;
using System.Collections;
using OPS.Components.Data;
using OPS.Components.Globalization;

namespace OPS.Components.Data
{
	/// <summary>
	/// That class provide a protected-helper methods in order to aid the
	/// implementation of Database compoonents
	/// All our components MAY BE localizable, so we want to implement the ICmpLocalizedDataSource interface
	/// </summary>
	public abstract class CmpBase : CmpLocalizedDataSourceAdapter
	{

		/// <summary>
		/// Gets DataTable with the SQL passed.
		/// </summary>
		/// <param name="rawsql">SQL string to run against database</param>
		/// <param name="values">values of parameters</param>
		/// <returns>DataTable with data</returns>
		public static DataTable GetRawData (string rawsql, object[] values)
		{
			Database d = DatabaseFactory.GetDatabase();
			return d.FillDataTable (rawsql, "TABLE", values);		// by default TABLE is used as table name.
		}

		public static int GetFieldLength(string tableName, string fieldName)
		{
			Database d = DatabaseFactory.GetDatabase();
			OPS.Components.Data.FieldSchemaInfo fi = d.GetFieldInfo(tableName, fieldName);

			int fieldSize = 0;
			if (fi != null) 
				fieldSize = fi.FieldSize;
			return fieldSize;
		}

		public static bool GetFieldNullable(string tableName, string fieldName)
		{
			Database d = DatabaseFactory.GetDatabase();
			OPS.Components.Data.FieldSchemaInfo fi = d.GetFieldInfo(tableName, fieldName);

			bool fieldNullable = true;
			if (fi != null) 
				fieldNullable = fi.FieldNullable;
			return fieldNullable;
		}

		/*public static bool GetFieldPrimaryKeyReadOnly(string tableName, string fieldName)
		{
			Database d = DatabaseFactory.GetDatabase();
			OPS.Components.Data.FieldSchemaInfo fi = d.GetFieldInfo(tableName, fieldName);

			bool PKReadOnly = false;
			if (fi != null) 
				fieldNullable = fi.
			return fieldNullable;
		}*/

		public static int GetUserSchemaFieldLength(string tableName, string fieldName)
		{
			int fieldSize = 20;

			if ((tableName != "") && (fieldName != ""))
			{
				// XNA_08_10_2007
				ArrayList theTable = (ArrayList) DatabaseFactory.GetDatabase().UserSchema[tableName];
				if (theTable != null)
				{
					for (int x = 0; x < theTable.Count; x++)
					{
						ArrayList theFields = ((OPS.Components.Data.TableUserSchemaInfo)theTable[x]).TableFieldUserSchemaInfo;

						for (int i=0; i<theFields.Count; i++)
						{
							if (((OPS.Components.Data.FieldUserSchemaInfo) theFields[i]).FieldName == fieldName)
								fieldSize = ((OPS.Components.Data.FieldUserSchemaInfo) theFields[i]).FieldWidth;
						}
					}
				}
				// XNA_08_10_2007
			}

			return fieldSize;
		}

		public static string GetUserSchemaFieldAlign(string tableName, string fieldName)
		{
			string fieldAlign = "l";

			if ((tableName != "") && (fieldName != ""))
			{
				// XNA_08_10_2007
				ArrayList theTable = (ArrayList) DatabaseFactory.GetDatabase().UserSchema[tableName];
				if (theTable != null)
				{
					for (int x = 0; x < theTable.Count; x++)
					{
						ArrayList theFields = ((OPS.Components.Data.TableUserSchemaInfo)theTable[x]).TableFieldUserSchemaInfo;

						for (int i=0; i<theFields.Count; i++)
						{
							if (((OPS.Components.Data.FieldUserSchemaInfo) theFields[i]).FieldName == fieldName)
								fieldAlign = ((OPS.Components.Data.FieldUserSchemaInfo) theFields[i]).FieldAlign;
						}
					}
				}
				// XNA_08_10_2007
			}

			return fieldAlign;
		}

		#region protected ctor

		protected CmpBase() {}

		#endregion

		#region Partial implementation of IExecutant (only implements GetData (string rawsql, object [] values)
		public override DataTable GetData(string rawsql, object[] values)
		{
			// Note that there is not needed to override that method in each class deriving from CmpBase
			// because that is a "Universal Method": it allows to do a SELECT against the Database, but
			// no additional derived-class scope info is needed at that point...
			return CmpBase.GetRawData (rawsql, values);

		}


		#endregion

		#region Protected HELPER METHODS
		/// <summary>
		/// Process the fields (converts an string[] to a string containing the fields, separated by commas
		/// All fields in pk parameter WILL be included in list, even thought does not appear in fields array
		/// </summary>
		/// <param name="fields">Array of fields (cannot be null)</param>
		/// <param name="pk">Array of name-fields which are the primary key</param>
		/// <returns>String with comma-separated fields (all fields of pk will be included)</returns>
		protected System.Text.StringBuilder ProcessFields(string[] fields, string[]pk)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			// Step 1: Include the PK
			if (pk!=null)
			{
				for (int i=0; i< pk.Length;i++)
				{
					sb.Append (pk[i]);
					sb.Append (',');
					sb.Append (' ');
				}
			}

			// Step 2: Include the rest
			if (fields!=null) 
			{
				string spk = sb.ToString();
				for (int i=0; i< fields.Length;i++) 
				{
					if (spk.IndexOf(fields[i]) == -1)	// Field was not found in pk, so we add it
					{
						sb.Append (fields[i]);
						sb.Append (',');
						sb.Append (' ');
					}
				}
			}
			// Remove the last 2 chars of sb (will be ", ")
			if (sb.Length>0) sb.Remove (sb.Length-2, 2);
			return sb;
		}

		/// <summary>
		/// Implements the functionality of IExecutant::GetData (string[])
		/// </summary>
		/// <param name="fields">Comma-separated list of fields</param>
		/// <param name="where">WHERE clausule (without WHERE keyword)</param>
		/// <param name="orderby">ORDER BY clausule (whithout ORDER BY keyword)</param>
		/// <param name="values">Array of values of the parameters (needed on where clausule)</param>
		/// <param name="tableBD">Table of the DataBase to SELECT</param>
		/// <param name="tableDT">Name of the DataTable returned</param>
		/// <param name="pk">Array of strings with the primary keys (can be null)</param>
		/// <returns>DataTable with the data</returns>
		protected DataTable DoGetData (string fields, string where, string orderby,object[] values,string tableBD, string tableDT, string[] pk)
		{
			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = d.FillDataTable(fields,tableBD, orderby, where, tableDT,-1, values);
			if (pk!=null) 
			{
				DataColumn [] dtPK = new DataColumn[pk.Length];
				for (int i=0; i< pk.Length; i++) 
				{
					dtPK[i] = dt.Columns[pk[i]];
				}
				dt.PrimaryKey = dtPK;
			}
			return dt;
		}


		/// <summary>
		/// Implements the functionality of IExecutant::GetPagedData 
		/// </summary>
		/// <param name="fields">Comma-separated list of fields</param>
		/// <param name="fields">Array of fields to get</param>
		/// <param name="orderByField">Field for sorting</param>
		/// <param name="orderByASc">true if sort is ASCending</param>
		/// <param name="where">WHERE clausule</param>
		/// <param name="rowstart">1st register to fetch</param>
		/// <param name="rowend">Last register to fetch</param>		
		/// <param name="tableBD">Table of the DataBase to SELECT</param>
		/// <param name="tableDT">Name of the DataTable returned</param>
		/// <param name="pk">Array of strings with the primary keys (CANNOT be null)</param>
		/// <returns>DataTable with the data</returns>
		protected DataTable DoGetPagedData (string fields, string orderByField, bool orderByAsc, string where, 
			int rowstart, int rowend, string tableBD, string tableDT, string[] pk)
		{
			// Converts possible null or "" values
			if (pk == null) throw new ArgumentNullException("pk", "PK cannot be null when paging!");
			if (orderByField == string.Empty) orderByField = null;
			if (!orderByAsc && orderByField != null) orderByField = orderByField + " DESC";
//			if (tableBD	== "OPERATIONS")
//			{
//				if (where == "OPE_VALID=1 AND OPE_DELETED=0")
//				{
//					where = "OPE_VALID=1 AND OPE_DELETED=0 AND OPE_RANK >= " + rowstart + " AND OPE_RANK <= " + rowend;
//					orderByField = "OPE_RANK ASC";
//				}
//			}
//			else if ((tableBD == "ALARMS_HIS") && (where == ""))
//			{
//				where = "HALA_RANK >= " + rowstart + " AND HALA_RANK <= " + rowend;
//				orderByField = "HALA_RANK ASC";
//			}
//			else if ((tableBD == "FINES_HIS") && (where == ""))
//			{
//				where = "HFIN_RANK >= " + rowstart + " AND HFIN_RANK <= " + rowend;
//				orderByField = "HFIN_RANK ASC";
//			}
//			else if ((tableBD == "MSGS_HIS") && (where == ""))
//			{
//				where = "HMSG_RANK >= " + rowstart + " AND HMSG_RANK <= " + rowend;
//				orderByField = "HMSG_RANK ASC";
//			}
//			else if ((tableBD == "OPERATIONS_HIS") && (where == ""))
//			{
//				where = "HOPE_RANK >= " + rowstart + " AND HOPE_RANK <= " + rowend;
//				orderByField = "HOPE_RANK ASC";
//			}

			Database d = DatabaseFactory.GetDatabase();
			DataTable dt = null;
			if (rowend == -1) 
			{
				dt = d.FillDataTable (fields,tableBD,orderByField, where, tableDT, -1);
				DataColumn [] dtPK = new DataColumn[pk.Length];
				for (int i=0; i< pk.Length; i++) 
				{
					dtPK[i] = dt.Columns[pk[i]];
				}
				dt.PrimaryKey = dtPK;
			}
			else 
			{
				dt = d.FillPagedDataTable (fields,tableDT,orderByField,where,tableDT,rowstart, rowend, pk);
			}
			return dt;			
		}

	#endregion
	}
}
