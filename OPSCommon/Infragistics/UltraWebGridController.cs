using System.Web;
using System;
using System.Data;
using System.Collections;
using System.Web.UI.WebControls;

using InfragisticsWebCombo	= Infragistics.WebUI.WebCombo;
using InfragisticsWebGrid	= Infragistics.WebUI.UltraWebGrid;
using Infragistics.WebUI.WebDataInput;
using Infragistics.WebUI.UltraWebGrid;

using OPS.Components.Data;

namespace OTS.Framework.WebUI.Infragistics
{

	public class SortEventArgs : EventArgs
	{
		protected string _column;
		protected bool _ascending;
		public SortEventArgs (string colName, bool bAsc) : base()
		{
			_column = colName;
			_ascending = bAsc;			
		}

		public string Column { get { return _column; } }
		public bool Ascending { get { return _ascending; } }
	}

	/// <summary>
	/// This class controls an unbound Infragistics UltraWebGrid
	/// </summary>
	public class UltraWebGridController
	{
		public delegate void SortOptionChangedEventHandler (object source, SortEventArgs e);
		public event SortOptionChangedEventHandler SortOptionChanged;
		
		// A placeholder to add Web Combos and other controls
		protected System.Web.UI.WebControls.PlaceHolder _placeHolder;
		// Grid to control
		protected UltraWebGrid _grid;
		// DataSet with data displayed by the grid
		protected DataSet _ds;

		// ResourceManager used to translate field names
		protected OPS.Components.Globalization.IResourceManager _rm;

		// Component used to save/load data
		protected OPS.Components.Data.ICmpLocalizedDataSource _component;
		// Collection with all DataRows added since the last save
		protected Hashtable _newDataRows;		// Column used to unique-identify a row
		protected string[] _pk;
		// If true the columns of the Primary Key will be Read Only in the grid.
		protected bool _pkReadOnly;
		// Contains the value of the field referenced by "SortColumnName" of the first/last register shown in the grid
		// Number of items per each page of the grid
		protected int _pagesize;
		// Initial and/or current page
		protected int _page;
		// Name of the column used to order (if any)
		protected string _sortColumnName;
		// true if order is ASCending (if order applied)
		protected bool _sortAscending;

		// A label indicating if the user press the save button or the cancel button
		protected string _lbActiveButton;
		// A label to know if the user has made changes to DataSet
		protected string _lbModifyGrid;

		// WebCombo for template-copy
		protected InfragisticsWebCombo.WebCombo _wc;
		protected DropDownList _dwc;
		
		// WebDateTimeEdit for template-copy
		protected WebDateTimeEdit _wdc;

		// true if read only
		protected bool _bReadOnly;

		// Variables to control what the user can do with the grid.
		protected bool _bInsAllowed;
		protected bool _bUpdAllowed;
		protected bool _bDelAllowed;
		protected bool _bExeAllowed;


		// WHERE clausule
		protected string _where;

		// Variable to store combos, this manner the fk data done once.
        protected ValueList[] _valueListCombos;
		
		// Trace System

		protected TraceContext m_Trace = null;

		/// <summary>
		/// Builds a new Helper object to control a Infragistics' UltraWebGridx
		/// </summary>
		/// <param name="grid">The Infragistics' UltraWebGrid to control</param>
		/// <param name="cmp">Component used to get refreshed data</param>
		/// <param name="pk">Name of the column used to unique identify a row</param>
		/// <param name="pkReadOnly">If true, columns of the Primary Key will be read-only</param>
		/// <param name="pagesize">Size (registers) of each page</param>
		/// <param name="rm">Resource Manager used to translate physical field names to logical and culture-dependent field names</param>
		public UltraWebGridController(UltraWebGrid grid, OPS.Components.Data.ICmpLocalizedDataSource cmp, bool pkReadOnly, int pagesize,
			OPS.Components.Globalization.IResourceManager rm, string where)
		{
			if (grid==null) throw new ArgumentNullException ("grid","The grid to control cannot be null!");
			if (cmp==null) throw new ArgumentNullException("cmp","Component used to get a refreshed data cannot be null!");
			_grid = grid;
			// Capture some interesting events of the grid...
			_grid.AddRowBatch += new AddRowBatchEventHandler(grid_AddRowBatch);
			_grid.DeleteRowBatch += new DeleteRowBatchEventHandler(grid_DeleteRowBatch);
			_grid.UpdateCellBatch += new UpdateCellBatchEventHandler(grid_UpdateCellBatch);
			_grid.SortColumn += new SortColumnEventHandler(grid_SortColumn);
			_grid.InitializeLayout += new InfragisticsWebGrid.InitializeLayoutEventHandler(grid_InitializeLayout);
			_grid.Page.Unload += new EventHandler(Page_Unload);
			_newDataRows = new System.Collections.Hashtable();
			//_pkReadOnly = pkReadOnly;
			
			_component = cmp;
			_pkReadOnly = this._component.GetPKReadOnly;
			_pagesize= pagesize;
			_page = 1;					// Page is 1-based (first page is #1)
			_sortAscending = true;
			_sortColumnName = string.Empty;
			_rm = rm;
			//_bReadOnly = false;
			//_bInsAllowed = true;
			//_bUpdAllowed = true;
			//_bDelAllowed = true;
			//_bExeAllowed = true;
			_where = where;
		}

		public string Where
		{
			get { return _where;}
			set { _where = value;}
		}

		public ValueList[] ValueList
		{
			get { return _valueListCombos;}
			set { _valueListCombos = value;}
		}

