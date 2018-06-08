using OPS.Comm;
using OPS.Comm.Becs.Messages;
using OPS.Components.Data;
using Oracle.DataAccess.Client;

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Web.Services;
using System.Xml;

namespace OPSWebServices
{
    /// <summary>
    /// Summary description for Messages.
    /// </summary>
    public class MessagesBB : System.Web.Services.WebService
	{
		static ILogger _logger=null;

		internal const string KEY_MESSAGE_0	= "75o73K3%0=53?73*h>7*32<5";
		internal const string KEY_MESSAGE_1	= "35s03!*3!8H3j33*53)73*lf";
		internal const string KEY_MESSAGE_2	= "7*32z5$8j07!3*35f5%73(30";
		internal const string KEY_MESSAGE_3	= "j07!(*h>7*32<5y8n%=!g5/&";
		internal const string KEY_MESSAGE_4	= "3!*50g73*5=57*3j$8j07!3*";
		internal const string KEY_MESSAGE_5	= "*5%37*kj3!*50,=2*3(6&k3%";
		internal const string KEY_MESSAGE_6	= "!8H37t3*5*3(65k3%57*3j3!";
		internal const string KEY_MESSAGE_7	= "253)73*lf5%73(30*32z5$8j";

		public static ILogger Logger
		{
			get { return _logger; }
		}

		public MessagesBB()
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


		#region Messages

		[WebMethod(	EnableSession=true,
			 Description="General OPS Message Interface")]
		public bool Message(byte [] msgIn, out byte [] msgOut)
		{
			bool bRdo = false;
			msgOut = null;

			try
			{
                _logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);
                ILogger logger = null;
                DatabaseFactory.Logger = _logger;
                if (logger == null)
				{
                    logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);
                    logger.AddLog("INCIO", LoggerSeverities.Debug);
                    Logger_AddLogMessage("INICIADO EL SERVICIO", LoggerSeverities.Info);
                    DatabaseFactory.Logger = _logger;
                }
				
				if (Session["MessagesSession"] == null) 
				{
					MessagesSession msgSession = new MessagesSession();
					Session["MessagesSession"] = msgSession;
				}

				string strMsgIn = DecryptMsg(msgIn);
				char [] charsTrim= new char[1];
				charsTrim[0]='\0';
				strMsgIn=strMsgIn.Trim(charsTrim);
				string strMsgOut ="";
				IRecvMessage msg = null;
				Logger_AddLogMessage("Msg In: "+strMsgIn.Replace("\n",""), LoggerSeverities.Info);

				msg = MessageFactory.GetReceivedMessage(strMsgIn);
				
				msg.Session = ((MessagesSession)Session["MessagesSession"]);

				StringCollection sc = msg.Process();



				System.Collections.Specialized.StringEnumerator myEnumerator = sc.GetEnumerator();
				while ( myEnumerator.MoveNext() )
					strMsgOut += myEnumerator.Current;

				if (strMsgOut!="")
				{
					msgOut = EncryptMsg(strMsgOut);
					Logger_AddLogMessage("Msg Out: "+strMsgOut, LoggerSeverities.Info);
					string strIP=Context.Request.UserHostAddress;
					LogMsgDB(strMsgIn,strMsgOut,strIP);
					bRdo=true;

				}
			}
			catch( Exception  err)
			{
                Logger_AddLogException(err);
				bRdo=false;
			}
			
