using System.Data;

namespace OPS.Components.Globalization
{
	
	/// <summary>
	/// Defines a literal to show into a MessageBox
	/// </summary>
	public sealed class MessageBoxLiteral
	{
		// title
		private string _title;
		// message
		private string _msg;
		/// <summary>
		/// Builds a Literal with a title and a message
		/// </summary>
		/// <param name="title">Title of the literal</param>
		/// <param name="msg">Body of the literal</param>
		public MessageBoxLiteral (string title, string msg)
		{
			_title = title;
			_msg = msg; 
		}
		/// <summary>
		/// Gets the title
		/// </summary>
		public string Title { get { return _title; } }
		/// <summary>
		/// Gets the Message
		/// </summary>
		public string Message { get { return _msg;} }
	}

	/// <summary>
	/// Defines a set of methods that can be used to translate ID to strings.
	/// </summary>
	public interface IResourceManager 
	{
		/// <summary>
		/// Gets the current culture of the class (that means in what language the IDs will be translated)
		/// </summary>
		string Culture {get;}
		/// <summary>
		/// Gets resource-string identified by its id (numerical)
		/// </summary>
		[System.Runtime.CompilerServices.IndexerName("GetString")]
		string this[int litid] {get;}

		/// <summary>
		/// Gets a resource-string identified by its descshort
		/// That ONLY applies to the resources that are logical name of database fields
		/// The decshort is always in format TABLE.FIELDNAME 
		/// </summary>
		[System.Runtime.CompilerServices.IndexerName("GetString")]
		string this[string litdescshort] {get;}

		/// <summary>
		/// Retrieves a static-resource string
		/// Static-resource string are used for labels and other controls placed
		/// in design-time (not for trees or menus dinamically built).
		/// That indexer has 2 parameters:
		/// Name of the class (NOT FULLY-QUALIFIED) who contains the control
		/// ID of the control
		/// <returns>The culture-dependent value of the selected control for the specified class</returns>
		/// </summary>
		[System.Runtime.CompilerServices.IndexerName("GetString")]
		string this [string classname, string cltid] { get; }

		/// <summary>
		/// Returns a message-box message resource
		/// A messagebox message has two items: title and message
		/// </summary>
		/// <param name="litid">ID of the literal. DESCLONG is message and DESCSHORT is title</param>
		/// <returns>The message (title and messge) to show in the MessageBox</returns>
		MessageBoxLiteral GetMessage (int litid);

		/// <summary>
		/// That method gets a DataColumn of a table, which has a column of integers that are references
		/// to literals, and adds a new column (of strings) with the value of this literals, using
		/// the ResourceManager specified.
		/// </summary>
		/// <param name="dt">DataTable to globalize</param>
		/// <param name="litCol">Name of the EXISTING DataColumn containing the values of literals IDs</param>
		/// <param name="newCol">Name of the NEW DataColumn that will be created with the values of literals</param>
		/// <param name="removeLitCol">If true, the column litCol is removed from DataTable</param>
		void GlobalizeTable (DataTable dt, string litCol, string newCol, bool removeLitCol);
	}

}