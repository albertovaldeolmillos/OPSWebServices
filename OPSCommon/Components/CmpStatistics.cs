using System;
using System.Data;
using System.Collections;
using OPS.Components.Data;
using OPS.Components.Statistics.Helper;
using System.Web;


namespace OPS.Components.Statistics
{
	#region Defintions


	// Deprecated operations have been deleted
	public enum SerieValue 
	{
		ParkingsNumber	=  1,
		ParkingsTime	=  2,
		FinesNumber		=  3,
		//FinesType		=  4,
		RefillsNumber	=  5,
		RefillsValue	=  6,
		RefundsNumber	=  7,
		RefundsValue	=  8,
		Occupation		=  9,
		//Revenue			=  10,
		OperationsNumber = 11,
		//OperationsType	= 12,
		PlatesNumber	= 13,
		BatteryState	= 14,
		GPRSTraffic		= 15,
		TicketsNumber	= 16,
		RevenueOperations		= 17,
		RevenuePDM		= 18,
		RevenuePDMOperations	= 19,
		Caudal	=20,
		Temperatura	=21,
		pH	=22,
		Humedad	=23,
		CO2	=24,
		AlarmsNumber = 25
	}


	public enum SerieGrouping 
	{
		None,
		Unit,
		Group,
		Zones,
		Sectors,
		Routes,
		PayMethod,
		ArticleType,
		Supervisor,
		Street,
		Street_Stretchs,
		FineType,
		OperationType,
		Time
	}
		
	public enum GraphType 
	{
		Default,
		Columns,
		Bars,
		Lines,
		Pie,
		Columns3D,
		Pie3D,
		ColumnsAndLines
	}


	public enum TimeGrouping 
	{
		DayPrecisionDate,
		WeekNumber,
		Month,
		Year
	}
 

	#endregion

	#region Base Class Serie

	public abstract class Serie 
	{
		protected	string			_unitId;
		protected	SerieGrouping 	_groupField = SerieGrouping.None; // Field to group by in the query
		protected	string			_groupId; // Group of streets, units, other groups.
		protected   string			_zoneId;
		protected	string			_sectorId;
		protected	string			_routeId;
		protected	string			_payTypeId;
		protected	string			_artTypeId;
		protected	string			_supervisorId;
		protected	string			_streetId;
		protected   string			_streetStretchId;
		protected	string			_fineTypeId;
		protected	string			_operationTypeId;
		protected	DataSet			_dataSet;
		protected	DataSet			_dataSetAlt;
		protected	SqlTimeManager	_sqlTm;
		protected	string			_strXML="";
		protected	string			_strChart="";
		protected	bool			_ReducedHist;
		
		private		SerieValue		_serieValue; // Read only protected property

		
		#region Static constants & constructor

		protected	static	int		OPERATIONSDEF_PARKING_ID; // Id of parking operation definition
		protected	static	int		OPERATIONSDEF_REFILL_ID; // Id of refund operation definition
		protected	static	int		OPERATIONSDEF_REFUND_ID; // Id of refund operation definition

		protected	static	char	GROUPSCHILDS_UNIT_ID; // Id of unit used in Group_Childs

		protected	static	int		CURRENCY_DIVISOR; // Divisor para la moneda

		protected internal const string CHART_COLUMN_3D_TYPE="MSColumn3D";
		protected internal const string CHART_DOUGHNUT_TYPE="Doughnut3D";
		protected internal const string CHART_COLUMN_2D_SCROLL_TYPE="ScrollColumn2D";

		protected internal const int CHART_DOUGHNUT_MAX	= 10;
		protected internal const int CHART_COLUMN_3D_MAX	= 35;


		static Serie() 
		{
			System.Configuration.AppSettingsReader	appSettings = new System.Configuration.AppSettingsReader();

			OPERATIONSDEF_PARKING_ID = (int) appSettings.GetValue("OperationsDef.ParkingId", typeof(int));
			OPERATIONSDEF_REFILL_ID = (int) appSettings.GetValue("OperationsDef.RefillId", typeof(int));
			OPERATIONSDEF_REFUND_ID = (int) appSettings.GetValue("OperationsDef.RefundId", typeof(int));

			GROUPSCHILDS_UNIT_ID = (char) appSettings.GetValue("GroupsChilds.UnitId", typeof(char));

			try
			{
				CURRENCY_DIVISOR = (int) appSettings.GetValue("CurrencyDivisor", typeof(int));
			}
			catch (Exception ex)
			{
				CURRENCY_DIVISOR = 100;
			}
		}

		#endregion

       
		public void Bind() 
		{
			Bind(ref _dataSet);
		}

		public abstract void Bind(ref DataSet _dataSet);

		public static Serie SerieFactory(SerieValue serVal) 
		{
			Serie e;

			// Specific work
			switch (serVal)
			{
				case SerieValue.ParkingsTime:
				case SerieValue.ParkingsNumber:
					e = new ParkingsSerie();
					break;
				case SerieValue.OperationsNumber:
					e = new OperationsSerie();
					break;
				case SerieValue.RefundsValue:
				case SerieValue.RefundsNumber:
					e = new RefundsSerie();
					break;
				case SerieValue.RefillsNumber:
				case SerieValue.RefillsValue:
					e = new RefillsSerie();
					break;
				case SerieValue.RevenueOperations:
					e = new RevenueOperationsSerie();
					break;
				case SerieValue.RevenuePDM:
					e = new RevenuePDMSerie();
					break;
				case SerieValue.RevenuePDMOperations:
					e = new RevenuePDMOperationsSerie();
					break;
				case SerieValue.PlatesNumber:
					e = new PlatesSerie();
					break;
				case SerieValue.TicketsNumber:
					e = new TicketsSerie();
					break;
				case SerieValue.FinesNumber:
					e = new FinesSerie();
					break;
				case SerieValue.BatteryState:
					e = new BatteryStateSerie();
					break;
				case SerieValue.GPRSTraffic:
					e = new GPRSTrafficSerie();
					break;
				case SerieValue.Occupation:
					e = new OccupationSerie();
					break;
				case SerieValue.Caudal:
					e = new CaudalSerie();
					break;
				case SerieValue.Temperatura:
					e = new TemperaturaSerie();
					break;
				case SerieValue.pH:
					e = new pHSerie();
					break;
				case SerieValue.Humedad:
					e = new HumedadSerie();
					break;
				case SerieValue.CO2:
					e = new CO2Serie();
					break;
				case SerieValue.AlarmsNumber:
					e = new AlarmsSerie();
					break;
				default:
					e= null;
					break;
//					throw new Exception("Coding error: serie [" + serVal + "] is not implemented.");
			}

			// Default work
			if (e != null) 
			{
				e._serieValue = serVal;
				e._sqlTm = new SqlTimeManager();
				e.ReducedHistoricalData= true;
			}
			return e;
		}

		protected virtual string GetXML (DataSet ds)
		{
			if ((ds.Tables[0].Rows.Count>CHART_DOUGHNUT_MAX)&&(ds.Tables[0].Rows.Count<=CHART_COLUMN_3D_MAX))
			{
				return GetXMLColumn3DChart(ds);
			}
			else if (ds.Tables[0].Rows.Count>CHART_COLUMN_3D_MAX)
			{
				return GetXMLColumn2DScrollChart(ds);
			}
			else
			{
				return GetXMLDoughnutChart(ds);
			}
		}

		protected virtual string GetXMLColumn3DChart (DataSet ds)
		{
			_strChart=CHART_COLUMN_3D_TYPE;
			string strRes="";
			string strRes1="";
			string strRes2="";
			
			System.Random oRandom= new System.Random();
			int iPalette = oRandom.Next(1,5);
			strRes+="<?xml version='1.0' encoding='iso-8859-1'?><chart caption='' axisname='' labelDisplay='ROTATE' slantLabels='1' animation='1' showValues='0' formatNumberScale='0' use3DLighting='1' palette='"+iPalette+"' >";
			strRes1+="<categories>";
			strRes2+="<dataset SeriesName=''>";

			foreach(DataTable t in ds.Tables)
			{

				foreach(DataRow r in t.Rows)
				{
					foreach(DataColumn c in t.Columns)
					{
						switch (c.Ordinal)
						{
							case 0:
								strRes1+="<category label='"+UnQuoteString(r[c].ToString())+"' />";
								break;
							case 1:
								strRes2+="<set value='"+UnQuoteString(r[c].ToString().Replace(",","."))+"' />";
								break;
							default:
								break;
						}
							
					}
				}
			}
			strRes1+="</categories>";
			strRes2+="</dataset>";
			
			strRes+=strRes1;
			strRes+=strRes2;
			strRes+="<styles><definition><style name='CanvasAnim' type='animation' param='_yScale' start='0' duration='2' /></definition><application><apply toObject='Canvas' styles='CanvasAnim' /></application></styles></chart>";

			return strRes;
		}

		protected virtual string GetXMLColumn2DScrollChart (DataSet ds)
		{
			_strChart=CHART_COLUMN_2D_SCROLL_TYPE;
			string strRes="";
			string strRes1="";
			string strRes2="";
			
			System.Random oRandom= new System.Random();
			int iPalette = oRandom.Next(1,5);
			strRes+="<?xml version='1.0' encoding='iso-8859-1'?><chart caption='' axisname='' labelDisplay='ROTATE' slantLabels='1' animation='1' showValues='0' formatNumberScale='0' use3DLighting='1' numVisiblePlot='"+CHART_COLUMN_3D_MAX.ToString()+"' palette='"+iPalette+"' >";
			strRes1+="<categories>";
			strRes2+="<dataset SeriesName=''>";

			foreach(DataTable t in ds.Tables)
			{

				foreach(DataRow r in t.Rows)
				{
					foreach(DataColumn c in t.Columns)
					{
						switch (c.Ordinal)
						{
							case 0:
								strRes1+="<category label='"+UnQuoteString(r[c].ToString())+"' />";
								break;
							case 1:
								strRes2+="<set value='"+UnQuoteString(r[c].ToString().Replace(",","."))+"' />";
								break;
							default:
								break;
						}
							
					}
				}
			}
			strRes1+="</categories>";
			strRes2+="</dataset>";
			
			strRes+=strRes1;
			strRes+=strRes2;
			strRes+="<styles><definition><style name='CanvasAnim' type='animation' param='_yScale' start='0' duration='2' /></definition><application><apply toObject='Canvas' styles='CanvasAnim' /></application></styles></chart>";

			return strRes;
		}

	

		protected virtual string GetXMLDoughnutChart (DataSet ds)
		{
			_strChart=CHART_DOUGHNUT_TYPE;
			string strRes="";
			System.Random oRandom= new System.Random();
			int iPalette = oRandom.Next(1,5);

			strRes+="<?xml version='1.0' encoding='iso-8859-1'?><chart caption='' animation='1' showPercentInToolTip='1' formatNumberScale='0' use3DLighting='1' palette='"+iPalette+"' >";

			foreach(DataTable t in ds.Tables)
			{

				foreach(DataRow r in t.Rows)
				{
					strRes+="<set";
					foreach(DataColumn c in t.Columns)
					{
						switch (c.Ordinal)
						{
							case 0:
								strRes+=" label='"+UnQuoteString(r[c].ToString())+"'";
								break;
							case 1:
								strRes+=" value='"+UnQuoteString(r[c].ToString().Replace(",","."))+"'";
								break;
							default:
								break;
						}
							
					}
					strRes+=" />";
				}
			}
			strRes+="<styles><definition><style name='CanvasAnim' type='animation' param='_xScale' start='0' duration='2' /></definition><application><apply toObject='Canvas' styles='CanvasAnim' /></application></styles></chart>";


			return strRes;
		}

		protected string UnQuoteString (string strIn)
		{
			string strRes=strIn;
			strRes=strRes.Replace("á","a");
			strRes=strRes.Replace("é","e");
			strRes=strRes.Replace("í","i");
			strRes=strRes.Replace("ó","o");
			strRes=strRes.Replace("ú","u");
			strRes=strRes.Replace("Á","A");
			strRes=strRes.Replace("É","E");
			strRes=strRes.Replace("Í","I");
			strRes=strRes.Replace("Ó","O");
			strRes=strRes.Replace("Ú","U");

			strRes=strRes.Replace("à","a");
			strRes=strRes.Replace("è","e");
			strRes=strRes.Replace("ì","i");
			strRes=strRes.Replace("ò","o");
			strRes=strRes.Replace("ù","u");
			strRes=strRes.Replace("À","A");
			strRes=strRes.Replace("È","E");
			strRes=strRes.Replace("Ì","I");
			strRes=strRes.Replace("Ò","O");
			strRes=strRes.Replace("Ù","U");

			strRes=strRes.Replace("ä","a");
			strRes=strRes.Replace("ë","e");
			strRes=strRes.Replace("ï","i");
			strRes=strRes.Replace("ö","o");
			strRes=strRes.Replace("ü","u");
			strRes=strRes.Replace("Ä","A");
			strRes=strRes.Replace("Ë","E");
			strRes=strRes.Replace("Ï","I");
			strRes=strRes.Replace("Ö","O");
			strRes=strRes.Replace("Ü","U");

			strRes=strRes.Replace("ç","c");
			strRes=strRes.Replace("Ç","C");
			strRes=strRes.Replace("ñ","n");
			strRes=strRes.Replace("Ñ","N");
			strRes=strRes.Replace("\"","");
			strRes=strRes.Replace("'","");

			return strRes;
		}

		#region Private & protected

		protected SerieValue CurrentSerieValue 
		{
			get 
			{
				return _serieValue;
			}
		}

		#endregion

		#region Public Attributes

		public DataSet DataSet 
		{
			get 
			{
				return _dataSet;
			}
		}

		public string XML
		{
			get 
			{
				return _strXML;
			}
		}

		public string Chart
		{
			get 
			{
				return _strChart;
			}
		}


		public DataSet DataSetAlt 
		{
			get 
			{
				return _dataSetAlt;
			}
		}

		public string UnitId 
		{
			get 
			{
				return _unitId;
			}
			set 
			{
				_unitId = value;
			}
		}

		public bool ReducedHistoricalData
		{
			get 
			{
				return _ReducedHist;
			}
			set 
			{
				_ReducedHist = value;
			}
		}