			return bRdo;
		}
		
		#endregion 

		private static void Logger_AddLogMessage(string msg, LoggerSeverities severity)
		{
			_logger.AddLog(msg, severity);
		}

		private static void Logger_AddLogException(Exception ex)
		{
			_logger.AddLog(ex);
		}

		protected string DecryptMsg(byte [] msgIn)
		{
			TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
			int sizeKey = System.Text.Encoding.UTF8.GetByteCount (KEY_MESSAGE_5);
			byte [] byKey;
			byKey = new byte[sizeKey];	
			System.Text.Encoding.UTF8.GetBytes(KEY_MESSAGE_5,0, KEY_MESSAGE_5.Length,byKey, 0);
			TripleDesProvider.Mode=CipherMode.ECB;
			TripleDesProvider.Key=byKey;
			Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
					
			OPSTripleDesEncryptor OPSTripleDesEnc= new OPSTripleDesEncryptor(TripleDesProvider);
			byte [] byRes=null;
			byte [] byTemp;

			byTemp=OPSTripleDesEnc.Desencriptar(msgIn);

			byRes=null;
			byRes=new byte[byTemp.Length];
			Array.Copy(byTemp,0,byRes,0,byTemp.Length);
			return GetDataAsString(byRes);
		}


		protected byte [] EncryptMsg(string msgOut)
		{

			byte[] byMsgOut = new byte[System.Text.Encoding.Default.GetByteCount(msgOut.ToCharArray(), 0, msgOut.Length)];
			System.Text.Encoding.Default.GetBytes(msgOut, 0, msgOut.Length, byMsgOut, 0);
			byMsgOut = DropUnWantedChars(byMsgOut);
			TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
			int sizeKey = System.Text.Encoding.UTF8.GetByteCount (KEY_MESSAGE_5);
			byte [] byKey;
			byKey = new byte[sizeKey];	
			System.Text.Encoding.UTF8.GetBytes(KEY_MESSAGE_5,0, KEY_MESSAGE_5.Length,byKey, 0);
			TripleDesProvider.Mode=CipherMode.ECB;
			TripleDesProvider.Key=byKey;
			Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
					
			OPSTripleDesEncryptor OPSTripleDesEnc= new OPSTripleDesEncryptor(TripleDesProvider);
			byte [] byRes=null;
			byte [] byTemp;

			byTemp=OPSTripleDesEnc.Encriptar(byMsgOut);

			byRes=null;
			byRes=new byte[byTemp.Length];
			Array.Copy(byTemp,0,byRes,0,byTemp.Length);
			return byRes;
		}


		protected byte [] DropUnWantedChars(byte [] body)
		{

			byte [] byRes=null;
			byte [] byTemp=body;

			int iNewLen=0;
			byte temp;
			for (int i=0;i<byTemp.Length;i++)
			{
				if ((byTemp[i]!=10)&&(byTemp[i]!=13))
				{
					temp=byTemp[i];
					byTemp[iNewLen++]=temp;
				}
			}
			

			byRes=null;
			byRes=new byte[iNewLen];
			Array.Copy(byTemp,0,byRes,0,iNewLen);
			return byRes;
		}

		protected string GetDataAsString(byte[] data)
		{

			System.Text.Decoder utf8Decoder = System.Text.Encoding.UTF8.GetDecoder();
			int charCount = utf8Decoder.GetCharCount(data, 0, (data.Length));
			char[] recievedChars = new char[charCount];
			utf8Decoder.GetChars(data, 0, data.Length, recievedChars, 0);
			string recievedString = new String(recievedChars);
			return recievedString;
		}


		protected static void LogMsgDB(string xmlIn,string xmlOut,string sIP)
		{
			try
			{
				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();			

				OracleConnection con = null;
				OracleCommand cmd = null;
                
				try
				{
					string sConn = (string)appSettings.GetValue("ConnectionString", typeof(string));
					string CCunitId = (string) appSettings.GetValue("UnitID", typeof(string));
					if(( sConn == null ) || ( CCunitId == null ))
						throw new Exception("No ConnectionString configuration");

					XmlDocument doc = new XmlDocument();
					doc.LoadXml(xmlIn);
					int iUnitId = GetUnitId(doc);

					if (iUnitId!=-1)
					{
						con = new OracleConnection( sConn );
	    				
						cmd = new OracleCommand();
						cmd.Connection = con;
						cmd.Connection.Open();

						cmd.CommandText = string.Format("insert into msgs_log (lmsg_src_uni_id,"+	
							"lmsg_dst_uni_id,lmsg_date,lmsg_type,lmsg_xml_in,"+
							"lmsg_xml_out) values ({0},{1},sysdate,'{2}','{3}','{4}')",
							iUnitId,CCunitId,doc.DocumentElement.Name,xmlIn,xmlOut.Replace("\n",""));

						cmd.ExecuteNonQuery();

						SetUnitIPFromID(iUnitId,sIP,con);
					}

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

		protected static int GetUnitId(XmlDocument root)
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

		protected static void SetUnitIPFromID(int iUnit,string sIP, OracleConnection con)
		{
			try
			{
				OracleCommand cmd = null;
                
				try
				{
					if( con == null )
						throw new Exception("Connection is null");

    				
					cmd = new OracleCommand();
					cmd.Connection = con;

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
				}
			}
			catch (Exception e)
			{
				Logger_AddLogException(e);
			}

		}


	}
}
