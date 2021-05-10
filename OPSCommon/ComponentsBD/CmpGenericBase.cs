using System;
using System.Data;
using OPS.Components.Data;
using OPS.Components.Globalization;
using System.Web;

using System.Collections.Specialized;
using System.Collections;

namespace OPS.Components.Data
{
	/// <summary>
	/// Summary description for CmpGenericBase.
	/// </summary>
	public class CmpGenericBase: CmpBase
	{
		protected CmpGenericBase() {}

		//private static OPS.Components.Data.Database _db = null;

		protected string[]	_standardFields;
		protected string[]	_standardPks;
		protected string	_standardTableName;
		protected string	strComponentsBDAssemblyName;
	
		// Field of main table whois foreign Key of table
		protected string[]	_standardRelationFileds;
		protected string[]	_standardRelationTables;
		protected string[]	_stValidDeleted;
		protected string	_standardOrderByField;
		protected string	_standardOrderByAsc;

		protected ArrayList standardFields = new ArrayList();
		protected ArrayList standardPks = new ArrayList();
		protected ArrayList standardRelationTables = new ArrayList();
		protected ArrayList standardRelationFields = new ArrayList();
		protected ArrayList standardRelationKeys = new ArrayList();
		protected ArrayList standardRelationShowns = new ArrayList();
		protected ArrayList standardRelationTranslate = new ArrayList();
		protected ArrayList stValidDeleted = new ArrayList();
		protected string standardOrderByField;
		protected string standardOrderByAsc;
		protected string standardTableName;
		// XNA_08_10_2007
		protected string	standardWhere;
		// XNA_08_10_2007

		protected string[]	sstandardFields;
		protected string[]	sstandardPks;
		protected string[]	sstandardRelationTables;
		protected string[]	sstandardRelationFields;
		protected string[]	sstandardRelationKeys;
		protected string[]	sstandardRelationShowns;
		protected bool[]	sstandardRelationTranslate;
		protected string[]	sstValidDeleted;
		

		protected bool		m_bPKReadOnly = true;

		#region Public properties
		/// <summary>
		/// Gets and sets the property for Relation Fields
		/// </summary>
		public string[]	GetStandardRelationFields
		{
			get { return _standardRelationFileds; }
			set { _standardRelationFileds = value; }
		}
		/// <summary>
		/// Gets and sets the property for Relation Tables
		/// </summary>
		public string[]	GetStandardRelationTables
		{
			get { return _standardRelationTables; }
			set { _standardRelationTables = value; }
		}
		/// <summary>
		/// Gets and sets the property for the Table
		/// </summary>
		public string	GetStandardTableName
		{
			get { return _standardTableName; }
			set { _standardTableName = value; }
		}

		// XNA_08_10_2007
		/// <summary>
		/// Gets and sets the standard Filter (if any)
		/// </summary>
		public string	GetStandardWhere
		{
			get { return standardWhere; }
			set { standardWhere = value; }
		}
		// XNA_08_10_2007


		/// <summary>
		/// Gets and sets the property for the fields of valid & deleted
		/// </summary>
		public string[]	GetstValidDeleted
		{
			get { return _stValidDeleted; }
			set { _stValidDeleted = value; }
		}
		/// <summary>
		/// Gets and sets the property for the Fields
		/// </summary>
		public string[]	GetStandardFields
		{
			get { return _standardFields; }
			set { _standardFields = value; }
		}
		/// <summary>
		/// Gets and sets the property for the Pk's
		/// </summary>
		public string[]	GetStandardPks
		{
			get { return _standardPks; }
			set { _standardPks = value; }
		}
		public string GetStandardOrderByField
		{
			get { return _standardOrderByField; }
			set { _standardOrderByField = value; }
		}
		public string GetStandardOrderByAsc
		{
			get { return _standardOrderByAsc; }
			set { _standardOrderByAsc = value; }
		}
		public override bool GetPKReadOnly
		{
			get { return m_bPKReadOnly;   }
			//set { m_bPKReadOnly = value;  }
		}

		#endregion

		#region Implementation of CmpDataSourceAdapter		

		public override void GetForeignData(DataSet ds, string sTable) 
		{
			GetForeignData(ds, sTable, null);
		}

