using System;
using System.Collections;
using System.Data;

using OPS.Components.Data;
using OPS.Components.Globalization;

namespace OPS.Components.Globalization
{


	/// <summary>
	/// Defines a set of methods that can be used to populate the class with the IDs and associated strings
	/// That interface is for using internally, and is not intended to be used outside of its own namespace
	/// </summary>
	interface IResourceLoader
	{
		/// <summary>
		/// Loads the resources (IDs and strings)
		/// </summary>
		void LoadResources();

		/// <summary>
		/// Frees the current resources loaded
		/// </summary>
		void FreeResources();
	}

	/// <summary>
	/// That class provides a set of methods used to get the string associated to a specific ID
	/// Each instance of that class can manage strings for a culture (language)
	/// That class loads the resources from a Database.
	/// </summary>
	[Serializable]
	public class ResourceManagerFromDB : IResourceManager, IResourceLoader
	{
		protected string _culture;
		protected int _logicalUnit;
		protected Hashtable _data;
		protected Hashtable _datamessage;
		protected Hashtable _fields;
		protected Hashtable _statics;
		protected Hashtable _messages;
		internal ResourceManagerFromDB(string culture, int logicalUnit)
		{
			_culture = culture;
			_logicalUnit = logicalUnit;
			_data = new Hashtable();
			_datamessage = new Hashtable();
			_fields = new Hashtable();
			_statics = new Hashtable();
			_messages = new Hashtable();
		}

		//********************* Implementation of IResourceLoader *************************
		public void LoadResources() 
		{
			// STEP 1: Loads the literals associated with the culture.
			if (_data.Count > 0) _data.Clear();
			OPS.Components.Data.ResourceManagerDB rmdb = new OPS.Components.Data.ResourceManagerDB();
			DataSet ds = rmdb.LoadResources(_culture, _logicalUnit);
			// Populates the hashtable with the info of the DataSet.
			foreach (DataRow dr in ds.Tables[0].Rows) 
			{
				_data.Add (Convert.ToInt32(dr[0]), dr[1].ToString());
			}

			// STEP 1.1: Loads the literals associated with the culture.
			if (_datamessage.Count > 0) _datamessage.Clear();
			//OPS.Components.Data.ResourceManagerDB rmdbm = new OPS.Components.Data.ResourceManagerBD();
			DataSet dsm = rmdb.LoadMenuResources(_culture, _logicalUnit);
			// Populates the hashtable with the info of the DataSet.
			foreach (DataRow dr in dsm.Tables[0].Rows) 
			{
				_datamessage.Add (Convert.ToInt32(dr[0]), new MessageBoxLiteral (dr[1].ToString(),dr[2].ToString()));
			}



			// STEP 2: Loads the logical field names associated with the culture
			if (_fields.Count > 0) _fields.Clear();
			ds.Clear();
			ds = rmdb.LoadFieldResources(_culture, _logicalUnit);
			// Populates the (second) hashtable with the info of the DataSet
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				_fields.Add (dr[0].ToString(), dr[1].ToString());
			}
			
