using System;
using System.Data;
using System.Collections;
using OPS.Components.Data;
using OPS.Components.Statistics.Helper;
using System.Web;


namespace OPS.Components.Statistics
{
	#region Definitions


	public enum ReportSerieValue 
	{
		None			=  0,
		Taking			=  1,
		OccupationValidTick	=  2,
		OccupationPlacesDist = 3,
		PlaceInventory	=  4,
		Works			=  5,
		Availability	=  6,
		Fines			=  7,
		ControlPanel	=  8,
		ControlPanelEv	= 9,
		CollectionTicket = 10,
		CollectionSummary = 11,
		CollectionTotalCash = 12,
		Alarms			= 13,
		Operations		= 14,
		OperationsPayType = 15
	}


	public enum ReportSerieGrouping 
	{
		None,
		Zones,
		Sectors,
		TipoSector,
		Routes,
		Unit,
		DayDef,
		AlarmsDef,
		AlarmsLevel,
		FinesDef,
		User,
		Time
	}
		


	#endregion

	#region Base Class ReportSerie

	public abstract class ReportSerie 
	{
		protected	string			_unitId;
		protected	ReportSerieGrouping 	_groupField = ReportSerieGrouping.None; // Field to group by in the query
		protected	string			_groupId; // Group of streets, units, other groups.
		protected   string			_zoneId;
		protected	string			_sectorId;
		protected	string			_tipoSectorId;
		protected	string			_routeId;
		protected	string			_alarmDefId;
		protected	string			_alarmLevelId;
		protected	string			_fineDefId;
		protected	string			_userId;
		protected	DataSet			_dataSet=null;
		protected	DataSet			_dataSetAlt=null;
		protected	SqlTimeManager	_sqlTm;
		protected	string			_strXML="";
		protected	string			_strChart="";
		protected	bool			_ReducedHist;
		protected	string			_litLanId;
		
		private		ReportSerieValue		_serieValue; // Read only protected property

		
		#region Static constants & constructor

		protected	static	int		OPERATIONSDEF_PARKING_ID; // Id of parking operation definition
		protected	static	int		OPERATIONSDEF_REFILL_ID; // Id of refund operation definition
		protected	static	int		OPERATIONSDEF_REFUND_ID; // Id of refund operation definition

		protected	static	char	GROUPSCHILDS_UNIT_ID; // Id of unit used in Group_Childs

		protected internal const string CHART_COLUMN_3D_TYPE="MSColumn3D";
		protected internal const string CHART_DOUGHNUT_TYPE="Doughnut3D";
		protected internal const string CHART_COLUMN_2D_SCROLL_TYPE="ScrollColumn2D";

		protected internal const int CHART_DOUGHNUT_MAX	= 10;
		protected internal const int CHART_COLUMN_3D_MAX	= 35;


		static ReportSerie() 
		{
			System.Configuration.AppSettingsReader	appSettings = new System.Configuration.AppSettingsReader();

			OPERATIONSDEF_PARKING_ID = (int) appSettings.GetValue("OperationsDef.ParkingId", typeof(int));
			OPERATIONSDEF_REFILL_ID = (int) appSettings.GetValue("OperationsDef.RefillId", typeof(int));
			OPERATIONSDEF_REFUND_ID = (int) appSettings.GetValue("OperationsDef.RefundId", typeof(int));

			GROUPSCHILDS_UNIT_ID = (char) appSettings.GetValue("GroupsChilds.UnitId", typeof(char));

		}

		#endregion

       
		public void Bind() 
		{
			Bind(ref _dataSet);
		}

		public abstract void Bind(ref DataSet _dataSet);