		public override void GetForeignData(DataSet ds, string sTable, OPS.Components.Globalization.IResourceManager rm) 
		{
			// Loads XML Schema of DataBase 
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			strComponentsBDAssemblyName=(string)appSettings.GetValue("ComponentsBDAssemblyName",typeof(string));

			LoadSchema();
			DataTable parent = ds.Tables[sTable];

			for (int aux=0; aux < sstandardRelationTables.Length; aux++)
			{
				Type t	= Type.GetType(sstandardRelationTables[aux]);
				System.Reflection.ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
				DataTable dtfk = new DataTable();

				//  For Yes No Blocks we get only Yes/No
				if (sstandardRelationTables[aux].ToString() == ("OPS.Components.Data.CmpCodesDB, "+strComponentsBDAssemblyName))
				{
					// Add the table of languages to the DataSet
					string[] cfields = new string[] {"COD_ID", "COD_LIT_ID"};
					dtfk = new OPS.Components.Data.CmpCodesDB().GetYesNoData(cfields);
					if (rm!=null) 
					{
						rm.GlobalizeTable(dtfk, "COD_LIT_ID", "COD_NEWDESC", true);
					}
				}
				else
				{
					dtfk = ((OPS.Components.Data.ICmpLocalizedDataSource)ci.Invoke(null)).GetData
						(new string[] {sstandardRelationKeys[aux], sstandardRelationShowns[aux]}, 
						null,
						sstandardRelationShowns[aux] + " ASC",
						null);
					if ((sstandardRelationTranslate[aux]) && (rm != null))
					{
						rm.GlobalizeTable(dtfk, sstandardRelationShowns[aux], "GLOBFIELD", true);
					}
				}

				if (!ds.Tables.Contains(dtfk.TableName))
				{
					while (dtfk.Columns.Count > 2)
						dtfk.Columns.Remove(dtfk.Columns[2]);
					ds.Tables.Add(dtfk);
				}
				else
				{
					dtfk = ds.Tables[dtfk.TableName];
				}

				try 
				{
					ds.Relations.Add ((dtfk.PrimaryKey)[0],parent.Columns[sstandardRelationFields[aux]]);
				}
				catch (ArgumentException)
				{
					throw new ArgumentException(string.Format("El campo {0} de la tabla {1} contiene algun valor invalido",sstandardRelationFields[aux], sTable));
				}
			}
		}

		public override DataTable GetData (string[] fields, string where, string orderby, object[] values) 
		{
			Database d = DatabaseFactory.GetDatabase ();
			// Loads XML Schema of DataBase 
			LoadSchema(d);

			if (where == null) 
			{
				// XNA_08_10_2007
				//where = "";
				if (standardWhere != null)
					where = standardWhere;
				else
					where = "";				
				// XNA_08_10_2007
			}
			where = AddValidAndDeleted2Where(where);

			if (fields == null) 
				fields = sstandardFields;

			string[] pk = sstandardPks;

			System.Text.StringBuilder sb = base.ProcessFields(fields,pk);
			if (standardOrderByField != null)
			{
				if (standardOrderByField.Length > 1)
				{
					if (orderby != null && orderby != "")
					{
						orderby	+= ", " + standardOrderByField + " " +  standardOrderByAsc;
					}
					else
					{
						orderby	= standardOrderByField + " " +  standardOrderByAsc;
					}

				}
			}
		
			return base.DoGetData (sb.ToString(),where, orderby, values, standardTableName,standardTableName,pk);
		}
		public override DataTable GetPagedData (string[] fields, string orderByField, bool orderByAsc, string where, int rowstart, int rowend)
		{
			Database d = DatabaseFactory.GetDatabase();
			// Loads XML Schema of DataBase 
			LoadSchema(d);

			if (where == null) 
			{
				// XNA_08_10_2007
				//where = "";
				if (standardWhere != null)
					where = standardWhere;
				else
					where = "";				
				// XNA_08_10_2007
			}
			where = AddValidAndDeleted2Where(where);

			if (fields == null)
				fields = sstandardFields;

			string[] pk = sstandardPks;

			System.Text.StringBuilder sb = base.ProcessFields(fields,pk);

			if (standardOrderByField != null)
			{
				if (standardOrderByField.Length > 1 && orderByField.Length > 1)
				{
					orderByField += ", " + standardOrderByField;
					//orderByAsc		= standardOrderByAsc;
				}
				else
				{
					if (standardOrderByField.Length > 1)
					{
						orderByField += " " + standardOrderByField;
						//orderByAsc		= standardOrderByAsc;
					}
				}
			}

			// This is a workaround
			if (orderByField != "")
			{
				string pkstring = "";
				for (int i=0; i<pk.Length; i++)
				{
					if (i==0) pkstring = pk[i];
					else pkstring += " || " + pk[i];
				}
				//orderByField = orderByField + " || '$' || " + pkstring;
			}

			return base.DoGetPagedData (sb.ToString(), orderByField,orderByAsc,where,rowstart,rowend,standardTableName,standardTableName,pk);
		}

