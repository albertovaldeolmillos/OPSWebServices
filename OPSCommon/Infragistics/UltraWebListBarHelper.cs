using System;
using System.Data;
using System.Security.Principal;
using Infragistics.WebUI.UltraWebListbar;


namespace OTS.Framework.WebUI.Infragistics
{
	/// <summary>
	/// Summary description for UltraWebListBarHelper.
	/// </summary>
	public class UltraWebListBarHelper
	{
		protected UltraWebListbar _uwlb;
		public UltraWebListBarHelper(UltraWebListbar control)
		{
			_uwlb = control;
			_uwlb.GroupClicked +=new GroupClickedEventHandler(uwlb_GroupClicked);
			_uwlb.ItemClicked +=new ItemClickedEventHandler(uwlb_ItemClicked);
			_uwlb.Load +=new EventHandler(uwlb_Load);
		}

		/// <summary>
		/// Sets the menu properties based on the info contained in userInfo object
		/// </summary>
		/// <param name="userInfo"></param>
		public void BuildMenu()
		{

		}

		private void uwlb_Load (object sender, System.EventArgs e) 
		{

			_uwlb.Groups[0].Items[0].TargetUrl					= "Webform1.aspx";
			_uwlb.Groups[0].Items[1].Text						= "Grid";

			_uwlb.Groups[0].Items[1].TargetUrl					= "http://thales/ig_UltraChartSamples/CS/CSChartGallery/Default.aspx";
			_uwlb.Groups[0].Items[1].Text						= "Estadístiques";

			_uwlb.Groups[0].Items[2].TargetUrl					= "http://thales/ig_UltraChartSamples/CS/CS3DDemo/Default.aspx";
			_uwlb.Groups[0].Items[2].Text						= "Estadístiques 3D";

			_uwlb.Groups[2].Items[0].TargetUrl					= "http://thales/ig_UltraWebNavigator2Samples/UltraWebTree/cs/FileUrl/webform1.aspx";
			_uwlb.Groups[2].Items[0].Text						= "Arbre";

			_uwlb.Groups[2].Items[1].TargetUrl					= "http://thales/FileUrl/WebForm2.aspx";
			_uwlb.Groups[2].Items[1].Text						= "Arbre 2";
		
		
			// Build the menu
			//CmpSecurity c = (CmpSecurity)Session["CmpSecurity"];
			//DataTable dtViews = c.ViewsAllowedByRole;
		}




		public UltraWebListbar UWListBar
		{
			get { return _uwlb;}
		}

		private void uwlb_GroupClicked(object sender, WebListbarGroupEvent e)
		{

		}

		private void uwlb_ItemClicked(object sender, WebListbarItemEvent e)
		{
			// An Item of the UltraWebListBar was clicked.

		}
	}
}