		public string OperationHisTable
		{
			get 
			{
				if (ReducedHistoricalData)
					return "VW_OPERATIONS_HIS";
				else
					return "VW_OPERATIONS_HIS_FULL";
			}
		}

		public string GroupId 
		{
			get 
			{
				return _groupId;
			}
			set 
			{
				_groupId = value;
			}
		}

		public string ZoneId 
		{
			get 
			{
				return _zoneId;
			}
			set 
			{
				_zoneId = value;
			}
		}

		public string SectorId 
		{
			get 
			{
				return _sectorId;
			}
			set 
			{
				_sectorId = value;
			}
		}

		public string RouteId 
		{
			get 
			{
				return _routeId;
			}
			set 
			{
				_routeId = value;
			}
		}

		public string PayTypeId 
		{
			get 
			{
				return _payTypeId;
			}
			set 
			{
				_payTypeId = value;
			}
		}

		public string ArticleTypeId 
		{
			get 
			{
				return _artTypeId;
			}
			set 
			{
				_artTypeId = value;
			}
		}

		public string SupervisorId 
		{
			get 
			{
				return _supervisorId;
			}
			set 
			{
				_supervisorId = value;
			}
		}

		public string StreetId 
		{
			get 
			{
				return _streetId;
			}
			set 
			{
				_streetId = value;
			}
		}

		public string StreetStretchId 
		{
			get 
			{
				return _streetStretchId;
			}
			set 
			{
				_streetStretchId = value;
			}
		}

		public string FineTypeId 
		{
			get 
			{
				return _fineTypeId;
			}
			set 
			{
				_fineTypeId = value;
			}
		}

		public string OperationTypeId 
		{
			get 
			{
				return _operationTypeId;
			}
			set 
			{
				_operationTypeId = value;
			}
		}

		public SerieGrouping GroupField 
		{
			get 
			{
				return _groupField;
			}
			set 
			{
				_groupField = value;
			}
		}

		public SqlTimeManager TimeManager 
		{
			get 
			{
				return _sqlTm;
			}
		}

			public virtual GraphType DefaultGraphType 
		{
			get 
			{
				return GraphType.Columns; 
			}
		}

		public virtual bool HasAlternativeDataSet()		{ return false; }

		public virtual bool AllowsUnit()				{ return false; }
		public virtual bool AllowsGroup()				{ return false; }
		public virtual bool AllowsZone()				{ return false; }
		public virtual bool AllowsSector()				{ return false; }
		public virtual bool AllowsRoute()				{ return false; }
		public virtual bool AllowStreetStretch()		{ return false; }
		public virtual bool AllowsPaymentType()			{ return false; }
		public virtual bool AllowsArticleDefinition()	{ return false; }
		public virtual bool AllowsSupervisor()			{ return false; }
		public virtual bool AllowsStreet()				{ return false; }
		public virtual bool AllowsTime()				{ return false; }
		public virtual bool AllowsOperationType()		{ return false; }
		public virtual bool AllowsFineType()			{ return false; }

		#endregion

	}


	#endregion

	#region Series Implementation