		public override void SaveData (DataTable dt)
		{
			// Loads XML Schema from database 
			LoadSchema();

			Database d = DatabaseFactory.GetDatabase();
			IDbConnection con = d.GetNewConnection();
			con.Open();

			IDbDataAdapter da = d.GetDataAdapter();
			da.InsertCommand = d.PrepareCommand(CreateInsertCommand(), false);
			da.UpdateCommand = d.PrepareCommand(CreateUpdateCommand(), false);
			da.DeleteCommand = d.PrepareCommand(CreateDeleteCommand(), false);
			da.InsertCommand.Connection = con;
			da.UpdateCommand.Connection = con;
			da.DeleteCommand.Connection = con;

			d.UpdateDataSet(da, dt);
			dt.AcceptChanges();
			con.Close();
		}

		public override string MainTable
		{
			get { return standardTableName; }
		}

		public override long GetCount(string where, params object[] values)
		{
			Database d = DatabaseFactory.GetDatabase();
			LoadSchema(d);

			if (where == null) 
			{
				// XNA_08_10_2007
				//where = "";
				if (standardWhere != null)
					where = standardWhere;
				else
					where = "";				
				// XNA_08_10_2007
			}
			where = AddValidAndDeleted2Where(where);

			string sql = "SELECT COUNT(*) FROM "  + standardTableName;
			if (where != "") 
			{
				sql += " WHERE " + where;
			}
			return Convert.ToInt64(d.ExecuteScalar(sql, values));
		}

		public override Int64 LastPKValue
		{
			get 
			{
				Database d = DatabaseFactory.GetDatabase();
				// Loads XML Schema of DataBase 
				LoadSchema(d);

				string slastpkvalue = "";
				slastpkvalue = CreateLastPkValueCommand();

				if (sstandardPks.Length <=1)
				{
					object olastpkvalue	= d.ExecuteScalar (slastpkvalue);

					if (olastpkvalue != null && olastpkvalue.ToString() != DBNull.Value.ToString() )
					{
						return Convert.ToInt64(olastpkvalue);
					}
					else
					{
						return Convert.ToInt64(0); 
					}
				}
				else
				{
					return Convert.ToInt64(-1); 
				}
			}
		}
		#endregion

		#region Automation of Database

		public string CreateUpdateCommand()
		{
			string supdateCommand = "";
			supdateCommand = "UPDATE " + standardTableName + " SET ";
			
			// We assume that the pk fields are in firsts position on the table, 
			// if not, update standardFields to match this implementation
			// The pk fields have to be less or equal thant fields, 

			for (int aux = sstandardPks.Length - 1; aux<sstandardFields.Length; aux++)
			{
				// temporary exception: Fin Vehicle Id cannot be modified.
				// We remove it from update command
				if (sstandardFields[aux] != "FIN_VEHICLEID" || standardTableName != "FINES")
				{
					supdateCommand = supdateCommand + standardFields[aux] + " = @" + standardTableName + "." + standardFields[aux] + "@";
				}
				if (aux < sstandardFields.Length - 1)
				{
					if (sstandardFields[aux+1] != "FIN_VEHICLEID" || standardTableName != "FINES")
					{
						supdateCommand	= supdateCommand + ", ";
					}
				}
			}
			
			supdateCommand = supdateCommand + " WHERE (";

			for (int aux = 0; aux < sstandardPks.Length;aux++)
			{
				supdateCommand = supdateCommand + standardFields[aux] + " = @" + standardTableName + "." + standardFields[aux] + "@";
				if (aux < sstandardPks.Length - 1)
				{
					supdateCommand	= supdateCommand + " AND ";
				}
			}

			// Add the validation of valid = 1 & deleted = 0;
			if (sstValidDeleted.Length > 0)
			{
				supdateCommand = supdateCommand + " AND " + stValidDeleted[0].ToString() +  "=1 AND " + stValidDeleted[1].ToString() + "=0";
			}

			supdateCommand = supdateCommand + ")";
			return supdateCommand;
		}
			

