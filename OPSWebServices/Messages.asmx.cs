using System;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using OPS.Comm.Becs.Messages;
using OPS.Comm;
using OPS.Components.Data;
//using System.Data.OracleClient;
using Oracle.ManagedDataAccess.Client;
using System.Text;
using System.Security.Cryptography;
using OPS.Comm.Cryptography.TripleDes;
using OPS.Comm.Channel;
using System.Reflection;

namespace OPSWebServices
{
	/// <summary>
	/// Summary description for Messages.
	/// </summary>
	public class Messages : System.Web.Services.WebService
	{
		static ILogger _logger=null;

		internal const string KEY_MESSAGE_TCP_0	= "75o73K3%0=53?73*h>7*32<5";
		internal const string KEY_MESSAGE_TCP_1	= "35s03!*3!8H3j33*53)73*lf";
		internal const string KEY_MESSAGE_TCP_2	= "7*32z5$8j07!3*35f5%73(30";
		internal const string KEY_MESSAGE_TCP_3	= "*5%57*3j3!*50,73*3(65k3%";
		internal const string KEY_MESSAGE_TCP_4	= "3!*50g73*5=57*3j$8j07!3*";
		internal const string KEY_MESSAGE_TCP_5	= "j07!(*h>7*32<5y8n%=!g5/&";
		internal const string KEY_MESSAGE_TCP_6	= "!8H37t3*5*3(65k3%57*3j3!";
		internal const string KEY_MESSAGE_TCP_7	= "253)73*lf5%73(30*32z5$8j";

		public static ILogger Logger
		{
			get { return _logger; }
		}

		public Messages()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		#region M59

		[WebMethod(	EnableSession=true,
					Description="Returns CC Current Time, format:<m59 id=\"81\"></m59>")]
		public string M59(string sMessage)
		{
			string strRdo = "";

			try
			{
				
				if (Session["MessagesSession"] == null) 
				{
					MessagesSession msgSession = new MessagesSession();
					Session["MessagesSession"] = msgSession;
				}

				IRecvMessage msg = null;
				msg = MessageFactory.GetReceivedMessage(sMessage);
				msg.Session = ((MessagesSession)Session["MessagesSession"]);


				StringCollection sc = msg.Process();

				System.Collections.Specialized.StringEnumerator myEnumerator = sc.GetEnumerator();
				while ( myEnumerator.MoveNext() )
					strRdo += myEnumerator.Current + "\n";
			}
			catch( Exception e)
			{
				strRdo = e.ToString();
			}
			
			return strRdo;
		}
		
		#endregion 

		#region M3

		[WebMethod(Description="Ping/Alarms/State Notification, format: <m3 id=\"328\" dst=\"4\"><u>126</u><a>200</a><s>0</s><d>185037120305</d></m3>")]
		public string M3(string sMessage)
		{
			string strRdo = "";
			try
			{
				IRecvMessage msg = null;
				msg = MessageFactory.GetReceivedMessage(sMessage);
				StringCollection sc = msg.Process();

				System.Collections.Specialized.StringEnumerator myEnumerator = sc.GetEnumerator();
				while ( myEnumerator.MoveNext() )
					strRdo += myEnumerator.Current + "\n";
			}
			catch( Exception e)
			{
				strRdo = e.ToString();
			}
			
			return strRdo;
		}
		
		#endregion 

		#region Messages