		public bool ReadOnly 
		{
			get { return _bReadOnly; }
			set { _bReadOnly = value; }
		}

		#region public functions to control actions over the grid

		public bool InsAllowed 
		{
			get { return _bInsAllowed; }
			set { _bInsAllowed = value; }
		}

		public bool UpdAllowed 
		{
			get { return _bUpdAllowed; }
			set { _bUpdAllowed = value; }
		}

		public bool DelAllowed 
		{
			get { return _bDelAllowed; }
			set { _bDelAllowed = value; }
		}

		public bool ExeAllowed 
		{
			get { return _bExeAllowed; }
			set { _bExeAllowed = value; }
		}

		#endregion

		/// <summary>
		/// Allow to get and set a PlaceHolder to put the dynamic-created controls (like combos).
		/// A PlaceHolder is a instance of a System.Web.UI.WebControls.PlaceHolder control
		/// </summary>
		public PlaceHolder PlaceHolderForControls 
		{
			get { return _placeHolder;}
			set { _placeHolder = value;}
		}

		public InfragisticsWebCombo.WebCombo WebComboTemplate
		{
			set { _wc = value;}
		}

		public DropDownList DWebComboTemplate
		{
			set { _dwc = value;}
		}

		public WebDateTimeEdit WebDateTimeEdit1
		{
			set { _wdc = value;}
		}
	
		public int ActualPage 
		{
			get { return _page; }
			set { _page = value;}
		}

		/// <summary>
		/// Changes the current page to a new value and refreshes the data
		/// </summary>
		/// <param name="page">New page (first is 1)</param>
		public void GoToPage (int page)
		{
			TraceUWGC("UltraWebGridController::GoToPage",">>");
			TraceUWGC("UltraWebGridController::GoToPage","Page:[" + page +"]");
			if (page<1) throw new ArgumentException ("Page must be at least 1","page");
			_page = page;
			DoDataBind();
			TraceUWGC("UltraWebGridController::GoToPage","<<");
		}


		
		// That method gets a DataSet with the Data to show in the grid.
		// Builds the where clausule based on: field sort and sort direction
		// Next adds a tracking column to DataSet in order to track rows added
		private void ObtainDataSet() 
		{
			TraceUWGC("UltraWebGridController::ObtainDataSet",">>");
			
			int rowstart = ((_page-1) * _pagesize) + 1;
			int rowend = rowstart + _pagesize - 1;
	
			TraceUWGC("UltraWebGridController::ObtainDataSet","RowStart :" + rowstart + " RowEnd :" + rowend);

			DataTable dt = _component.GetPagedData(_sortColumnName, _sortAscending, _where, rowstart , rowend);
			_ds = new DataSet();
			_ds.Tables.Add (dt);
			
			TraceUWGC("UltraWebGridController::ObtainDataSet"," Num Tables : " + _ds.Tables.Count);


			// NEW TRACTAMENT PK
			int contador = 0;
			_pk = new string[_ds.Tables[_component.MainTable].PrimaryKey.Length];
			foreach (DataColumn dc in _ds.Tables[_component.MainTable].PrimaryKey)
			{
				_pk[contador] = dc.ColumnName;
				contador ++;
			}

//>> LDR 2004.07.22
//			// After data is loaded, foreigndata needs to be loaded too
//			// If there are no data there is no need of getForeingData
//			if (dt.Rows.Count > 0)
//			{
//				_component.GetForeignData(_ds,_component.MainTable,_rm);
//			}
			_component.GetForeignData(_ds,_component.MainTable,_rm);
//<< LDR 2004.07.22

			// Finally a GUID col is added to DataSet (that col will allow us to track the newly added rows)
			DataColumn dguid = new DataColumn("GUID", Type.GetType("System.String"));
			_ds.Tables[_component.MainTable].Columns.Add (dguid);

			TraceUWGC("UltraWebGridController::ObtainDataSet","<<");
		}


		// That method gets a DataSet with the Data to Export (Full Data filtered).
		// Builds the where clausule based on: field sort and sort direction
		// Next adds a tracking column to DataSet in order to track rows added
		public DataSet ObtainFullDataSet() 
		{
			DataTable dt = _component.GetData(null,_where,null,null);
			_ds = new DataSet();
			_ds.Tables.Add (dt);
			// NEW TRACTAMENT PK
			int contador = 0;
			_pk = new string[_ds.Tables[_component.MainTable].PrimaryKey.Length];
			foreach (DataColumn dc in _ds.Tables[_component.MainTable].PrimaryKey)
			{
				_pk[contador] = dc.ColumnName;
				contador ++;
			}

			// After data is loaded, foreigndata needs to be loaded too
			_component.GetForeignData(_ds,_component.MainTable,_rm);

			return _ds;
		}


		private static string[] _sNumericTypes = {"System.Byte", "System.Decimal", "System.Double", "System.Int16","System.Int32",
													 "System.Int64","System.SByte","System.Single","System.UInt16","System.UInt32",
													 "System.UInt64"};