		public string CreateLastPkValueCommand()
		{
			string slastpkvalue = "";
				slastpkvalue = "SELECT MAX (" ;
				for (int aux =0; aux< sstandardPks.Length; aux++)
				{
					slastpkvalue = slastpkvalue + standardPks[aux];
					if (aux < sstandardPks.Length - 1)
					{
						slastpkvalue	= slastpkvalue + ",";
					}			
				}
				slastpkvalue	= slastpkvalue + ") FROM " + standardTableName;
			return slastpkvalue;
		}


		public string CreateInsertCommand()
		{
			string		sinsertcommand		= "";
			
			sinsertcommand		= "INSERT INTO " + standardTableName + " ( ";
			
			for (int aux = 0; aux < sstandardFields.Length; aux++)
			{
				sinsertcommand	= sinsertcommand + standardFields[aux];
				if (aux < sstandardFields.Length - 1)
				{
					sinsertcommand	= sinsertcommand + ",";
				}
			}

			sinsertcommand	= sinsertcommand + ") VALUES ( ";

			for (int aux = 0; aux < sstandardFields.Length; aux++)
			{
				sinsertcommand	= sinsertcommand + "@" + standardTableName + "." + standardFields[aux] + "@";
				if (aux < sstandardFields.Length - 1)
				{
					sinsertcommand	= sinsertcommand + ",";
				}
			}
			sinsertcommand	 = sinsertcommand + ")";	

			return sinsertcommand;
		}

		public string CreateDeleteCommand()
		{
			string sdeletecommand = "";
			// Add the validation of valid = 1 & deleted = 0;
			if (sstValidDeleted.Length > 0)
			{
				sdeletecommand = "UPDATE " + standardTableName + " SET ";

				// Add the validation of valid = 1 & deleted = 0;
				sdeletecommand = sdeletecommand + stValidDeleted[1].ToString() + "=1" + " WHERE (";
				sdeletecommand = sdeletecommand + stValidDeleted[0].ToString() + "=1 AND ";
				for (int aux = 0; aux< sstandardPks.Length;aux++)
				{
					sdeletecommand = sdeletecommand + standardFields[aux] + " = @" + standardTableName + "." + standardFields[aux] + "@";
					if (aux < sstandardPks.Length - 1)
					{
						sdeletecommand	= sdeletecommand + " AND ";
					}
				}
				sdeletecommand = sdeletecommand + ")";
			}
			else
			{
				sdeletecommand = "DELETE FROM " + standardTableName + " WHERE ";

				for (int aux = 0; aux< sstandardPks.Length;aux++)
				{
					sdeletecommand = sdeletecommand + standardFields[aux] + " = @" + standardTableName + "." + standardFields[aux] + "@";
					if (aux < sstandardPks.Length - 1)
					{
						sdeletecommand	= sdeletecommand + " AND ";
					}
				}
			}
			return sdeletecommand;
		}

		#endregion

		#region Load database schema
		public void LoadSchema()
		{
			Database d = DatabaseFactory.GetDatabase();
			// Loads XML Schema of DataBase 
			LoadSchema(d);
		}