	public class OperationsSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool	bJoinOperationsDef = false;
			bool	bJoinUnits	= false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.OperationsNumber)
			{
				switch(GroupField) 
				{
					case SerieGrouping.Routes:
					case SerieGrouping.Street_Stretchs:
					case SerieGrouping.Street:
						oBuilder.AddSelectFieldAggregate(OutputType.Sum,"v1.SS_PERCENT");
						break;
					default:
						oBuilder.AddSelectFieldAggregate(OutputType.Count,"*");
						break;
				}
			}
			else
				throw new Exception("Coding error (OperationsSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause(OperationHisTable);

			// WHERE (& necessary FROMs). 
			if (this._operationTypeId != null && GroupField != SerieGrouping.OperationType) 
			{
				oBuilder.AddWhereAndClause("HOPE_DOPE_ID_VIS = @"+OperationHisTable+".HOPE_DOPE_ID_VIS@");
				paramValues.Add(_operationTypeId);
			}
			/*else  // if _operationTypeId == null -> Default operations = Parkings + Refills + Refunds
			{
				oBuilder.AddFromJoinClause("INNER JOIN OPERATIONS_DEF ON (HOPE_DOPE_ID = DOPE_ID)");
				oBuilder.AddWhereAndClause("DOPE_STAT = 1");
				bJoinOperationsDef = true;
			}*/

			if (_payTypeId != null && GroupField != SerieGrouping.PayMethod) 
			{
				oBuilder.AddWhereAndClause("HOPE_DPAY_ID = @"+OperationHisTable+".HOPE_DPAY_ID@");
				paramValues.Add(_payTypeId);
			}

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HOPE_UNI_ID = @"+OperationHisTable+".HOPE_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_zoneId != null && GroupField != SerieGrouping.Zones) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v2 ON (HOPE_GRP_ID = v2.CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_GROUPS_ZONES_SECTORS.GRP_ID@");
				paramValues.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("HOPE_GRP_ID = @"+OperationHisTable+".HOPE_GRP_ID@");
				paramValues.Add(_sectorId);
			}


			if (_routeId != null && GroupField != SerieGrouping.Routes) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_ROUTES_UNITS.GRP_ID@");
				paramValues.Add(_routeId);
			}

			if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_ID = @STREETS_STRETCHS.SS_ID@");
				paramValues.Add(_streetStretchId);
			}


			if (_artTypeId != null && GroupField != SerieGrouping.ArticleType) 
			{
				oBuilder.AddWhereAndClause("HOPE_DART_ID = @"+OperationHisTable+".HOPE_DART_ID@");
				paramValues.Add(_artTypeId);
			}

			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET ON (HOPE_UNI_ID = USS_UNI_ID)");
				oBuilder.AddWhereAndClause("SS_STR_ID = @V_UNIT_STREET.SS_STR_ID@");
				paramValues.Add(_streetId);

			}
	
			// Time config
			this._sqlTm.TimeField = "HOPE_MOVDATE";
			oBuilder.AddWhereAndClause("HOPE_MOVDATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere(OperationHisTable, "HOPE_MOVDATE", paramValues));
			

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.ArticleType:
					//oBuilder.AddFromJoinClause("INNER JOIN ARTICLES ON (HOPE_ART_ID = ART_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN ARTICLES_DEF ON (HOPE_DART_ID = DART_ID)");
					oBuilder.AddGroupByClause("DART_DESCLONG");
					oBuilder.AddSelectFieldLeft("DART_DESCLONG", true);
					oBuilder.AddOrderByClause("DART_DESCLONG");

					break;
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HOPE_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;
				case SerieGrouping.Zones:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v1 ON (HOPE_GRP_ID = v1.CGRPG_CHILD)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Sectors:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (HOPE_GRP_ID = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true,  "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Street_Stretchs:
					oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.SS_EXT_ID");
					oBuilder.AddSelectFieldLeft("v1.SS_EXT_ID", true, "SS_EXT_ID");
					oBuilder.AddOrderByClause("v1.SS_EXT_ID");
					break;
				case SerieGrouping.PayMethod:
					oBuilder.AddFromJoinClause("INNER JOIN PAYTYPES_DEF ON (HOPE_DPAY_ID = DPAY_ID)");
					oBuilder.AddGroupByClause("DPAY_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DPAY_DESCSHORT", true);
					oBuilder.AddOrderByClause("DPAY_DESCSHORT");
					break;
				case SerieGrouping.Street:
					oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (v1.SS_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");
					break;
				case SerieGrouping.OperationType:
					if (!bJoinOperationsDef)
						oBuilder.AddFromJoinClause("INNER JOIN OPERATIONS_DEF ON (HOPE_DOPE_ID = DOPE_ID)");
					oBuilder.AddGroupByClause("DOPE_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DOPE_DESCSHORT", true);
					oBuilder.AddOrderByClause("DOPE_DESCSHORT");
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());


			_strXML=GetXML(_dataSet);
		}

		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsArticleDefinition()	{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsOperationType()		{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }


	}
	
	public class ParkingsSerie : Serie 
	{		
		
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits	= false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.ParkingsNumber)
			{

				switch(GroupField) 
				{
					case SerieGrouping.Routes:
					case SerieGrouping.Street_Stretchs:
					case SerieGrouping.Street:
						oBuilder.AddSelectFieldAggregate(OutputType.Sum,"v1.SS_PERCENT");
						break;
					default:
						oBuilder.AddSelectFieldAggregate(OutputType.Count,"*");
						break;
				}
			}


			else if (this.CurrentSerieValue == SerieValue.ParkingsTime) 
			{
				oBuilder.AddSelectFieldAggregate(OutputType.Average,"HOPE_REALDURATION"); 
				oBuilder.AddWhereAndClause("(HOPE_INIDATE IS NOT NULL AND HOPE_ENDDATE IS NOT NULL)");
				
				//oBuilder.AddSelectFieldAggregate(OutputType.Average, "(HOPE_end - HOPE_ini) * 24 * 60");
				//oBuilder.AddSelectFieldAggregate(OutputType.AverageLessStdDev, "(HOPE_end - HOPE_ini) * 24 * 60");
				//oBuilder.AddSelectFieldAggregate(OutputType.AveragePlusStdDev, "(HOPE_end - HOPE_ini) * 24 * 60");
			}				
			else
				throw new Exception("Coding error (ParkingsSerie): serie [" + CurrentSerieValue + "] is not implemented.");


			// FROM
			oBuilder.AddFromTableClause(OperationHisTable);

			// WHERE (& necessary FROMs)
			oBuilder.AddWhereAndClause("HOPE_DOPE_ID = @"+OperationHisTable+".HOPE_DOPE_ID@");
			paramValues.Add(Serie.OPERATIONSDEF_PARKING_ID);

			

			//         Beg  v            End v
			// ----------------------------------
			//   |Park Refill Refill |
			//
			// It could happend somebody parks his/her car and pays for some time X. After that while that
			// person wants to stay longer there and therefore he/she pays for more time(refill).
			// Let's suppose we start tracking the statistic from the moment X + 1: should we count this car parking?
			// We should!
			// The problem is we do not have a parking operation in the period; instead we have a refill operation
			// that belongs to that first parking operation.
			// Fortunately there is an easy solution: parking operations have an end_time filled out once the whole
			// operation finishes (this includes refill). Thus, what we have to count, in the period is:
			// 
			//	1. Obviously the number of parking operations that begin inside the period.
			//		+
			//  2. That parking operations that begin outside the period but end inside.
			//
			
			// TO DO: inclure aquest al where les clàusules, juntament amb la gestió del temps.
			
			// Time config
			this._sqlTm.TimeField = "HOPE_INIDATE";
			oBuilder.AddWhereAndClause("HOPE_INIDATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere(OperationHisTable, "HOPE_INIDATE", paramValues));

			if (_payTypeId != null && GroupField != SerieGrouping.PayMethod) 
			{
				oBuilder.AddWhereAndClause("HOPE_DPAY_ID = @"+OperationHisTable+".HOPE_DPAY_ID@");
				paramValues.Add(_payTypeId);
			}

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HOPE_UNI_ID = @"+OperationHisTable+".HOPE_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_zoneId != null && GroupField != SerieGrouping.Zones) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v2 ON (HOPE_GRP_ID = v2.CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_GROUPS_ZONES_SECTORS.GRP_ID@");
				paramValues.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("HOPE_GRP_ID = @"+OperationHisTable+".HOPE_GRP_ID@");
				paramValues.Add(_sectorId);
			}


			if (_routeId != null && GroupField != SerieGrouping.Routes) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_ROUTES_UNITS.GRP_ID@");
				paramValues.Add(_routeId);
			}

			if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_ID = @STREETS_STRETCHS.SS_ID@");
				paramValues.Add(_streetStretchId);
			}

			if (_artTypeId != null && GroupField != SerieGrouping.ArticleType) 
			{
				oBuilder.AddWhereAndClause("HOPE_DART_ID = @"+OperationHisTable+".HOPE_DART_ID@");
				paramValues.Add(_artTypeId);
			}
		
			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_STR_ID = @V_UNIT_STREET.SS_STR_ID@");
				paramValues.Add(_streetId);
			}

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.ArticleType:
					//oBuilder.AddFromJoinClause("INNER JOIN ARTICLES ON (HOPE_ART_ID = ART_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN ARTICLES_DEF ON (HOPE_DART_ID = DART_ID)");
					oBuilder.AddGroupByClause("DART_DESCLONG");
					oBuilder.AddSelectFieldLeft("DART_DESCLONG", true);
					oBuilder.AddOrderByClause("DART_DESCLONG");

					break;
				case SerieGrouping.Unit:
					if (!bJoinUnits) 
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HOPE_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;
				case SerieGrouping.Zones:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v1 ON (HOPE_GRP_ID = v1.CGRPG_CHILD)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Sectors:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (HOPE_GRP_ID = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true,  "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Street_Stretchs:
					oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.SS_EXT_ID");
					oBuilder.AddSelectFieldLeft("v1.SS_EXT_ID", true, "SS_EXT_ID");
					oBuilder.AddOrderByClause("v1.SS_EXT_ID");
					break;
				case SerieGrouping.PayMethod:
					oBuilder.AddFromJoinClause("INNER JOIN PAYTYPES_DEF ON (HOPE_DPAY_ID = DPAY_ID)");
					oBuilder.AddGroupByClause("DPAY_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DPAY_DESCSHORT", true);
					oBuilder.AddOrderByClause("DPAY_DESCSHORT");
					break;
				case SerieGrouping.Street:
					oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (v1.SS_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");

					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());
						

			if (this.CurrentSerieValue == SerieValue.ParkingsNumber)
			{
				_strXML=GetXML(_dataSet);
			}
			else if (this.CurrentSerieValue == SerieValue.ParkingsTime)
			{
				if (_dataSet.Tables[0].Rows.Count<=CHART_COLUMN_3D_MAX)
				{
					_strXML =  GetXMLColumn3DChart(_dataSet);
				}
				else
				{
					_strXML = GetXMLColumn2DScrollChart(_dataSet);
				}
			}

		}



		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsPaymentType()		{ return true; }
		public override bool AllowsArticleDefinition()	{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }

	}

	public class RefundsSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits = false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.RefundsNumber)
			{

				switch(GroupField) 
				{
					case SerieGrouping.Routes:
					case SerieGrouping.Street_Stretchs:
					case SerieGrouping.Street:
						oBuilder.AddSelectFieldAggregate(OutputType.Sum,"v1.SS_PERCENT");
						break;
					default:
						oBuilder.AddSelectFieldAggregate(OutputType.Count,"*");
						break;
				}
			}			
			else if (this.CurrentSerieValue == SerieValue.RefundsValue) 
			{
				oBuilder.AddSelectFieldAggregate(OutputType.Average,"HOPE_VALUE/" + CURRENCY_DIVISOR.ToString()); // Date diff in hours
				oBuilder.AddWhereAndClause("HOPE_VALUE IS NOT NULL");

				//oBuilder.AddSelectFieldAggregate(OutputType.Average, "(HOPE_end - HOPE_ini) * 24 * 60");
				//oBuilder.AddSelectFieldAggregate(OutputType.AverageLessStdDev, "(HOPE_end - HOPE_ini) * 24 * 60");
				//oBuilder.AddSelectFieldAggregate(OutputType.AveragePlusStdDev, "(HOPE_end - HOPE_ini) * 24 * 60");
			}				
			else
				throw new Exception("Coding error (RefundsSerie): serie [" + CurrentSerieValue + "] is not implemented.");


			// FROM
			oBuilder.AddFromTableClause(OperationHisTable);

			// WHERE (& necessary FROMs)
			oBuilder.AddWhereAndClause("HOPE_DOPE_ID = @"+OperationHisTable+".HOPE_DOPE_ID@");
			paramValues.Add(Serie.OPERATIONSDEF_REFUND_ID);
			
			if (_payTypeId != null && GroupField != SerieGrouping.PayMethod) 
			{
				oBuilder.AddWhereAndClause("HOPE_DPAY_ID = @"+OperationHisTable+".HOPE_DPAY_ID@");
				paramValues.Add(_payTypeId);
			}

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HOPE_UNI_ID = @"+OperationHisTable+".HOPE_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_zoneId != null && GroupField != SerieGrouping.Zones) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v2 ON (HOPE_GRP_ID = v2.CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_GROUPS_ZONES_SECTORS.GRP_ID@");
				paramValues.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("HOPE_GRP_ID = @"+OperationHisTable+".HOPE_GRP_ID@");
				paramValues.Add(_sectorId);
			}


			if (_routeId != null && GroupField != SerieGrouping.Routes) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_ROUTES_UNITS.GRP_ID@");
				paramValues.Add(_routeId);
			}

			if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_ID = @STREETS_STRETCHS.SS_ID@");
				paramValues.Add(_streetStretchId);
			}


			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_STR_ID = @V_UNIT_STREET.SS_STR_ID@");
				paramValues.Add(_streetId);

			}

			// Time config
			this._sqlTm.TimeField = "HOPE_MOVDATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere(OperationHisTable, "HOPE_MOVDATE", paramValues));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits) 
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HOPE_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;
				case SerieGrouping.Zones:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v1 ON (HOPE_GRP_ID = v1.CGRPG_CHILD)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Sectors:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (HOPE_GRP_ID = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true,  "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Street_Stretchs:
					oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.SS_EXT_ID");
					oBuilder.AddSelectFieldLeft("v1.SS_EXT_ID", true, "SS_EXT_ID");
					oBuilder.AddOrderByClause("v1.SS_EXT_ID");
					break;

				case SerieGrouping.PayMethod:
					oBuilder.AddFromJoinClause("INNER JOIN PAYTYPES_DEF ON (HOPE_DPAY_ID = DPAY_ID)");
					oBuilder.AddGroupByClause("DPAY_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DPAY_DESCSHORT", true);
					break;
				case SerieGrouping.Street:
					oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (v1.SS_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");

					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			if (this.CurrentSerieValue == SerieValue.RefundsNumber)
			{
				_strXML=GetXML(_dataSet);
			}
			else if (this.CurrentSerieValue == SerieValue.RefundsValue)
			{
				if (_dataSet.Tables[0].Rows.Count<=CHART_COLUMN_3D_MAX)
				{
					_strXML =  GetXMLColumn3DChart(_dataSet);
				}
				else
				{
					_strXML = GetXMLColumn2DScrollChart(_dataSet);
				}

			}
		}


		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsPaymentType()		{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }

	}
	
	public class RefillsSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits = false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.RefillsNumber)
			{

				switch(GroupField) 
				{
					case SerieGrouping.Routes:
					case SerieGrouping.Street_Stretchs:
					case SerieGrouping.Street:
						oBuilder.AddSelectFieldAggregate(OutputType.Sum,"v1.SS_PERCENT");
						break;
					default:
						oBuilder.AddSelectFieldAggregate(OutputType.Count,"*");
						break;
				}
			}			
			else if (this.CurrentSerieValue == SerieValue.RefillsValue) 
			{
				oBuilder.AddSelectFieldAggregate(OutputType.Average,"HOPE_VALUE/" + CURRENCY_DIVISOR.ToString());

				//oBuilder.AddSelectFieldAggregate(OutputType.Average, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AverageLessStdDev, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AveragePlusStdDev, "HOPE_VALUE");
			}				
			else
				throw new Exception("Coding error (RefillsSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause(OperationHisTable);

			// WHERE (& necessary FROMs)
			oBuilder.AddWhereAndClause("HOPE_DOPE_ID = @"+OperationHisTable+".HOPE_DOPE_ID@");
			paramValues.Add(Serie.OPERATIONSDEF_REFILL_ID);
			
			if (_payTypeId != null && GroupField != SerieGrouping.PayMethod) 
			{
				oBuilder.AddWhereAndClause("HOPE_DPAY_ID = @"+OperationHisTable+".HOPE_DPAY_ID@");
				paramValues.Add(_payTypeId);
			}

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HOPE_UNI_ID = @"+OperationHisTable+".HOPE_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_zoneId != null && GroupField != SerieGrouping.Zones) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v2 ON (HOPE_GRP_ID = v2.CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_GROUPS_ZONES_SECTORS.GRP_ID@");
				paramValues.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("HOPE_GRP_ID = @"+OperationHisTable+".HOPE_GRP_ID@");
				paramValues.Add(_sectorId);
			}


			if (_routeId != null && GroupField != SerieGrouping.Routes) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_ROUTES_UNITS.GRP_ID@");
				paramValues.Add(_routeId);
			}

			if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_ID = @STREETS_STRETCHS.SS_ID@");
				paramValues.Add(_streetStretchId);
			}


			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_STR_ID = @V_UNIT_STREET.SS_STR_ID@");
				paramValues.Add(_streetId);

			}

			// Time config
			this._sqlTm.TimeField = "HOPE_MOVDATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere(OperationHisTable, "HOPE_MOVDATE", paramValues));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits) 
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HOPE_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;
				case SerieGrouping.Zones:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v1 ON (HOPE_GRP_ID = v1.CGRPG_CHILD)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Sectors:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (HOPE_GRP_ID = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true,  "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Street_Stretchs:
					oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.SS_EXT_ID");
					oBuilder.AddSelectFieldLeft("v1.SS_EXT_ID", true, "SS_EXT_ID");
					oBuilder.AddOrderByClause("v1.SS_EXT_ID");
					break;

				case SerieGrouping.PayMethod:
					oBuilder.AddFromJoinClause("INNER JOIN PAYTYPES_DEF ON (HOPE_DPAY_ID = DPAY_ID)");
					oBuilder.AddGroupByClause("DPAY_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DPAY_DESCSHORT", true);
					oBuilder.AddOrderByClause("DPAY_DESCSHORT");
					break;
				case SerieGrouping.Street:
					oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (v1.SS_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			if (this.CurrentSerieValue == SerieValue.RefillsNumber)
			{
				_strXML=GetXML(_dataSet);
			}
			else if (this.CurrentSerieValue == SerieValue.RefillsValue)
			{
				if (_dataSet.Tables[0].Rows.Count<=CHART_COLUMN_3D_MAX)
				{
					_strXML =  GetXMLColumn3DChart(_dataSet);
				}
				else
				{
					_strXML = GetXMLColumn2DScrollChart(_dataSet);
				}
			}

		}

		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsPaymentType()		{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }

	}
	
	public class RevenueOperationsSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits = false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.RevenueOperations) 
			{
				oBuilder.AddSelectFieldAggregate(OutputType.Sum,"DOPE_SIGN*HOPE_VALUE/" + CURRENCY_DIVISOR.ToString());

				//oBuilder.AddSelectFieldAggregate(OutputType.Average, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AverageLessStdDev, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AveragePlusStdDev, "HOPE_VALUE");
			}				
			else
				throw new Exception("Coding error (RevenueSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause(OperationHisTable);
			oBuilder.AddFromJoinClause("INNER JOIN OPERATIONS_DEF ON (HOPE_DOPE_ID = DOPE_ID)");

			// WHERE (& necessary FROMs)
			
			if (_payTypeId != null && GroupField != SerieGrouping.PayMethod) 
			{
				oBuilder.AddWhereAndClause("HOPE_DPAY_ID = @"+OperationHisTable+".HOPE_DPAY_ID@");
				paramValues.Add(_payTypeId);
			}

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HOPE_UNI_ID = @"+OperationHisTable+".HOPE_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_zoneId != null && GroupField != SerieGrouping.Zones) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v2 ON (HOPE_GRP_ID = v2.CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_GROUPS_ZONES_SECTORS.GRP_ID@");
				paramValues.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("HOPE_GRP_ID = @"+OperationHisTable+".HOPE_GRP_ID@");
				paramValues.Add(_sectorId);
			}


			if (_routeId != null && GroupField != SerieGrouping.Routes) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_ROUTES_UNITS.GRP_ID@");
				paramValues.Add(_routeId);
			}

			if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_ID = @STREETS_STRETCHS.SS_ID@");
				paramValues.Add(_streetStretchId);
			}


			if (_artTypeId != null && GroupField != SerieGrouping.ArticleType) 
			{
				oBuilder.AddWhereAndClause("HOPE_DART_ID = @"+OperationHisTable+".HOPE_DART_ID@");
				paramValues.Add(_artTypeId);
			}

			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_STR_ID = @V_UNIT_STREET.SS_STR_ID@");
				paramValues.Add(_streetId);

			}

			// Time config
			this._sqlTm.TimeField = "HOPE_MOVDATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere(OperationHisTable, "HOPE_MOVDATE", paramValues));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.ArticleType:
					oBuilder.AddFromJoinClause("INNER JOIN ARTICLES_DEF ON (HOPE_DART_ID = DART_ID)");
					oBuilder.AddGroupByClause("DART_DESCLONG");
					oBuilder.AddSelectFieldLeft("DART_DESCLONG", true);
					oBuilder.AddOrderByClause("DART_DESCLONG");
					break;
				case SerieGrouping.Unit:
					if (!bJoinUnits) 
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HOPE_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;
				case SerieGrouping.Zones:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v1 ON (HOPE_GRP_ID = v1.CGRPG_CHILD)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Sectors:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (HOPE_GRP_ID = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true,  "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Street_Stretchs:
					oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.SS_EXT_ID");
					oBuilder.AddSelectFieldLeft("v1.SS_EXT_ID", true, "SS_EXT_ID");
					oBuilder.AddOrderByClause("v1.SS_EXT_ID");
					break;

				case SerieGrouping.PayMethod:
					oBuilder.AddFromJoinClause("INNER JOIN PAYTYPES_DEF ON (HOPE_DPAY_ID = DPAY_ID)");
					oBuilder.AddGroupByClause("DPAY_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DPAY_DESCSHORT", true);
					oBuilder.AddOrderByClause("DPAY_DESCSHORT");
					break;
				case SerieGrouping.Street:
					oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (v1.SS_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");

					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}



		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsPaymentType()		{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsArticleDefinition()	{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }


	}

	public class RevenuePDMSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits = false;
			bool		bJoinGroupsChilds = false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.RevenuePDM) 
			{
				oBuilder.AddSelectFieldAggregate(OutputType.Sum,"RUNI_PARCIAL");

				//oBuilder.AddSelectFieldAggregate(OutputType.Average, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AverageLessStdDev, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AveragePlusStdDev, "HOPE_VALUE");
			}				
			else
				throw new Exception("Coding error (RevenuePDMSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("UNITS_RESULTS");

			// WHERE (& necessary FROMs)

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("RUNI_UNI_ID = @UNITS_RESULTS.RUNI_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_groupId != null && GroupField != SerieGrouping.Group) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (RUNI_UNI_ID = CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("CGRPG_ID = @GROUPS_CHILDS_GIS.CGRPG_ID@");
				// We look for Groups whose childs are Units, these units have RUNI_UNI_ID as identifier
				oBuilder.AddWhereAndClause("CGRPG_TYPE = '" + Serie.GROUPSCHILDS_UNIT_ID + "'");				
				paramValues.Add(_groupId);
				bJoinGroupsChilds = true;
			}

			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (RUNI_UNI_ID = UNI_ID)");
				oBuilder.AddWhereAndClause("UNI_STR_ID = @UNITS.UNI_STR_ID@");
				paramValues.Add(_streetId);
				bJoinUnits = true;
			}

			// Time config
			this._sqlTm.TimeField = "RUNI_INIDATE";
			oBuilder.AddWhereAndClause("RUNI_INIDATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("UNITS_RESULTS", "RUNI_INIDATE", paramValues));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (RUNI_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					break;
				case SerieGrouping.Group:
					if (!bJoinGroupsChilds) 
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (RUNI_UNI_ID = CGRPG_CHILD)");
					// We look for Groups whose childs are Units, these units have RUNI_UNI_ID as identifier
					oBuilder.AddWhereAndClause("CGRPG_TYPE = '" + Serie.GROUPSCHILDS_UNIT_ID + "'");
					oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");			
					oBuilder.AddGroupByClause("GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					break;

				case SerieGrouping.Street:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (RUNI_UNI_ID = UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (UNI_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}


		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }


	}

	public class RevenuePDMOperationsSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			// Get PDM Data Set (main one)
			Serie	pdmSerie = Serie.SerieFactory(SerieValue.RevenuePDM);

			pdmSerie.UnitId = this.UnitId;
			pdmSerie.GroupId = this.GroupId;
			pdmSerie.PayTypeId = this.PayTypeId;
			pdmSerie.ArticleTypeId = this.ArticleTypeId;
			pdmSerie.SupervisorId = this.SupervisorId;
			pdmSerie.StreetId = this.StreetId;
			pdmSerie.FineTypeId = this.FineTypeId;
			pdmSerie.OperationTypeId = this.OperationTypeId;			
			pdmSerie.TimeManager.StartDate = this.TimeManager.StartDate;
			pdmSerie.TimeManager.EndDate = this.TimeManager.EndDate;
			pdmSerie.TimeManager.DayDefId = this.TimeManager.DayDefId;
			pdmSerie.TimeManager.TimeTableId = this.TimeManager.TimeTableId;
			
			pdmSerie.GroupField = this.GroupField;
			pdmSerie.TimeManager.TimeGroup = this.TimeManager.TimeGroup;
			
			pdmSerie.Bind(ref _dataSet);
			
			// Get Operations Data Set (alternative)
			Serie	opeSerie = Serie.SerieFactory(SerieValue.RevenueOperations);

			opeSerie.UnitId = this.UnitId;
			opeSerie.GroupId = this.GroupId;
			opeSerie.PayTypeId = this.PayTypeId;
			opeSerie.ArticleTypeId = this.ArticleTypeId;
			opeSerie.SupervisorId = this.SupervisorId;
			opeSerie.StreetId = this.StreetId;
			opeSerie.FineTypeId = this.FineTypeId;
			opeSerie.OperationTypeId = this.OperationTypeId;		
			opeSerie.TimeManager.StartDate = this.TimeManager.StartDate;
			opeSerie.TimeManager.EndDate = this.TimeManager.EndDate;
			opeSerie.TimeManager.DayDefId = this.TimeManager.DayDefId;
			opeSerie.TimeManager.TimeTableId = this.TimeManager.TimeTableId;
			
			opeSerie.GroupField = this.GroupField;
			opeSerie.TimeManager.TimeGroup = this.TimeManager.TimeGroup;

			opeSerie.Bind(ref _dataSetAlt);			

			
			// Programmatically do a FULL OUTER JOIN
			
			
			// Set PKs to columns to do the merge, and ensure table names are the same
			_dataSet.Tables[0].PrimaryKey = new DataColumn[] { _dataSet.Tables[0].Columns[0] };
			_dataSetAlt.Tables[0].PrimaryKey = new DataColumn[] { _dataSetAlt.Tables[0].Columns[0] };
			_dataSet.Tables[0].TableName = _dataSetAlt.Tables[0].TableName;
				
			// Build base set of data ("FULL" data table).
			DataSet		dsBase = _dataSet.Copy();
			dsBase.Merge(_dataSetAlt);

			// Erase values
			foreach(DataRow dr in dsBase.Tables[0].Rows) 
			{
				dr[1] = 0;
			}
			dsBase.AcceptChanges();

			// Merge
			_dataSet.Merge(dsBase, true);
			_dataSetAlt.Merge(dsBase, true);

			_dataSet.AcceptChanges();
			_dataSetAlt.AcceptChanges();
		}

		public override bool HasAlternativeDataSet()	{ return true; }

		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }


		public override GraphType DefaultGraphType
		{
			get
			{
				return GraphType.ColumnsAndLines;
			}
		}

	}

	public class PlatesSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits = false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.PlatesNumber)
				oBuilder.AddSelectFieldAggregate(OutputType.Count,"DISTINCT HOPE_VEHICLEID");
			else
				throw new Exception("Coding error (OperationsSerie): serie [" + CurrentSerieValue + "] is not implemented.");


			// FROM
			oBuilder.AddFromTableClause(OperationHisTable);

			// WHERE (& necessary FROMs)

			
			if (_payTypeId != null && GroupField != SerieGrouping.PayMethod) 
			{
				oBuilder.AddWhereAndClause("HOPE_DPAY_ID = @"+OperationHisTable+".HOPE_DPAY_ID@");
				paramValues.Add(_payTypeId);
			}

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HOPE_UNI_ID = @"+OperationHisTable+".HOPE_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_zoneId != null && GroupField != SerieGrouping.Zones) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v2 ON (HOPE_GRP_ID = v2.CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_GROUPS_ZONES_SECTORS.GRP_ID@");
				paramValues.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("HOPE_GRP_ID = @"+OperationHisTable+".HOPE_GRP_ID@");
				paramValues.Add(_sectorId);
			}


			if (_routeId != null && GroupField != SerieGrouping.Routes) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_ROUTES_UNITS.GRP_ID@");
				paramValues.Add(_routeId);
			}

			if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_ID = @STREETS_STRETCHS.SS_ID@");
				paramValues.Add(_streetStretchId);
			}
	

			if (_artTypeId != null && GroupField != SerieGrouping.ArticleType) 
			{
				oBuilder.AddWhereAndClause("HOPE_DART_ID = @"+OperationHisTable+".HOPE_DART_ID@");
				paramValues.Add(_artTypeId);
			}

			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v2 ON (HOPE_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_STR_ID = @V_UNIT_STREET.SS_STR_ID@");
				paramValues.Add(_streetId);
			}

			// Time config
			this._sqlTm.TimeField = "HOPE_INIDATE";
			oBuilder.AddWhereAndClause("HOPE_INIDATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere(OperationHisTable, "HOPE_INIDATE", paramValues));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.ArticleType:
					oBuilder.AddFromJoinClause("INNER JOIN ARTICLES_DEF ON (HOPE_DART_ID = DART_ID)");
					oBuilder.AddGroupByClause("DART_DESCLONG");
					oBuilder.AddSelectFieldLeft("DART_DESCLONG", true);
					oBuilder.AddOrderByClause("DART_DESCLONG");
					break;
				case SerieGrouping.Unit:
					if (!bJoinUnits) 
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HOPE_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;
				case SerieGrouping.Zones:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v1 ON (HOPE_GRP_ID = v1.CGRPG_CHILD)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Sectors:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (HOPE_GRP_ID = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true,  "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Street_Stretchs:
					oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.SS_EXT_ID");
					oBuilder.AddSelectFieldLeft("v1.SS_EXT_ID", true, "SS_EXT_ID");
					oBuilder.AddOrderByClause("v1.SS_EXT_ID");
					break;

				case SerieGrouping.PayMethod:
					oBuilder.AddFromJoinClause("INNER JOIN PAYTYPES_DEF ON (HOPE_DPAY_ID = DPAY_ID)");
					oBuilder.AddGroupByClause("DPAY_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DPAY_DESCSHORT", true);
					oBuilder.AddOrderByClause("DPAY_DESCSHORT");
					break;
				case SerieGrouping.Street:
					oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (v1.SS_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}



		public override GraphType DefaultGraphType
		{
			get
			{
				return GraphType.Columns;
			}
		}


		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsPaymentType()		{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsArticleDefinition()	{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }


	}
	
	public class TicketsSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			ArrayList	paramValues = new ArrayList();
			SqlHelper	oBuilder = new SqlHelper();

			// Example of generated select (group by units)
			//
			//
			//	SELECT		UNI_DESCSHORT, SUM(SUMA) AS SUMA
			//	FROM
			//	(
			//
			//	SELECT 		HOPE_UNI_ID as UNIT_ID, SUM(NVL(DOPE_TICKETS_NUM, 0)) AS SUMA
			//	FROM		OPERATIONS_HIS INNER JOIN OPERATIONS_DEF ON (HOPE_DOPE_ID = DOPE_ID)
			//	GROUP BY 	HOPE_UNI_ID
			//
			//	UNION
			//
			//	SELECT		UNI_ID AS UNIT, SUM(NVL(DFIN_TICKETS_NUM, 0)) AS SUMA
			//	FROM		FINES_HIS INNER JOIN FINES_DEF ON (HFIN_DFIN_ID = DFIN_ID)
			//	GROUP BY	HFIN_UNI_ID
			//
			//	)
			//	GROUP BY	UNI_DESCSHORT
			//
			// Note that subqueries do already inner joins to pick up descriptions


			// SELECT
			if (this.CurrentSerieValue == SerieValue.TicketsNumber)
				oBuilder.AddSelectFieldAggregate(OutputType.Count, "*");
			else
				throw new Exception("Coding error (TicketsSerie): serie [" + CurrentSerieValue + "] is not implemented.");


			if (GroupField == SerieGrouping.Unit)
				oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
			else if (GroupField == SerieGrouping.Street)
				oBuilder.AddSelectFieldLeft("STR_DESC", true);
			else if (GroupField == SerieGrouping.Time) 
			{
				oBuilder.AddSelectFieldLeft("DATA", true);
				oBuilder.AddOrderByClause("DATA");
			}

			// FROM
			string	subquery1 = SqlInnerSelect(	"DOPE_NUMTICKETS", 
												OperationHisTable,
												"INNER JOIN OPERATIONS_DEF ON (HOPE_DOPE_ID = DOPE_ID)",
												"HOPE_UNI_ID", 
												OperationHisTable+".HOPE_UNI_ID", 
												"HOPE_INIDATE",
												paramValues);

			string	subquery2 = SqlInnerSelect( "DFIN_NUMTICKETS", 
												"FINES_HIS", 
												"INNER JOIN FINES_DEF ON (HFIN_DFIN_ID = DFIN_ID)",
												"HFIN_UNI_ID", 
												"FINES_HIS.HFIN_UNI_ID", 
												"HFIN_DATE",
												paramValues);
			
			oBuilder.AddFromTableClause("(" + subquery1 + " UNION " + subquery2 + ")");			
			
			// WHERE (& necessary FROMs)

			// GROUP BY (& necessary SELECTs & FROMs)
			if (GroupField == SerieGrouping.Unit)
				oBuilder.AddGroupByClause("UNI_DESCSHORT");
			else if (GroupField == SerieGrouping.Street)
				oBuilder.AddGroupByClause("STR_DESC");		
			else if (GroupField == SerieGrouping.Time)
				oBuilder.AddGroupByClause("DATA");

			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}

		protected string SqlInnerSelect(string aggregatedField, 
										string baseTable,
										string joinDefTable,
										string unitField, 
										string qualifiedUnitField,
										string timeField,
										ArrayList paramValues)
		{
			bool		bJoinUnits = false;
			SqlHelper	oBuilder = new SqlHelper();

			// Example of generated select (group by unit)
			//
			//	SELECT		UNI_DESCSHORT, SUM(SUMA) AS SUMA
			//	FROM
			//	(
			//
			// ---> generate this part: <---
			//			SELECT 		HOPE_UNI_ID, SUM(NVL(DOPE_TICKETS_NUM, 0)) AS SUMA
			//			FROM		OPERATIONS_HIS INNER JOIN OPERATIONS_DEF ON (HOPE_DOPE_ID = DOPE_ID)
			//			GROUP BY 	HOPE_UNI_ID
			// -----------------------------
			//
			//	UNION
			//
			// ------> or this part: <------
			//			SELECT		UNI_DESCSHORT, SUM(NVL(DFIN_TICKETS_NUM, 0)) AS SUM
			//			FROM		FINES_HIS INNER JOIN FINES_DEF ON (HFIN_DFIN_ID = DFIN_ID)
			//			GROUP BY	HFIN_UNI_ID
			// -----------------------------
			//
			//	)
			//	GROUP BY	UNI_DESCSHORT
			//
			// Note that subqueries do already inner joins to pick up descriptions


			// SELECT
			oBuilder.AddSelectFieldAggregate(OutputType.Sum, aggregatedField);
			
			// FROM
			oBuilder.AddFromTableClause(baseTable);
			oBuilder.AddFromJoinClause(joinDefTable);

			// WHERE (& necessary FROMs)
			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause(unitField + " = @" + qualifiedUnitField + "@");
				paramValues.Add(_unitId);
			}

			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (" + unitField + " = UNI_ID)");
				oBuilder.AddWhereAndClause("UNI_STR_ID = @UNITS.UNI_STR_ID@");
				paramValues.Add(_streetId);
				bJoinUnits = true;
			}

			// Time config
			this._sqlTm.TimeField = timeField;
			oBuilder.AddWhereAndClause(timeField + " IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere(baseTable, timeField, paramValues));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (" + unitField + " = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					bJoinUnits = true;
					break;
				case SerieGrouping.Street:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (" + unitField + " = UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (UNI_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					bJoinUnits = true;
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					//oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			

			return oBuilder.SqlFullSentence;
		}

		public override GraphType DefaultGraphType
		{
			get
			{
				return GraphType.Columns;
			}
		}


		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }

	}

	public class BatteryStateSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits = false;
			bool		bJoinGroupsChilds = false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.BatteryState) 
			{
				oBuilder.AddSelectFieldAggregate(OutputType.Sum,"MUNI_VALUE1");

				//oBuilder.AddSelectFieldAggregate(OutputType.Average, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AverageLessStdDev, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AveragePlusStdDev, "HOPE_VALUE");
			}				
			else
				throw new Exception("Coding error (BatteryStateSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("UNITS_MEASURES");

			// WHERE (& necessary FROMs)

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("MUNI_UNI_ID = @UNITS_MEASURES.MUNI_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_groupId != null && GroupField != SerieGrouping.Group) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (MUNI_UNI_ID = CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("CGRPG_ID = @GROUPS_CHILDS_GIS.CGRPG_ID@");
				// We look for Groups whose childs are Units, these units have RUNI_UNI_ID as identifier
				oBuilder.AddWhereAndClause("CGRPG_TYPE = '" + Serie.GROUPSCHILDS_UNIT_ID + "'");				
				paramValues.Add(_groupId);
				bJoinGroupsChilds = true;
			}

			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (MUNI_UNI_ID = UNI_ID)");
				oBuilder.AddWhereAndClause("UNI_STR_ID = @UNITS.UNI_STR_ID@");
				paramValues.Add(_streetId);
				bJoinUnits = true;
			}

			// Time config
			this._sqlTm.TimeField = "MUNI_DATE";
			oBuilder.AddWhereAndClause("MUNI_DATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("UNITS_MEASURES", "MUNI_DATE", paramValues));	

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (MUNI_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					break;
				case SerieGrouping.Group:
					if (!bJoinGroupsChilds) 
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (MUNI_UNI_ID = CGRPG_CHILD)");
					// We look for Groups whose childs are Units, these units have RUNI_UNI_ID as identifier
					oBuilder.AddWhereAndClause("CGRPG_TYPE = '" + Serie.GROUPSCHILDS_UNIT_ID + "'");
					oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");			
					oBuilder.AddGroupByClause("GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					break;

				case SerieGrouping.Street:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (MUNI_UNI_ID = UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (UNI_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}


		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }

	}

	public class GPRSTrafficSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits = false;
			bool		bJoinGroupsChilds = false;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.GPRSTraffic) 
			{
				oBuilder.AddSelectFieldAggregate(OutputType.Sum,"GTRA_BYTES_OK_IN + GTRA_BYTES_NOK_IN + GTRA_BYTES_OK_OUT + GTRA_BYTES_NOK_OUT");

				//oBuilder.AddSelectFieldAggregate(OutputType.Average, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AverageLessStdDev, "HOPE_VALUE");
				//oBuilder.AddSelectFieldAggregate(OutputType.AveragePlusStdDev, "HOPE_VALUE");
			}				
			else
				throw new Exception("Coding error (GPRSTrafficSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("GPRS_TRAFFIC");

			// WHERE (& necessary FROMs)

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("GTRA_UNI_ID = @GPRS_TRAFFIC.GTRA_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_groupId != null && GroupField != SerieGrouping.Group) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (GTRA_UNI_ID = CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("CGRPG_ID = @GROUPS_CHILDS_GIS.CGRPG_ID@");
				// We look for Groups whose childs are Units, these units have RUNI_UNI_ID as identifier
				oBuilder.AddWhereAndClause("CGRPG_TYPE = '" + Serie.GROUPSCHILDS_UNIT_ID + "'");				
				paramValues.Add(_groupId);
				bJoinGroupsChilds = true;
			}

			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (GTRA_UNI_ID = UNI_ID)");
				oBuilder.AddWhereAndClause("UNI_STR_ID = @UNITS.UNI_STR_ID@");
				paramValues.Add(_streetId);
				bJoinUnits = true;
			}

			// Time config
			this._sqlTm.TimeField = "GTRA_INIDATE";
			oBuilder.AddWhereAndClause("GTRA_INIDATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("GPRS_TRAFFIC", "GTRA_INIDATE", paramValues));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (GTRA_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					break;
				case SerieGrouping.Group:
					if (!bJoinGroupsChilds) 
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (GTRA_UNI_ID = CGRPG_CHILD)");
					// We look for Groups whose childs are Units, these units have RUNI_UNI_ID as identifier
					oBuilder.AddWhereAndClause("CGRPG_TYPE = '" + Serie.GROUPSCHILDS_UNIT_ID + "'");
					oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");			
					oBuilder.AddGroupByClause("GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					break;

				case SerieGrouping.Street:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (GTRA_UNI_ID = UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (UNI_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}


		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }

	}


	public class CaudalSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool	bJoinUnits	= false;
			bool	bJoinGroups	= false;

			// SELECT
			if (this.CurrentSerieValue == SerieValue.Caudal)
				oBuilder.AddSelectFieldAggregate(OutputType.Average,"HMUNI_VALUE3");
			else
				throw new Exception("Coding error (OperationsSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("UNITS_MEASURES_HIS");

			// WHERE (& necessary FROMs). 
			oBuilder.AddWhereAndClause("HMUNI_VALUE3 IS NOT NULL");

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HMUNI_UNI_ID = @UNITS_MEASURES_HIS.HMUNI_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_groupId != null && GroupField != SerieGrouping.Group) 
			{

				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_ID = @GROUPS.GRP_ID@");
				paramValues.Add(_groupId);
				bJoinGroups=true;
			}
		
			// Time config
			this._sqlTm.TimeField = "HMUNI_DATE";
			oBuilder.AddWhereAndClause("HMUNI_DATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("UNITS_MEASURES_HIS", "HMUNI_DATE", paramValues));
			

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HMUNI_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					break;
				case SerieGrouping.Group:
					if (!bJoinGroups)
					{
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
					}
					oBuilder.AddGroupByClause("GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}

		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }

		public override GraphType DefaultGraphType
		{
			get
			{
				if (this.GroupField==SerieGrouping.Time)
					return GraphType.Lines;
				else
					return GraphType.Columns;
			}
		}
	}

	public class TemperaturaSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool	bJoinUnits	= false;
			bool	bJoinGroups	= false;

			// SELECT
			if (this.CurrentSerieValue == SerieValue.Temperatura)
				oBuilder.AddSelectFieldAggregate(OutputType.Average,"HMUNI_VALUE1");
			else
				throw new Exception("Coding error (OperationsSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("UNITS_MEASURES_HIS");

			// WHERE (& necessary FROMs). 
			oBuilder.AddWhereAndClause("HMUNI_VALUE1 IS NOT NULL");

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HMUNI_UNI_ID = @UNITS_MEASURES_HIS.HMUNI_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_groupId != null && GroupField != SerieGrouping.Group) 
			{

				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_ID = @GROUPS.GRP_ID@");
				paramValues.Add(_groupId);
				bJoinGroups=true;
			}
		
			// Time config
			this._sqlTm.TimeField = "HMUNI_DATE";
			oBuilder.AddWhereAndClause("HMUNI_DATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("UNITS_MEASURES_HIS", "HMUNI_DATE", paramValues));
			

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HMUNI_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					break;
				case SerieGrouping.Group:
					if (!bJoinGroups)
					{
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
					}
					oBuilder.AddGroupByClause("GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}

		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }

		public override GraphType DefaultGraphType
		{
			get
			{
				if (this.GroupField==SerieGrouping.Time)
					return GraphType.Lines;
				else
					return GraphType.Columns;
			}
		}
	}

	public class pHSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool	bJoinUnits	= false;
			bool	bJoinGroups	= false;

			// SELECT
			if (this.CurrentSerieValue == SerieValue.pH)
				oBuilder.AddSelectFieldAggregate(OutputType.Average,"HMUNI_VALUE10");
			else
				throw new Exception("Coding error (OperationsSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("UNITS_MEASURES_HIS");

			// WHERE (& necessary FROMs). 
			oBuilder.AddWhereAndClause("HMUNI_VALUE10 IS NOT NULL");

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HMUNI_UNI_ID = @UNITS_MEASURES_HIS.HMUNI_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_groupId != null && GroupField != SerieGrouping.Group) 
			{

				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_ID = @GROUPS.GRP_ID@");
				paramValues.Add(_groupId);
				bJoinGroups=true;
			}

		
			// Time config
			this._sqlTm.TimeField = "HMUNI_DATE";
			oBuilder.AddWhereAndClause("HMUNI_DATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("UNITS_MEASURES_HIS", "HMUNI_DATE", paramValues));
			

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HMUNI_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					break;
				case SerieGrouping.Group:
					if (!bJoinGroups)
					{
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
					}
					oBuilder.AddGroupByClause("GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}

		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }


		public override GraphType DefaultGraphType
		{
			get
			{
				if (this.GroupField==SerieGrouping.Time)
					return GraphType.Lines;
				else
					return GraphType.Columns;
			}
		}
	}

	public class HumedadSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool	bJoinUnits	= false;
			bool	bJoinGroups	= false;

			// SELECT
			if (this.CurrentSerieValue == SerieValue.Humedad)
				oBuilder.AddSelectFieldAggregate(OutputType.Average,"HMUNI_VALUE8");
			else
				throw new Exception("Coding error (OperationsSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("UNITS_MEASURES_HIS");

			// WHERE (& necessary FROMs). 
			oBuilder.AddWhereAndClause("HMUNI_VALUE8 IS NOT NULL");

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HMUNI_UNI_ID = @UNITS_MEASURES_HIS.HMUNI_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_groupId != null && GroupField != SerieGrouping.Group) 
			{

				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_ID = @GROUPS.GRP_ID@");
				paramValues.Add(_groupId);
				bJoinGroups=true;
			}

		
			// Time config
			this._sqlTm.TimeField = "HMUNI_DATE";
			oBuilder.AddWhereAndClause("HMUNI_DATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("UNITS_MEASURES_HIS", "HMUNI_DATE", paramValues));
			

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HMUNI_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					break;
				case SerieGrouping.Group:
					if (!bJoinGroups)
					{
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
					}
					oBuilder.AddGroupByClause("GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			this.
				// Run query

				_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}

		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }

		public override GraphType DefaultGraphType
		{
			get
			{
				if (this.GroupField==SerieGrouping.Time)
					return GraphType.Lines;
				else
					return GraphType.Columns;
			}
		}
	}

	public class CO2Serie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool	bJoinUnits	= false;
			bool	bJoinGroups	= false;

			// SELECT
			if (this.CurrentSerieValue == SerieValue.CO2)
				oBuilder.AddSelectFieldAggregate(OutputType.Average,"HMUNI_VALUE2");
			else
				throw new Exception("Coding error (OperationsSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("UNITS_MEASURES_HIS");

			// WHERE (& necessary FROMs). 
			oBuilder.AddWhereAndClause("HMUNI_VALUE2 IS NOT NULL");

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HMUNI_UNI_ID = @UNITS_MEASURES_HIS.HMUNI_UNI_ID@");
				paramValues.Add(_unitId);
			}

			if (_groupId != null && GroupField != SerieGrouping.Group) 
			{

				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_ID = @GROUPS.GRP_ID@");
				paramValues.Add(_groupId);
				bJoinGroups=true;
			}

		
			// Time config
			this._sqlTm.TimeField = "HMUNI_DATE";
			oBuilder.AddWhereAndClause("HMUNI_DATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("UNITS_MEASURES_HIS", "HMUNI_DATE", paramValues));
			

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					if (!bJoinUnits)
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HMUNI_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					break;
				case SerieGrouping.Group:
					if (!bJoinGroups)
					{
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS ON (CGRPG_CHILD = HMUNI_UNI_ID)");
						oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (CGRPG_ID = GRP_ID)");
					}
					oBuilder.AddGroupByClause("GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}

		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }

		public override GraphType DefaultGraphType
		{
			get
			{
				if (this.GroupField==SerieGrouping.Time)
					return GraphType.Lines;
				else
					return GraphType.Columns;
			}
		}
	}


	public class FinesSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();


			// SELECT
			if (this.CurrentSerieValue == SerieValue.FinesNumber)
				oBuilder.AddSelectFieldAggregate(OutputType.Count,"*");
			else
				throw new Exception("Coding error (FinesSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("FINES_HIS");

			// WHERE (& necessary FROMs). Operations = Parkings + Refills + Refunds

			if (_payTypeId != null && GroupField != SerieGrouping.PayMethod) 
			{
				oBuilder.AddWhereAndClause("HFIN_STATUSADMON = 1"); /* MULTA CANCELADA */
				oBuilder.AddFromJoinClause("INNER JOIN "+OperationHisTable+" ON (HOPE_FIN_ID = HFIN_ID)");
				oBuilder.AddWhereAndClause("HOPE_DPAY_ID = @"+OperationHisTable+".HOPE_DPAY_ID@");
				paramValues.Add(_payTypeId);

			}

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HFIN_UNI_ID = @FINES_HIS.HFIN_UNI_ID@");
				paramValues.Add(_unitId);
			}


			if (_zoneId != null && GroupField != SerieGrouping.Zones) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v2 ON (HFIN_GRP_ID_ZONE = v2.CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_GROUPS_ZONES_SECTORS.GRP_ID@");
				paramValues.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("HFIN_GRP_ID_ZONE = @FINES_HIS.HFIN_GRP_ID_ZONE@");
				paramValues.Add(_sectorId);
			}


			if (_routeId != null && GroupField != SerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("HFIN_GRP_ID_ROUTE = @FINES_HIS.HFIN_GRP_ID_ROUTE@");
				paramValues.Add(_routeId);
			}

			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddWhereAndClause("HFIN_STR_ID = @FINES_HIS.HFIN_STR_ID@");
				paramValues.Add(_streetId);
			}

			if (this._supervisorId != null && GroupField != SerieGrouping.Supervisor)
			{
				oBuilder.AddWhereAndClause("HFIN_USR_ID = @FINES_HIS.HFIN_USR_ID@");
				paramValues.Add(_supervisorId);
			}

			if (this._fineTypeId != null && GroupField != SerieGrouping.FineType) 
			{
				oBuilder.AddWhereAndClause("HFIN_DFIN_ID = @FINES_HIS.HFIN_DFIN_ID@");
				paramValues.Add(_fineTypeId);
			}

			// Time config
			this._sqlTm.TimeField = "HFIN_DATE";
			oBuilder.AddWhereAndClause("HFIN_DATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("FINES_HIS", "HFIN_DATE", paramValues));		

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HFIN_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;
				case SerieGrouping.Zones:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v1 ON (HFIN_GRP_ID_ZONE = v1.CGRPG_CHILD)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Sectors:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (HFIN_GRP_ID_ZONE = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_routes v1 ON (HFIN_GRP_ID_ROUTE = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true,  "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;				
				case SerieGrouping.PayMethod:
					oBuilder.AddWhereAndClause("HFIN_STATUSADMON = 1"); /* MULTA CANCELADA */
					oBuilder.AddFromJoinClause("INNER JOIN "+OperationHisTable+" ON (HOPE_FIN_ID = HFIN_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN PAYTYPES_DEF ON (HOPE_DPAY_ID = DPAY_ID)");
					oBuilder.AddGroupByClause("DPAY_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DPAY_DESCSHORT", true);
					break;
				case SerieGrouping.Street:

					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (HFIN_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");

					break;
				case SerieGrouping.Supervisor:
					oBuilder.AddFromJoinClause("INNER JOIN USERS ON (HFIN_USR_ID = USR_ID)");
					oBuilder.AddGroupByClause("USR_LOGIN");
					oBuilder.AddSelectFieldLeft("USR_LOGIN",true);
					oBuilder.AddOrderByClause("USR_LOGIN");
					break;
				case SerieGrouping.FineType:
					oBuilder.AddFromJoinClause("INNER JOIN FINES_DEF ON (HFIN_DFIN_ID = DFIN_ID)");
					oBuilder.AddGroupByClause("DFIN_DESCSHORT");
					oBuilder.AddSelectFieldLeft("DFIN_DESCSHORT", true);
					oBuilder.AddOrderByClause("DFIN_DESCSHORT");

					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}

		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsPaymentType()		{ return true; }
		public override bool AllowsSupervisor()			{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsFineType()			{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return false; }

	}



	public class AlarmsSerie : Serie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();


			// SELECT
			if (this.CurrentSerieValue == SerieValue.AlarmsNumber)
			{

				switch(GroupField) 
				{
					case SerieGrouping.Routes:
					case SerieGrouping.Street_Stretchs:
					case SerieGrouping.Street:
						oBuilder.AddSelectFieldAggregate(OutputType.Sum,"v1.SS_PERCENT");
						break;
					default:
						oBuilder.AddSelectFieldAggregate(OutputType.Count,"*");
						break;
				}
			}	
			else
				throw new Exception("Coding error (AlarmsSerie): serie [" + CurrentSerieValue + "] is not implemented.");

			// FROM
			oBuilder.AddFromTableClause("ALARMS_HIS");

			// WHERE (& necessary FROMs). Operations = Parkings + Refills + Refunds
			oBuilder.AddFromJoinClause("INNER JOIN ALARMS_DEF ON (HALA_DALA_ID = DALA_ID)");
			oBuilder.AddWhereAndClause("DALA_DALV_ID = 4");

			if (_unitId != null && GroupField != SerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HALA_UNI_ID = @ALARMS_HIS.HALA_UNI_ID@");
				paramValues.Add(_unitId);
			}


			if (_zoneId != null && GroupField != SerieGrouping.Zones) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS v3 ON (v3.CGRPG_CHILD = HALA_UNI_ID AND CGRPG_TYPE='U')");
				oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v2 ON (v3.CGRPG_ID = v2.CGRPG_CHILD)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_GROUPS_ZONES_SECTORS.GRP_ID@");
				paramValues.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS v2 ON (v2.CGRPG_CHILD = HALA_UNI_ID AND CGRPG_TYPE='U')");
				oBuilder.AddWhereAndClause("v2.CGRPG_ID = @GROUPS_CHILDS_GIS.CGRPG_ID@");
				paramValues.Add(_sectorId);
			}


			if (_routeId != null && GroupField != SerieGrouping.Routes) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v2 ON (HALA_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.GRP_ID = @VW_ROUTES_UNITS.GRP_ID@");
				paramValues.Add(_routeId);
			}

			if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v2 ON (HALA_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_ID = @STREETS_STRETCHS.SS_ID@");
				paramValues.Add(_streetStretchId);
			}



			if (this._streetId != null && GroupField != SerieGrouping.Street) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v2 ON (HALA_UNI_ID = v2.USS_UNI_ID)");
				oBuilder.AddWhereAndClause("v2.SS_STR_ID = @V_UNIT_STREET.SS_STR_ID@");
				paramValues.Add(_streetId);
			}

			// Time config
			this._sqlTm.TimeField = "HALA_INIDATE";
			oBuilder.AddWhereAndClause("HALA_INIDATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("ALARMS_HIS", "HALA_INIDATE", paramValues));		

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HALA_UNI_ID = UNI_ID)");
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;
				case SerieGrouping.Zones:
					oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS v0 ON (v0.CGRPG_CHILD = HALA_UNI_ID AND CGRPG_TYPE='U')");
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_zones_sectors v1 ON (v0.CGRPG_ID = v1.CGRPG_CHILD)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Sectors:
					oBuilder.AddFromJoinClause("INNER JOIN GROUPS_CHILDS_GIS v0 ON (v0.CGRPG_CHILD = HALA_UNI_ID AND CGRPG_TYPE='U')");
					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (v0.CGRPG_ID = v1.GRP_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true, "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v1 ON (HALA_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddSelectFieldLeft("v1.GRP_DESCSHORT", true,  "GRP_DESCSHORT");
					oBuilder.AddOrderByClause("v1.GRP_DESCSHORT");
					break;
				case SerieGrouping.Street_Stretchs:
					oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v1 ON (HALA_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddGroupByClause("v1.SS_EXT_ID");
					oBuilder.AddSelectFieldLeft("v1.SS_EXT_ID", true, "SS_EXT_ID");
					oBuilder.AddOrderByClause("v1.SS_EXT_ID");
					break;


				case SerieGrouping.Street:

					oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v1 ON (HALA_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (v1.SS_STR_ID = STR_ID)");
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");

					break;
				case SerieGrouping.Time:
					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());

			_strXML=GetXML(_dataSet);

		}

		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }

	}

	public class OccupationSerie : Serie 
	{		
		
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			ArrayList	paramValues = new ArrayList();
			bool		bJoinUnits	= false;
			SqlTimeManager tmDayTariffminutes=new SqlTimeManager();
			SqlHelper   oBuilderTariffMinutes= new SqlHelper();
			SqlHelper	oBuilderTariff = new SqlHelper();

			tmDayTariffminutes.StartDate = this._sqlTm.StartDate;
			tmDayTariffminutes.EndDate   =	this._sqlTm.EndDate;
			tmDayTariffminutes.DayDefId  =	this._sqlTm.DayDefId;
			tmDayTariffminutes.TimeTableId =	this._sqlTm.TimeTableId; 
			tmDayTariffminutes.TimeGroup =	this._sqlTm.TimeGroup;


			// SELECT
			if (this.CurrentSerieValue == SerieValue.Occupation) 
			{
				switch(GroupField) 
				{

					case SerieGrouping.Routes:
					case SerieGrouping.Street_Stretchs:
					case SerieGrouping.Street:

						oBuilder.AddSelectFieldLeft(
							"nvl((case "+
							"when ROUND((100 * SUM(HOPE_REALDURATION*SS_PERCENT) / TOTAL_MINUTES)+ PERC_RESIDENTS_SIN_TICKET+PERC_SIN_TICKET,2) > 100 then "+
							"100 "+
							"else "+
							"ROUND((100 * SUM(HOPE_REALDURATION*SS_PERCENT) / TOTAL_MINUTES)+ PERC_RESIDENTS_SIN_TICKET+PERC_SIN_TICKET,2) "+
							"end), "+
							"0)",false);

						oBuilder.AddWhereAndClause("(HOPE_INIDATE IS NOT NULL AND HOPE_ENDDATE IS NOT NULL AND HOPE_DOPE_ID IN (1,2))");
						break;
					default:
						oBuilder.AddSelectFieldLeft(
							"nvl((case "+
							"when ROUND((100 * SUM(HOPE_REALDURATION) / TOTAL_MINUTES)+ PERC_RESIDENTS_SIN_TICKET+PERC_SIN_TICKET,2) > 100 then "+
							"100 "+
							"else "+
							"ROUND((100 * SUM(HOPE_REALDURATION) / TOTAL_MINUTES)+ PERC_RESIDENTS_SIN_TICKET+PERC_SIN_TICKET,2) "+
							"end), "+
							"0)",false);

						oBuilder.AddWhereAndClause("(HOPE_INIDATE IS NOT NULL AND HOPE_ENDDATE IS NOT NULL AND HOPE_DOPE_ID IN (1,2))");
						break;	
				}
			}				
			else
				throw new Exception("Coding error (Occupation): serie [" + CurrentSerieValue + "] is not implemented.");


			// FROM
			oBuilder.AddFromTableClause(OperationHisTable);
			// Time config
			this._sqlTm.TimeField = "HOPE_INIDATE";


			switch(GroupField) 
			{

				case SerieGrouping.Unit:
					if (!bJoinUnits) 
						oBuilder.AddFromJoinClause("INNER JOIN UNITS ON (HOPE_UNI_ID = UNI_ID)");
					oBuilderTariffMinutes.AddFromTableClause("DAY_TARIFF_MINUTES");


					if (_zoneId != null && GroupField != SerieGrouping.Zones) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT SS_GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID =  v.cgrpg_child " +
							" and GRP_ID =  @GROUPS.GRP_ID@ " +
							" GROUP BY SS_GRP_ID) v10");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v10.v_grp_id");
						paramValues.Add(_zoneId);
					}


					if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
					{
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = @DAY_TARIFF_MINUTES.DTM_GRP_ID@");
						paramValues.Add(_sectorId);
					}

					if (_routeId != null && GroupField != SerieGrouping.Routes) 
					{


						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v, " +
							" V_GROUPS_CHILDS_ROUTES gc "+
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and  u.SS_ID= gc.CGRP_CHILD "+
							" and  gc.CGRP_ID =  @GROUPS_CHILDS.CGRP_ID@ " +
							" GROUP BY GRP_ID) v11");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v11.v_grp_id");
						paramValues.Add(_routeId);

					}


					if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_ID =  @STREETS_STRETCHS.SS_ID@ " +
							" GROUP BY GRP_ID) v12");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v12.v_grp_id");
						paramValues.Add(_streetStretchId);
					}

		
					if (this._streetId != null && GroupField != SerieGrouping.Street) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_STR_ID =  @STREETS_STRETCHS.SS_STR_ID@ " +
							" GROUP BY GRP_ID) v13");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v13.v_grp_id");
						paramValues.Add(_streetId);
					}

					oBuilderTariffMinutes.AddSelectFieldLeft("DTM_GRP_ID",false);
					oBuilderTariffMinutes.AddSelectFieldRight("SUM(DTM_MINUTES) DTM_MINUTES",false);
					tmDayTariffminutes.TimeField= "DTM_DAY";
					oBuilderTariffMinutes.AddWhereAndClause(tmDayTariffminutes.SqlWhereWithoutTrunc("DAY_TARIFF_MINUTES", "DTM_DAY", paramValues));
					oBuilderTariffMinutes.AddGroupByClause("DTM_GRP_ID");

					oBuilderTariff.AddFromTableClause("(select "+oBuilderTariffMinutes.SqlSelect+" "+
						"from "+oBuilderTariffMinutes.SqlFrom+" "+
						"where "+oBuilderTariffMinutes.SqlWhere+" "+
						"group by "+oBuilderTariffMinutes.SqlGroupBy +
						")");
					oBuilderTariff.AddFromTableClause("V_UNIT_PARK_SPACES");
					oBuilderTariff.AddFromTableClause("V_UNITS_STREETS_STRETCHS");
					oBuilderTariff.AddWhereAndClause("UPS_UNI_ID = uss_uni_id");
					oBuilderTariff.AddWhereAndClause("SS_GRP_ID = DTM_GRP_ID");
					oBuilderTariff.AddSelectFieldLeft("UPS_UNI_ID",false);
					oBuilderTariff.AddSelectFieldRight("PARK_SPACES * DTM_MINUTES TOTAL_MINUTES",false);
					oBuilderTariff.AddSelectFieldRight("PERC_RESIDENTS_SIN_TICKET",false);
					oBuilderTariff.AddSelectFieldRight("PERC_SIN_TICKET",false);
					oBuilderTariff.AddGroupByClause("UPS_UNI_ID");
					oBuilderTariff.AddGroupByClause("PARK_SPACES");
					oBuilderTariff.AddGroupByClause("DTM_MINUTES");
					oBuilderTariff.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilderTariff.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddFromJoinClause(" INNER JOIN (select "+oBuilderTariff.SqlSelect+" "+
						"from "+oBuilderTariff.SqlFrom+" "+
						"where "+oBuilderTariff.SqlWhere+" "+
						"group by "+oBuilderTariff.SqlGroupBy +
						") ON (UPS_UNI_ID = HOPE_UNI_ID)");

					break;
				case SerieGrouping.Zones:

					oBuilder.AddFromJoinClause("INNER JOIN  vw_groups_zones_sectors v1 ON (HOPE_GRP_ID = v1.CGRPG_CHILD)");
					oBuilderTariffMinutes.AddFromTableClause("DAY_TARIFF_MINUTES");
					oBuilderTariffMinutes.AddFromTableClause("V_ZONE_PARK_SPACES");
					oBuilderTariffMinutes.AddFromTableClause("vw_groups_zones_sectors");


					if (_unitId != null && GroupField != SerieGrouping.Unit) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
																" FROM V_UNITS_STREETS_STRETCHS u, " +
																" vw_groups_zones_sectors  v " +
																" WHERE u.SS_GRP_ID = v.cgrpg_child " +
																" and USS_UNI_ID =  @UNITS.UNI_ID@ " +
																" GROUP BY GRP_ID) v10");
						oBuilderTariffMinutes.AddWhereAndClause("ZPS_GRP_ID = v10.v_grp_id");
						paramValues.Add(_unitId);
					}

		
					if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID = v.cgrpg_child " +
							" and SS_GRP_ID =  @GROUPS.GRP_ID@ " +
							" GROUP BY GRP_ID) v11");
						oBuilderTariffMinutes.AddWhereAndClause("ZPS_GRP_ID = v11.v_grp_id");
						paramValues.Add(_sectorId);
					}


					if (_routeId != null && GroupField != SerieGrouping.Routes) 
					{


						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v, " +
						    " V_GROUPS_CHILDS_ROUTES gc "+
							" WHERE u.SS_GRP_ID = v.cgrpg_child " +
						    " and  u.SS_ID= gc.CGRP_CHILD "+
							" and  gc.CGRP_ID =  @GROUPS_CHILDS.CGRP_ID@ " +
							" GROUP BY GRP_ID) v12");
						oBuilderTariffMinutes.AddWhereAndClause("ZPS_GRP_ID = v12.v_grp_id");
						paramValues.Add(_routeId);

					}

					if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID = v.cgrpg_child " +
							" and u.SS_ID =  @STREETS_STRETCHS.SS_ID@ " +
							" GROUP BY GRP_ID) v13");
						oBuilderTariffMinutes.AddWhereAndClause("ZPS_GRP_ID = v13.v_grp_id");
						paramValues.Add(_streetStretchId);
					}

		
					if (this._streetId != null && GroupField != SerieGrouping.Street) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID = v.cgrpg_child " +
							" and u.SS_STR_ID =  @STREETS_STRETCHS.SS_STR_ID@ " +
							" GROUP BY GRP_ID) v14");
						oBuilderTariffMinutes.AddWhereAndClause("ZPS_GRP_ID = v14.v_grp_id");
						paramValues.Add(_streetId);
					}

					oBuilderTariffMinutes.AddSelectFieldLeft("DTM_GRP_ID",false);
					oBuilderTariffMinutes.AddSelectFieldRight("SUM(DTM_MINUTES * PARK_SPACES) TOTAL_MINUTES",false);
					oBuilderTariffMinutes.AddSelectFieldRight("PERC_RESIDENTS_SIN_TICKET",false);
					oBuilderTariffMinutes.AddSelectFieldRight("PERC_SIN_TICKET",false);
					tmDayTariffminutes.TimeField= "DTM_DAY";
					oBuilderTariffMinutes.AddWhereAndClause(" GRP_ID=ZPS_GRP_ID ");
					oBuilderTariffMinutes.AddWhereAndClause(" CGRPG_CHILD = DTM_GRP_ID ");
					oBuilderTariffMinutes.AddWhereAndClause(tmDayTariffminutes.SqlWhereWithoutTrunc("DAY_TARIFF_MINUTES", "DTM_DAY", paramValues));
					oBuilderTariffMinutes.AddGroupByClause("DTM_GRP_ID");
					oBuilderTariffMinutes.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilderTariffMinutes.AddGroupByClause("PERC_SIN_TICKET");


					oBuilder.AddFromJoinClause(" INNER JOIN (select "+oBuilderTariffMinutes.SqlSelect+" "+
						"from "+oBuilderTariffMinutes.SqlFrom+" "+
						"where "+oBuilderTariffMinutes.SqlWhere+" "+
						"group by "+oBuilderTariffMinutes.SqlGroupBy +
						") ON (DTM_GRP_ID = HOPE_GRP_ID)");
					break;
				case SerieGrouping.Sectors:

					oBuilder.AddFromJoinClause("INNER JOIN vw_groups_sectors v1 ON (HOPE_GRP_ID = v1.GRP_ID)");
					oBuilderTariffMinutes.AddFromTableClause("DAY_TARIFF_MINUTES");
					oBuilderTariffMinutes.AddFromTableClause("V_SECTOR_PARK_SPACES");


					if (_unitId != null && GroupField != SerieGrouping.Unit) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and USS_UNI_ID =  @UNITS.UNI_ID@ " +
							" GROUP BY GRP_ID) v10");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v10.v_grp_id");
						paramValues.Add(_unitId);
					}

		
					if (_zoneId != null && GroupField != SerieGrouping.Zones) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT SS_GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID =  v.cgrpg_child " +
							" and GRP_ID =  @GROUPS.GRP_ID@ " +
							" GROUP BY SS_GRP_ID) v11");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v11.v_grp_id");
						paramValues.Add(_zoneId);
					}


					if (_routeId != null && GroupField != SerieGrouping.Routes) 
					{


						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v, " +
							" V_GROUPS_CHILDS_ROUTES gc "+
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and  u.SS_ID= gc.CGRP_CHILD "+
							" and  gc.CGRP_ID =  @GROUPS_CHILDS.CGRP_ID@ " +
							" GROUP BY GRP_ID) v12");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v12.v_grp_id");
						paramValues.Add(_routeId);

					}

					if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_ID =  @STREETS_STRETCHS.SS_ID@ " +
							" GROUP BY GRP_ID) v13");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v13.v_grp_id");
						paramValues.Add(_streetStretchId);
					}

		
					if (this._streetId != null && GroupField != SerieGrouping.Street) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_STR_ID =  @STREETS_STRETCHS.SS_STR_ID@ " +
							" GROUP BY GRP_ID) v14");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v14.v_grp_id");
						paramValues.Add(_streetId);
					}

					oBuilderTariffMinutes.AddSelectFieldLeft("DTM_GRP_ID",false);
					oBuilderTariffMinutes.AddSelectFieldRight("SUM(DTM_MINUTES * PARK_SPACES) TOTAL_MINUTES",false);
					oBuilderTariffMinutes.AddSelectFieldRight("PERC_RESIDENTS_SIN_TICKET",false);
					oBuilderTariffMinutes.AddSelectFieldRight("PERC_SIN_TICKET",false);
					tmDayTariffminutes.TimeField= "DTM_DAY";
					oBuilderTariffMinutes.AddWhereAndClause(" SEPS_GRP_ID = DTM_GRP_ID ");
					oBuilderTariffMinutes.AddWhereAndClause(tmDayTariffminutes.SqlWhereWithoutTrunc("DAY_TARIFF_MINUTES", "DTM_DAY", paramValues));
					oBuilderTariffMinutes.AddGroupByClause("DTM_GRP_ID");
					oBuilderTariffMinutes.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilderTariffMinutes.AddGroupByClause("PERC_SIN_TICKET");


					oBuilder.AddFromJoinClause(" INNER JOIN (select "+oBuilderTariffMinutes.SqlSelect+" "+
						"from "+oBuilderTariffMinutes.SqlFrom+" "+
						"where "+oBuilderTariffMinutes.SqlWhere+" "+
						"group by "+oBuilderTariffMinutes.SqlGroupBy +
						") ON (DTM_GRP_ID = HOPE_GRP_ID)");
					break;

				case SerieGrouping.Routes:
					oBuilder.AddFromJoinClause("INNER JOIN vw_routes_units v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					
					oBuilderTariffMinutes.AddFromTableClause("DAY_TARIFF_MINUTES");


					if (_unitId != null && GroupField != SerieGrouping.Unit) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and USS_UNI_ID =  @UNITS.UNI_ID@ " +
							" GROUP BY GRP_ID) v10");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v10.v_grp_id");
						paramValues.Add(_unitId);
					}

					if (_zoneId != null && GroupField != SerieGrouping.Zones) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT SS_GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID =  v.cgrpg_child " +
							" and GRP_ID =  @GROUPS.GRP_ID@ " +
							" GROUP BY SS_GRP_ID) v11");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v11.v_grp_id");
						paramValues.Add(_zoneId);
					}


					if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
					{
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = @DAY_TARIFF_MINUTES.DTM_GRP_ID@");
						paramValues.Add(_sectorId);
					}

					if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_ID =  @STREETS_STRETCHS.SS_ID@ " +
							" GROUP BY GRP_ID) v12");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v12.v_grp_id");
						paramValues.Add(_streetStretchId);
					}

		
					if (this._streetId != null && GroupField != SerieGrouping.Street) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_STR_ID =  @STREETS_STRETCHS.SS_STR_ID@ " +
							" GROUP BY GRP_ID) v13");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v13.v_grp_id");
						paramValues.Add(_streetId);
					}


					oBuilderTariffMinutes.AddSelectFieldLeft("DTM_GRP_ID",false);
					oBuilderTariffMinutes.AddSelectFieldRight("SUM(DTM_MINUTES) DTM_MINUTES",false);
					tmDayTariffminutes.TimeField= "DTM_DAY";
					oBuilderTariffMinutes.AddWhereAndClause(tmDayTariffminutes.SqlWhereWithoutTrunc("DAY_TARIFF_MINUTES", "DTM_DAY", paramValues));
					oBuilderTariffMinutes.AddGroupByClause("DTM_GRP_ID");


					oBuilderTariff.AddFromTableClause("(select "+oBuilderTariffMinutes.SqlSelect+" "+
						"from "+oBuilderTariffMinutes.SqlFrom+" "+
						"where "+oBuilderTariffMinutes.SqlWhere+" "+
						"group by "+oBuilderTariffMinutes.SqlGroupBy +
						")");
					oBuilderTariff.AddFromTableClause("V_ROUTE_PARK_SPACES");
					oBuilderTariff.AddFromTableClause("V_GROUPS_CHILDS_ROUTES");
					oBuilderTariff.AddFromTableClause("STREETS_STRETCHS");
					oBuilderTariff.AddWhereAndClause("RPS_GRP_ID = CGRP_ID");
					oBuilderTariff.AddWhereAndClause("CGRP_CHILD = SS_ID");
					oBuilderTariff.AddWhereAndClause("SS_GRP_ID = DTM_GRP_ID");
					oBuilderTariff.AddSelectFieldLeft("RPS_GRP_ID",false);
					oBuilderTariff.AddSelectFieldRight("PARK_SPACES * DTM_MINUTES TOTAL_MINUTES",false);
					oBuilderTariff.AddSelectFieldRight("PERC_RESIDENTS_SIN_TICKET",false);
					oBuilderTariff.AddSelectFieldRight("PERC_SIN_TICKET",false);
					oBuilderTariff.AddGroupByClause("RPS_GRP_ID");
					oBuilderTariff.AddGroupByClause("PARK_SPACES");
					oBuilderTariff.AddGroupByClause("DTM_MINUTES");
					oBuilderTariff.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilderTariff.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddFromJoinClause(" INNER JOIN (select "+oBuilderTariff.SqlSelect+" "+
						"from "+oBuilderTariff.SqlFrom+" "+
						"where "+oBuilderTariff.SqlWhere+" "+
						"group by "+oBuilderTariff.SqlGroupBy +
						") ON (RPS_GRP_ID = v1.GRP_ID)");
					break;

				case SerieGrouping.Street_Stretchs:

					oBuilder.AddFromJoinClause("INNER JOIN v_units_streets_stretchs v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					
					oBuilderTariffMinutes.AddFromTableClause("DAY_TARIFF_MINUTES");


					if (_unitId != null && GroupField != SerieGrouping.Unit) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and USS_UNI_ID =  @UNITS.UNI_ID@ " +
							" GROUP BY GRP_ID) v10");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v10.v_grp_id");
						paramValues.Add(_unitId);
					}

					if (_zoneId != null && GroupField != SerieGrouping.Zones) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT SS_GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID =  v.cgrpg_child " +
							" and GRP_ID =  @GROUPS.GRP_ID@ " +
							" GROUP BY SS_GRP_ID) v11");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v11.v_grp_id");
						paramValues.Add(_zoneId);
					}


					if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
					{
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = @DAY_TARIFF_MINUTES.DTM_GRP_ID@");
						paramValues.Add(_sectorId);
					}

					if (_routeId != null && GroupField != SerieGrouping.Routes) 
					{


						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v, " +
							" V_GROUPS_CHILDS_ROUTES gc "+
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and  u.SS_ID= gc.CGRP_CHILD "+
							" and  gc.CGRP_ID =  @GROUPS_CHILDS.CGRP_ID@ " +
							" GROUP BY GRP_ID) v12");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v12.v_grp_id");
						paramValues.Add(_routeId);

					}

		
					if (this._streetId != null && GroupField != SerieGrouping.Street) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_STR_ID =  @STREETS_STRETCHS.SS_STR_ID@ " +
							" GROUP BY GRP_ID) v13");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v13.v_grp_id");
						paramValues.Add(_streetId);
					}

					oBuilderTariffMinutes.AddSelectFieldLeft("DTM_GRP_ID",false);
					oBuilderTariffMinutes.AddSelectFieldRight("SUM(DTM_MINUTES) DTM_MINUTES",false);
					tmDayTariffminutes.TimeField= "DTM_DAY";
					oBuilderTariffMinutes.AddWhereAndClause(tmDayTariffminutes.SqlWhereWithoutTrunc("DAY_TARIFF_MINUTES", "DTM_DAY", paramValues));
					oBuilderTariffMinutes.AddGroupByClause("DTM_GRP_ID");


					oBuilderTariff.AddFromTableClause("(select "+oBuilderTariffMinutes.SqlSelect+" "+
						"from "+oBuilderTariffMinutes.SqlFrom+" "+
						"where "+oBuilderTariffMinutes.SqlWhere+" "+
						"group by "+oBuilderTariffMinutes.SqlGroupBy +
						")");
					oBuilderTariff.AddFromTableClause("V_STREET_STRETCH_PARK_SPACES");
					oBuilderTariff.AddFromTableClause("STREETS_STRETCHS");
					oBuilderTariff.AddWhereAndClause("SSPS_SS_ID = SS_ID");
					oBuilderTariff.AddWhereAndClause("SS_GRP_ID = DTM_GRP_ID");
					oBuilderTariff.AddSelectFieldLeft("SSPS_SS_ID",false);
					oBuilderTariff.AddSelectFieldRight("PARK_SPACES * DTM_MINUTES TOTAL_MINUTES",false);
					oBuilderTariff.AddSelectFieldRight("PERC_RESIDENTS_SIN_TICKET",false);
					oBuilderTariff.AddSelectFieldRight("PERC_SIN_TICKET",false);
					oBuilderTariff.AddGroupByClause("SSPS_SS_ID");
					oBuilderTariff.AddGroupByClause("PARK_SPACES");
					oBuilderTariff.AddGroupByClause("DTM_MINUTES");
					oBuilderTariff.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilderTariff.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddFromJoinClause(" INNER JOIN (select "+oBuilderTariff.SqlSelect+" "+
						"from "+oBuilderTariff.SqlFrom+" "+
						"where "+oBuilderTariff.SqlWhere+" "+
						"group by "+oBuilderTariff.SqlGroupBy +
						") ON (SSPS_SS_ID = v1.SS_ID)");
					break;
				
				
				case SerieGrouping.Street:
					oBuilder.AddFromJoinClause("INNER JOIN V_UNIT_STREET v1 ON (HOPE_UNI_ID = v1.USS_UNI_ID)");
					oBuilder.AddFromJoinClause("INNER JOIN STREETS ON (v1.SS_STR_ID = STR_ID)");

					
					oBuilderTariffMinutes.AddFromTableClause("DAY_TARIFF_MINUTES");


					if (_unitId != null && GroupField != SerieGrouping.Unit) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and USS_UNI_ID =  @UNITS.UNI_ID@ " +
							" GROUP BY GRP_ID) v10");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v10.v_grp_id");
						paramValues.Add(_unitId);
					}

					if (_zoneId != null && GroupField != SerieGrouping.Zones) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT SS_GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID =  v.cgrpg_child " +
							" and GRP_ID =  @GROUPS.GRP_ID@ " +
							" GROUP BY SS_GRP_ID) v11");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v11.v_grp_id");
						paramValues.Add(_zoneId);
					}


					if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
					{
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = @DAY_TARIFF_MINUTES.DTM_GRP_ID@");
						paramValues.Add(_sectorId);
					}

					if (_routeId != null && GroupField != SerieGrouping.Routes) 
					{


						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v, " +
							" V_GROUPS_CHILDS_ROUTES gc "+
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and  u.SS_ID= gc.CGRP_CHILD "+
							" and  gc.CGRP_ID =  @GROUPS_CHILDS.CGRP_ID@ " +
							" GROUP BY GRP_ID) v12");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v12.v_grp_id");
						paramValues.Add(_routeId);

					}


					if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_ID =  @STREETS_STRETCHS.SS_ID@ " +
							" GROUP BY GRP_ID) v13");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v13.v_grp_id");
						paramValues.Add(_streetStretchId);
					}

					oBuilderTariffMinutes.AddSelectFieldLeft("DTM_GRP_ID",false);
					oBuilderTariffMinutes.AddSelectFieldRight("SUM(DTM_MINUTES) DTM_MINUTES",false);
					tmDayTariffminutes.TimeField= "DTM_DAY";
					oBuilderTariffMinutes.AddWhereAndClause(tmDayTariffminutes.SqlWhereWithoutTrunc("DAY_TARIFF_MINUTES", "DTM_DAY", paramValues));
					oBuilderTariffMinutes.AddGroupByClause("DTM_GRP_ID");

					oBuilderTariff.AddFromTableClause("(select "+oBuilderTariffMinutes.SqlSelect+" "+
						"from "+oBuilderTariffMinutes.SqlFrom+" "+
						"where "+oBuilderTariffMinutes.SqlWhere+" "+
						"group by "+oBuilderTariffMinutes.SqlGroupBy +
						")");
					oBuilderTariff.AddFromTableClause("V_STREET_PARK_SPACES");
					oBuilderTariff.AddFromTableClause("STREETS_STRETCHS");
					oBuilderTariff.AddWhereAndClause("SPS_STR_ID = SS_STR_ID");
					oBuilderTariff.AddWhereAndClause("SS_GRP_ID = DTM_GRP_ID");
					oBuilderTariff.AddSelectFieldLeft("SPS_STR_ID",false);
					oBuilderTariff.AddSelectFieldRight("PARK_SPACES * DTM_MINUTES TOTAL_MINUTES",false);
					oBuilderTariff.AddSelectFieldRight("PERC_RESIDENTS_SIN_TICKET",false);
					oBuilderTariff.AddSelectFieldRight("PERC_SIN_TICKET",false);
					oBuilderTariff.AddGroupByClause("SPS_STR_ID");
					oBuilderTariff.AddGroupByClause("PARK_SPACES");
					oBuilderTariff.AddGroupByClause("DTM_MINUTES");
					oBuilderTariff.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilderTariff.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddFromJoinClause(" INNER JOIN (select "+oBuilderTariff.SqlSelect+" "+
						"from "+oBuilderTariff.SqlFrom+" "+
						"where "+oBuilderTariff.SqlWhere+" "+
						"group by "+oBuilderTariff.SqlGroupBy +
						") ON (SPS_STR_ID = STR_ID)");
	
					break;
				case SerieGrouping.Time:

					oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);

					tmDayTariffminutes.TimeField= "DTM_DAY";
					oBuilderTariffMinutes.AddFromTableClause("DAY_TARIFF_MINUTES");
					oBuilderTariffMinutes.AddFromTableClause("V_SECTOR_PARK_SPACES");


					if (_unitId != null && GroupField != SerieGrouping.Unit) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and USS_UNI_ID =  @UNITS.UNI_ID@ " +
							" GROUP BY GRP_ID) v10");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v10.v_grp_id");
						paramValues.Add(_unitId);
					}

					if (_zoneId != null && GroupField != SerieGrouping.Zones) 
					{
						oBuilderTariffMinutes.AddFromTableClause("(SELECT SS_GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID =  v.cgrpg_child " +
							" and GRP_ID =  @GROUPS.GRP_ID@ " +
							" GROUP BY SS_GRP_ID) v11");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v11.v_grp_id");
						paramValues.Add(_zoneId);
					}


					if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
					{
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = @DAY_TARIFF_MINUTES.DTM_GRP_ID@");
						paramValues.Add(_sectorId);
					}

					if (_routeId != null && GroupField != SerieGrouping.Routes) 
					{


						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v, " +
							" V_GROUPS_CHILDS_ROUTES gc "+
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and  u.SS_ID= gc.CGRP_CHILD "+
							" and  gc.CGRP_ID =  @GROUPS_CHILDS.CGRP_ID@ " +
							" GROUP BY GRP_ID) v12");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v12.v_grp_id");
						paramValues.Add(_routeId);

					}


					if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_ID =  @STREETS_STRETCHS.SS_ID@ " +
							" GROUP BY GRP_ID) v13");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v13.v_grp_id");
						paramValues.Add(_streetStretchId);
					}

					if (this._streetId != null && GroupField != SerieGrouping.Street) 
					{

						oBuilderTariffMinutes.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_STR_ID =  @STREETS_STRETCHS.SS_STR_ID@ " +
							" GROUP BY GRP_ID) v14");
						oBuilderTariffMinutes.AddWhereAndClause("DTM_GRP_ID = v14.v_grp_id");
						paramValues.Add(_streetId);
					}


					oBuilderTariffMinutes.AddSelectFieldLeft(tmDayTariffminutes.SqlGroupBy + " DTM_DATE", true);
					oBuilderTariffMinutes.AddSelectFieldRight("SUM(DTM_MINUTES * PARK_SPACES) TOTAL_MINUTES",false);
					oBuilderTariffMinutes.AddSelectFieldRight("PARK_SPACES",false);
					oBuilderTariffMinutes.AddSelectFieldRight("PERC_RESIDENTS_SIN_TICKET",false);
					oBuilderTariffMinutes.AddSelectFieldRight("PERC_SIN_TICKET",false);
					oBuilderTariffMinutes.AddWhereAndClause(" SEPS_GRP_ID = DTM_GRP_ID ");
					oBuilderTariffMinutes.AddWhereAndClause(tmDayTariffminutes.SqlWhereWithoutTrunc("DAY_TARIFF_MINUTES", "DTM_DAY", paramValues));
					oBuilderTariffMinutes.AddGroupByClause("DTM_GRP_ID");
					oBuilderTariffMinutes.AddGroupByClause(tmDayTariffminutes.SqlGroupBy);
					oBuilderTariffMinutes.AddGroupByClause("PARK_SPACES");
					oBuilderTariffMinutes.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilderTariffMinutes.AddGroupByClause("PERC_SIN_TICKET");

					oBuilderTariff.AddFromTableClause("(select "+oBuilderTariffMinutes.SqlSelect+" "+
						"from "+oBuilderTariffMinutes.SqlFrom+" "+
						"where "+oBuilderTariffMinutes.SqlWhere+" "+
						"group by "+oBuilderTariffMinutes.SqlGroupBy +
						")");
					oBuilderTariff.AddSelectFieldLeft("DTM_DATE",true);
					oBuilderTariff.AddSelectFieldRight("SUM(TOTAL_MINUTES) TOTAL_MINUTES",false);
					oBuilderTariff.AddSelectFieldRight("SUM(PERC_RESIDENTS_SIN_TICKET * PARK_SPACES) / SUM(PARK_SPACES) PERC_RESIDENTS_SIN_TICKET",false);
					oBuilderTariff.AddSelectFieldRight("SUM(PERC_SIN_TICKET * PARK_SPACES) / SUM(PARK_SPACES) PERC_SIN_TICKET",false);
					oBuilderTariff.AddGroupByClause("DTM_DATE");
					oBuilder.AddFromJoinClause(" INNER JOIN (select "+oBuilderTariff.SqlSelect+" "+
						"from "+oBuilderTariff.SqlFrom+" "+
						//"where "+oBuilderTariff.SqlWhere+" "+
						"group by "+oBuilderTariff.SqlGroupBy +
						") ON (DTM_DATE = "+this._sqlTm.SqlGroupBy+")");				

					if (_unitId != null && GroupField != SerieGrouping.Unit) 
					{
						oBuilder.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and USS_UNI_ID =  @UNITS.UNI_ID@ " +
							" GROUP BY GRP_ID) v20");
						oBuilder.AddWhereAndClause("HOPE_GRP_ID = v20.v_grp_id");
						paramValues.Add(_unitId);
					}

					if (_zoneId != null && GroupField != SerieGrouping.Zones) 
					{
						oBuilder.AddFromTableClause("(SELECT SS_GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_zones_sectors  v " +
							" WHERE u.SS_GRP_ID =  v.cgrpg_child " +
							" and GRP_ID =  @GROUPS.GRP_ID@ " +
							" GROUP BY SS_GRP_ID) v21");
						oBuilder.AddWhereAndClause("HOPE_GRP_ID = v21.v_grp_id");
						paramValues.Add(_zoneId);
					}


					if (_sectorId != null && GroupField != SerieGrouping.Sectors) 
					{
						oBuilder.AddWhereAndClause("HOPE_GRP_ID = @DAY_TARIFF_MINUTES.DTM_GRP_ID@");
						paramValues.Add(_sectorId);
					}

					if (_routeId != null && GroupField != SerieGrouping.Routes) 
					{


						oBuilder.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v, " +
							" V_GROUPS_CHILDS_ROUTES gc "+
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and  u.SS_ID= gc.CGRP_CHILD "+
							" and  gc.CGRP_ID =  @GROUPS_CHILDS.CGRP_ID@ " +
							" GROUP BY GRP_ID) v22");
						oBuilder.AddWhereAndClause("HOPE_GRP_ID = v22.v_grp_id");
						paramValues.Add(_routeId);

					}


					if (_streetStretchId != null && GroupField != SerieGrouping.Street_Stretchs) 
					{

						oBuilder.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_ID =  @STREETS_STRETCHS.SS_ID@ " +
							" GROUP BY GRP_ID) v23");
						oBuilder.AddWhereAndClause("HOPE_GRP_ID = v23.v_grp_id");
						paramValues.Add(_streetStretchId);
					}

					if (this._streetId != null && GroupField != SerieGrouping.Street) 
					{

						oBuilder.AddFromTableClause("(SELECT GRP_ID v_grp_id " +
							" FROM V_UNITS_STREETS_STRETCHS u, " +
							" vw_groups_sectors  v " +
							" WHERE u.SS_GRP_ID = v.GRP_ID " +
							" and u.SS_STR_ID =  @STREETS_STRETCHS.SS_STR_ID@ " +
							" GROUP BY GRP_ID) v24");
						oBuilder.AddWhereAndClause("HOPE_GRP_ID = v24.v_grp_id");
						paramValues.Add(_streetId);
					}

					
					break;
			}


			
			oBuilder.AddWhereAndClause("HOPE_INIDATE IS NOT NULL");
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere(OperationHisTable, "HOPE_INIDATE", paramValues));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case SerieGrouping.Unit:
					oBuilder.AddGroupByClause("UNI_DESCSHORT");
					oBuilder.AddGroupByClause("TOTAL_MINUTES");
					oBuilder.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilder.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddSelectFieldLeft("UNI_DESCSHORT", true);
					oBuilder.AddOrderByClause("UNI_DESCSHORT");
					break;

				case SerieGrouping.Zones:

					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddGroupByClause("v1.GRP_ID");
					oBuilder.AddGroupByClause("TOTAL_MINUTES");
					oBuilder.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilder.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					oBuilder.AddOrderByClause("GRP_ID");
					break;
				case SerieGrouping.Sectors:

					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddGroupByClause("v1.GRP_ID");
					oBuilder.AddGroupByClause("TOTAL_MINUTES");
					oBuilder.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilder.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					oBuilder.AddOrderByClause("GRP_ID");
					break;
				case SerieGrouping.Routes:

					oBuilder.AddGroupByClause("v1.GRP_DESCSHORT");
					oBuilder.AddGroupByClause("v1.GRP_ID");
					oBuilder.AddGroupByClause("TOTAL_MINUTES");
					oBuilder.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilder.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddSelectFieldLeft("GRP_DESCSHORT", true);
					oBuilder.AddOrderByClause("GRP_ID");
					break;
				case SerieGrouping.Street_Stretchs:
					oBuilder.AddGroupByClause("SS_EXT_ID");
					oBuilder.AddGroupByClause("TOTAL_MINUTES");
					oBuilder.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilder.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddSelectFieldLeft("SS_EXT_ID", true);
					oBuilder.AddOrderByClause("SS_EXT_ID");

					break;
				case SerieGrouping.Street:
					oBuilder.AddGroupByClause("STR_DESC");
					oBuilder.AddGroupByClause("TOTAL_MINUTES");
					oBuilder.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilder.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddSelectFieldLeft("STR_DESC", true);
					oBuilder.AddOrderByClause("STR_DESC");

					break;
				case SerieGrouping.Time:
					/*oBuilder.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder.AddOrderByClause(this._sqlTm.SqlOrderBy);*/
					oBuilder.AddGroupByClause("TOTAL_MINUTES");
					oBuilder.AddGroupByClause("PERC_RESIDENTS_SIN_TICKET");
					oBuilder.AddGroupByClause("PERC_SIN_TICKET");
					oBuilder.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " AS DATA", false);
					break;
			}
			
			
			// Run query

			_dataSet = new StatisticsDB().FillDataSet(
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues.ToArray());
						

			if (_dataSet.Tables[0].Rows.Count<=CHART_COLUMN_3D_MAX)
			{
				_strXML =  GetXMLColumn3DChart(_dataSet);
			}
			else
			{
				_strXML = GetXMLColumn2DScrollChart(_dataSet);
			}

		}



		public override bool AllowsUnit()				{ return true; }
		public override bool AllowsGroup()				{ return true; }
		public override bool AllowsStreet()				{ return true; }
		public override bool AllowsTime()				{ return true; }
		public override bool AllowsZone()				{ return true; }
		public override bool AllowsSector()				{ return true; }
		public override bool AllowsRoute()				{ return true; }
		public override bool AllowStreetStretch()		{ return true; }

	}


	#endregion

}