		[WebMethod(	EnableSession=true,
			 Description="General OPS Message Interface")]
		public string Message(string sMessage)
		{
			string strRdo = "";

			try
			{

				if (_logger==null)
				{
                    System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();

					// *** TODO
					_logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);
					//               _logger = new FileLogger(LoggerSeverities.Debug, (string)appSettings.GetValue("ServiceLog", typeof(string)));
					//OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
					//OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);
					DatabaseFactory.Logger=_logger;
				}

				Logger_AddLogMessage(string.Format("Message.LogMsgDB: Empty unit. xml={0}", "Hello"), LoggerSeverities.Error);

				if (Session["MessagesSession"] == null) 
				{
					MessagesSession msgSession = new MessagesSession();
					Session["MessagesSession"] = msgSession;
				}

				IRecvMessage msg = null;
				msg = MessageFactory.GetReceivedMessage(sMessage);
				
				msg.Session = ((MessagesSession)Session["MessagesSession"]);

				StringCollection sc = msg.Process();

				System.Collections.Specialized.StringEnumerator myEnumerator = sc.GetEnumerator();
				while (myEnumerator.MoveNext())
					strRdo += myEnumerator.Current + "\n";

				try
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(sMessage);
					int iUnitId = GetUnitIdForLogging(doc);

					if (iUnitId != -1)
					{					
						System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
						string CCunitId = (string) appSettings.GetValue("UnitID", typeof(string));
						LogMsgDB(iUnitId, int.Parse(CCunitId),doc.DocumentElement.Name, sMessage, strRdo.Replace("\n",""));
					}
					else
					{
						Logger_AddLogMessage(string.Format("Message.LogMsgDB: Empty unit. xml={0}",sMessage),LoggerSeverities.Error);
					}
				}
				catch(Exception e)
				{
					Logger_AddLogException(e);
				}

			}
			catch( Exception e)
			{
				strRdo = e.ToString();
			}
			