		public static ReportSerie ReportSerieFactory(ReportSerieValue serVal) 
		{
			ReportSerie e=null;


			// Specific work
			switch (serVal)
			{
				case ReportSerieValue.Taking:
					e = new ReportTakingSerie();
					break;
				case ReportSerieValue.OccupationValidTick:
					e = new ReportOccupationValidTickSerie();
					break;
				case ReportSerieValue.OccupationPlacesDist:
					e = new ReportOccupationPlacesDistSerie();
					break;
				case ReportSerieValue.PlaceInventory:
					e = new ReportPlacesInventorySerie();
					break;
				case ReportSerieValue.Works:
					e = new ReportWorksSerie();
					break;
				case ReportSerieValue.Availability:
					e = new ReportAvailabilitySerie();
					break;
				case ReportSerieValue.Fines:
					e = new ReportFinesSerie();
					break;
				case ReportSerieValue.ControlPanel:				
					e = new ReportControlPanelSerie();
					break;
				case ReportSerieValue.ControlPanelEv:				
					e = new ReportControlPanelEvSerie();
					break;
				case ReportSerieValue.CollectionTicket:				
					e = new ReportCollectionTicketSerie();
					break;
				case ReportSerieValue.CollectionSummary:				
					e = new ReportCollectionSummarySerie();
					break;
				case ReportSerieValue.CollectionTotalCash:				
					e = new ReportCollectionTotalCashSerie();
					break;
				case ReportSerieValue.Alarms:				
					e = new ReportAlarmsSerie();
					break;
				case ReportSerieValue.Operations:				
					e = new ReportOperationsSerie();
					break;
				case ReportSerieValue.OperationsPayType:				
					e = new ReportOperationsPayTypeSerie();
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

		protected ReportSerieValue CurrentSerieValue 
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
			set
			{
				_dataSet=null;
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

		public string TipoSectorId 
		{
			get 
			{
				return _tipoSectorId;
			}
			set 
			{
				_tipoSectorId = value;
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


		public string AlarmDefId
		{
			get 
			{
				return _alarmDefId;
			}
			set 
			{
				_alarmDefId = value;
			}
		}

		public string AlarmLevelId
		{
			get 
			{
				return _alarmLevelId;
			}
			set 
			{
				_alarmLevelId = value;
			}
		}

		public string FineDefId
		{
			get 
			{
				return _fineDefId;
			}
			set 
			{
				_fineDefId = value;
			}
		}

		public string UserId 
		{
			get 
			{
				return _userId;
			}
			set 
			{
				_userId = value;
			}
		}

		public string LitLanId 
		{
			get 
			{
				return _litLanId;
			}
			set 
			{
				_litLanId = value;
			}
		}

		public ReportSerieGrouping GroupField 
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

		public virtual bool AllowsGroupByUnit()				{ return false; }
		public virtual bool AllowsGroupByZone()				{ return false; }
		public virtual bool AllowsGroupBySector()			{ return false; }
		public virtual bool AllowsGroupByTipoSector()		{ return false; }
		public virtual bool AllowsGroupByRoute()			{ return false; }
		public virtual bool AllowsGroupByTime()				{ return false; }
		public virtual bool AllowsGroupByDaysDef()			{ return false; }
		public virtual bool AllowsGroupByAlarmsDef()		{ return false; }
		public virtual bool AllowsGroupByAlarmsLevel()		{ return false; }
		public virtual bool AllowsGroupByFinesDef()			{ return false; }
		public virtual bool AllowsGroupByUser()				{ return false; }

		#endregion

	}


	#endregion

	#region Series Implementation

	public class ReportTakingSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder2 = new SqlHelper();
			SqlHelper	oBuilder3 = new SqlHelper();
			SqlHelper	oBuilder4 = new SqlHelper();
			SqlHelper	oBuilder5 = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();
			SqlHelper	oBuilder7 = new SqlHelper();
			SqlHelper	oBuilder8 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues2 = new ArrayList();
			ArrayList	paramValues3 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();
			ArrayList	paramValues5 = new ArrayList();
			ArrayList	paramValues6 = new ArrayList();
			string strGroupbyView="";


			// SELECT
			oBuilder.AddSelectFieldLeft("rtp_desc",true);
			oBuilder.AddSelectFieldLeft("rtp_id",true);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPERS) NUM_OPERS",false);
			oBuilder.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_OPERS)>0)  THEN ROUND((SUM(OPE_VALUE) / SUM(OPE_NUM_OPERS)) / 100, 2) ELSE 0 END AVG_COST",false);
			oBuilder.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_OPERS)>0)  THEN ROUND(SUM(OPE_DURATION) / SUM(OPE_NUM_OPERS), 0)ELSE 0 END AVG_DURATION",false);
			oBuilder.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_OPERS)>0)  THEN ROUND(SUM(OPE_REALDURATION) / SUM(OPE_NUM_OPERS), 0) ELSE 0 END AVG_REALDURATION",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_COINS) / 100, 2) EUR_COINS",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_BANKCARD) / 100, 2) EUR_BANKCARD",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_CHIPCARD) / 100, 2) EUR_CHIPCARD",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_WAP) / 100, 2) EUR_WAP",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_TOTAL) / 100, 2) EUR_TOTAL",false);

			oBuilder7.AddSelectFieldLeft("rtp_desc",true);
			oBuilder7.AddSelectFieldLeft("rtp_id",true);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPERS) NUM_OPERS",false);
			oBuilder7.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_OPERS)>0)  THEN ROUND((SUM(OPE_VALUE) / SUM(OPE_NUM_OPERS)) / 100, 2) ELSE 0 END AVG_COST",false);
			oBuilder7.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_OPERS)>0)  THEN ROUND(SUM(OPE_DURATION) / SUM(OPE_NUM_OPERS), 0)ELSE 0 END AVG_DURATION",false);
			oBuilder7.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_OPERS)>0)  THEN ROUND(SUM(OPE_REALDURATION) / SUM(OPE_NUM_OPERS), 0) ELSE 0 END AVG_REALDURATION",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_COINS) / 100, 2) EUR_COINS",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_BANKCARD) / 100, 2) EUR_BANKCARD",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_CHIPCARD) / 100, 2) EUR_CHIPCARD",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_WAP) / 100, 2) EUR_WAP",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_VALUE_TOTAL) / 100, 2) EUR_TOTAL",false);


			oBuilder2.AddSelectFieldRight("SUM(SERV_HOURS) SERV_HOURS",false);
			oBuilder3.AddSelectFieldRight("ROUND(AVG(OPE_SERVICE_HOURS),2) SERV_HOURS",false);


			oBuilder4.AddSelectFieldRight("ROUND(SUM(NUM_PLACES)) NUM_PLACES",false);
			oBuilder5.AddSelectFieldRight("AVG(OPE_UNI_NUM_PLACES) NUM_PLACES",false);
			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);

			oBuilder8.AddSelectFieldRight("ROUND((SUM(OPE_NUM_OPERS)/COUNT(*))/AVG(OPE_UNI_NUM_PLACES * OPE_SERVICE_HOURS),3) OPE_ROTATION",false);
			oBuilder8.AddSelectFieldRight("ROUND(((SUM(OPE_DURATION)/60)/COUNT(*))/AVG(OPE_UNI_NUM_PLACES * OPE_SERVICE_HOURS),3) OPE_ATRACTION",false);


			// FROM
			oBuilder.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder.AddFromJoinClause("INNER JOIN REPORT_TAKING_PRODUCTS ON (OPE_RTP_ID = RTP_ID)");

			oBuilder7.AddFromTableClause("REPORT_TAKING_DATA R");
			oBuilder7.AddFromJoinClause("INNER JOIN REPORT_TAKING_PRODUCTS ON (OPE_RTP_ID = RTP_ID)");

			
			oBuilder3.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder5.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder8.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder8.AddWhereAndClause("OPE_RTP_ID <= 100");
			oBuilder8.AddWhereAndClause("OPE_UNI_NUM_PLACES > 0");
			oBuilder8.AddWhereAndClause("OPE_SERVICE_HOURS > 0");
		


			// WHERE (& necessary FROMs). 

			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues1.Add(_unitId);
				oBuilder7.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues5.Add(_unitId);
				oBuilder3.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues2.Add(_unitId);
				oBuilder5.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues3.Add(_unitId);
				oBuilder8.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues6.Add(_unitId);

			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues5.Add(_zoneId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues2.Add(_zoneId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues3.Add(_zoneId);
				oBuilder8.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues6.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues1.Add(_sectorId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues5.Add(_sectorId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues2.Add(_sectorId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues3.Add(_sectorId);
				oBuilder8.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues6.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder7.AddFromJoinClause("INNER JOIN GROUPS F ON (OPE_GRP_ID = F.GRP_ID)");
				oBuilder7.AddWhereAndClause("F.GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues5.Add(_tipoSectorId);

				oBuilder3.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder3.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues2.Add(_tipoSectorId);

				oBuilder5.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder5.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues3.Add(_tipoSectorId);

				oBuilder8.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder8.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues6.Add(_tipoSectorId);

			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues1.Add(_routeId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues5.Add(_routeId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues2.Add(_routeId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues3.Add(_routeId);
				oBuilder8.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues6.Add(_routeId);
			}

			
			// Time config
			this._sqlTm.TimeField = "OPE_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues1));
			oBuilder.AddGroupByClause("RTP_ID");
			oBuilder.AddGroupByClause("RTP_DESC");
			oBuilder.AddOrderByClause("to_number(RTP_ID)");

			oBuilder7.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues5));
			oBuilder7.AddGroupByClause("RTP_ID");
			oBuilder7.AddGroupByClause("RTP_DESC");

			oBuilder3.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues2));
			oBuilder3.AddGroupByClause("OPE_DATE");

			oBuilder5.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues3));
			oBuilder5.AddGroupByClause("OPE_UNI_ID");

			oBuilder8.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues6));

			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case ReportSerieGrouping.DayDef:
					oBuilder7.AddFromJoinClause("INNER JOIN DAY_DAYS_DEF D ON (R.OPE_DATE=D.DDAY_DATE)");
					oBuilder7.AddGroupByClause("DDAY_CODE");
					oBuilder7.AddGroupByClause("DDAY_DESCSHORT");
					oBuilder7.AddSelectFieldLeft("/*+INDEX(REPORT_TAKING_DATA RTD_OPE_DATE)*/DDAY_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("DDAY_CODE DESC");
					oBuilder7.AddOrderByClause("DDAY_DESCSHORT");
					oBuilder7.AddOrderByClause("to_number(RTP_ID)");
					strGroupbyView="V_REPORT_TAKING_DATA_GROUP";

					break;
				case ReportSerieGrouping.Unit:
					oBuilder7.AddFromJoinClause("INNER JOIN UNITS ON (OPE_UNI_ID = UNI_ID)");
					oBuilder7.AddGroupByClause("UNI_ID");
					oBuilder7.AddGroupByClause("UNI_DESCSHORT");
					oBuilder7.AddSelectFieldLeft("UNI_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("UNI_ID");
					oBuilder7.AddOrderByClause("to_number(RTP_ID)");
					strGroupbyView="V_REPORT_TAKING_DATA_GROUP";
					break;
				case ReportSerieGrouping.Zones:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ZONE_ID = T.GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					oBuilder7.AddOrderByClause("to_number(RTP_ID)");
					strGroupbyView="V_REPORT_TAKING_DATA_GROUP";

					break;
				case ReportSerieGrouping.Sectors:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ID = T.GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					oBuilder7.AddOrderByClause("to_number(RTP_ID)");
					strGroupbyView="V_REPORT_TAKING_DATA_GROUP";
					break;
				case ReportSerieGrouping.TipoSector:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ID = T.GRP_ID)");
					oBuilder7.AddFromJoinClause("INNER JOIN SECTOR_DEF T2 ON (T.GRP_SECTORTYPE_ID = T2.SED_ID)");
					oBuilder7.AddGroupByClause("T2.SED_ID");
					oBuilder7.AddGroupByClause("T2.SED_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T2.SED_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T2.SED_ID");
					oBuilder7.AddOrderByClause("to_number(RTP_ID)");
					strGroupbyView="V_REPORT_TAKING_DATA_GROUP";
					break;
				case ReportSerieGrouping.Routes:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ROUTE_ID = GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					oBuilder7.AddOrderByClause("to_number(RTP_ID)");
					strGroupbyView="V_REPORT_TAKING_DATA_GROUP";
					break;
				case ReportSerieGrouping.Time:
					oBuilder7.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder7.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddOrderByClause("to_number(RTP_ID)");
					oBuilder7.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " GROUP_DESC", false);
					strGroupbyView="V_REPORT_TAKING_DATA_GROUP";
					break;
			}
			
			
			// Run queries

			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_DATA_FULL",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_SERV_HOURS",
				oBuilder2.SqlSelect,
				"("+oBuilder3.SqlFullSentence+")",
				oBuilder2.SqlWhere,
				oBuilder2.SqlGroupBy,
				oBuilder2.SqlOrderBy,
				paramValues2.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_NUM_PLACES",
				oBuilder4.SqlSelect,
				"("+oBuilder5.SqlFullSentence+")",
				oBuilder4.SqlWhere,
				oBuilder4.SqlGroupBy,
				oBuilder4.SqlOrderBy,
				paramValues3.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_ROT_ATRAC",
				oBuilder8.SqlSelect,
				oBuilder8.SqlFrom,
				oBuilder8.SqlWhere,
				oBuilder8.SqlGroupBy,
				oBuilder8.SqlOrderBy,
				paramValues6.ToArray());

			if (strGroupbyView.Length>0)
			{
				new StatisticsDB().FillDataSet(
					ref _dataSet,
					strGroupbyView,
					oBuilder7.SqlSelect,
					oBuilder7.SqlFrom,
					oBuilder7.SqlWhere,
					oBuilder7.SqlGroupBy,
					oBuilder7.SqlOrderBy,
					paramValues5.ToArray());
			}


		}




		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return true; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}


	public class ReportOccupationValidTickSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder2 = new SqlHelper();
			SqlHelper	oBuilder3 = new SqlHelper();
			SqlHelper	oBuilder4 = new SqlHelper();
			SqlHelper	oBuilder5 = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues2 = new ArrayList();
			ArrayList	paramValues3 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();


			// SELECT
			oBuilder.AddSelectFieldLeft("OPE_HOUR_INI \"Hora\"",true);
			oBuilder.AddSelectFieldRight("round(avg((OPE_VALID_TICKETS_1 / OPE_UNI_NUM_PLACES_1) * 100),2) \"Lunes\"",false);
			oBuilder.AddSelectFieldRight("round(avg((OPE_VALID_TICKETS_2 / OPE_UNI_NUM_PLACES_2) * 100),2) \"Martes\"",false);
			oBuilder.AddSelectFieldRight("round(avg((OPE_VALID_TICKETS_3 / OPE_UNI_NUM_PLACES_3) * 100),2) \"Miercoles\"",false);
			oBuilder.AddSelectFieldRight("round(avg((OPE_VALID_TICKETS_4 / OPE_UNI_NUM_PLACES_4) * 100),2) \"Jueves\"",false);
			oBuilder.AddSelectFieldRight("round(avg((OPE_VALID_TICKETS_5 / OPE_UNI_NUM_PLACES_5) * 100),2) \"Viernes\"",false);
			oBuilder.AddSelectFieldRight("round(avg((OPE_VALID_TICKETS_6 / OPE_UNI_NUM_PLACES_6) * 100),2) \"Sabado\"",false);
			oBuilder.AddSelectFieldRight("round(avg((OPE_VALID_TICKETS_7 / OPE_UNI_NUM_PLACES_7) * 100),2) \"Domingo\"",false);
			oBuilder.AddSelectFieldRight("round(avg((OPE_VALID_TICKETS_LAB / OPE_UNI_NUM_PLACES_LAB) * 100),2) \"Laborables\"",false);

			
			oBuilder2.AddSelectFieldRight("SUM(SERV_HOURS) SERV_HOURS",false);
			oBuilder3.AddSelectFieldRight("ROUND(AVG(OPE_SERVICE_HOURS),2) SERV_HOURS",false);


			oBuilder4.AddSelectFieldRight("ROUND(SUM(NUM_PLACES)) NUM_PLACES",false);
			oBuilder5.AddSelectFieldRight("AVG(OPE_UNI_NUM_PLACES) NUM_PLACES",false);
			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);


			// FROM
			oBuilder.AddFromTableClause("V_REPORT_WEEK_VALID_TICKETS");
			oBuilder.AddWhereAndClause("1=1");

			
			oBuilder3.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder5.AddFromTableClause("REPORT_TAKING_DATA");
		


			// WHERE (& necessary FROMs). 

			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("OPE_UNI_ID = @V_REPORT_WEEK_VALID_TICKETS.OPE_UNI_ID@");
				paramValues1.Add(_unitId);
				oBuilder3.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues2.Add(_unitId);
				oBuilder5.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues3.Add(_unitId);

			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ZONE_ID = @V_REPORT_WEEK_VALID_TICKETS.OPE_GRP_ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues2.Add(_zoneId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues3.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ID = @V_REPORT_WEEK_VALID_TICKETS.OPE_GRP_ID@");
				paramValues1.Add(_sectorId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues2.Add(_sectorId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues3.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder3.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder3.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues2.Add(_tipoSectorId);

				oBuilder5.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder5.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues3.Add(_tipoSectorId);

			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ROUTE_ID = @V_REPORT_WEEK_VALID_TICKETS.OPE_GRP_ROUTE_ID@");
				paramValues1.Add(_routeId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues2.Add(_routeId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues3.Add(_routeId);
			}

			
			// Time config
			this._sqlTm.TimeField = "OPE_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_WEEK_VALID_TICKETS", "OPE_DATE", paramValues1));
			oBuilder.AddGroupByClause("OPE_HOUR_INI");
			oBuilder.AddOrderByClause("OPE_HOUR_INI");

			oBuilder3.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues2));
			oBuilder3.AddGroupByClause("OPE_DATE");

			oBuilder5.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues3));
			oBuilder5.AddGroupByClause("OPE_UNI_ID");

			
			// Run queries

			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_VALID_TICKETS",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_SERV_HOURS",
				oBuilder2.SqlSelect,
				"("+oBuilder3.SqlFullSentence+")",
				oBuilder2.SqlWhere,
				oBuilder2.SqlGroupBy,
				oBuilder2.SqlOrderBy,
				paramValues2.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_NUM_PLACES",
				oBuilder4.SqlSelect,
				"("+oBuilder5.SqlFullSentence+")",
				oBuilder4.SqlWhere,
				oBuilder4.SqlGroupBy,
				oBuilder4.SqlOrderBy,
				paramValues3.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());


		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return false; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	public class ReportOccupationPlacesDistSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder2 = new SqlHelper();
			SqlHelper	oBuilder3 = new SqlHelper();
			SqlHelper	oBuilder4 = new SqlHelper();
			SqlHelper	oBuilder5 = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();
			SqlHelper	oBuilder8 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues2 = new ArrayList();
			ArrayList	paramValues3 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();
			ArrayList	paramValues6 = new ArrayList();


			// SELECT
			oBuilder.AddSelectFieldLeft("OPE_HOUR_INI",true);
			oBuilder.AddSelectFieldRight("ROUND(AVG((OPE_ROT_LEGALLY_PARK / OPE_UNI_NUM_PLACES) * 100),2) OPE_ROT_LEGALLY_PARK",false);
			oBuilder.AddSelectFieldRight("ROUND(AVG((OPE_ROT_ILLEGALLY_PARK / OPE_UNI_NUM_PLACES) * 100),2) OPE_ROT_ILLEGALLY_PARK",false);
			oBuilder.AddSelectFieldRight("ROUND(AVG((OPE_RES_LEGALLY_PARK / OPE_UNI_NUM_PLACES) * 100),2) OPE_RES_LEGALLY_PARK",false);
			oBuilder.AddSelectFieldRight("ROUND(AVG((OPE_RES_ILLEGALLY_PARK / OPE_UNI_NUM_PLACES) * 100),2) OPE_RES_ILLEGALLY_PARK",false);
			oBuilder.AddSelectFieldRight("ROUND(AVG((OPE_PLACES_AFFEC_WORKS / OPE_UNI_NUM_PLACES) * 100),2) OPE_PLACES_AFFEC_WORKS",false);
			oBuilder.AddSelectFieldRight("ROUND(AVG((OPE_EMPTY_PLACES / OPE_UNI_NUM_PLACES) * 100),2) OPE_EMPTY_PLACES",false);


			oBuilder2.AddSelectFieldRight("SUM(SERV_HOURS) SERV_HOURS",false);
			oBuilder3.AddSelectFieldRight("ROUND(AVG(OPE_SERVICE_HOURS),2) SERV_HOURS",false);


			oBuilder4.AddSelectFieldRight("ROUND(SUM(NUM_PLACES)) NUM_PLACES",false);
			oBuilder5.AddSelectFieldRight("AVG(OPE_UNI_NUM_PLACES) NUM_PLACES",false);
			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);

			oBuilder8.AddSelectFieldRight("ROUND((SUM(OPE_THIN_GRAINED_DATA) / SUM(OPE_TOTAL_DATA)) * 100,2) OPE_THIN_GRAINED_DATA",false);
			oBuilder8.AddSelectFieldRight("ROUND((SUM(OPE_AVG_DATA) / SUM(OPE_TOTAL_DATA)) * 100, 2) OPE_AVG_DATA",false);


			// FROM
			oBuilder.AddFromTableClause("V_REPORT_OCC_PLACES_DIST_BASE");
			oBuilder.AddWhereAndClause("OPE_UNI_NUM_PLACES > 0");
			
			
			oBuilder3.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder5.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder8.AddFromTableClause("V_REPORT_OCC_PLACES_DIST_BASE");
			oBuilder8.AddWhereAndClause("OPE_UNI_NUM_PLACES > 0");
		


			// WHERE (& necessary FROMs). 

			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("OPE_UNI_ID = @V_REPORT_OCC_PLACES_DIST_BASE.OPE_UNI_ID@");
				paramValues1.Add(_unitId);
				oBuilder3.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues2.Add(_unitId);
				oBuilder5.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues3.Add(_unitId);
				oBuilder8.AddWhereAndClause("OPE_UNI_ID = @V_REPORT_OCC_PLACES_DIST_BASE.OPE_UNI_ID@");
				paramValues6.Add(_unitId);

			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ZONE_ID = @V_REPORT_OCC_PLACES_DIST_BASE.OPE_GRP_ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues2.Add(_zoneId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues3.Add(_zoneId);
				oBuilder8.AddWhereAndClause("OPE_GRP_ZONE_ID = @V_REPORT_OCC_PLACES_DIST_BASE.OPE_GRP_ZONE_ID@");
				paramValues6.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ID = @V_REPORT_OCC_PLACES_DIST_BASE.OPE_GRP_ID@");
				paramValues1.Add(_sectorId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues2.Add(_sectorId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues3.Add(_sectorId);
				oBuilder8.AddWhereAndClause("OPE_GRP_ID = @V_REPORT_OCC_PLACES_DIST_BASE.OPE_GRP_ID@");
				paramValues6.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder3.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder3.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues2.Add(_tipoSectorId);

				oBuilder5.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder5.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues3.Add(_tipoSectorId);

				oBuilder8.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder8.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues6.Add(_tipoSectorId);

			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ROUTE_ID = @V_REPORT_OCC_PLACES_DIST_BASE.OPE_GRP_ROUTE_ID@");
				paramValues1.Add(_routeId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues2.Add(_routeId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues3.Add(_routeId);
				oBuilder8.AddWhereAndClause("OPE_GRP_ROUTE_ID = @V_REPORT_OCC_PLACES_DIST_BASE.OPE_GRP_ROUTE_ID@");
				paramValues6.Add(_routeId);
			}

			
			// Time config
			this._sqlTm.TimeField = "OPE_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_OCC_PLACES_DIST_BASE", "OPE_DATE", paramValues1));
			oBuilder.AddGroupByClause("OPE_HOUR_INI");
			oBuilder.AddOrderByClause("OPE_HOUR_INI");

			oBuilder3.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues2));
			oBuilder3.AddGroupByClause("OPE_DATE");

			oBuilder5.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues3));
			oBuilder5.AddGroupByClause("OPE_UNI_ID");

			oBuilder8.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_OCC_PLACES_DIST_BASE", "OPE_DATE", paramValues6));

			
			
			// Run queries

			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_OCC_PLACES_DIST",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_SERV_HOURS",
				oBuilder2.SqlSelect,
				"("+oBuilder3.SqlFullSentence+")",
				oBuilder2.SqlWhere,
				oBuilder2.SqlGroupBy,
				oBuilder2.SqlOrderBy,
				paramValues2.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_NUM_PLACES",
				oBuilder4.SqlSelect,
				"("+oBuilder5.SqlFullSentence+")",
				oBuilder4.SqlWhere,
				oBuilder4.SqlGroupBy,
				oBuilder4.SqlOrderBy,
				paramValues3.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REP_OCC_PLAC_DIST_DATA_QUAL",
				oBuilder8.SqlSelect,
				oBuilder8.SqlFrom,
				oBuilder8.SqlWhere,
				oBuilder8.SqlGroupBy,
				oBuilder8.SqlOrderBy,
				paramValues6.ToArray());


		}



		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return true; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	public class ReportPlacesInventorySerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder2 = new SqlHelper();
			SqlHelper	oBuilder3 = new SqlHelper();
			SqlHelper	oBuilder4 = new SqlHelper();
			SqlHelper	oBuilder5 = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();
			SqlHelper	oBuilder7 = new SqlHelper();
			SqlHelper	oBuilder8 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues2 = new ArrayList();
			ArrayList	paramValues3 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();
			ArrayList	paramValues5 = new ArrayList();
			ArrayList	paramValues6 = new ArrayList();
			string strGroupbyView="";

			// SELECT
			//INVENTARIO AL FINAL DEL PERIODO
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_EN_LINEA) SSIV_P_EN_LINEA",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_EN_BATERIA) SSIV_P_EN_BATERIA",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_C_D_DIA_ENTERO) SSIV_P_C_D_DIA_ENTERO",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_C_D_MEDIODIA) SSIV_P_C_D_MEDIODIA",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_PMR) SSIV_P_PMR",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_VADO_DIA_ENTERO) SSIV_P_VADO_DIA_ENTERO",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_VADO_MEDIODIA) SSIV_P_VADO_MEDIODIA",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_BASURA) SSIV_P_BASURA",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_MOTOS) SSIV_P_MOTOS",false);
			oBuilder.AddSelectFieldRight("SUM(SSIV_P_WORKS) SSIV_P_WORKS",false);

			//INVENTARIO AL INICIO DEL PERIODO
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_EN_LINEA) SSIV_P_EN_LINEA",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_EN_BATERIA) SSIV_P_EN_BATERIA",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_C_D_DIA_ENTERO) SSIV_P_C_D_DIA_ENTERO",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_C_D_MEDIODIA) SSIV_P_C_D_MEDIODIA",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_PMR) SSIV_P_PMR",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_VADO_DIA_ENTERO) SSIV_P_VADO_DIA_ENTERO",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_VADO_MEDIODIA) SSIV_P_VADO_MEDIODIA",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_BASURA) SSIV_P_BASURA",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_MOTOS) SSIV_P_MOTOS",false);
			oBuilder8.AddSelectFieldRight("SUM(SSIV_P_WORKS) SSIV_P_WORKS",false);


			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_EN_LINEA) SSIV_P_EN_LINEA",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_EN_BATERIA) SSIV_P_EN_BATERIA",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_C_D_DIA_ENTERO) SSIV_P_C_D_DIA_ENTERO",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_C_D_MEDIODIA) SSIV_P_C_D_MEDIODIA",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_PMR) SSIV_P_PMR",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_VADO_DIA_ENTERO) SSIV_P_VADO_DIA_ENTERO",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_VADO_MEDIODIA) SSIV_P_VADO_MEDIODIA",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_BASURA) SSIV_P_BASURA",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_MOTOS) SSIV_P_MOTOS",false);
			oBuilder7.AddSelectFieldRight("SUM(SSIV_P_WORKS) SSIV_P_WORKS",false);



			oBuilder2.AddSelectFieldRight("SUM(SERV_HOURS) SERV_HOURS",false);
			oBuilder3.AddSelectFieldRight("ROUND(AVG(OPE_SERVICE_HOURS),2) SERV_HOURS",false);


			oBuilder4.AddSelectFieldRight("ROUND(SUM(NUM_PLACES)) NUM_PLACES",false);
			oBuilder5.AddSelectFieldRight("AVG(OPE_UNI_NUM_PLACES) NUM_PLACES",false);
			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);


			// FROM

			if (GroupField != ReportSerieGrouping.Routes)
			{
				
				if (_routeId != null)
				{
					oBuilder.AddFromTableClause("V_REPORT_PLACES_INVENTORY");
					oBuilder8.AddFromTableClause("V_REPORT_PLACES_INVENTORY");
					oBuilder7.AddFromTableClause("V_REPORT_PLACES_INVENTORY R");

				}
				else
				{
					oBuilder.AddFromTableClause("V_REPORT_PLACES_INV_NO_ROUTE");
					oBuilder8.AddFromTableClause("V_REPORT_PLACES_INV_NO_ROUTE");
					oBuilder7.AddFromTableClause("V_REPORT_PLACES_INV_NO_ROUTE R");
				}

			}
			else
			{
				oBuilder7.AddFromTableClause("V_REPORT_PLACES_INVENTORY R");
				oBuilder.AddFromTableClause("V_REPORT_PLACES_INV_NO_ROUTE");
				oBuilder8.AddFromTableClause("V_REPORT_PLACES_INV_NO_ROUTE");

			}
			
			oBuilder3.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder5.AddFromTableClause("REPORT_TAKING_DATA");
		


			// WHERE (& necessary FROMs). 


			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("ZONE_ID = @V_REPORT_PLACES_INVENTORY.ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder8.AddWhereAndClause("ZONE_ID = @V_REPORT_PLACES_INVENTORY.ZONE_ID@");
				paramValues6.Add(_zoneId);
				oBuilder7.AddWhereAndClause("ZONE_ID = @V_REPORT_PLACES_INVENTORY.ZONE_ID@");
				paramValues5.Add(_zoneId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues2.Add(_zoneId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues3.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("SECTOR_ID = @V_REPORT_PLACES_INVENTORY.SECTOR_ID@");
				paramValues1.Add(_sectorId);
				oBuilder8.AddWhereAndClause("SECTOR_ID = @V_REPORT_PLACES_INVENTORY.SECTOR_ID@");
				paramValues6.Add(_sectorId);
				oBuilder7.AddWhereAndClause("SECTOR_ID = @V_REPORT_PLACES_INVENTORY.SECTOR_ID@");
				paramValues5.Add(_sectorId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues2.Add(_sectorId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues3.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddWhereAndClause("SECTORDEF_ID = @V_REPORT_PLACES_INVENTORY.SECTORDEF_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder8.AddWhereAndClause("SECTORDEF_ID = @V_REPORT_PLACES_INVENTORY.SECTORDEF_ID@");
				paramValues6.Add(_tipoSectorId);


				oBuilder7.AddWhereAndClause("SECTORDEF_ID = @V_REPORT_PLACES_INVENTORY.SECTORDEF_ID@");
				paramValues5.Add(_tipoSectorId);

				oBuilder3.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder3.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues2.Add(_tipoSectorId);

				oBuilder5.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder5.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues3.Add(_tipoSectorId);
			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("ROUTE_ID = @V_REPORT_PLACES_INVENTORY.ROUTE_ID@");
				paramValues1.Add(_routeId);
				
				oBuilder8.AddWhereAndClause("ROUTE_ID = @V_REPORT_PLACES_INVENTORY.ROUTE_ID@");
				paramValues6.Add(_routeId);
				
				oBuilder7.AddWhereAndClause("ROUTE_ID = @V_REPORT_PLACES_INVENTORY.ROUTE_ID@");
				paramValues5.Add(_routeId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues2.Add(_routeId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues3.Add(_routeId);
			}

			


			// Time config
			this._sqlTm.TimeField = "SSIV_END_DATE";
			oBuilder.AddWhereAndClause("SSIV_INI_DATE<=@V_REPORT_PLACES_INVENTORY.SSIV_INI_DATE@");
			paramValues1.Add(_sqlTm.EndDate);
			oBuilder.AddWhereAndClause("SSIV_END_DATE>=@V_REPORT_PLACES_INVENTORY.SSIV_END_DATE@");
			paramValues1.Add(_sqlTm.EndDate);

			oBuilder8.AddWhereAndClause("SSIV_INI_DATE<=@V_REPORT_PLACES_INVENTORY.SSIV_INI_DATE@");
			paramValues6.Add(_sqlTm.StartDate);
			oBuilder8.AddWhereAndClause("SSIV_END_DATE>=@V_REPORT_PLACES_INVENTORY.SSIV_END_DATE@");
			paramValues6.Add(_sqlTm.StartDate);
			
			
			oBuilder7.AddWhereAndClause("SSIV_INI_DATE<=@V_REPORT_PLACES_INVENTORY.SSIV_INI_DATE@");
			paramValues5.Add(_sqlTm.EndDate);
			oBuilder7.AddWhereAndClause("SSIV_END_DATE>=@V_REPORT_PLACES_INVENTORY.SSIV_END_DATE@");
			paramValues5.Add(_sqlTm.EndDate);

			oBuilder3.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues2));
			oBuilder3.AddGroupByClause("OPE_DATE");


			oBuilder5.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues3));
			oBuilder5.AddGroupByClause("OPE_UNI_ID");


			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{

				case ReportSerieGrouping.Zones:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (ZONE_ID = T.GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_PLACES_INV_GROUP";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");

					break;
				case ReportSerieGrouping.Sectors:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (SECTOR_ID = T.GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_PLACES_INV_GROUP";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");
					break;
				case ReportSerieGrouping.TipoSector:
					oBuilder7.AddFromJoinClause("INNER JOIN SECTOR_DEF T2 ON (SECTORDEF_ID = T2.SED_ID)");
					oBuilder7.AddSelectFieldLeft("T2.SED_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T2.SED_ID");
					oBuilder7.AddGroupByClause("T2.SED_DESCLONG");
					oBuilder7.AddGroupByClause("T2.SED_ID");
					strGroupbyView="V_REPORT_PLACES_INV_GROUP";
					break;
				case ReportSerieGrouping.Routes:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (ROUTE_ID = GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_PLACES_INV_GROUP";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");
					break;

			}
			
			_dataSet = new DataSet();			
			// Run queries

			if (strGroupbyView!="")
			{
				new StatisticsDB().FillDataSet(
					ref _dataSet,
					strGroupbyView,
					oBuilder7.SqlSelect,
					oBuilder7.SqlFrom,
					oBuilder7.SqlWhere,
					oBuilder7.SqlGroupBy,
					oBuilder7.SqlOrderBy,
					paramValues5.ToArray());
			}


			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_PLACES_INVENTORY",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_PLACES_INV_DATE_INI",
				oBuilder8.SqlSelect,
				oBuilder8.SqlFrom,
				oBuilder8.SqlWhere,
				oBuilder8.SqlGroupBy,
				oBuilder8.SqlOrderBy,
				paramValues6.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_SERV_HOURS",
				oBuilder2.SqlSelect,
				"("+oBuilder3.SqlFullSentence+")",
				oBuilder2.SqlWhere,
				oBuilder2.SqlGroupBy,
				oBuilder2.SqlOrderBy,
				paramValues2.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_NUM_PLACES",
				oBuilder4.SqlSelect,
				"("+oBuilder5.SqlFullSentence+")",
				oBuilder4.SqlWhere,
				oBuilder4.SqlGroupBy,
				oBuilder4.SqlOrderBy,
				paramValues3.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());

		}


		public override bool AllowsGroupByUnit()				{ return false; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return false; }
		public override bool AllowsGroupByDaysDef()				{ return false; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }


	}

	public class ReportWorksSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder2 = new SqlHelper();
			SqlHelper	oBuilder3 = new SqlHelper();
			SqlHelper	oBuilder4 = new SqlHelper();
			SqlHelper	oBuilder5 = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();
			SqlHelper	oBuilder7 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues2 = new ArrayList();
			ArrayList	paramValues3 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();
			ArrayList	paramValues5 = new ArrayList();
			string strGroupbyView="";

			// SELECT

			oBuilder.AddSelectFieldLeft("SS_EXT_ID",false);
			oBuilder.AddSelectFieldRight("WORK_INI_DATE_STR",false);
			oBuilder.AddSelectFieldRight("WORK_END_DATE_STR",false);
			oBuilder.AddSelectFieldRight("WORK_NUM_PARK_SPACES",false);
			oBuilder.AddSelectFieldRight("WORK_HOURS",false);
			oBuilder.AddSelectFieldRight("WORK_LICENSE",false);
			oBuilder.AddSelectFieldRight("WORK_COMPANY",false);

			oBuilder7.AddSelectFieldLeft("SUM(WORK_SPACE_HOURS) WORK_SPACE_HOURS",false);
			oBuilder7.AddSelectFieldRight("SUM(WORK_NUM_PARK_SPACES) WORK_NUM_PARK_SPACES",false);
			oBuilder7.AddSelectFieldRight("SUM(WORK_HOURS) WORK_HOURS",false);

			oBuilder2.AddSelectFieldRight("SUM(SERV_HOURS) SERV_HOURS",false);
			oBuilder3.AddSelectFieldRight("ROUND(AVG(OPE_SERVICE_HOURS),2) SERV_HOURS",false);


			oBuilder4.AddSelectFieldRight("ROUND(SUM(NUM_PLACES)) NUM_PLACES",false);
			oBuilder5.AddSelectFieldRight("AVG(OPE_UNI_NUM_PLACES) NUM_PLACES",false);
			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);


			if (GroupField != ReportSerieGrouping.Routes)
			{
				
				if (_routeId != null)
				{
					oBuilder.AddFromTableClause("V_REPORT_WORKS");
					oBuilder7.AddFromTableClause("V_REPORT_WORKS R");

				}
				else
				{
					oBuilder.AddFromTableClause("V_REPORT_WORKS_NO_ROUTE");
					oBuilder7.AddFromTableClause("V_REPORT_WORKS_NO_ROUTE R");
				}

			}
			else
			{
				oBuilder7.AddFromTableClause("V_REPORT_WORKS R");
				oBuilder.AddFromTableClause("V_REPORT_WORKS_NO_ROUTE");

			}


			// FROM

			
			oBuilder3.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder5.AddFromTableClause("REPORT_TAKING_DATA");
		


			// WHERE (& necessary FROMs). 

			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("WORK_UNI_ID = @V_REPORT_WORKS.WORK_UNI_ID@");
				paramValues1.Add(_unitId);
				oBuilder7.AddWhereAndClause("WORK_UNI_ID = @V_REPORT_WORKS.WORK_UNI_ID@");
				paramValues5.Add(_unitId);
				oBuilder3.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues2.Add(_unitId);
				oBuilder5.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues3.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("ZONE_ID = @V_REPORT_WORKS.ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder7.AddWhereAndClause("ZONE_ID = @V_REPORT_WORKS.ZONE_ID@");
				paramValues5.Add(_zoneId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues2.Add(_zoneId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues3.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("SECTOR_ID = @V_REPORT_WORKS.SECTOR_ID@");
				paramValues1.Add(_sectorId);
				oBuilder7.AddWhereAndClause("SECTOR_ID = @V_REPORT_WORKS.SECTOR_ID@");
				paramValues5.Add(_sectorId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues2.Add(_sectorId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues3.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddWhereAndClause("SECTORDEF_ID = @V_REPORT_WORKS.SECTORDEF_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder7.AddWhereAndClause("SECTORDEF_ID = @V_REPORT_WORKS.SECTORDEF_ID@");
				paramValues5.Add(_tipoSectorId);

				oBuilder3.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder3.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues2.Add(_tipoSectorId);

				oBuilder5.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder5.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues3.Add(_tipoSectorId);
			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("ROUTE_ID = @V_REPORT_WORKS.ROUTE_ID@");
				paramValues1.Add(_routeId);
				oBuilder7.AddWhereAndClause("ROUTE_ID = @V_REPORT_WORKS.ROUTE_ID@");
				paramValues5.Add(_routeId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues2.Add(_routeId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues3.Add(_routeId);
			}

			

			if (_userId != null && GroupField != ReportSerieGrouping.User) 
			{
				oBuilder.AddWhereAndClause("WORK_USR_ID = @V_REPORT_WORKS.WORK_USR_ID@");
				paramValues1.Add(_userId);
				oBuilder7.AddWhereAndClause("WORK_USR_ID = @V_REPORT_WORKS.WORK_USR_ID@");
				paramValues5.Add(_userId);
			}


			// Time config
			this._sqlTm.TimeField = "WORK_INI_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithNVLSysdate("V_REPORT_WORKS", "WORK_END_DATE", paramValues1));
			oBuilder.AddOrderByClause("WORK_INI_DATE ASC");
			oBuilder7.AddWhereAndClause(_sqlTm.SqlWhereWithNVLSysdate("V_REPORT_WORKS", "WORK_END_DATE", paramValues5));

			oBuilder3.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues2));
			oBuilder3.AddGroupByClause("OPE_DATE");


			oBuilder5.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues3));
			oBuilder5.AddGroupByClause("OPE_UNI_ID");


			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case ReportSerieGrouping.DayDef:
					oBuilder7.AddFromJoinClause("INNER JOIN DAY_DAYS_DEF D ON (TRUNC(R.WORK_INI_DATE)=D.DDAY_DATE)");
					oBuilder7.AddSelectFieldLeft("DDAY_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("DDAY_CODE DESC");
					oBuilder7.AddOrderByClause("DDAY_DESCSHORT");
					strGroupbyView="V_REPORT_WORKS_GROUP";
					oBuilder7.AddGroupByClause("DDAY_DESCSHORT");
					oBuilder7.AddGroupByClause("DDAY_CODE");

					break;
				case ReportSerieGrouping.Unit:
					oBuilder7.AddFromJoinClause("INNER JOIN UNITS ON (WORK_UNI_ID = UNI_ID)");
					oBuilder7.AddSelectFieldLeft("UNI_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("UNI_ID");
					strGroupbyView="V_REPORT_WORKS_GROUP";
					oBuilder7.AddGroupByClause("UNI_DESCSHORT");
					oBuilder7.AddGroupByClause("UNI_ID");
					break;
				case ReportSerieGrouping.Zones:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (ZONE_ID = T.GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_WORKS_GROUP";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");

					break;
				case ReportSerieGrouping.Sectors:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (SECTOR_ID = T.GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_WORKS_GROUP";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");
					break;
				case ReportSerieGrouping.TipoSector:
					oBuilder7.AddFromJoinClause("INNER JOIN SECTOR_DEF T2 ON (SECTORDEF_ID = T2.SED_ID)");
					oBuilder7.AddSelectFieldLeft("T2.SED_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T2.SED_ID");
					oBuilder7.AddGroupByClause("T2.SED_DESCLONG");
					oBuilder7.AddGroupByClause("T2.SED_ID");
					strGroupbyView="V_REPORT_WORKS_GROUP";
					break;
				case ReportSerieGrouping.Routes:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (ROUTE_ID = GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_WORKS_GROUP";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");
					break;

				case ReportSerieGrouping.User:
					oBuilder7.AddSelectFieldLeft("TO_CHAR(WORK_USR_ID) GROUP_DESC",false);
					oBuilder7.AddOrderByClause("WORK_USR_ID");
					strGroupbyView="V_REPORT_WORKS_GROUP";
					oBuilder7.AddGroupByClause("WORK_USR_ID");
					break;				
				case ReportSerieGrouping.Time:
					oBuilder7.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder7.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " GROUP_DESC", false);
					strGroupbyView="V_REPORT_WORKS_GROUP";
					break;
			}
			
			_dataSet = new DataSet();			
			// Run queries

			if (strGroupbyView!="")
			{
				new StatisticsDB().FillDataSet(
					ref _dataSet,
					strGroupbyView,
					oBuilder7.SqlSelect,
					oBuilder7.SqlFrom,
					oBuilder7.SqlWhere,
					oBuilder7.SqlGroupBy,
					oBuilder7.SqlOrderBy,
					paramValues5.ToArray());
			}


			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_WORKS",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());


			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_SERV_HOURS",
				oBuilder2.SqlSelect,
				"("+oBuilder3.SqlFullSentence+")",
				oBuilder2.SqlWhere,
				oBuilder2.SqlGroupBy,
				oBuilder2.SqlOrderBy,
				paramValues2.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_NUM_PLACES",
				oBuilder4.SqlSelect,
				"("+oBuilder5.SqlFullSentence+")",
				oBuilder4.SqlWhere,
				oBuilder4.SqlGroupBy,
				oBuilder4.SqlOrderBy,
				paramValues3.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());

		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return true; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return true; }

	}

	public class ReportAvailabilitySerie : ReportSerie 
	{				

		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder2 = new SqlHelper();
			SqlHelper	oBuilder3 = new SqlHelper();
			SqlHelper	oBuilder4 = new SqlHelper();
			SqlHelper	oBuilder5 = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();
			SqlHelper	oBuilder7 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues2 = new ArrayList();
			ArrayList	paramValues3 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();
			ArrayList	paramValues5 = new ArrayList();
			string strGroupbyView="";

			// SELECT
			oBuilder.AddSelectFieldLeft("hala_id",false);
			oBuilder.AddSelectFieldRight("dala_desclong",true);
			oBuilder.AddSelectFieldRight("hala_inidate",true);
			oBuilder.AddSelectFieldRight("hala_enddate",true);
			oBuilder.AddSelectFieldRight("hala_uni_id",false);
			oBuilder.AddSelectFieldRight("dalv_descshort",false);

			oBuilder7.AddSelectFieldLeft("hala_id",false);
			oBuilder7.AddSelectFieldRight("dala_desclong",true);
			oBuilder7.AddSelectFieldRight("hala_inidate",true);
			oBuilder7.AddSelectFieldRight("hala_enddate",true);
			oBuilder7.AddSelectFieldRight("hala_uni_id",false);
			oBuilder7.AddSelectFieldRight("dalv_descshort",false);


			oBuilder2.AddSelectFieldRight("SUM(SERV_HOURS) SERV_HOURS",false);
			oBuilder3.AddSelectFieldRight("ROUND(AVG(OPE_SERVICE_HOURS),2) SERV_HOURS",false);


			oBuilder4.AddSelectFieldRight("ROUND(SUM(NUM_PLACES)) NUM_PLACES",false);
			oBuilder5.AddSelectFieldRight("AVG(OPE_UNI_NUM_PLACES) NUM_PLACES",false);
			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);


			// FROM
			oBuilder.AddFromTableClause("V_ALARMS_HIS");

			oBuilder7.AddFromTableClause("V_ALARMS_HIS R");

			
			oBuilder3.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder5.AddFromTableClause("REPORT_TAKING_DATA");
		


			// WHERE (& necessary FROMs). 

			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HALA_UNI_ID = @V_ALARMS_HIS.HALA_UNI_ID@");
				paramValues1.Add(_unitId);
				oBuilder7.AddWhereAndClause("HALA_UNI_ID = @V_ALARMS_HIS.HALA_UNI_ID@");
				paramValues5.Add(_unitId);
				oBuilder3.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues2.Add(_unitId);
				oBuilder5.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues3.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("ZONE_ID = @V_ALARMS_HIS.ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder7.AddWhereAndClause("ZONE_ID = @V_ALARMS_HIS.ZONE_ID@");
				paramValues5.Add(_zoneId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues2.Add(_zoneId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues3.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("SECTOR_ID = @V_ALARMS_HIS.SECTOR_ID@");
				paramValues1.Add(_sectorId);
				oBuilder7.AddWhereAndClause("SECTOR_ID = @V_ALARMS_HIS.SECTOR_ID@");
				paramValues5.Add(_sectorId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues2.Add(_sectorId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues3.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddWhereAndClause("SECTORDEF_ID = @V_ALARMS_HIS.SECTORDEF_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder7.AddWhereAndClause("SECTORDEF_ID = @V_ALARMS_HIS.SECTORDEF_ID@");
				paramValues5.Add(_tipoSectorId);

				oBuilder3.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder3.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues2.Add(_tipoSectorId);

				oBuilder5.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder5.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues3.Add(_tipoSectorId);
			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("ROUTE_ID = @V_ALARMS_HIS.ROUTE_ID@");
				paramValues1.Add(_routeId);
				oBuilder7.AddWhereAndClause("ROUTE_ID = @V_ALARMS_HIS.ROUTE_ID@");
				paramValues5.Add(_routeId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues2.Add(_routeId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues3.Add(_routeId);
			}

			

			if (_alarmDefId != null && GroupField != ReportSerieGrouping.AlarmsDef) 
			{
				oBuilder.AddWhereAndClause("HALA_DALA_ID = @V_ALARMS_HIS.HALA_DALA_ID@");
				paramValues1.Add(_alarmDefId);
				oBuilder7.AddWhereAndClause("HALA_DALA_ID = @V_ALARMS_HIS.HALA_DALA_ID@");
				paramValues5.Add(_alarmDefId);
			}


			if (_alarmLevelId != null && GroupField != ReportSerieGrouping.AlarmsLevel) 
			{
				oBuilder.AddWhereAndClause("DALA_DALV_ID = @V_ALARMS_HIS.DALA_DALV_ID@");
				paramValues1.Add(_alarmLevelId);
				oBuilder7.AddWhereAndClause("DALA_DALV_ID = @V_ALARMS_HIS.DALA_DALV_ID@");
				paramValues5.Add(_alarmLevelId);
			}


			// Time config
			this._sqlTm.TimeField = "HALA_INIDATED";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("V_ALARMS_HIS", "HALA_INIDATED", paramValues1));
			oBuilder.AddOrderByClause("HALA_ID DESC");

			oBuilder7.AddWhereAndClause(_sqlTm.SqlWhere("V_ALARMS_HIS", "HALA_INIDATED", paramValues5));

			oBuilder3.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues2));
			oBuilder3.AddGroupByClause("OPE_DATE");


			oBuilder5.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues3));
			oBuilder5.AddGroupByClause("OPE_UNI_ID");


			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case ReportSerieGrouping.DayDef:
					oBuilder7.AddFromJoinClause("INNER JOIN DAY_DAYS_DEF D ON (TRUNC(R.HALA_INIDATED)=D.DDAY_DATE)");
					oBuilder7.AddSelectFieldLeft("DDAY_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("DDAY_CODE DESC");
					oBuilder7.AddOrderByClause("DDAY_DESCSHORT");
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					strGroupbyView="V_ALARMS_HIS_GROUP";

					break;
				case ReportSerieGrouping.Unit:
					oBuilder7.AddFromJoinClause("INNER JOIN UNITS ON (HALA_UNI_ID = UNI_ID)");
					oBuilder7.AddSelectFieldLeft("UNI_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("UNI_ID");
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					strGroupbyView="V_ALARMS_HIS_GROUP";
					break;
				case ReportSerieGrouping.Zones:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (ZONE_ID = T.GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					strGroupbyView="V_ALARMS_HIS_GROUP";

					break;
				case ReportSerieGrouping.Sectors:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (SECTOR_ID = T.GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					strGroupbyView="V_ALARMS_HIS_GROUP";
					break;
				case ReportSerieGrouping.TipoSector:
					oBuilder7.AddFromJoinClause("INNER JOIN SECTOR_DEF T2 ON (SECTORDEF_ID = T2.SED_ID)");
					oBuilder7.AddSelectFieldLeft("T2.SED_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T2.SED_ID");
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					strGroupbyView="V_ALARMS_HIS_GROUP";
					break;
				case ReportSerieGrouping.Routes:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (ROUTE_ID = GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					strGroupbyView="V_ALARMS_HIS_GROUP";
					break;
				case ReportSerieGrouping.AlarmsDef:
					oBuilder7.AddSelectFieldLeft("DALA_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("HALA_DALA_ID");
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					strGroupbyView="V_ALARMS_HIS_GROUP";
					break;
				case ReportSerieGrouping.AlarmsLevel:
					oBuilder7.AddSelectFieldLeft("DALV_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("DALA_DALV_ID");
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					strGroupbyView="V_ALARMS_HIS_GROUP";
					break;				
				case ReportSerieGrouping.Time:
					oBuilder7.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddOrderByClause("HALA_ID DESC");
					oBuilder7.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " GROUP_DESC", false);
					strGroupbyView="V_ALARMS_HIS_GROUP";
					break;
			}
			
			_dataSet = new DataSet();			
			// Run queries
			if (strGroupbyView.Length>0)
			{
				new StatisticsDB().FillDataSet(
					ref _dataSet,
					strGroupbyView,
					oBuilder7.SqlSelect,
					oBuilder7.SqlFrom,
					oBuilder7.SqlWhere,
					oBuilder7.SqlGroupBy,
					oBuilder7.SqlOrderBy,
					paramValues5.ToArray());
			}
			else
			{
				new StatisticsDB().FillDataSet(
					ref _dataSet,
					"V_ALARMS_HIS",
					oBuilder.SqlSelect,
					oBuilder.SqlFrom,
					oBuilder.SqlWhere,
					oBuilder.SqlGroupBy,
					oBuilder.SqlOrderBy,
					paramValues1.ToArray());

			}


			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_SERV_HOURS",
				oBuilder2.SqlSelect,
				"("+oBuilder3.SqlFullSentence+")",
				oBuilder2.SqlWhere,
				oBuilder2.SqlGroupBy,
				oBuilder2.SqlOrderBy,
				paramValues2.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_NUM_PLACES",
				oBuilder4.SqlSelect,
				"("+oBuilder5.SqlFullSentence+")",
				oBuilder4.SqlWhere,
				oBuilder4.SqlGroupBy,
				oBuilder4.SqlOrderBy,
				paramValues3.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());



		}


		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return true; }
		public override bool AllowsGroupByAlarmsDef()			{ return true; }
		public override bool AllowsGroupByAlarmsLevel()			{ return true; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	public class ReportFinesSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder2 = new SqlHelper();
			SqlHelper	oBuilder3 = new SqlHelper();
			SqlHelper	oBuilder4 = new SqlHelper();
			SqlHelper	oBuilder5 = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();
			SqlHelper	oBuilder7 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues2 = new ArrayList();
			ArrayList	paramValues3 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();
			ArrayList	paramValues5 = new ArrayList();
			string strGroupbyView="";

			// SELECT

			oBuilder.AddSelectFieldRight("COUNT(*) NUM_FINES",false);
			oBuilder7.AddSelectFieldLeft("COUNT(*) NUM_FINES",false);

			oBuilder2.AddSelectFieldRight("SUM(SERV_HOURS) SERV_HOURS",false);
			oBuilder3.AddSelectFieldRight("ROUND(AVG(OPE_SERVICE_HOURS),2) SERV_HOURS",false);


			oBuilder4.AddSelectFieldRight("ROUND(SUM(NUM_PLACES)) NUM_PLACES",false);
			oBuilder5.AddSelectFieldRight("AVG(OPE_UNI_NUM_PLACES) NUM_PLACES",false);
			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);


			// FROM
			oBuilder.AddFromTableClause("V_FINES_HIS");

			oBuilder7.AddFromTableClause("V_FINES_HIS R");

			
			oBuilder3.AddFromTableClause("REPORT_TAKING_DATA");
			oBuilder5.AddFromTableClause("REPORT_TAKING_DATA");
		


			// WHERE (& necessary FROMs). 

			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("HFIN_UNI_ID = @V_FINES_HIS.HFIN_UNI_ID@");
				paramValues1.Add(_unitId);
				oBuilder7.AddWhereAndClause("HFIN_UNI_ID = @V_FINES_HIS.HFIN_UNI_ID@");
				paramValues5.Add(_unitId);
				oBuilder3.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues2.Add(_unitId);
				oBuilder5.AddWhereAndClause("OPE_UNI_ID = @REPORT_TAKING_DATA.OPE_UNI_ID@");
				paramValues3.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("ZONE_ID = @V_FINES_HIS.ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder7.AddWhereAndClause("ZONE_ID = @V_FINES_HIS.ZONE_ID@");
				paramValues5.Add(_zoneId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues2.Add(_zoneId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_TAKING_DATA.OPE_GRP_ZONE_ID@");
				paramValues3.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("SECTOR_ID = @V_FINES_HIS.SECTOR_ID@");
				paramValues1.Add(_sectorId);
				oBuilder7.AddWhereAndClause("SECTOR_ID = @V_FINES_HIS.SECTOR_ID@");
				paramValues5.Add(_sectorId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues2.Add(_sectorId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ID = @REPORT_TAKING_DATA.OPE_GRP_ID@");
				paramValues3.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddWhereAndClause("SECTORDEF_ID = @V_FINES_HIS.SECTORDEF_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder7.AddWhereAndClause("SECTORDEF_ID = @V_FINES_HIS.SECTORDEF_ID@");
				paramValues5.Add(_tipoSectorId);

				oBuilder3.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder3.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues2.Add(_tipoSectorId);

				oBuilder5.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder5.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues3.Add(_tipoSectorId);
			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("ROUTE_ID = @V_FINES_HIS.ROUTE_ID@");
				paramValues1.Add(_routeId);
				oBuilder7.AddWhereAndClause("ROUTE_ID = @V_FINES_HIS.ROUTE_ID@");
				paramValues5.Add(_routeId);
				oBuilder3.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues2.Add(_routeId);
				oBuilder5.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_TAKING_DATA.OPE_GRP_ROUTE_ID@");
				paramValues3.Add(_routeId);
			}

			

			if (_fineDefId != null && GroupField != ReportSerieGrouping.FinesDef) 
			{
				oBuilder.AddWhereAndClause("HFIN_DFIN_ID = @V_FINES_HIS.HFIN_DFIN_ID@");
				paramValues1.Add(_fineDefId);
				oBuilder7.AddWhereAndClause("HFIN_DFIN_ID = @V_FINES_HIS.HFIN_DFIN_ID@");
				paramValues5.Add(_fineDefId);
			}

			if (_userId != null && GroupField != ReportSerieGrouping.User) 
			{
				oBuilder.AddWhereAndClause("HFIN_USR_ID = @V_FINES_HIS.HFIN_USR_ID@");
				paramValues1.Add(_userId);
				oBuilder7.AddWhereAndClause("HFIN_USR_ID = @V_FINES_HIS.HFIN_USR_ID@");
				paramValues5.Add(_userId);
			}


			// Time config
			this._sqlTm.TimeField = "HFIN_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhere("V_FINES_HIS", "HFIN_DATE", paramValues1));

			oBuilder7.AddWhereAndClause(_sqlTm.SqlWhere("V_FINES_HIS", "HFIN_DATE", paramValues5));

			oBuilder3.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues2));
			oBuilder3.AddGroupByClause("OPE_DATE");


			oBuilder5.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_TAKING_DATA", "OPE_DATE", paramValues3));
			oBuilder5.AddGroupByClause("OPE_UNI_ID");


			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case ReportSerieGrouping.DayDef:
					oBuilder7.AddFromJoinClause("INNER JOIN DAY_DAYS_DEF D ON (TRUNC(R.HFIN_DATE)=D.DDAY_DATE)");
					oBuilder7.AddSelectFieldLeft("DDAY_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("DDAY_CODE DESC");
					oBuilder7.AddOrderByClause("DDAY_DESCSHORT");
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					oBuilder7.AddGroupByClause("DDAY_DESCSHORT");
					oBuilder7.AddGroupByClause("DDAY_CODE");
					break;
				case ReportSerieGrouping.Unit:
					oBuilder7.AddFromJoinClause("INNER JOIN UNITS ON (HFIN_UNI_ID = UNI_ID)");
					oBuilder7.AddSelectFieldLeft("UNI_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("UNI_ID");
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					oBuilder7.AddGroupByClause("UNI_DESCSHORT");
					oBuilder7.AddGroupByClause("UNI_ID");
					break;
				case ReportSerieGrouping.Zones:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (ZONE_ID = T.GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");

					break;
				case ReportSerieGrouping.Sectors:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (SECTOR_ID = T.GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");
					break;
				case ReportSerieGrouping.TipoSector:
					oBuilder7.AddFromJoinClause("INNER JOIN SECTOR_DEF T2 ON (SECTORDEF_ID = T2.SED_ID)");
					oBuilder7.AddSelectFieldLeft("T2.SED_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T2.SED_ID");
					oBuilder7.AddGroupByClause("T2.SED_DESCLONG");
					oBuilder7.AddGroupByClause("T2.SED_ID");
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					break;
				case ReportSerieGrouping.Routes:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (ROUTE_ID = GRP_ID)");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					oBuilder7.AddGroupByClause("GRP_DESCLONG");
					oBuilder7.AddGroupByClause("GRP_ID");
					break;
				case ReportSerieGrouping.FinesDef:
					oBuilder7.AddFromJoinClause("INNER JOIN FINES_DEF T ON (HFIN_DFIN_ID = DFIN_ID)");
					oBuilder7.AddSelectFieldLeft("DFIN_DESCSHORT||'-'||DFIN_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("DFIN_ID");
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					oBuilder7.AddGroupByClause("DFIN_ID");
					oBuilder7.AddGroupByClause("DFIN_DESCSHORT");
					oBuilder7.AddGroupByClause("DFIN_DESCLONG");
					break;
				case ReportSerieGrouping.User:
					oBuilder7.AddSelectFieldLeft("TO_CHAR(HFIN_USR_ID) GROUP_DESC",false);
					oBuilder7.AddOrderByClause("HFIN_USR_ID");
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					oBuilder7.AddGroupByClause("HFIN_USR_ID");
					break;				
				case ReportSerieGrouping.Time:
					oBuilder7.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder7.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddOrderByClause("RTP_ID");
					oBuilder7.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " GROUP_DESC", false);
					strGroupbyView="V_FINES_HIS_GROUP_COUNT";
					break;
			}
			
			_dataSet = new DataSet();			
			// Run queries

			new StatisticsDB().FillDataSet(
					ref _dataSet,
					strGroupbyView,
					oBuilder7.SqlSelect,
					oBuilder7.SqlFrom,
					oBuilder7.SqlWhere,
					oBuilder7.SqlGroupBy,
					oBuilder7.SqlOrderBy,
					paramValues5.ToArray());

			new StatisticsDB().FillDataSet(
					ref _dataSet,
					"V_FINES_HIS_COUNT",
					oBuilder.SqlSelect,
					oBuilder.SqlFrom,
					oBuilder.SqlWhere,
					oBuilder.SqlGroupBy,
					oBuilder.SqlOrderBy,
					paramValues1.ToArray());


			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_SERV_HOURS",
				oBuilder2.SqlSelect,
				"("+oBuilder3.SqlFullSentence+")",
				oBuilder2.SqlWhere,
				oBuilder2.SqlGroupBy,
				oBuilder2.SqlOrderBy,
				paramValues2.ToArray());

			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_TAKING_NUM_PLACES",
				oBuilder4.SqlSelect,
				"("+oBuilder5.SqlFullSentence+")",
				oBuilder4.SqlWhere,
				oBuilder4.SqlGroupBy,
				oBuilder4.SqlOrderBy,
				paramValues3.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());



		}


		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return true; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return true; }
		public override bool AllowsGroupByUser()				{ return true; }

	}

	public class ReportControlPanelSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();
			SqlHelper	oBuilder7 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();
			ArrayList	paramValues5 = new ArrayList();
			string strGroupbyView="";


			// SELECT
			oBuilder.AddSelectFieldLeft("(round(avg(ope_uni_num_places) * count(distinct ope_uni_id))) OPE_UNI_NUM_PLACES",false);
			oBuilder.AddSelectFieldRight("round(sum(OPE_SERVICE_HOURS)/count(distinct ope_uni_id),2) OPE_SERVICE_HOURS",false);
			oBuilder.AddSelectFieldRight("count(distinct ope_uni_id) OPE_PDMS",false);			
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_ESTANCIAS) OPE_NUM_ESTANCIAS",false);
			oBuilder.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_ESTANCIAS)>0)  THEN ROUND(SUM(OPE_MINUTES)/60) ELSE 0 END OPE_MINUTES",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_CASH) OPE_NUM_OPE_CASH",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_BANKCARD) OPE_NUM_OPE_BANKCARD",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_CHIPCARD) OPE_NUM_OPE_CHIPCARD",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_MOBILE) OPE_NUM_OPE_MOBILE",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_TOTAL) OPE_NUM_OPE_TOTAL",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_FINES) OPE_NUM_FINES",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_FINES_PAID) OPE_NUM_FINES_PAID",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_ALARMS) OPE_NUM_ALARMS",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_SALES)/60) OPE_MIN_STAT_SALES",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_NO_SALES)/60) OPE_MIN_STAT_NO_SALES",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_OUT_SERV)/60) OPE_MIN_STAT_OUT_SERV",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_MAINT)/60) OPE_MIN_STAT_MAINT",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_TAKING)/60) OPE_MIN_STAT_TAKING",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_OFFLINE) OPE_NUM_OPE_OFFLINE",false);


			oBuilder7.AddSelectFieldLeft("(round(avg(ope_uni_num_places) * count(distinct ope_uni_id))) OPE_UNI_NUM_PLACES",false);
			oBuilder7.AddSelectFieldRight("round(sum(OPE_SERVICE_HOURS)/count(distinct ope_uni_id),2) OPE_SERVICE_HOURS",false);
			oBuilder7.AddSelectFieldRight("count(distinct ope_uni_id) OPE_PDMS",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_ESTANCIAS) OPE_NUM_ESTANCIAS",false);
			oBuilder7.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_ESTANCIAS)>0)  THEN ROUND(SUM(OPE_MINUTES)/60) ELSE 0 END OPE_MINUTES",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_CASH) OPE_NUM_OPE_CASH",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_BANKCARD) OPE_NUM_OPE_BANKCARD",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_CHIPCARD) OPE_NUM_OPE_CHIPCARD",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_MOBILE) OPE_NUM_OPE_MOBILE",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_TOTAL) OPE_NUM_OPE_TOTAL",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_FINES) OPE_NUM_FINES",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_FINES_PAID) OPE_NUM_FINES_PAID",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_ALARMS) OPE_NUM_ALARMS",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_SALES)/60) OPE_MIN_STAT_SALES",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_NO_SALES)/60) OPE_MIN_STAT_NO_SALES",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_OUT_SERV)/60) OPE_MIN_STAT_OUT_SERV",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_MAINT)/60) OPE_MIN_STAT_MAINT",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_TAKING)/60) OPE_MIN_STAT_TAKING",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_OFFLINE) OPE_NUM_OPE_OFFLINE",false);

			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);



			// FROM
			oBuilder.AddFromTableClause("REPORT_CONTROL_PANEL_DATA");

			oBuilder7.AddFromTableClause("REPORT_CONTROL_PANEL_DATA R");

		

			// WHERE (& necessary FROMs). 

			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("OPE_UNI_ID = @REPORT_CONTROL_PANEL_DATA.OPE_UNI_ID@");
				paramValues1.Add(_unitId);
				oBuilder7.AddWhereAndClause("OPE_UNI_ID = @REPORT_CONTROL_PANEL_DATA.OPE_UNI_ID@");
				paramValues5.Add(_unitId);

			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ZONE_ID@");
				paramValues5.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ID@");
				paramValues1.Add(_sectorId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ID@");
				paramValues5.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder7.AddFromJoinClause("INNER JOIN GROUPS F ON (OPE_GRP_ID = F.GRP_ID)");
				oBuilder7.AddWhereAndClause("F.GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues5.Add(_tipoSectorId);


			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ROUTE_ID@");
				paramValues1.Add(_routeId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ROUTE_ID@");
				paramValues5.Add(_routeId);
			}

			
			// Time config
			this._sqlTm.TimeField = "OPE_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_CONTROL_PANEL_DATA", "OPE_DATE", paramValues1));

			oBuilder7.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_CONTROL_PANEL_DATA", "OPE_DATE", paramValues5));


			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case ReportSerieGrouping.DayDef:
					oBuilder7.AddFromJoinClause("INNER JOIN DAY_DAYS_DEF D ON (R.OPE_DATE=D.DDAY_DATE)");
					oBuilder7.AddGroupByClause("DDAY_CODE");
					oBuilder7.AddGroupByClause("DDAY_DESCSHORT");
					oBuilder7.AddSelectFieldLeft("DDAY_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("DDAY_CODE DESC");
					oBuilder7.AddOrderByClause("DDAY_DESCSHORT");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";

					break;
				case ReportSerieGrouping.Unit:
					oBuilder7.AddFromJoinClause("INNER JOIN UNITS ON (OPE_UNI_ID = UNI_ID)");
					oBuilder7.AddGroupByClause("UNI_ID");
					oBuilder7.AddGroupByClause("UNI_DESCSHORT");
					oBuilder7.AddSelectFieldLeft("UNI_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("UNI_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
				case ReportSerieGrouping.Zones:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ZONE_ID = T.GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";

					break;
				case ReportSerieGrouping.Sectors:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ID = T.GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
				case ReportSerieGrouping.TipoSector:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ID = T.GRP_ID)");
					oBuilder7.AddFromJoinClause("INNER JOIN SECTOR_DEF T2 ON (T.GRP_SECTORTYPE_ID = T2.SED_ID)");
					oBuilder7.AddGroupByClause("T2.SED_ID");
					oBuilder7.AddGroupByClause("T2.SED_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T2.SED_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T2.SED_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
				case ReportSerieGrouping.Routes:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ROUTE_ID = GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
				case ReportSerieGrouping.Time:
					oBuilder7.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder7.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " GROUP_DESC", false);
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
			}
			
			
			// Run queries

			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_CONTROL_PANEL_DATA",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());


			if (strGroupbyView.Length>0)
			{
				new StatisticsDB().FillDataSet(
					ref _dataSet,
					strGroupbyView,
					oBuilder7.SqlSelect,
					oBuilder7.SqlFrom,
					oBuilder7.SqlWhere,
					oBuilder7.SqlGroupBy,
					oBuilder7.SqlOrderBy,
					paramValues5.ToArray());
			}

		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return true; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}


	public class ReportControlPanelEvSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();
			SqlHelper	oBuilder6 = new SqlHelper();
			SqlHelper	oBuilder7 = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();
			ArrayList	paramValues4 = new ArrayList();
			ArrayList	paramValues5 = new ArrayList();
			string strGroupbyView="";


			// SELECT
			oBuilder.AddSelectFieldLeft("(round(avg(ope_uni_num_places) * count(distinct ope_uni_id))) OPE_UNI_NUM_PLACES",false);
			oBuilder.AddSelectFieldRight("round(sum(OPE_SERVICE_HOURS)/count(distinct ope_uni_id),2) OPE_SERVICE_HOURS",false);
			oBuilder.AddSelectFieldRight("count(distinct ope_uni_id) OPE_PDMS",false);			
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_ESTANCIAS) OPE_NUM_ESTANCIAS",false);
			oBuilder.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_ESTANCIAS)>0)  THEN ROUND(SUM(OPE_MINUTES)/60) ELSE 0 END OPE_MINUTES",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_CASH) OPE_NUM_OPE_CASH",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_BANKCARD) OPE_NUM_OPE_BANKCARD",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_CHIPCARD) OPE_NUM_OPE_CHIPCARD",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_MOBILE) OPE_NUM_OPE_MOBILE",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_TOTAL) OPE_NUM_OPE_TOTAL",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_FINES) OPE_NUM_FINES",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_FINES_PAID) OPE_NUM_FINES_PAID",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_ALARMS) OPE_NUM_ALARMS",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_SALES)/60) OPE_MIN_STAT_SALES",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_NO_SALES)/60) OPE_MIN_STAT_NO_SALES",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_OUT_SERV)/60) OPE_MIN_STAT_OUT_SERV",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_MAINT)/60) OPE_MIN_STAT_MAINT",false);
			oBuilder.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_TAKING)/60) OPE_MIN_STAT_TAKING",false);
			oBuilder.AddSelectFieldRight("SUM(OPE_NUM_OPE_OFFLINE) OPE_NUM_OPE_OFFLINE",false);


			oBuilder7.AddSelectFieldLeft("(round(avg(ope_uni_num_places) * count(distinct ope_uni_id))) OPE_UNI_NUM_PLACES",false);
			oBuilder7.AddSelectFieldRight("round(sum(OPE_SERVICE_HOURS)/count(distinct ope_uni_id),2) OPE_SERVICE_HOURS",false);
			oBuilder7.AddSelectFieldRight("count(distinct ope_uni_id) OPE_PDMS",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_ESTANCIAS) OPE_NUM_ESTANCIAS",false);
			oBuilder7.AddSelectFieldRight("CASE WHEN (SUM(OPE_NUM_ESTANCIAS)>0)  THEN ROUND(SUM(OPE_MINUTES)/60) ELSE 0 END OPE_MINUTES",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_CASH) OPE_NUM_OPE_CASH",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_BANKCARD) OPE_NUM_OPE_BANKCARD",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_CHIPCARD) OPE_NUM_OPE_CHIPCARD",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_MOBILE) OPE_NUM_OPE_MOBILE",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_TOTAL) OPE_NUM_OPE_TOTAL",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_FINES) OPE_NUM_FINES",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_FINES_PAID) OPE_NUM_FINES_PAID",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_ALARMS) OPE_NUM_ALARMS",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_SALES)/60) OPE_MIN_STAT_SALES",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_NO_SALES)/60) OPE_MIN_STAT_NO_SALES",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_OUT_SERV)/60) OPE_MIN_STAT_OUT_SERV",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_MAINT)/60) OPE_MIN_STAT_MAINT",false);
			oBuilder7.AddSelectFieldRight("ROUND(SUM(OPE_MIN_STAT_TAKING)/60) OPE_MIN_STAT_TAKING",false);
			oBuilder7.AddSelectFieldRight("SUM(OPE_NUM_OPE_OFFLINE) OPE_NUM_OPE_OFFLINE",false);

			oBuilder6.AddSelectFieldRight("TO_CHAR(MIN(VALID_DATA_FROM),'DD/MM/YYYY HH24:MI:SS') VALID_DATA_FROM",false);



			// FROM
			oBuilder.AddFromTableClause("REPORT_CONTROL_PANEL_DATA");

			oBuilder7.AddFromTableClause("REPORT_CONTROL_PANEL_DATA R");

		

			// WHERE (& necessary FROMs). 

			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("OPE_UNI_ID = @REPORT_CONTROL_PANEL_DATA.OPE_UNI_ID@");
				paramValues1.Add(_unitId);
				oBuilder7.AddWhereAndClause("OPE_UNI_ID = @REPORT_CONTROL_PANEL_DATA.OPE_UNI_ID@");
				paramValues5.Add(_unitId);

			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ZONE_ID@");
				paramValues1.Add(_zoneId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ZONE_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ZONE_ID@");
				paramValues5.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ID@");
				paramValues1.Add(_sectorId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ID@");
				paramValues5.Add(_sectorId);
			}

			if (_tipoSectorId != null && GroupField != ReportSerieGrouping.TipoSector) 
			{
				oBuilder.AddFromJoinClause("INNER JOIN GROUPS ON (OPE_GRP_ID = GRP_ID)");
				oBuilder.AddWhereAndClause("GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues1.Add(_tipoSectorId);

				oBuilder7.AddFromJoinClause("INNER JOIN GROUPS F ON (OPE_GRP_ID = F.GRP_ID)");
				oBuilder7.AddWhereAndClause("F.GRP_SECTORTYPE_ID= @GROUPS.GRP_SECTORTYPE_ID@");
				paramValues5.Add(_tipoSectorId);


			}

			if (_routeId != null && GroupField != ReportSerieGrouping.Routes) 
			{
				oBuilder.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ROUTE_ID@");
				paramValues1.Add(_routeId);
				oBuilder7.AddWhereAndClause("OPE_GRP_ROUTE_ID = @REPORT_CONTROL_PANEL_DATA.OPE_GRP_ROUTE_ID@");
				paramValues5.Add(_routeId);
			}

			
			// Time config
			this._sqlTm.TimeField = "OPE_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_CONTROL_PANEL_DATA", "OPE_DATE", paramValues1));

			oBuilder7.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("REPORT_CONTROL_PANEL_DATA", "OPE_DATE", paramValues5));


			// GROUP BY (& necessary SELECTs & FROMs)
			switch(GroupField) 
			{
				case ReportSerieGrouping.DayDef:
					oBuilder7.AddFromJoinClause("INNER JOIN DAY_DAYS_DEF D ON (R.OPE_DATE=D.DDAY_DATE)");
					oBuilder7.AddGroupByClause("DDAY_CODE");
					oBuilder7.AddGroupByClause("DDAY_DESCSHORT");
					oBuilder7.AddSelectFieldLeft("DDAY_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("DDAY_CODE DESC");
					oBuilder7.AddOrderByClause("DDAY_DESCSHORT");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";

					break;
				case ReportSerieGrouping.Unit:
					oBuilder7.AddFromJoinClause("INNER JOIN UNITS ON (OPE_UNI_ID = UNI_ID)");
					oBuilder7.AddGroupByClause("UNI_ID");
					oBuilder7.AddGroupByClause("UNI_DESCSHORT");
					oBuilder7.AddSelectFieldLeft("UNI_DESCSHORT GROUP_DESC",false);
					oBuilder7.AddOrderByClause("UNI_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
				case ReportSerieGrouping.Zones:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ZONE_ID = T.GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";

					break;
				case ReportSerieGrouping.Sectors:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ID = T.GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
				case ReportSerieGrouping.TipoSector:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ID = T.GRP_ID)");
					oBuilder7.AddFromJoinClause("INNER JOIN SECTOR_DEF T2 ON (T.GRP_SECTORTYPE_ID = T2.SED_ID)");
					oBuilder7.AddGroupByClause("T2.SED_ID");
					oBuilder7.AddGroupByClause("T2.SED_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T2.SED_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T2.SED_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
				case ReportSerieGrouping.Routes:
					oBuilder7.AddFromJoinClause("INNER JOIN GROUPS T ON (OPE_GRP_ROUTE_ID = GRP_ID)");
					oBuilder7.AddGroupByClause("T.GRP_ID");
					oBuilder7.AddGroupByClause("T.GRP_DESCLONG");
					oBuilder7.AddSelectFieldLeft("T.GRP_DESCLONG GROUP_DESC",false);
					oBuilder7.AddOrderByClause("T.GRP_ID");
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
				case ReportSerieGrouping.Time:
					oBuilder7.AddGroupByClause(this._sqlTm.SqlGroupBy);
					oBuilder7.AddGroupByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddOrderByClause(this._sqlTm.SqlOrderBy);
					oBuilder7.AddSelectFieldLeft(this._sqlTm.SqlGroupBy + " GROUP_DESC", false);
					strGroupbyView="V_REPORT_CONTROL_PANEL_GROUP";
					break;
			}
			
			
			// Run queries

			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_CONTROL_PANEL_DATA",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());

			paramValues4.Add(_sqlTm.EndDate);
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_VALID_DATA_FROM_CHAR",
				oBuilder6.SqlSelect,
				"(select valid_data_from from v_valid_data_from union select @V_VALID_DATA_FROM.VALID_DATA_FROM@ from dual)",
				oBuilder6.SqlWhere,
				oBuilder6.SqlGroupBy,
				oBuilder6.SqlOrderBy,
				paramValues4.ToArray());


			if (strGroupbyView.Length>0)
			{
				new StatisticsDB().FillDataSet(
					ref _dataSet,
					strGroupbyView,
					oBuilder7.SqlSelect,
					oBuilder7.SqlFrom,
					oBuilder7.SqlWhere,
					oBuilder7.SqlGroupBy,
					oBuilder7.SqlOrderBy,
					paramValues5.ToArray());
			}

		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return true; }
		public override bool AllowsGroupByRoute()				{ return true; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return true; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	public class ReportCollectionTicketSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();

			// SELECT
			oBuilder.AddSelectFieldLeft("col_id",true);
			oBuilder.AddSelectFieldLeft("col_num",true);
			oBuilder.AddSelectFieldLeft("col_uni_id",true);
			oBuilder.AddSelectFieldLeft("col_uni_desclong",true);
			oBuilder.AddSelectFieldLeft("col_zone",true);
			oBuilder.AddSelectFieldLeft("col_sector",true);
			oBuilder.AddSelectFieldRight("col_date",true);
			oBuilder.AddSelectFieldRight("col_date_format",true);
			oBuilder.AddSelectFieldRight("col_cash_total_ops",true);
			oBuilder.AddSelectFieldRight("col_crcard_total_ops",true);
			oBuilder.AddSelectFieldRight("col_chcard_total_ops",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_q1",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_v1",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_t1",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_q2",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_v2",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_t2",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_q3",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_v3",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_t3",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_q4",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_v4",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_t4",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_q5",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_v5",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_t5",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_q6",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_v6",true);
			oBuilder.AddSelectFieldRight("col_cash_coins_t6",true);
			oBuilder.AddSelectFieldRight("col_cash_total_num",false);
			oBuilder.AddSelectFieldRight("col_crcard_total",true);
			oBuilder.AddSelectFieldRight("col_chcard_total",true);
			oBuilder.AddSelectFieldRight("col_back_col_total",true);
			oBuilder.AddSelectFieldRight("col_total_num",false);

			// FROM
			oBuilder.AddFromTableClause("V_REPORT_COLLECTION_TICKET");
		
			// WHERE (& necessary FROMs). 
			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("COL_UNI_ID = @V_REPORT_COLLECTION_TICKET.COL_UNI_ID@");
				paramValues1.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("COL_ZONE = @V_REPORT_COLLECTION_TICKET.COL_ZONE@");
				paramValues1.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("COL_SECTOR = @V_REPORT_COLLECTION_TICKET.COL_SECTOR@");
				paramValues1.Add(_sectorId);
			}
			
			// Time config
			this._sqlTm.TimeField = "COL_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_COLLECTION_TICKET", "COL_DATE", paramValues1));

			oBuilder.AddOrderByClause("to_number(COL_ZONE)");
			oBuilder.AddOrderByClause("to_number(COL_SECTOR)");
			oBuilder.AddOrderByClause("to_number(COL_UNI_ID)");
			oBuilder.AddOrderByClause("to_number(COL_NUM)");
			
			// Run queries
			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_COLLECTION_TICKET",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());
		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return false; }
		public override bool AllowsGroupByRoute()				{ return false; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return false; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	public class ReportCollectionSummarySerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();

			// SELECT
			oBuilder.AddSelectFieldLeft("col_id",true);
			oBuilder.AddSelectFieldLeft("col_num",true);
			oBuilder.AddSelectFieldLeft("col_uni_id",true);
			oBuilder.AddSelectFieldLeft("col_uni_desclong",true);
			oBuilder.AddSelectFieldLeft("col_zone",true);
			oBuilder.AddSelectFieldLeft("col_sector",true);
			oBuilder.AddSelectFieldRight("col_date",true);
			oBuilder.AddSelectFieldRight("col_date_format",true);
			oBuilder.AddSelectFieldRight("col_cash_total_ops",true);
			oBuilder.AddSelectFieldRight("col_crcard_total_ops",true);
			oBuilder.AddSelectFieldRight("col_chcard_total_ops",true);
			oBuilder.AddSelectFieldRight("col_total_ops",true);
			oBuilder.AddSelectFieldRight("col_cash_total",true);
			oBuilder.AddSelectFieldRight("col_crcard_total",true);
			oBuilder.AddSelectFieldRight("col_chcard_total",true);
			oBuilder.AddSelectFieldRight("col_total",true);
			oBuilder.AddSelectFieldRight("col_total_num",false);

			// FROM
			oBuilder.AddFromTableClause("V_REPORT_COLLECTION_TICKET");
		
			// WHERE (& necessary FROMs). 
			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("COL_UNI_ID = @V_REPORT_COLLECTION_TICKET.COL_UNI_ID@");
				paramValues1.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("COL_ZONE = @V_REPORT_COLLECTION_TICKET.COL_ZONE@");
				paramValues1.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("COL_SECTOR = @V_REPORT_COLLECTION_TICKET.COL_SECTOR@");
				paramValues1.Add(_sectorId);
			}
			
			// Time config
			this._sqlTm.TimeField = "COL_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_COLLECTION_TICKET", "COL_DATE", paramValues1));

			oBuilder.AddOrderByClause("to_number(COL_ZONE)");
			oBuilder.AddOrderByClause("to_number(COL_SECTOR)");
			oBuilder.AddOrderByClause("to_number(COL_UNI_ID)");
			oBuilder.AddOrderByClause("to_number(COL_NUM)");
			
			// Run queries
			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_COLLECTION_TICKET",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());
		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return false; }
		public override bool AllowsGroupByRoute()				{ return false; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return false; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}
	
	public class ReportCollectionTotalCashSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();

			// SELECT
			oBuilder.AddSelectFieldLeft("col_id",true);
			oBuilder.AddSelectFieldLeft("col_num",true);
			oBuilder.AddSelectFieldLeft("col_uni_id",true);
			oBuilder.AddSelectFieldLeft("col_uni_desclong",true);
			oBuilder.AddSelectFieldLeft("col_zone",true);
			oBuilder.AddSelectFieldLeft("col_sector",true);
			oBuilder.AddSelectFieldRight("col_date",true);
			oBuilder.AddSelectFieldRight("col_date_format",true);
			oBuilder.AddSelectFieldRight("col_cash_total_ops",false);
			oBuilder.AddSelectFieldRight("col_cash_total_num",false);

			// FROM
			oBuilder.AddFromTableClause("V_REPORT_COLLECTION_TICKET");
		
			// WHERE (& necessary FROMs). 
			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("COL_UNI_ID = @V_REPORT_COLLECTION_TICKET.COL_UNI_ID@");
				paramValues1.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("COL_ZONE = @V_REPORT_COLLECTION_TICKET.COL_ZONE@");
				paramValues1.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("COL_SECTOR = @V_REPORT_COLLECTION_TICKET.COL_SECTOR@");
				paramValues1.Add(_sectorId);
			}
			
			// Time config
			this._sqlTm.TimeField = "COL_DATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_COLLECTION_TICKET", "COL_DATE", paramValues1));

			oBuilder.AddOrderByClause("to_number(COL_ZONE)");
			oBuilder.AddOrderByClause("to_number(COL_SECTOR)");
			oBuilder.AddOrderByClause("to_number(COL_UNI_ID)");
			oBuilder.AddOrderByClause("to_number(COL_NUM)");
			
			// Run queries
			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_COLLECTION_TICKET",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());
		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return false; }
		public override bool AllowsGroupByRoute()				{ return false; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return false; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	public class ReportAlarmsSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();

			// SELECT
			oBuilder.AddSelectFieldLeft("ala_uni_id",true);
			oBuilder.AddSelectFieldLeft("ala_uni_desclong",true);
			oBuilder.AddSelectFieldLeft("ala_zone",true);
			oBuilder.AddSelectFieldLeft("ala_sector",true);
			oBuilder.AddSelectFieldLeft("ala_desclong",true);
			oBuilder.AddSelectFieldRight("ala_inidate",true);
			oBuilder.AddSelectFieldRight("ala_inidate_format",true);
			oBuilder.AddSelectFieldRight("ala_lit_lan_id",true);

			// FROM
			oBuilder.AddFromTableClause("V_REPORT_ALARMS");
		
			// WHERE (& necessary FROMs). 
			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("ALA_UNI_ID = @V_REPORT_ALARMS.ALA_UNI_ID@");
				paramValues1.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("ALA_ZONE = @V_REPORT_ALARMS.ALA_ZONE@");
				paramValues1.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("ALA_SECTOR = @V_REPORT_ALARMS.ALA_SECTOR@");
				paramValues1.Add(_sectorId);
			}
			
			// Time config
			this._sqlTm.TimeField = "ALA_INIDATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_ALARMS", "ALA_INIDATE", paramValues1));
			oBuilder.AddWhereAndClause("ALA_LIT_LAN_ID = @V_REPORT_ALARMS.ALA_LIT_LAN_ID@");
			paramValues1.Add(_litLanId);

			oBuilder.AddOrderByClause("to_number(ALA_ZONE)");
			oBuilder.AddOrderByClause("to_number(ALA_SECTOR)");
			oBuilder.AddOrderByClause("to_number(ALA_UNI_ID)");
			oBuilder.AddOrderByClause("ALA_INIDATE");
			
			// Run queries
			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_ALARMS",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());
		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return false; }
		public override bool AllowsGroupByRoute()				{ return false; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return false; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	public class ReportOperationsSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();

			// SELECT
			oBuilder.AddSelectFieldLeft("ope_id",true);
			oBuilder.AddSelectFieldLeft("ope_uni_id",true);
			oBuilder.AddSelectFieldLeft("ope_uni_desclong",true);
			oBuilder.AddSelectFieldLeft("ope_zone",true);
			oBuilder.AddSelectFieldLeft("ope_sector",true);
			oBuilder.AddSelectFieldRight("ope_movdate",true);
			oBuilder.AddSelectFieldRight("ope_movdate_format",true);
			oBuilder.AddSelectFieldRight("ope_dope_descshort",true);
			oBuilder.AddSelectFieldRight("ope_dpay_descshort",true);
			oBuilder.AddSelectFieldRight("ope_value",false);

			// FROM
			oBuilder.AddFromTableClause("V_REPORT_OPERATIONS");
		
			// WHERE (& necessary FROMs). 
			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("OPE_UNI_ID = @V_REPORT_OPERATIONS.OPE_UNI_ID@");
				paramValues1.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("OPE_ZONE = @V_REPORT_OPERATIONS.OPE_ZONE@");
				paramValues1.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("OPE_SECTOR = @V_REPORT_OPERATIONS.OPE_SECTOR@");
				paramValues1.Add(_sectorId);
			}
			
			// Time config
			this._sqlTm.TimeField = "OPE_MOVDATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_OPERATIONS", "OPE_MOVDATE", paramValues1));

			oBuilder.AddOrderByClause("to_number(OPE_ZONE)");
			oBuilder.AddOrderByClause("to_number(OPE_SECTOR)");
			oBuilder.AddOrderByClause("to_number(OPE_UNI_ID)");
			oBuilder.AddOrderByClause("OPE_MOVDATE");
			
			// Run queries
			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_OPERATIONS",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());
		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return false; }
		public override bool AllowsGroupByRoute()				{ return false; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return false; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	public class ReportOperationsPayTypeSerie : ReportSerie 
	{				
		public override void Bind(ref DataSet _dataSet)
		{
			SqlHelper	oBuilder = new SqlHelper();

			ArrayList	paramValues1 = new ArrayList();

			// SELECT
			oBuilder.AddSelectFieldLeft("ope_uni_id",true);
			oBuilder.AddSelectFieldLeft("ope_uni_desclong",true);
			oBuilder.AddSelectFieldLeft("ope_zone",true);
			oBuilder.AddSelectFieldLeft("ope_sector",true);
			oBuilder.AddSelectFieldRight("ope_dpay_descshort",true);
			oBuilder.AddSelectFieldRight("sum(ope_value) ope_value",false);
			oBuilder.AddSelectFieldRight("count(ope_value) ope_count",false);

			// FROM
			oBuilder.AddFromTableClause("V_REPORT_OPERATIONS");
		
			// WHERE (& necessary FROMs). 
			if (_unitId != null && GroupField != ReportSerieGrouping.Unit) 
			{
				oBuilder.AddWhereAndClause("OPE_UNI_ID = @V_REPORT_OPERATIONS.OPE_UNI_ID@");
				paramValues1.Add(_unitId);
			}

			if (_zoneId != null && GroupField != ReportSerieGrouping.Zones) 
			{
				oBuilder.AddWhereAndClause("OPE_ZONE = @V_REPORT_OPERATIONS.OPE_ZONE@");
				paramValues1.Add(_zoneId);
			}

			if (_sectorId != null && GroupField != ReportSerieGrouping.Sectors) 
			{
				oBuilder.AddWhereAndClause("OPE_SECTOR = @V_REPORT_OPERATIONS.OPE_SECTOR@");
				paramValues1.Add(_sectorId);
			}
			
			// Time config
			this._sqlTm.TimeField = "OPE_MOVDATE";
			oBuilder.AddWhereAndClause(_sqlTm.SqlWhereWithoutTrunc("V_REPORT_OPERATIONS", "OPE_MOVDATE", paramValues1));

			oBuilder.AddGroupByClause("OPE_ZONE");
			oBuilder.AddGroupByClause("OPE_SECTOR");
			oBuilder.AddGroupByClause("OPE_UNI_ID");
			oBuilder.AddGroupByClause("OPE_UNI_DESCLONG");
			oBuilder.AddGroupByClause("OPE_DPAY_DESCSHORT");

			oBuilder.AddOrderByClause("to_number(OPE_ZONE)");
			oBuilder.AddOrderByClause("to_number(OPE_SECTOR)");
			oBuilder.AddOrderByClause("to_number(OPE_UNI_ID)");
			oBuilder.AddOrderByClause("OPE_DPAY_DESCSHORT");
			
			// Run queries
			_dataSet = new DataSet();
			new StatisticsDB().FillDataSet(
				ref _dataSet,
				"V_REPORT_OPERATIONS",
				oBuilder.SqlSelect,
				oBuilder.SqlFrom,
				oBuilder.SqlWhere,
				oBuilder.SqlGroupBy,
				oBuilder.SqlOrderBy,
				paramValues1.ToArray());
		}

		public override bool AllowsGroupByUnit()				{ return true; }
		public override bool AllowsGroupByZone()				{ return true; }
		public override bool AllowsGroupBySector()				{ return true; }
		public override bool AllowsGroupByTipoSector()			{ return false; }
		public override bool AllowsGroupByRoute()				{ return false; }
		public override bool AllowsGroupByTime()				{ return true; }
		public override bool AllowsGroupByDaysDef()				{ return false; }
		public override bool AllowsGroupByAlarmsDef()			{ return false; }
		public override bool AllowsGroupByAlarmsLevel()			{ return false; }
		public override bool AllowsGroupByFinesDef()			{ return false; }
		public override bool AllowsGroupByUser()				{ return false; }

	}

	#endregion

}