		// Returns true if col named scol in DataSet _ds is a alphanumeric column
		private bool IsAlphaColumn (string scol)
		{
			Type t = _ds.Tables[_component.MainTable].Columns[scol].DataType;
			for (int i=0; i< _sNumericTypes.Length;i++) 
			{
				if (Type.GetType(_sNumericTypes[i]) == t) 
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Gets and sets the name of the columnn used for sorting
		/// </summary>
		public string SortColumnName
		{
			get 
			{
				return _sortColumnName;
			}
			set 
			{
				_sortColumnName = value;
			}
		}

		/// <summary>
		/// Gets and sets if sorting will be ASCending or DESCending
		/// </summary>
		public bool SortAscending
		{
			get 
			{
				return _sortAscending;
			}
			set 
			{
				_sortAscending = value;
			}
		}

		/// <summary>
		/// Gets the <c>UltraWebGrid</c> associated to the class
		/// </summary>
		public UltraWebGrid Grid 
		{
			get {return _grid;}
		}
		/// <summary>
		/// Gets the <c>DataSet</c> associated to the class
		/// That DataSet contains the same data as the control
		/// </summary>
		public DataSet Data
		{
			
			get 
			{
				TraceUWGC("UltraWebGridController::Data",">>");
				if (_ds == null) 
				{

					ObtainDataSet();
				}
				TraceUWGC("UltraWebGridController::Data","<<");
				return _ds;
			}
			
		}

		// This event is called each time a row is added to the grid.
		// We don't have the Cell Values at this time, so we have to add
		// the row to a hashtable with a unique-GUID and update the grid
		// to set the same GUID at corresponding row
		private void grid_AddRowBatch(object sender, RowEventArgs e)
		{
			TraceUWGC("UltraWebGridController::grid_AddRowBatch",">>");
			// A new row was added to the cell.
			if (_ds == null || _ds.Tables.Count == 0)
			{ 
				ObtainDataSet();
			}

			DataRow nrow = _ds.Tables[_component.MainTable].NewRow();
			nrow["GUID"] = Guid.NewGuid().ToString("N");
			_newDataRows.Add(nrow["GUID"].ToString(),nrow);
			// Set the GUID in the WebGrid row
			e.Row.Cells.FromKey ("GUID").Value = nrow["GUID"];
			TraceUWGC("UltraWebGridController::grid_AddRowBatch","NEW GUID" + e.Row.Cells.FromKey ("GUID").Value.ToString());
			_lbModifyGrid = "Add";
			TraceUWGC("UltraWebGridController::grid_AddRowBatch","<<");

		}

		// This event is called each time a row is deleted in the grid.
		// We have to found the original row in the underlying DataSet 
		// and delete it 
		private void grid_DeleteRowBatch(object sender, RowEventArgs e)
		{
			TraceUWGC("UltraWebGridController::grid_DeleteRowBatch",">>");
			if (_ds == null || _ds.Tables.Count == 0) 
			{
				ObtainDataSet();
			}

			TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","Getting Primery Keys");
			string[] svPks = new string[_pk.Length];
			if (_pk.Length > 1) 
			{
				TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","Primary Key Length > 1");
				for (int aux=0;aux < _pk.Length; aux ++)
				{
					TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","Getting PrimaryKey Values");
					TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","Type :" + e.Row.DataKey.GetType().ToString());
					//svPks[aux] = (string)((Array)e.Row.DataKey).GetValue(aux); //
					svPks[aux] = ((Array)e.Row.DataKey).GetValue(aux).ToString();
				}
			}
			else
			{
				TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","Primary Key Length <= 1");
				svPks[0] = e.Row.DataKey.ToString();
			}

			TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","Getting Row");
			DataRow dr = GetRowByPk(svPks);
			if (dr == null) 
			{
				TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","Original row not found in DataSet!");
				TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","<<");
				throw new System.Exception("Original row not found in DataSet! key = " + e.Row.DataKey.ToString());
			}
			// atempt to delete row
			try 
			{
				TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","Deleting DataRow");
				dr.Delete();
			}
			catch (Exception ex) 
			{
				TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","<<");
				throw new System.Exception("Delete of a row failed. See inner exception for details.",ex);
			}
			_lbModifyGrid = "Delete";
			TraceUWGC("UltraWebGridController::grid_DeleteRowBatch","<<");
		}

		// This event is called each time a Cell is updated in the grid, and is
		// called after Insert and Delete rows events.
		// We have the real value of the cell at this point, so we have to
		// find the row of the cell and updateIt.
		// Note that the cell updated can belong to a row added to the grid, and
		// in this case the row will not be found in the DataSet but in the
		// NewRows (_newDataRows) collection
		private void grid_UpdateCellBatch(object sender, CellEventArgs e)
		{
			
			TraceUWGC("UltraWebGridController::grid_UpdateCellBatch",">>");
			TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Row Key" + e.Cell.Row.Key);
			TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Row DataKey" + e.Cell.Row.DataKey);
			TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Row Index " + e.Cell.Row.Index);
			TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Column Index " + e.Cell.Column.Index);
			TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Cell Value " + e.Cell.Text);
			if (_ds == null || _ds.Tables.Count == 0)
				ObtainDataSet();
			DataRow nrow = null;

			//>> LDR 2004.07.26
			//			if (e.Cell.Row.DataKey == null) 
			//			{
			//				// Search in the hashtable for the new key
			//				nrow = (DataRow)_newDataRows[e.Cell.Row.Cells.FromKey("GUID").Value.ToString()];
			//			}
			//			else 
			//			{
			//				// try to find row containing the changed key
			//				string[] svPks = new string[_pk.Length];
			//				if (_pk.Length > 1) 
			//				{
			//					for (int aux = 0; aux < _pk.Length; aux++)
			//					{
			//						svPks[aux] = ((Array)e.Cell.Row.DataKey).GetValue(aux).ToString();
			//					}
			//				}
			//				else
			//				{
			//					svPks[0] = (string)e.Cell.Row.DataKey.ToString();
			//				}
			//
			//				nrow = GetRowByPk(svPks);
			//				if (nrow == null) 
			//					throw new System.Exception ("Original row not found in DataSet! key = " + e.Cell.Row.DataKey.ToString());
			//			}
			if (_pk.Length > 1)
			{
				TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","PK Length > 1");
				bool noKey = false;
				for (int aux = 0; aux < _pk.Length; aux++)
				{
					noKey = noKey || (((Array)e.Cell.Row.DataKey).GetValue(aux) == null);
				}

				if (noKey) 
				{
					TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","NO KEY");
					// Search in the hashtable for the new key
					nrow = (DataRow)_newDataRows[e.Cell.Row.Cells.FromKey("GUID").Value.ToString()];
				}
				else 
				{
					TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","WE HAVE KEY");
					// try to find row containing the changed key
					string[] svPks = new string[_pk.Length];
					for (int aux = 0; aux < _pk.Length; aux++)
					{
						svPks[aux] = ((Array)e.Cell.Row.DataKey).GetValue(aux).ToString();
					}
	
					nrow = GetRowByPk(svPks);
					if (nrow == null)
					{
						TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","We have a key BUT we have not found in Data Set");
						nrow = (DataRow)_newDataRows[e.Cell.Row.Cells.FromKey("GUID").Value.ToString()];
					}
					else
					{
						TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","We have a key AND we have not found in Data Set");
					}
				}
			}
			else
			{
				TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","PK Length <= 1");
				if (e.Cell.Row.DataKey == null) 
				{
					// If e.Cell.Row.DataKey == null ==> Is a New Record
					// Search in the hashtable for the new key
					TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Search in the hashtable for the new key");
					nrow = (DataRow)_newDataRows[e.Cell.Row.Cells.FromKey("GUID").Value.ToString()];
				}
				else 
				{
					// If e.Cell.Row.DataKey == null ==> Is a New Record or is an update

					// try to find row containing the changed key
					TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","try to find row containing the changed key");
					string[] svPks = new string[_pk.Length];
					svPks[0] = (string)e.Cell.Row.DataKey.ToString();
					nrow = GetRowByPk(svPks);

					if (nrow == null) 
					{
						TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","We have NOT found the primary Key ==> The Row NOT exists");
						//orc 2005.02.08 throw new System.Exception ("Original row not found in DataSet! key = " + e.Cell.Row.DataKey.ToString());
						TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Search in the newDataRows");
						if( _newDataRows != null )
						{
							TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","_newDataRows is NOT NULL");
							if(e.Cell.Row.Cells.FromKey("GUID").Value != null) 
							{
								TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","FromKey.Value != null");
								if(_newDataRows.ContainsKey(e.Cell.Row.Cells.FromKey("GUID").Value.ToString()))
								{
									TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","ContainsKey == TRUE");
									nrow = (DataRow)_newDataRows[e.Cell.Row.Cells.FromKey("GUID").Value.ToString()];
								}
								else
								{
									TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","ContainsKey == FALSE");
									throw new System.Exception ("You can not Update Primary Key");
								}
							}
							else
							{	
								throw new System.Exception ("You can not Update Primary Key");
							}	
						}
						else
						{
							throw new System.Exception ("You can not Update Primary Key");
						}

						if(nrow == null)
						{
							throw new System.Exception ("Original row not found in DataSet! key = " + e.Cell.Row.DataKey.ToString());
						}
						else
						{
							TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Is a New Record and we have changed key");
						}

					}
					else
					{
						TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","We have found the primary Key ==> The Row exists");
						TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Column Index : " + e.Cell.Column.Index );
						TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","Primary Key : " + _grid.Columns.FromKey(_pk[0]).Index);
						//if( e.Cell.Row.Cells.FromKey("GUID").Value.ToString() != nrow.GetHashCode().ToString())
						if( e.Cell.Column.Index == _grid.Columns.FromKey(_pk[0]).Index)
						{
							TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","We have modified PK and this PK exists!!");
							throw new System.Exception ("You can not Update Primary Key");
						}
						else
						{
							TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","The row exists and PK is NOT modified");
						}
					}
				}

			}
			//<< LDR 2004.07.26

			//>> LDR 2004.07.29
			//if (e.Cell.Value == null)
			if ((e.Cell.Value == null) || (string.Format("{0}", e.Cell.Value) == "-999999"))
			//<< LDR 2004.07.29
			{
				nrow[e.Cell.Column.Key] = DBNull.Value;
			}
			else
			{
				nrow[e.Cell.Column.Key] = e.Cell.Value;
			}

			_lbModifyGrid = "Modify";

			TraceUWGC("UltraWebGridController::grid_UpdateCellBatch","<<");

		}

		// This event is called each time the WebForm containing the grid is unloaded
		// We have to update the DataSource (aka Database) at this time
		private void Page_Unload(object sender, EventArgs e)
		{
			if (_ds == null || _lbActiveButton == "Clear") 
			{
				_lbActiveButton	= "Clear";
				_lbModifyGrid	= "Clear";
				return;
			}

//>> LDR 2004.07.22
//			if (_newDataRows.Count > 0) 
//			{
//				//	The pk can be of different types
//				//	If autoincremental the field cannot be modified and a default value must be placed.
//				long lastpk = _component.LastPKValue;
//				//if (lastpk == null) lastpk = -2;
//
//				// we have new rows to be added
//				foreach (System.Collections.DictionaryEntry o in _newDataRows) 
//				{
//					DataRow dr = (DataRow)o.Value;
//
//					if (lastpk >= 0) 
//					{
//						dr[_grid.Columns.FromKey(_pk[0]).Index] = ++lastpk;
//					}
//					try
//					{
//						_ds.Tables[_component.MainTable].Rows.Add (dr);
//					}
//					catch
//					{
//
//					}
//				}
//			}
//			// Send updates to database
//			_component.SaveData(_ds);
//<< LDR 2004.07.22
			
			// Every time the data is refreshed, we mark the data to not save.
			// Only if the user changes the value of the button (in btnSave_Clik) the changes go to DataBase
			_lbActiveButton	= "Clear";
			_lbModifyGrid	= "Clear";

			//+DoDataBind();
		}

		// This functions gets a row identified by unique pk value.
		private DataRow GetRowByPk (string[] pkValue) 
		{
			TraceUWGC("UltraWebGridController::GetRowByPk",">>");
			int nCount = 0;
			
			TraceUWGC("UltraWebGridController::GetRowByPk","Value Of MainTable" + _component.MainTable );
			TraceUWGC("UltraWebGridController::GetRowByPk","Number Of Table " + _ds.Tables.Count );
			foreach (DataRow dr in _ds.Tables[_component.MainTable].Rows) 
			{
				
				bool bDataRow = true;
				// _pk Primary Key may be more than one field
				// We must compare all the fields
				for (int aux = 0; aux < _pk.Length; aux ++) 
				{
					//TraceUWGC("UltraWebGridController::GetRowByPk","Comparing PK Row [" + nCount.ToString() + "] Key "
					//	+ pkValue[aux] + " With: " + dr[_pk[aux]].ToString());
					if (pkValue[aux] != dr[_pk[aux]].ToString()) 
					{
						bDataRow = false;
					}
					nCount ++;
				}
				if (bDataRow == true) 
				{
					TraceUWGC("UltraWebGridController::GetRowByPk","<<");
					return dr;
				}
				bDataRow = false;
			}
				
			//throw new System.Exception ("Updated row with PK:" + pkValue + " not found in DataSet!!!!");
			////return rows[0];
			TraceUWGC("UltraWebGridController::GetRowByPk","<<");
			return null;
		}

		// This event is called each time the user clicks a header of the grid
		// Clicking the header of a grid is the method choosed to perform a sorting
		private void grid_SortColumn(object sender, SortColumnEventArgs e)
		{
			// Cancel grid action
			e.Cancel = true;

			// Store the values
			_sortColumnName = _grid.Bands[e.BandNo].Columns[e.ColumnNo].Key;
			_sortAscending = !_sortAscending;

			// Launch the event...
			if (SortOptionChanged != null) 
				SortOptionChanged(this, new SortEventArgs(_sortColumnName, _sortAscending));
		}

		// This event is raised each time the grid has to initialize its layout.
		// We have to clear sort indicators and set some properties
		// Also a hidden column containing a 128bit GUID is added to the grid
		// and set to hidden. This column will track the rows added
		private void grid_InitializeLayout(object sender, LayoutEventArgs e)
		{
			TraceUWGC("UltraWebGridController::grid_InitializeLayout",">>");
			TraceUWGC("UltraWebGridController::grid_InitializeLayout","Clear Custom sort Indicators");
			// Clear custom sort Indicators
			foreach (UltraGridColumn uwgcol in e.Layout.Bands[0].Columns)
			{
				if (_rm == null)
					uwgcol.HeaderText = uwgcol.BaseColumnName;
				else 
					uwgcol.HeaderText = _rm[_component.MainTable + "." + uwgcol.BaseColumnName];
			}

			// Add a hidden column with GUID
			e.Layout.Bands[0].Columns.Add("GUID");
			e.Layout.Bands[0].Columns.FromKey ("GUID").Hidden = true;


			// Initialize Layout Parameters
			// set the default cell click action to edit
			e.Layout.HeaderStyleDefault.HorizontalAlign	= HorizontalAlign.Left;
			e.Layout.RowSelectorsDefault		= RowSelectors.Yes;
			e.Layout.AllowDeleteDefault			= AllowDelete.Yes;
			e.Layout.CellClickActionDefault		= CellClickAction.Edit;
			e.Layout.Bands[0].DataKeyField = _pk[0];
			e.Layout.Pager.PageSize				= _pagesize;
			e.Layout.Pager.AllowCustomPaging	= true;
			
			// XNA_08_10_2007
			// Indicates the pk column
			// lets see the pk type (read only or can be modified by user)
			// if LastPKValue returns a value 
			/*long lastpk = _component.LastPKValue;
			e.Layout.Bands[0].DataKeyField = _pk[0];
			TraceUWGC("UltraWebGridController::grid_InitializeLayout","ReadOnly[" + _bReadOnly.ToString() + "]");
			if (lastpk < 0 && !_bReadOnly) 
			{ 
				TraceUWGC("UltraWebGridController::grid_InitializeLayout","NOT PK READ ONLY");
				e.Layout.Bands[0].Columns[_grid.Columns.FromKey(_pk[0]).Index].AllowUpdate = AllowUpdate.Yes; 
			}
			else 
			{
				TraceUWGC("UltraWebGridController::grid_InitializeLayout","PK READ ONLY");
				e.Layout.Bands[0].Columns[_grid.Columns.FromKey(_pk[0]).Index].AllowUpdate = AllowUpdate.No; 
			}

			for (int aux=1; aux < _pk.Length; aux ++)
			{
				e.Layout.Bands[0].DataKeyField = e.Layout.Bands[0].DataKeyField + "," + _pk[aux];
				// orc 2005.02.08
				if (lastpk < 0 && !_pkReadOnly) 
				{ 
					TraceUWGC("UltraWebGridController::grid_InitializeLayout","NOT PK READ ONLY");
					e.Layout.Bands[0].Columns[_grid.Columns.FromKey(_pk[aux]).Index].AllowUpdate = AllowUpdate.Yes; 
				}
				else 
				{
					TraceUWGC("UltraWebGridController::grid_InitializeLayout","PK READ ONLY");
					e.Layout.Bands[0].Columns[_grid.Columns.FromKey(_pk[aux]).Index].AllowUpdate = AllowUpdate.No;
				}
			}*/
			// XNA_08_10_2007


			// Automatic calculation of Grid Columns
			// Substract 2 invisible columns (GUID and RANK_VALUE). Substract 30 from total value for borders and row_selectors
			// Defaults are a char columns are 3 times larger than an integer column.

			int itypestring		= 0;
			int itypedatetime	= 0;
			int itypeint		= 0;
			double dwidthgrid	= 0;

			// give some more space for fk columns, don't has to be treated as integer
			itypeint = _ds.Relations.Count * 2;

			for (int aux =0;aux < (_grid.Columns.Count -2);aux ++)
			{

				if (_grid.Columns[aux].Hidden == false)
				{
					if (_grid.Columns[aux].DataType == "System.Decimal")
					{
						itypeint ++;
					}
					else
					{
						if (_grid.Columns[aux].DataType == "System.DateTime")
						{
							itypedatetime++;
						}
						else
						{
							itypestring++;
						}
					}
				}
			}

			itypeint = itypeint + 3*itypestring + 3*itypedatetime;

			for (int aux =0;aux < (_grid.Columns.Count -2);aux ++)
			{
				if (_grid.Columns[aux].Hidden == false)
				{
					
					if (_grid.Columns[aux].DataType == "System.Decimal")
					{
						dwidthgrid	=  Math.Floor((_grid.Width.Value) / itypeint);
					}
					else
					{
						if (_grid.Columns[aux].DataType == "System.DateTime")
						{
							dwidthgrid	=  Math.Floor((_grid.Width.Value) / itypeint*3);
						}
						else
						{
							dwidthgrid	=  Math.Floor((_grid.Width.Value) / itypeint*3);
						}
					}
					e.Layout.Bands[0].Columns[aux].Width = Unit.Percentage(dwidthgrid);
				}
				else
				{
					e.Layout.Bands[0].Columns[aux].Width = Unit.Percentage(0);
				}
			}

			for (int aux=0;aux < _ds.Relations.Count; aux++)
			{
				dwidthgrid	=  Math.Floor((_grid.Width.Value) / itypeint*3);
				e.Layout.Bands[0].Columns[_ds.Tables[0].Columns[_ds.Relations[aux].ChildColumns[0].ColumnName].Ordinal].Width = Unit.Percentage(dwidthgrid);
			}

			int totalpercent = 0;
			for (int aux=0; aux < _grid.Columns.Count - 3; aux ++)
			{
				totalpercent = totalpercent + Int32.Parse(e.Layout.Bands[0].Columns[aux].Width.Value.ToString());
			}
			int aux2 = _grid.Columns.Count - 3;
			e.Layout.Bands[0].Columns[aux2].Width = Unit.Percentage(100 - totalpercent);

			#region Control Actions over the grid 

			// Only insert, Update & Delete, execution is controlled before enter the grid.

			if (_bReadOnly) 
			{
				e.Layout.Bands[0].AllowAdd		= InfragisticsWebGrid.AllowAddNew.No;
				e.Layout.Bands[0].AllowUpdate	= InfragisticsWebGrid.AllowUpdate.No;
				e.Layout.Bands[0].AllowDelete	= InfragisticsWebGrid.AllowDelete.No;
				e.Layout.AllowAddNewDefault		= InfragisticsWebGrid.AllowAddNew.No;
				e.Layout.AllowUpdateDefault		= InfragisticsWebGrid.AllowUpdate.No;
				e.Layout.AllowDeleteDefault		= InfragisticsWebGrid.AllowDelete.No;
			}
				if (_bInsAllowed) 
				{
					e.Layout.Bands[0].AllowAdd		= InfragisticsWebGrid.AllowAddNew.Yes;
					e.Layout.AllowAddNewDefault		= InfragisticsWebGrid.AllowAddNew.Yes;
				}
				if (_bUpdAllowed) 
				{
					e.Layout.Bands[0].AllowUpdate	= InfragisticsWebGrid.AllowUpdate.Yes;
					e.Layout.AllowUpdateDefault		= InfragisticsWebGrid.AllowUpdate.Yes;
				}
				if (_bDelAllowed) 
				{
					e.Layout.Bands[0].AllowDelete	= InfragisticsWebGrid.AllowDelete.Yes;
					e.Layout.AllowDeleteDefault		= InfragisticsWebGrid.AllowDelete.Yes;
				}

			for (int contadorpk = 0; contadorpk < _pk.Length; contadorpk++)
			{
				if(_pkReadOnly)
				{
					TraceUWGC("UltraWebGridController::grid_InitializeLayout","READ ONLY");
					_grid.Columns[contadorpk].CellStyle.BackColor = System.Drawing.Color.LightGray;
					e.Layout.Bands[0].Columns[contadorpk].AllowUpdate = AllowUpdate.No; 
				}
				else
				{
					TraceUWGC("UltraWebGridController::grid_InitializeLayout","NOT PK READ ONLY==>CAN UPDATE");
					e.Layout.Bands[0].Columns[contadorpk].AllowUpdate = AllowUpdate.Yes; 
				}

			}

			if( this._component.GetPKReadOnly )
				TraceUWGC("UltraWebGridController::grid_InitializeLayout","PK READ ONLY BY SCHEMA");
			else
				TraceUWGC("UltraWebGridController::grid_InitializeLayout","PK NOT READ ONLY BY SCHEMA");

			#endregion
			TraceUWGC("UltraWebGridController::grid_InitializeLayout","<<");
		}

		/// <summary>
		///  Do a Data Binding to the grid.
		/// </summary>
		public void DoDataBind()
		{
			TraceUWGC("UltraWebGridController::DoDataBind",">>");
			if (_lbActiveButton != "Save")
			{
				ObtainDataSet();
				_grid.DataSource = _ds;
				_grid.DataMember = _ds.Tables[_component.MainTable].TableName;
				_grid.DataBind();
			}
			TraceUWGC("UltraWebGridController::DoDataBind","<<");
		}

		public string ActiveButton
		{
			get { return _lbActiveButton;}
			set { _lbActiveButton = value;}
		}

		public string ModifyGrid
		{
			get { return _lbModifyGrid;}
			set { _lbModifyGrid = value;}
		}

		/// <summary>
		///  Return the total number of pages
		/// </summary>
		public long GetTotalPages
		{
			get 
			{ 
				double valor = (Double)_component.GetCount()/_pagesize;
				return (long)Math.Ceiling(valor);
			}
		}

		/// <summary>
		///  Return the total number of pages filtered by where
		/// </summary>
		public long GetTotalPagesFiltered(string where)
		{
			double valor = 0;
				 
			if (where.ToString() == "") 
			{
				valor = (Double)_component.GetCount()/_pagesize;
			}				
			else
			{
				valor = (Double)_component.GetCount(where,null)/_pagesize;
			}
			return (long)Math.Ceiling(valor);
		}
		
		/// <summary>
		/// This function assign a web combo to a grid cell
		/// </summary>
		/// <param name="ds">DataSet that containts the data to show in the web combo</param>
		/// <param name="ColumnaGrid">Name of the column[0..N-1] where the combo is assigned</param>
		/// <param name="ColumnaValueCombo">Name of the column[0..N-1] of the Dataset that has the value to store in the grid</param>
		/// <param name="ColumnaComboText">Name of the column[0..N-1] of the Dataset that has the value to show in the grid</param>
		/// <param name="TaulaCombo">Name of The Web Combo Table</param>
		/// <param name="wc">Name of the web combo</param>
		/// <param name="uwg">Name of the grid</param>
		public void OmpleWebCombo ( DataSet ds, string ColumnaGrid, string ColumnaValueCombo, string ColumnaComboText,string TaulaCombo, UltraWebGrid uwg)
		{
			ValueList vl = new ValueList();
			//vl.DataSource	= ds;
			vl.DataMember	= TaulaCombo;

			if (ds.Tables[TaulaCombo].TableName == "CODES")
			{
				_rm.GlobalizeTable(ds.Tables[TaulaCombo], "COD_LIT_ID", "COD_NEWDESC", true);
			}

			foreach (DataRow dr in ds.Tables[TaulaCombo].Rows)
			{
				vl.ValueListItems.Add((object)dr[0],dr[1].ToString());
			}

			int iColumnaValue	= ds.Tables[_component.MainTable].Columns[ColumnaGrid].Ordinal;

			_grid.Columns.FromKey(ColumnaGrid).AllowUpdate = _bReadOnly ?  AllowUpdate.No :  AllowUpdate.Yes;
			_grid.Columns.FromKey(ColumnaGrid).Type=ColumnType.DropDownList;	
			_grid.Columns.FromKey(ColumnaGrid).ValueList.DisplayStyle = ValueListDisplayStyle.DisplayText;

			_grid.Columns.FromKey(ColumnaGrid).ValueList = vl;

		}

		public void CreateValidators(string identifier)
		{
			foreach (UltraGridColumn col in _grid.Columns) 
			{
				if (!_bReadOnly & col.DataType == "System.DateTime" & _grid.Visible)
				{
					_grid.Columns[col.Index].AllowUpdate		= InfragisticsWebGrid.AllowUpdate.Yes;
					_grid.Columns[col.Index].Type				= InfragisticsWebGrid.ColumnType.Custom;
					_grid.Columns[col.Index].EditorControlID	= identifier;
					_grid.Columns[col.Index].Format				= "dd/MM/yyyy hh:mm";	
				}
			}
		}

		// Create dynamic combos and add them to the grid
		public ValueList[] CreateCombos(ValueList[] _valueListCombos)
		{
			string sColumnaGrid		= "";
			string sTaulaCombo		= "";
//			bool FirstTime			= true;

			// Create one WebCombo for each relation 
//			if (_valueListCombos == null)
//			{
				_valueListCombos = new ValueList[_ds.Relations.Count];
//				FirstTime = true;
//			}
//			else
//			{
//				FirstTime = false;
//			}

			for (int auxx = 0 ; auxx < _ds.Relations.Count; auxx++)
			{
				sColumnaGrid	= _ds.Relations[auxx].ChildColumns[0].ColumnName;
				sTaulaCombo		= _ds.Relations[auxx].ParentTable.TableName;

//				if (!FirstTime)
//				{
//					// Create a ValueList for Drop-Down
//					ValueList vl = _valueListCombos[auxx];
//					vl.DataBind();
//
//					// Special table (Globalizes table CODES)
//					if (_ds.Tables[sTaulaCombo].TableName == "CODES")
//					{
//						_rm.GlobalizeTable(_ds.Tables[sTaulaCombo], "COD_LIT_ID", "COD_NEWDESC", true);
//					}
//
//					_grid.Columns.FromKey(sColumnaGrid).AllowUpdate				= _bReadOnly ?  AllowUpdate.No :  AllowUpdate.Yes;
//					_grid.Columns.FromKey(sColumnaGrid).Type					= ColumnType.DropDownList;	
//					_grid.Columns.FromKey(sColumnaGrid).ValueList.DisplayStyle	= ValueListDisplayStyle.DisplayText;
//					_grid.Columns.FromKey(sColumnaGrid).ValueList = vl;
//				}
//				else
//				{
					// Special table (Globalizes table CODES)
					if (_ds.Tables[sTaulaCombo].TableName == "CODES")
					{
						_rm.GlobalizeTable(_ds.Tables[sTaulaCombo], "COD_LIT_ID", "COD_NEWDESC", true);
					}

					// Create a ValueList for Drop-Down
					ValueList vl		= new ValueList();
					vl.DataMember		= sTaulaCombo;
					// Convention: The id of the filed always is field 0, and the description always 1.
					vl.ValueMember		= _ds.Tables[sTaulaCombo].Columns[0].ColumnName;
					vl.DisplayMember	= _ds.Tables[sTaulaCombo].Columns[1].ColumnName;
					vl.DataSource		= _ds.Tables[sTaulaCombo];
					vl.DataBind();
					vl.ValueListItems.Insert(0, -999999, "-");

					_grid.Columns.FromKey(sColumnaGrid).AllowUpdate				= _bReadOnly ?  AllowUpdate.No :  AllowUpdate.Yes;
					_grid.Columns.FromKey(sColumnaGrid).Type					= ColumnType.DropDownList;	
					_grid.Columns.FromKey(sColumnaGrid).ValueList.DisplayStyle	= ValueListDisplayStyle.DisplayText;
					_grid.Columns.FromKey(sColumnaGrid).ValueList = vl;
					_valueListCombos[auxx] = vl;
//				}
			}
			return _valueListCombos;
		}

		public void ManageDateEvent(string e)
		{
			_grid.Rows[0].Cells[4].Text = e.ToString();
			// Create one WebCombo for each relation 
		}

		public string SaveData()
		{
			TraceUWGC("UltraWebGridController::SaveData",">>");
			if (_newDataRows.Count > 0) 
			{
				TraceUWGC("UltraWebGridController::SaveData","NewDataRows.Count > 0 ==> We are adding rows");
				//	The pk can be of different types
				TraceUWGC("UltraWebGridController::SaveData","The pk can be of different types");
				//	If autoincremental the field cannot be modified and a default value must be placed.
				long lastpk = _component.LastPKValue;
				
				TraceUWGC("UltraWebGridController::SaveData","New Key : " + lastpk);
				//if (lastpk == null) lastpk = -2;

				// we have new rows to be added
				foreach (System.Collections.DictionaryEntry o in _newDataRows) 
				{
					DataRow dr = (DataRow)o.Value;

					if (lastpk >= 0) 
					{
						if ((dr[_grid.Columns.FromKey(_pk[0]).Index] == null)   ||
							(dr[_grid.Columns.FromKey(_pk[0]).Index].ToString() == ""))

					
						{
							TraceUWGC("UltraWebGridController::SaveData","Primary Key is null ==> Use AutoIncrement");
							TraceUWGC("UltraWebGridController::SaveData","Old Key : " + dr[_grid.Columns.FromKey(_pk[0]).Index].ToString());
							dr[_grid.Columns.FromKey(_pk[0]).Index] = ++lastpk;
							TraceUWGC("UltraWebGridController::SaveData","New Key : " + dr[_grid.Columns.FromKey(_pk[0]).Index].ToString());
						}
						else
						{
							TraceUWGC("UltraWebGridController::SaveData","FromKey Value : " + dr[_grid.Columns.FromKey(_pk[0]).Index].ToString());
							TraceUWGC("UltraWebGridController::SaveData","Primary Key is NOT null ==> NOT Use AutoIncrement");
						}
					}
					try
					{
						_ds.Tables[_component.MainTable].Rows.Add (dr);
					}
					catch
					{
						TraceUWGC("UltraWebGridController::SaveData","ERROR : Adding Rows");
					}
				}
				
			}
			else
			{
				TraceUWGC("UltraWebGridController::SaveData","NewDataRows.Count <= 0 ==> We are Updating or Deleting");
			}

			if (_ds != null)
			{
				try
				{
					TraceUWGC("UltraWebGridController::SaveData","_ds != null");
					_component.SaveData(_ds);
				}
				catch (Exception ex)
				{
					TraceUWGC("UltraWebGridController::SaveData","<<");
					return ex.Message;
				}
			}
			TraceUWGC("UltraWebGridController::SaveData","<<");
			return string.Empty;
		}

		public void SetTrace(TraceContext trace)
		{
			m_Trace = trace;
		}

		private void TraceUWGC(string szContext,string szMessage)
		{
			if( m_Trace != null )
				m_Trace.Write(szContext,szMessage);
			
		}
	}
}