using System;
using System.Data;

using Infragistics.WebUI.UltraWebToolbar;

namespace OTS.Framework.WebUI.Infragistics
{
	/// <summary>
	/// That clas configures a Infragistic's UltraWebToolbar to allow filtering by
	/// fields of a DataTable
	/// </summary>
	public class UltraWebToolbarFilterHelper
	{
		protected UltraWebToolbar _toolbar;
		protected DataTable _data;
		/// <summary>
		/// Constructs a new ToolbarFilterHelper object.
		/// Toolbar will allow filter by fields of DataTable specified
		/// </summary>
		/// <param name="toolbar">Toolbar to configure</param>
		public UltraWebToolbarFilterHelper(UltraWebToolbar toolbar)
		{
			_toolbar = toolbar;
			_toolbar.ButtonClicked+=new UltraWebToolbar.ButtonClickedEventHandler(toolbar_ButtonClicked);
			// Set some layout properties
			_toolbar.Movable = false;

		}

		/// <summary>
		/// Allow to get and set the DataTable object used to create the filter toolbar
		/// </summary>
		public DataTable DataSource
		{
			get  { return _data;}
			set  { _data = value;}
		}

		/// <summary>
		/// Creates the filter toolbar
		/// </summary>
		public void DataBind()
		{
			if (_data == null) return;

			foreach (DataColumn dc in _data.Columns) 
			{
				TBarButton button =  _toolbar.Items.AddButton (dc.ColumnName,dc.ColumnName);
				button.ToggleButton = true;
			}
		}

		private void toolbar_ButtonClicked(object sender, ButtonEvent e)
		{

		}
	}
}