		public void LoadSchema(OPS.Components.Data.Database d)
		{
			standardOrderByField	= _standardOrderByField;
			standardOrderByAsc	= _standardOrderByAsc;
			standardTableName	= _standardTableName;
			
			stValidDeleted.Clear();
			standardFields.Clear();
			standardPks.Clear();
			standardRelationTables.Clear();
			standardRelationFields.Clear();
			standardRelationKeys.Clear();
			standardRelationShowns.Clear();
			standardRelationTranslate.Clear();

			for (int aux=0;aux < _stValidDeleted.Length; aux++)
			{
				stValidDeleted.Add(_stValidDeleted[aux]);
			}

			// When we have the table described in the UserSchema, we must load
			// its settings for the fields/columns of the table from there. 
			if (d.UserSchema[standardTableName] != null)
			{
				ArrayList _array = (ArrayList) d.UserSchema[standardTableName];
				string[] _newStandardFields = new string[_array.Count];

				// Creates an enumerator and get all elements
				IEnumerator myEnumerator = _array.GetEnumerator();
				
				while ( myEnumerator.MoveNext() )
				{
					// XNA_08_10_2007
					TableUserSchemaInfo currentTUSI = (TableUserSchemaInfo)myEnumerator.Current;

					standardWhere = currentTUSI.Where;

					IEnumerator enumTables = currentTUSI.TableFieldUserSchemaInfo.GetEnumerator();
					while (enumTables.MoveNext())
					{
						FieldUserSchemaInfo currentFUSI = (FieldUserSchemaInfo)enumTables.Current;
						if (currentFUSI.FieldVisible)
						{
							standardFields.Add(currentFUSI.FieldName);
							if (currentFUSI.FieldPrimaryKey)
							{
								standardPks.Add(currentFUSI.FieldName);
								standardFields.Remove(currentFUSI.FieldName);
								standardFields.Insert(0, currentFUSI.FieldName);
							}
							if (currentFUSI.FieldRelTable.Length > 0)
							{
								standardRelationTables.Add(TranslateTable2Component(currentFUSI.FieldRelTable));
								standardRelationFields.Add(currentFUSI.FieldName);
								standardRelationKeys.Add(currentFUSI.FieldRelField);
								standardRelationShowns.Add(currentFUSI.FieldRelShown);
								standardRelationTranslate.Add(currentFUSI.FieldRelTranslate);
							}
							if ( currentFUSI.FieldReadOnly)
							{
								m_bPKReadOnly = true;
							}
							else
							{
								m_bPKReadOnly = false;
							}
						}
					}
					// XNA_08_10_2007
				}
			}
				// In case schema does not contain information about fields, We get standard.
			else
			{

				for (int aux=0;aux < _standardFields.Length; aux++)
				{
					standardFields.Add(_standardFields[aux]);
				}
				for (int aux=0;aux < _standardPks.Length; aux++)
				{
					standardPks.Add(_standardPks[aux]);
				}
				for (int aux=0;aux < _standardRelationFileds.Length; aux++)
				{
					standardRelationFields.Add(_standardRelationFileds[aux]);
				}
				for (int aux=0;aux < _standardRelationTables.Length; aux++)
				{
					standardRelationTables.Add(_standardRelationTables[aux]);
				}
			}

			IEnumerator mEnumerator = null;
			int contador = 0;

			mEnumerator = standardFields.GetEnumerator();
			contador = 0;
			sstandardFields = new string[standardFields.Count];
			while ( mEnumerator.MoveNext() )
			{
				sstandardFields[contador] = mEnumerator.Current.ToString();
				contador ++;
			}
			
			mEnumerator = standardPks.GetEnumerator();
			contador = 0;
			sstandardPks = new string[standardPks.Count];
			while ( mEnumerator.MoveNext() )
			{
				sstandardPks[contador] = mEnumerator.Current.ToString();
				contador ++;
			}
			
			mEnumerator = standardRelationTables.GetEnumerator();
			contador = 0;
			sstandardRelationTables = new string[standardRelationTables.Count];
			while ( mEnumerator.MoveNext() )
			{
				sstandardRelationTables[contador] = mEnumerator.Current.ToString();
				contador ++;
			}
			
			mEnumerator = standardRelationFields.GetEnumerator();
			contador = 0;
			sstandardRelationFields = new string[standardRelationFields.Count];
			while ( mEnumerator.MoveNext() )
			{
				sstandardRelationFields[contador] = mEnumerator.Current.ToString();
				contador ++;
			}
			
			mEnumerator = standardRelationKeys.GetEnumerator();
			contador = 0;
			sstandardRelationKeys = new string[standardRelationKeys.Count];
			while ( mEnumerator.MoveNext() )
			{
				sstandardRelationKeys[contador] = mEnumerator.Current.ToString();
				contador ++;
			}
			
			mEnumerator = standardRelationShowns.GetEnumerator();
			contador = 0;
			sstandardRelationShowns = new string[standardRelationShowns.Count];
			while ( mEnumerator.MoveNext() )
			{
				sstandardRelationShowns[contador] = mEnumerator.Current.ToString();
				contador ++;
			}

			mEnumerator = standardRelationTranslate.GetEnumerator();
			contador = 0;
			sstandardRelationTranslate = new bool[standardRelationTranslate.Count];
			while ( mEnumerator.MoveNext() )
			{
				sstandardRelationTranslate[contador] = bool.Parse(mEnumerator.Current.ToString());
				contador ++;
			}

			mEnumerator = stValidDeleted.GetEnumerator();
			contador = 0;
			sstValidDeleted = new string[stValidDeleted.Count];
			if (stValidDeleted.Count > 0)
			{
				while ( mEnumerator.MoveNext() )
				{
					sstValidDeleted[contador] = mEnumerator.Current.ToString();
					contador ++;
				}
			}
		}
		#endregion Load database schema