			// STEP 3: Loads the static resources (like labels and so one)			
			if (_statics.Count > 0) _statics.Clear();
			ds.Clear();
			ds = rmdb.LoadStaticResources(_culture, _logicalUnit);
			// Populates the (third) hashtable with the info of the DataSet
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				_statics.Add (dr[0].ToString(), dr[1].ToString());
			}
			// STEP 4: Loads the messagebox resources (title and desc)
			if (_messages.Count > 0) _messages.Clear();
			ds.Clear();
			ds = rmdb.LoadMessageBoxResources (_culture, _logicalUnit);
			// Populates the (fourth) hashtable with the info of the DataSet
			foreach (DataRow dr in ds.Tables[0].Rows)
			{
				_messages.Add (Convert.ToInt32(dr[0]),new MessageBoxLiteral (dr[1].ToString(),dr[2].ToString()));
			}
		}

		public void FreeResources()
		{
			_data.Clear();
			_fields.Clear();
		}
		//******************** Implementation of IResourceManager **************************
		public string Culture 
		{
			get {return _culture;}
		}
		[System.Runtime.CompilerServices.IndexerName("GetString")]
		public string this[int litid] 
		{
			get 
			{
				return (string)_data[litid];
			}
		}

		public OPS.Components.Globalization.MessageBoxLiteral GetComplete (int litid)
		{
			return (MessageBoxLiteral)_datamessage[litid];
		}

		[System.Runtime.CompilerServices.IndexerName("GetString")]
		public string this[string litdescshort]
		{
			get 
			{
				string sret = (string)_fields[litdescshort];
				if (sret == null)
				{
					sret = (string)_fields[litdescshort.ToUpper()];
				}
				return sret != null ? sret : litdescshort;
			}
		}
		
		[System.Runtime.CompilerServices.IndexerName("GetString")]
		public string this [string classname, string cltid]
		{
			get 
			{
				string litdescshort = classname + "." + cltid;
				string sret = (string)_statics[litdescshort.ToUpper()];
				if (sret==null) 
				{
					// If not found search for * in the class name (* means all classes)
					string litdescshortcom = "*." + cltid;
					sret = (string)_statics[litdescshortcom.ToUpper()];
				}


				if (sret==null)
				{
					if (litdescshort.ToUpper().StartsWith("CONTROLS_"))
					{
						sret = (string)_statics[litdescshort.Remove(0,9).ToUpper()];
					}

				}
				return sret != null ? sret : "{" + litdescshort + "}";
			}
		}

		public OPS.Components.Globalization.MessageBoxLiteral GetMessage (int litid)
		{
			return (MessageBoxLiteral)_messages[litid];
		}

		public void GlobalizeTable (DataTable dt,string litCol, string newCol, bool removeLitCol)
		{
			DataColumn dcol = new DataColumn(newCol,Type.GetType("System.String"));
			dcol.AllowDBNull = true;
			dcol.DefaultValue = DBNull.Value;
	
			if (dt.Columns[dcol.ColumnName] == null)
			{
				dt.Columns.Add(dcol);
				foreach (DataRow dr in dt.Rows)
				{
					if (dr[litCol] != DBNull.Value)
					{
						dr[newCol] = this[Convert.ToInt32(dr[litCol])];
					}
					else
					{
						dr[newCol] = dr[litCol];
					}
				}
				dcol.ReadOnly = true;
				dt.AcceptChanges();
				if (removeLitCol) 
				{
					dt.Columns.Remove(litCol);
				}
			}
		}
	}

	/// <summary>
	/// Factory class to obtain instances od IResourceManager associated to a specified culture
	/// </summary>
	public sealed class ResourceManagerFactory 
	{
		// This hashtable will store all ResourceManager objects created (one for each culture used)
		private static Hashtable _resourceManagers;
		// No one can create ResourceManagerFactory objects
		private ResourceManagerFactory() {}
		/// <summary>
		/// Gets a IResourceManager object associated to a specified culture
		/// </summary>
		/// <param name="culture">Culture to use</param>
		/// <returns>A IResourceManager ready to use, with the data associated to specified culture</returns>
		public static IResourceManager GetResourceManager (string culture, int logicalUnit) 
		{
			if (_resourceManagers == null) _resourceManagers = new Hashtable();
 
			// Search if exists a ResourceManager for the specified culture
			IResourceManager rm = (IResourceManager)_resourceManagers[culture];
			if (rm!=null) return rm;
			// If not exists a ResourceManager, one is created and returned
			ResourceManagerFromDB rmo = new ResourceManagerFromDB(culture, logicalUnit);
			rmo.LoadResources();
			_resourceManagers.Add(culture, rmo);
			return (IResourceManager)rmo;
		}
	}	
}