			return strRdo;
		}
		
		#endregion 

        #region SetUnitIP

        [WebMethod(EnableSession = true,
             Description = "Set IP for an unit from the unit identifier")]
        public void SetUnitIPFromID(int iUnit,string sIP)
        {
            try
            {
                System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();			
                if (_logger == null)
                {
					// *** TODO
                    //_logger = new FileLogger(LoggerSeverities.Debug, (string)appSettings.GetValue("ServiceLog", typeof(string)));
                    //OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
                    //OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);
                    //DatabaseFactory.Logger = _logger;
                }

                OracleConnection con = null;
                OracleCommand cmd = null;
                
                try
			    {
                    string sConn = (string)appSettings.GetValue("ConnectionString", typeof(string));
				    if( sConn == null )
					    throw new Exception("No ConnectionString configuration");

				    con = new OracleConnection( sConn );
    				
				    cmd = new OracleCommand();
				    cmd.Connection = con;
				    cmd.Connection.Open();

                    cmd.CommandText = "update units set uni_date=sysdate, uni_ip='" + sIP + "' where uni_id="+iUnit.ToString();

                    cmd.ExecuteNonQuery();

			    }
			    catch(Exception e)
			    {
                    Logger_AddLogException(e);
                }
			    finally
			    {
				    if( cmd != null )
				    {
					    cmd.Dispose();
					    cmd = null;
				    }

				    if( con != null )
				    {
					    con.Close();
					    con = null;
				    }	
    			
			    }




            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }

        }

        [WebMethod(EnableSession = true,
        Description = "Set IP for an unit from the xml telegram")]
        public void SetUnitIPFromXML(XmlDocument doc, string sIP)
        {
            try
            {
                System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
                if (_logger == null)
                {
					// *** TODO
					//_logger = new FileLogger(LoggerSeverities.Debug, (string)appSettings.GetValue("ServiceLog", typeof(string)));
     //               OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
     //               OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);
     //               DatabaseFactory.Logger = _logger;
                }

                try
                {
                    int iUnitId = GetUnitId(doc);
                    if (iUnitId != -1)
                    {
                        SetUnitIPFromID(iUnitId, sIP);
                    }
                }
                catch
                {
                }
                 
            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }

        }



       private int GetUnitId(XmlDocument root)
        {
            int iUnit = -1;
            try
            {
                try
                {
                    iUnit = int.Parse(root.DocumentElement.Attributes["src"].Value);
                }
                catch
                {
                    iUnit = -1;
                }


                if (iUnit == -1)
                {
                    foreach (XmlNode n in root.DocumentElement.ChildNodes[0])
                    {
                        if (n.Name.ToLower() == "u")
                        {
                            iUnit = int.Parse(n.InnerText);
                            break;
                        }
                    }
                }
            }
            catch
            {

            }
            return iUnit;
        }
        #endregion


        #region GetUnitIP

        [WebMethod(EnableSession = true,
             Description = "Get the IP of an unit")]
        public string GetUnitIP(int iUnit)
        {
            string strRdo = "";

            try
            {
                System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
                if (_logger == null)
                {
					// *** TODO
					//_logger = new FileLogger(LoggerSeverities.Debug, (string)appSettings.GetValue("ServiceLog", typeof(string)));
     //               OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
     //               OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);
     //               DatabaseFactory.Logger = _logger;
                }

                OracleConnection con = null;
                OracleCommand cmd = null;

                try
                {
                    string sConn = (string)appSettings.GetValue("ConnectionString", typeof(string));
                    if (sConn == null)
                        throw new Exception("No ConnectionString configuration");

                    con = new OracleConnection(sConn);

                    cmd = new OracleCommand();
                    cmd.Connection = con;
                    cmd.Connection.Open();

                    cmd.CommandText = "select uni_ip from units where uni_id="+iUnit.ToString();
					// *** TODO
					//strRdo = cmd.ExecuteOracleScalar().ToString(); ;

                }
                catch (Exception e)
                {
                    Logger_AddLogException(e);
                }
                finally
                {
                    if (cmd != null)
                    {
                        cmd.Dispose();
                        cmd = null;
                    }

                    if (con != null)
                    {
                        con.Close();
                        con = null;
                    }

                }

            }
            catch (Exception e)
            {
                Logger_AddLogException(e);
            }

            return strRdo;

        }

        #endregion 

		#region InsertFineTicket

		[WebMethod(EnableSession = true,
			 Description = "Insert Fine from 3rd system")]
		public bool InsertFineTicket(long iFineID, long iFineTypeID, string sDate, long iUnitID, long iGrpID)
		{
			bool bRdo = true;

			try
			{
				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				if (_logger == null)
				{
					// *** TODO
					//_logger = new FileLogger(LoggerSeverities.Debug, (string)appSettings.GetValue("ServiceLog", typeof(string)));
					//OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
					//OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);
					//DatabaseFactory.Logger = _logger;
				}

				OracleConnection con = null;
				OracleCommand cmd = null;

				try
				{
					// Testing UnitID
					if (iUnitID <= 0)
					{
						string sUnitID = (string)appSettings.GetValue("DefaultPDAID", typeof(string));
						iUnitID = Int32.Parse(sUnitID);
					}
                    
					// Testing GrpID
					if (iGrpID <= 0)
					{
						string sGrpID = (string)appSettings.GetValue("DefaultGroupID", typeof(string));
						iGrpID = Int32.Parse(sGrpID);
					}

					string sConn = (string)appSettings.GetValue("ConnectionString", typeof(string));
					if (sConn == null)
						throw new Exception("No ConnectionString configuration");

					con = new OracleConnection(sConn);

					cmd = new OracleCommand();
					cmd.Connection = con;
					cmd.Connection.Open();

					cmd.CommandText = "select count(*) from fines where fin_id=" + iFineID.ToString();

					// *** TODO
					//if (cmd.ExecuteOracleScalar().ToString() != "0")
					//{ // update

					//	cmd.CommandText = "update FINES set FIN_DFIN_ID = " + iFineTypeID.ToString() + 
					//		",FIN_DATE = TO_DATE('" +sDate+"','HH24MISSDDMMYY')"+
					//		",FIN_UNI_ID = " +  iUnitID.ToString() +
					//		",FIN_GRP_ID_ZONE = " + iGrpID.ToString() +
					//		" where FIN_ID = " + iFineID.ToString();
    
					//}
					//else 
					//{ // insert

					//	cmd.CommandText = "insert into FINES (FIN_ID,FIN_DFIN_ID,FIN_DATE,FIN_UNI_ID,FIN_GRP_ID_ZONE) " +
					//		" values  (" + iFineID.ToString()    + "," +
					//		iFineTypeID.ToString()  + "," +
					//		"TO_DATE('" + sDate + "','HH24MISSDDMMYY')" + "," +
					//		iUnitID.ToString() + "," + 
					//		iGrpID.ToString() + ")";

					//}

                    
					cmd.ExecuteNonQuery();

				}
				catch (Exception e)
				{
					Logger_AddLogException(e);

					bRdo = false;
				}
				finally
				{
					if (cmd != null)
					{
						cmd.Dispose();
						cmd = null;
					}

					if (con != null)
					{
						con.Close();
						con = null;
					}

				}

			}
			catch (Exception e)
			{
				Logger_AddLogException(e);

				bRdo = false;
			}

			return bRdo;
		}

		[WebMethod(EnableSession = true,
			 Description = "Insert Fine from 3rd system - full Interface")]
		public string InsertFineTicketExt(string sDoc)
		{
			bool bRdo = true;

			try
			{

				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				if (_logger == null)
				{
					// *** TODO
					//_logger = new FileLogger(LoggerSeverities.Debug, (string)appSettings.GetValue("ServiceLog", typeof(string)));
					//OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
					//OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);
					//DatabaseFactory.Logger = _logger;
				}

                
				OracleConnection con = null;
				OracleCommand cmd = null;

				try
				{

					/* Column               Type            Nullable       Description
					 -----------------------------------------------------------------------------------------------------
					 FIN_ID               NUMBER          N               Fine ID (Unique identifier)
					 FIN_DFIN_ID          NUMBER          N               Fine Type ID
					 FIN_VEHICLEID        VARCHAR2(20)    Y               Plate Number (standard form)
					 FIN_MODEL            VARCHAR2(20)    Y               Vehicle model (text)
					 FIN_MANUFACTURER     VARCHAR2(20)    Y               Vehicle manufacturer (text)
					 FIN_COLOUR           VARCHAR2(20)    Y               Vehicle color (text)
					 FIN_STR_ID           NUMBER          Y               Street ID where car was parked when fined
					 FIN_STRNUMBER        NUMBER          Y               Street number where car was parked when fined
					 FIN_DATE             DATE            N               Date of fine
					 FIN_COMMENTS         VARCHAR2(255)   Y               Fine comments
					 FIN_USR_ID           NUMBER          Y               Enforcement agent ID
					 FIN_UNI_ID           NUMBER          Y               PDA ID
					 FIN_GRP_ID_ZONE      NUMBER          Y               Zone ID where car was parked when fined
					 FIN_GRP_ID_ROUTE     NUMBER          Y               Route ID of group where car was parked when fined
					 FIN_STATUS           NUMBER          Y               Status ( 30: OK; 50: Cancelled )
					 FIN_LATITUDE         NUMBER          Y               GPS latitude
					 FIN_LONGITUD         NUMBER          Y               GPS longitude */
                              

					XmlDocument doc = new XmlDocument();
					doc.LoadXml(sDoc);

					// If UNIT_ID and GROUP_ID are not present, get and use default values
					bool bDefUnitID = true;
					bool bDefGrpID = true;

					StringBuilder sbFields = new StringBuilder();
					StringBuilder sbValues = new StringBuilder();
					bool bFirst = true;
					foreach (XmlNode n in doc.DocumentElement.ChildNodes)
					{
						if (n.Name.ToLower() == "FIN_UNI_ID")
							bDefUnitID = false;
						if (n.Name.ToLower() == "FIN_GRP_ID_ZONE")
							bDefGrpID = false;

						switch (n.Name.ToUpper())
						{ 
							case "FIN_DATE": // DATEs 
								if(!bFirst)
								{
									sbFields.Append(",");
									sbValues.Append(",");
								}
								else
									bFirst = false;
                                
								sbFields.Append(n.Name);
								sbValues.Append("TO_DATE('" + n.InnerText + "','HH24MISSDDMMYY')");

								break;

							case "FIN_ID": // NUMBERs
							case "FIN_DFIN_ID":
							case "FIN_STR_ID":
							case "FIN_STRNUMBER":
							case "FIN_USR_ID":
							case "FIN_UNI_ID":
							case "FIN_GRP_ID_ZONE":
							case "FIN_GRP_ID_ROUTE":
							case "FIN_STATUS":
							case "FIN_LATITUDE":
							case "FIN_LONGITUD":
								if(!bFirst)
								{
									sbFields.Append(",");
									sbValues.Append(",");
								}
								else
									bFirst = false;
                                
								sbFields.Append(n.Name);
								sbValues.Append(n.InnerText);

								break;

							case "FIN_VEHICLEID": // VARCHAR2s( 
							case "FIN_MODEL":
							case "FIN_MANUFACTURER":
							case "FIN_COLOUR":
							case "FIN_COMMENTS":
								if (!bFirst)
								{
									sbFields.Append(",");
									sbValues.Append(",");
								}
								else
									bFirst = false;

								sbFields.Append(n.Name);
								sbValues.Append("'"+n.InnerText+"'");
								break;
						}

					}

					// Testing UnitID
					if (bDefUnitID)
					{
						string sUnitID = (string)appSettings.GetValue("DefaultPDAID", typeof(string));
						sbFields.Append(",FIN_UNI_ID");
						sbValues.Append("," + sUnitID);

					}

					// Testing GrpID
					if (bDefGrpID)
					{
						string sGrpID = (string)appSettings.GetValue("DefaultGroupID", typeof(string));
						sbFields.Append(",FIN_GRP_ID_ZONE");
						sbValues.Append("," + sGrpID);
					}

					string sConn = (string)appSettings.GetValue("ConnectionString", typeof(string));
					if (sConn == null)
						throw new Exception("No ConnectionString configuration");

					con = new OracleConnection(sConn);

					cmd = new OracleCommand();
					cmd.Connection = con;
					cmd.Connection.Open();
					// insert

					cmd.CommandText = "insert into FINES ( " + sbFields.ToString() + " )" +
						" values  (" + sbValues.ToString() + " ) ";
                    
					cmd.ExecuteNonQuery();

				}
				catch (Exception e)
				{
					Logger_AddLogException(e);

					bRdo = false;
				}
				finally
				{
					if (cmd != null)
					{
						cmd.Dispose();
						cmd = null;
					}

					if (con != null)
					{
						con.Close();
						con = null;
					}

				}

			}
			catch (Exception e)
			{
				Logger_AddLogException(e);
				bRdo = false;

			}


			return "<rdo>" + bRdo.ToString() + "</rdo>";

		}


		[WebMethod(EnableSession = true,
			 Description = "Insert Fine from 3rd system - full Interface XML")]
		public XmlDocument InsertFineTicketExtXML(XmlDocument doc)
		{
//			XmlDocument doc = new XmlDocument();
//			doc.LoadXml(InsertFineTicketExt(xmlDoc.ToString()));
//			return doc;
			// If UNIT_ID and GROUP_ID are not present, get and use default values
			bool bRdo = true;
			string sRdo = "";
			try
			{

				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				if (_logger == null)
				{
					// *** TODO
					//_logger = new FileLogger(LoggerSeverities.Debug, (string)appSettings.GetValue("ServiceLog", typeof(string)));
					//OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
					//OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);
					//DatabaseFactory.Logger = _logger;
				}

                
				OracleConnection con = null;
				OracleCommand cmd = null;

				try
				{

					/* Column               Type            Nullable       Description
					 -----------------------------------------------------------------------------------------------------
					 FIN_ID               NUMBER          N               Fine ID (Unique identifier)
					 FIN_DFIN_ID          NUMBER          N               Fine Type ID
					 FIN_VEHICLEID        VARCHAR2(20)    Y               Plate Number (standard form)
					 FIN_MODEL            VARCHAR2(20)    Y               Vehicle model (text)
					 FIN_MANUFACTURER     VARCHAR2(20)    Y               Vehicle manufacturer (text)
					 FIN_COLOUR           VARCHAR2(20)    Y               Vehicle color (text)
					 FIN_STR_ID           NUMBER          Y               Street ID where car was parked when fined
					 FIN_STRNUMBER        NUMBER          Y               Street number where car was parked when fined
					 FIN_DATE             DATE            N               Date of fine
					 FIN_COMMENTS         VARCHAR2(255)   Y               Fine comments
					 FIN_USR_ID           NUMBER          Y               Enforcement agent ID
					 FIN_UNI_ID           NUMBER          Y               PDA ID
					 FIN_GRP_ID_ZONE      NUMBER          Y               Zone ID where car was parked when fined
					 FIN_GRP_ID_ROUTE     NUMBER          Y               Route ID of group where car was parked when fined
					 FIN_STATUS           NUMBER          Y               Status ( 30: OK; 50: Cancelled )
					 FIN_LATITUDE         NUMBER          Y               GPS latitude
					 FIN_LONGITUD         NUMBER          Y               GPS longitude */
                              
			
			bool bDefUnitID = true;
			bool bDefGrpID = true;

			StringBuilder sbFields = new StringBuilder();
			StringBuilder sbValues = new StringBuilder();
			bool bFirst = true;
			foreach (XmlNode n in doc.DocumentElement.ChildNodes)
			{
				if (n.Name.ToLower() == "FIN_UNI_ID")
					bDefUnitID = false;
				if (n.Name.ToLower() == "FIN_GRP_ID_ZONE")
					bDefGrpID = false;

				switch (n.Name.ToUpper())
				{ 
					case "FIN_DATE": // DATEs 
						if(!bFirst)
						{
							sbFields.Append(",");
							sbValues.Append(",");
						}
						else
							bFirst = false;
                                
						sbFields.Append(n.Name);
						sbValues.Append("TO_DATE('" + n.InnerText + "','HH24MISSDDMMYY')");

						break;

					case "FIN_ID": // NUMBERs
					case "FIN_DFIN_ID":
					case "FIN_STR_ID":
					case "FIN_STRNUMBER":
					case "FIN_USR_ID":
					case "FIN_UNI_ID":
					case "FIN_GRP_ID_ZONE":
					case "FIN_GRP_ID_ROUTE":
					case "FIN_STATUS":
					case "FIN_LATITUDE":
					case "FIN_LONGITUD":
						if(!bFirst)
						{
							sbFields.Append(",");
							sbValues.Append(",");
						}
						else
							bFirst = false;
                                
						sbFields.Append(n.Name);
						sbValues.Append(n.InnerText);

						break;

					case "FIN_VEHICLEID": // VARCHAR2s( 
					case "FIN_MODEL":
					case "FIN_MANUFACTURER":
					case "FIN_COLOUR":
					case "FIN_COMMENTS":
						if (!bFirst)
						{
							sbFields.Append(",");
							sbValues.Append(",");
						}
						else
							bFirst = false;

						sbFields.Append(n.Name);
						sbValues.Append("'"+n.InnerText+"'");
						break;
				}

			}

			// Testing UnitID
			if (bDefUnitID)
			{
				string sUnitID = (string)appSettings.GetValue("DefaultPDAID", typeof(string));
				sbFields.Append(",FIN_UNI_ID");
				sbValues.Append("," + sUnitID);

			}

			// Testing GrpID
			if (bDefGrpID)
			{
				string sGrpID = (string)appSettings.GetValue("DefaultGroupID", typeof(string));
				sbFields.Append(",FIN_GRP_ID_ZONE");
				sbValues.Append("," + sGrpID);
			}

			string sConn = (string)appSettings.GetValue("ConnectionString", typeof(string));
			if (sConn == null)
				throw new Exception("No ConnectionString configuration");

			con = new OracleConnection(sConn);

			cmd = new OracleCommand();
			cmd.Connection = con;
			cmd.Connection.Open();
			// insert

			cmd.CommandText = "insert into FINES ( " + sbFields.ToString() + " )" +
				" values  (" + sbValues.ToString() + " ) ";
                    
			cmd.ExecuteNonQuery();

		}
		catch (Exception e)
		{
			Logger_AddLogException(e);
			sRdo = e.ToString();
			bRdo = false;
		}
		finally
		{
			if (cmd != null)
			{
				cmd.Dispose();
				cmd = null;
			}

			if (con != null)
			{
				con.Close();
				con = null;
			}

		}

	}
	catch (Exception e)
	{
		Logger_AddLogException(e);
		bRdo = false;
sRdo = e.ToString();
	}

	XmlDocument rdodoc = new XmlDocument();
	rdodoc.LoadXml("<r><rdo>" + bRdo.ToString() + "</rdo>"+"<sRdo>"+sRdo.ToString()+"</sRdo></r>");
	return rdodoc;
}

		#endregion



		public static void LogMsgDB(int iSrcUnit,int iDstUnit,string msgType,string xmlIn,string xmlOut)
		{
			try
			{
				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();			

				OracleConnection con = null;
				OracleCommand cmd = null;
                
				try
				{
					string sConn = (string)appSettings.GetValue("ConnectionString", typeof(string));
					if( sConn == null )
						throw new Exception("No ConnectionString configuration");

					con = new OracleConnection( sConn );
    				
					cmd = new OracleCommand();
					cmd.Connection = con;
					cmd.Connection.Open();

					cmd.CommandText = string.Format("insert into msgs_log (lmsg_src_uni_id,"+	
						"lmsg_dst_uni_id,lmsg_date,lmsg_type,lmsg_xml_in,"+
						"lmsg_xml_out) values ({0},{1},sysdate,'{2}','{3}','{4}')",
						iSrcUnit,iDstUnit,msgType,EncryptMessageFields(xmlIn),EncryptMessageFields(xmlOut));

					cmd.ExecuteNonQuery();

				}
				catch(Exception e)
				{
					Logger_AddLogException(e);
				}
				finally
				{
					if( cmd != null )
					{
						cmd.Dispose();
						cmd = null;
					}

					if( con != null )
					{
						con.Close();
						con = null;
					}	    			
				}
			}
			catch (Exception e)
			{
				Logger_AddLogException(e);
			}

		}

		protected static int GetUnitIdForLogging(XmlDocument root)
		{
			int iUnit = -1;
			try
			{
				try
				{
					iUnit = int.Parse(root.DocumentElement.Attributes["src"].Value);
				}
				catch
				{
					iUnit = -1;
				}


				if (iUnit == -1)
				{
					foreach (XmlNode n in root.ChildNodes[0])
					{
						if (n.Name.ToLower() == "u")
						{
							iUnit = int.Parse(n.InnerText);
							break;
						}
					}
				}
			}
			catch
			{

			}
			return iUnit;
		}

		public static string EncryptMessageFields(string strMessage)
		{

			string strRes=strMessage;
			string strToEncrypt;
			string strEncrypted;

			int startPos=strRes.IndexOf("<tn>");
			int endPos;
					
			if (startPos>0)
			{
				startPos+=4;
				endPos=strRes.IndexOf("</tn>",startPos);

				if (endPos>0)
				{
					endPos-=1;
					strToEncrypt=strRes.Substring(startPos,endPos-startPos+1);
					strEncrypted=encrypt(strToEncrypt);

					strRes =	strRes.Substring(0,startPos)+
						strEncrypted+
						strRes.Substring(endPos+1,strRes.Length-endPos-1);
				}

			}

			startPos=strRes.IndexOf("<tm>");
					
			if (startPos>0)
			{
				startPos+=4;
				endPos=strRes.IndexOf("</tm>",startPos);

				if (endPos>0)
				{
					endPos-=1;
					strToEncrypt=strRes.Substring(startPos,endPos-startPos+1);
					strEncrypted=encrypt(strToEncrypt);

					strRes =	strRes.Substring(0,startPos)+
						strEncrypted+
						strRes.Substring(endPos+1,strRes.Length-endPos-1);
				}

			}

			startPos=strRes.IndexOf("<tdd>");
					
			if (startPos>0)
			{
				startPos+=5;
				endPos=strRes.IndexOf("</tdd>",startPos);

				if (endPos>0)
				{
					endPos-=1;
					strToEncrypt=strRes.Substring(startPos,endPos-startPos+1);
					strEncrypted=encrypt(strToEncrypt);

					strRes =	strRes.Substring(0,startPos)+
						strEncrypted+
						strRes.Substring(endPos+1,strRes.Length-endPos-1);
				}

			}


			startPos=strRes.IndexOf("<rtn>");
					
			if (startPos>0)
			{
				startPos+=5;
				endPos=strRes.IndexOf("</rtn>",startPos);

				if (endPos>0)
				{
					endPos-=1;
					strToEncrypt=strRes.Substring(startPos,endPos-startPos+1);
					strEncrypted=encrypt(strToEncrypt);

					strRes =	strRes.Substring(0,startPos)+
						strEncrypted+
						strRes.Substring(endPos+1,strRes.Length-endPos-1);
				}

			}


			startPos=strRes.IndexOf("<rtm>");
					
			if (startPos>0)
			{
				startPos+=5;
				endPos=strRes.IndexOf("</rtm>",startPos);

				if (endPos>0)
				{
					endPos-=1;
					strToEncrypt=strRes.Substring(startPos,endPos-startPos+1);
					strEncrypted=encrypt(strToEncrypt);

					strRes =	strRes.Substring(0,startPos)+
						strEncrypted+
						strRes.Substring(endPos+1,strRes.Length-endPos-1);
				}

			}

			return strRes;
		}

		private static string encrypt(string strDecrypt)
		{
			string strKey = KEY_MESSAGE_TCP_5;
			byte [] byDecrypt;
			string strRes="";

			int sizeDecrypt = System.Text.Encoding.Default.GetByteCount (strDecrypt);
			byDecrypt = new byte[sizeDecrypt];	
			System.Text.Encoding.Default.GetBytes(strDecrypt,0, strDecrypt.Length,byDecrypt, 0);

			TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
			int sizeKey = System.Text.Encoding.Default.GetByteCount (strKey);
			byte [] byKey;
			byKey = new byte[sizeKey];	
			System.Text.Encoding.Default.GetBytes(strKey,0, strKey.Length,byKey, 0);
			TripleDesProvider.Mode=CipherMode.ECB;
			TripleDesProvider.Key=byKey;
			Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
					
			OPSTripleDesEncryptor OPSTripleDesEnc= new OPSTripleDesEncryptor(TripleDesProvider);
			byte [] byEncrypt;

			byEncrypt=OPSTripleDesEnc.Encriptar(byDecrypt);

			strRes = Bytes_To_HexString(byEncrypt);

			return strRes;

		}

		///
		/// Convert byte_array to string
		///
		///
		private static string Bytes_To_HexString(byte[] bytes_Input)
		{
			// convert the byte array back to a true string
			string strTemp = "";
			for (int x = 0; x <= bytes_Input.GetUpperBound(0); x++)
			{
				int number = int.Parse(bytes_Input[x].ToString());
				strTemp += number.ToString("X").PadLeft(2, '0');
			}
			// return the finished string of hex values
			return strTemp;
		}

		protected static string GetDataAsString(byte[] data, int ignoreLastBytes)
		{
			if (data.Length < ignoreLastBytes)
				return "";

			System.Text.Decoder utf8Decoder = System.Text.Encoding.UTF8.GetDecoder();
			int charCount = utf8Decoder.GetCharCount(data, 0, (data.Length  - ignoreLastBytes));
			char[] recievedChars = new char[charCount];
			utf8Decoder.GetChars(data, 0, data.Length - ignoreLastBytes, recievedChars, 0);
			String recievedString = new String(recievedChars);
			return recievedString;
		}


		protected static string GetKeyToApply(string key)
		{
			string strRes=KEY_MESSAGE_TCP_0;
			int iSum=0;
			int iMod;

			if (key.Length >= 24)
			{
				strRes = key.Substring(0,24);
			}
			else
			{
				for(int i=0; i<key.Length;i++)
				{
					iSum+=Convert.ToInt32(key[i]);
					
				}

				iMod=iSum%8;

				switch(iMod)
				{
					case 0:
						strRes=KEY_MESSAGE_TCP_0;
						break;
					case 1:
						strRes=KEY_MESSAGE_TCP_1;
						break;
					case 2:
						strRes=KEY_MESSAGE_TCP_2;
						break;
					case 3:
						strRes=KEY_MESSAGE_TCP_3;
						break;
					case 4:
						strRes=KEY_MESSAGE_TCP_4;
						break;
					case 5:
						strRes=KEY_MESSAGE_TCP_5;
						break;
					case 6:
						strRes=KEY_MESSAGE_TCP_6;
						break;
					case 7:
						strRes=KEY_MESSAGE_TCP_7;
						break;

					default:
						strRes=KEY_MESSAGE_TCP_0;
						break;
				}


				strRes = key + strRes.Substring(0,24-key.Length);


			}


			return strRes;

		}

		private static void Logger_AddLogMessage(string msg, LoggerSeverities severity)
		{
			_logger.AddLog(msg, severity);
		}

		private static void Logger_AddLogException(Exception ex)
		{
			_logger.AddLog(ex);
		}


	}
}