		#region Private support members
		private string TranslateTable2Component(string tableName)
		{
			string componentName = "";
			switch (tableName)
			{
				case "ALARMS":			componentName = "CmpAlarmsDB"; break;
				case "ALARMS_DEF":		componentName = "CmpAlarmsDefDB"; break;
				case "ALARMS_HIS":		componentName = "CmpAlarmsHisDB"; break;
				case "ALARMS_MSGS_DEF": componentName = "CmpAlarmsMsgsDefDB"; break;
				case "ARTICLES":		componentName = "CmpArticlesDB"; break;
				case "ARTICLES_DEF":	componentName = "CmpArticlesDefDB"; break;
				case "ARTICLES_RULES":	componentName = "CmpArticlesRulesDB"; break;
				case "BLACK_LISTS":		componentName = "CmpBlackListsDB"; break;
				case "BLACK_LISTS_DEF":	componentName = "CmpBlackListsDefDB"; break;
				case "CODES":			componentName = "CmpCodesDB"; break;
				case "CONSTRAINTS":		componentName = "CmpConstraintsDB"; break;
				case "CONSTRAINTS_DEF": componentName = "CmpConstraintsDefDB"; break;
				case "CURRENCIES":		componentName = "CmpCurrenciesDB"; break;
				case "CUSTOMERS":		componentName = "CmpCustomersDB"; break;
				case "DAYS":			componentName = "CmpDaysDB"; break;
				case "DAYS_DEF":		componentName = "CmpDaysDefDB"; break;
				case "DEVICES":			componentName = "CmpDevicesDB"; break;
				case "DEVICES_DEF":		componentName = "CmpDevicesDefDB"; break;
				case "DEVICES_PARAM":	componentName = "CmpDevicesParamDB"; break;
				case "FINES":			componentName = "CmpFinesDB"; break;
				case "FINES_DEF":		componentName = "CmpFinesDefDB"; break;
				case "FINES_HIS":		componentName = "CmpFinesHisDB"; break;
				//case "GPRS_TARIFF": componentName = "CmpGPRS_TARIFFDB"; break;
				//case "GPRS_TRAFFIC": componentName = "CmpGPRS_TRAFFICDB"; break;
				case "GROUPS":			componentName = "CmpGroupsDB"; break;
				case "VW_GROUPS_ZONES":	componentName = "CmpGroupsZonesDB"; break;
				case "VW_GROUPS_SECTORS": componentName = "CmpGroupsSectorsDB"; break;
				case "VW_GROUPS_ROUTES": componentName = "CmpGroupsRoutesDB"; break;
				case "GROUPS_CHILDS":	componentName = "CmpGroupsChildsDB"; break;
				case "GROUPS_CHILDS_GIS":	componentName = "CmpGroupsChildsGisDB"; break;
				case "GROUPS_DEF":		componentName = "CmpGroupsDefDB"; break;
				//case "GROUPS_OCCUPATION": componentName = "CmpGROUPS_OCCUPATIONDB"; break;
				case "GROUPS_PDA":		componentName = "CmpGroupsPdaDB"; break;
				case "INTERVALS":		componentName = "CmpIntervalsDB"; break;
				case "LANGUAGES":		componentName = "CmpLanguagesDB"; break;
				case "LITERALS":		componentName = "CmpLiteralsDB"; break;
				case "MODULES":			componentName = "CmpModulesDB"; break;
				case "MOBILE_USERS":			componentName = "CmpMobileUsersDB"; break;
				case "MOBILE_USERS_PLATES":			componentName = "CmpMobileUsersPlatesDB"; break;
				case "MOBILE_USERS_COMPANY_DEF":			componentName = "CmpMobileUsersCompanyDefDB"; break;
				case "MSGS":			componentName = "CmpMsgsDB"; break;
				case "MSGS_CONFIG":		componentName = "CmpMsgsConfigDB"; break;
				case "MSGS_DEF":		componentName = "CmpMsgsDefDB"; break;
				case "MSGS_FMT":		componentName = "CmpMsgsFmtDB"; break;
				case "MSGS_HIS":		componentName = "CmpMsgsHisDB"; break;
				case "MSGS_MEDIA":		componentName = "CmpMsgsMediaDB"; break;
				case "MSGS_RETRIES_DEF": componentName = "CmpMsgsRetriesDefDB"; break;
				case "MSGS_RULES":		componentName = "CmpMsgsRulesDB"; break;
				case "OPERATIONS":		componentName = "CmpOperationsDB"; break;
				case "VW_OPERATIONS_CC":		componentName = "CmpOperationsCCDB"; break;
				case "VW_OPERATIONS_HIS_CC":		componentName = "CmpOperationsHisCCDB"; break;
				case "OPERATIONS_DEF":	componentName = "CmpOperationsDefDB"; break;
				case "OPERATIONS_HIS":	componentName = "CmpOperationsHisDB"; break;
				case "PARAMETERS":		componentName = "CmpParametersDB"; break;
				case "PAYTYPES_DEF":	componentName = "CmpPaytypesDefDB"; break;
				//case "PDA_MAILS": componentName = "CmpPDA_MAILSDB"; break;
				//case "PDA_USER_REG": componentName = "CmpPDA_USER_REGDB"; break;
				case "PDA_USER_STAT":	componentName = "CmpPdaUserStatDB"; break;
				case "REPLICATIONS_DEF": componentName = "CmpReplicationsDefDB"; break;
				case "REPLICATIONS_PDA": componentName = "CmpReplicationsPdaDB"; break;
				case "REPLICATIONS_TABLES": componentName = "CmpReplicationsTablesDB"; break;
				case "ROLES":			componentName = "CmpRolesDB"; break;
				//case "ROL_ACCESS": componentName = "CmpROL_ACCESSDB"; break;
				//case "ROL_PERMISSIONS": componentName = "CmpROL_PERMISSIONSDB"; break;
				case "STATUS":			componentName = "CmpStatusDB"; break;
				case "STATUS_DEF":		componentName = "CmpStatusDefDB"; break;
				case "STATUS_PROFILES": componentName = "CmpStatusProfilesDB"; break;
				case "STREETS":			componentName = "CmpStreetsDB"; break;
				case "STREETS_STRETCHS":componentName = "CmpStreetsStretchsDB"; break;
				case "SUBTARIFFS":		componentName = "CmpSubtariffsDB"; break;
				case "TARIFFS":			componentName = "CmpTariffsDB"; break;
				case "TAXES":			componentName = "CmpTaxesDB"; break;
				case "TIMETABLES":		componentName = "CmpTimetablesDB"; break;
				case "UNITS":			componentName = "CmpUnitsDB"; break;
				case "UNITS_LOG_DEF":	componentName = "CmpUnitsLogDefDB"; break;
				case "UNITS_MEASURES":	componentName = "CmpUnitsMeasuresDB"; break;
				case "UNITS_OCCUPATION": componentName = "CmpUnitsOccupationDB"; break;
				case "UNITS_PARAMS":	componentName = "CmpUnitsParamsDB"; break;
				case "UNITS_PHY_DEF":	componentName = "CmpUnitsPhyDefDB"; break;
				case "UNITS_RESULTS":	componentName = "CmpUnitsResultsDB"; break;
				case "V_MB_UNITS_STREETS_STRETCHS": componentName = "CmpUnitsStreetsStretchsDB"; break;
				//case "USERS": componentName = "CmpUsersDB"; break;
				case "USERS":			componentName = "CmpUsuarioDB"; break;
				case "USR_ACCESS":		componentName = "CmpUsrAccessDB"; break;
				case "USR_PERMISSIONS": componentName = "CmpUsrPermissionsDB"; break;
				//case "VEHMANUFACTURERS": componentName = "CmpVehmanufacturersDB"; break;
				case "VEHMANUFACTURERS": componentName = "CmpVehManufacturersDB"; break;
				//case "VEHMODELS": componentName = "CmpVehmodelsDB"; break;
				case "VEHMODELS":		componentName = "CmpVehModelsDB"; break;
				case "VERSIONS":		componentName = "CmpVersionsDB"; break;
				case "VIEWS":			componentName = "CmpViewsDB"; break;
				case "VIEWS_ELEMENTS":	componentName = "CmpViewsElementsDB"; break;
				case "VIEWS_MODULES":	componentName = "CmpViewsModulesDB"; break;
				// CFE - Introduzco componente para nuevas tablas de multas
				case "FINES_STSADMON_DEF": componentName = "CmpFinesStsAdmonDB"; break;
				case "VEHCOLORS":		componentName = "CmpVehColorDB"; break;
				case "V_GROUPS_CHILDS_ROUTES": componentName = "CmpRoutesDB"; break;
				case "SENDING_EVENTS_RULES": componentName = "CmpSendingEventRulesDB"; break;
				case "SENDING_EVENT_TYPE": componentName = "CmpSendingEventTypeDB"; break;
				case "ALARMS_LEVEL_DEF": componentName = "CmpAlarmsLevelDefDB"; break;
				case "DISTRIBUTION_LISTS_REMOTE": componentName = "CmpDistributionsListsDB"; break;
				case "CC_TRANSACTION_STATE_DEF": componentName = "CmpCCTransactionStateDefDB"; break;
				case "CREDIT_CARDS_TRANSACTIONS": componentName = "CmpCreditCardsTransactionsDB"; break;
				case "RESIDENTS": componentName = "CmpResidentsDB"; break;
				case "RESIDENTS_DATA": componentName = "CmpResidentsDataDB"; break;
				case "V_PDA_MSGS": componentName = "CmpPDAMsgDB"; break;
				case "PDAMSG_DEF": componentName = "CmpPDAMsgDefDB"; break;
				case "USER_EVENTS": componentName = "CmpUserEventsDB"; break;
				case "USER_EVENTS_DEF": componentName = "CmpUserEventsDefDB"; break;
				case "MAP_ALARMS_DEF": componentName = "CmpMapAlarmsDefDB"; break;
				case "MAP_ALARMS_HIS": componentName = "CmpMapAlarmHisDB"; break;
				case "MAP_GUARDS_HIS": componentName = "CmpMapGuardsHisDB"; break;
				case "MOBILE_ORDERS": componentName = "CmpMobileOrdersDB"; break;
				case "MOBILE_QUOTES": componentName = "CmpMobileQuotesDB"; break;

			}

			return "OPS.Components.Data." + componentName + ", " + strComponentsBDAssemblyName;
		}

		private string AddValidAndDeleted2Where(string where)
		{
			if (sstValidDeleted.Length == 2)
			{
				if (where.Length <= 1)
					where = stValidDeleted[0].ToString() + "=1 AND " + stValidDeleted[1].ToString() + "=0";
				else
					where += " AND " + stValidDeleted[0].ToString() + "=1 AND " +  stValidDeleted[1].ToString() + "=0";
			}
			return where;
		}
		#endregion // Private support members
	}
}
