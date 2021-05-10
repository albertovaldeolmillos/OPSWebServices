using System;
using System.Collections.Specialized;
using System.Xml;
//using Oracle.DataAccess.Client;
using OPS.FineLib;
using System.Globalization;


using OPS.Components.Data;
using Oracle.ManagedDataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// Class to handle de M53 message.
	/// </summary>
	internal sealed class Msg53 : MsgReceived, IRecvMessage
	{
		#region DefinedRootTag (m53)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m53"; } }
		#endregion

		int iFine=-1;			
		string [] lista_campos;
		string [] lista_valores;

		/// <summary>
		/// Constructs a new M53 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg53(XmlDocument msgXml) : base(msgXml) {}

		protected override void DoParseMessage()
		{
			int iTemp;
			string sTemp;
			int numValores=0;
			CultureInfo culture = new CultureInfo("", false);

			lista_campos= new string[_root.ChildNodes.Count];
			lista_valores= new string[_root.ChildNodes.Count];

			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
						//<f >fine</f> FIN_ID
					case "f":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_ID";
						lista_valores[numValores++]=iTemp.ToString();
						iFine=iTemp;
						break;
						
						//<y>fine type</y> FIN_DFIN_ID
					case "y":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_DFIN_ID";
						lista_valores[numValores++]=iTemp.ToString();
						break;
							
						//<rf>related fine</rf> FIN_FIN_ID
					case "rf":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_FIN_ID";
						lista_valores[numValores++]=iTemp.ToString();
						break;

						//<m>vehicle id</m> FIN_VEHICLEID
					case "m":
						sTemp=n.InnerText;
						lista_campos[numValores]="FIN_VEHICLEID";
						lista_valores[numValores++]="'"+sTemp+"'";
						break;
							
						//<j>model</j> FIN_MODEL
					case "j":
						sTemp=n.InnerText;
						lista_campos[numValores]="FIN_MODEL";
						lista_valores[numValores++]="'"+sTemp+"'";
						break;

						//<k>manufacturer</k> FIN_MANUFACTURER
					case "k":
						sTemp=n.InnerText;
						lista_campos[numValores]="FIN_MANUFACTURER";
						lista_valores[numValores++]="'"+sTemp+"'";
						break;

						//<l>colour</l> FIN_COLOUR
					case "l":
						sTemp=n.InnerText;
						lista_campos[numValores]="FIN_COLOUR";
						lista_valores[numValores++]="'"+sTemp+"'";
						break;
						//<g>group</g> FIN_GRP_ID_ZONE
					case "g":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_GRP_ID_ZONE";
						lista_valores[numValores++]=iTemp.ToString();
						break;

						//<r>route</r> FIN_GRP_ID_ROUTE
					case "r":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_GRP_ID_ROUTE";
						lista_valores[numValores++]=iTemp.ToString();
						break;						
						//<w>street</w> FIN_STR_ID
					case "w":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_STR_ID";
						lista_valores[numValores++]=iTemp.ToString();
						break;
						//<n>street number</n> FIN_STRNUMBER
					case "n":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_STRNUMBER";
						lista_valores[numValores++]=iTemp.ToString();
						break;
						//<nl>number letter</nl> FIN_LETTER
					case "nl":
						sTemp=n.InnerText;
						lista_campos[numValores]="FIN_LETTER";
						lista_valores[numValores++]="'"+sTemp+"'";
						break;
						//<p>street position</p> FIN_POSITION
					case "p":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_POSITION";
						lista_valores[numValores++]=iTemp.ToString();
						break;
						//<d>date and time</d> FIN_DATE
					case "d":

						lista_campos[numValores]="FIN_DATE";
						lista_valores[numValores++]= "TO_DATE('"+n.InnerText+"','HH24MISSDDMMYY')";

						break;
						
						//<e>comments</e> FIN_COMMENTS
					case "e":
						sTemp=n.InnerText;
						lista_campos[numValores]="FIN_COMMENTS";
						lista_valores[numValores++]="'"+sTemp+"'";
						break;
						//<z>user</z> FIN_USR_ID
					case "z":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_USR_ID";
						lista_valores[numValores++]=iTemp.ToString();
						break;

						//<u>PDA</u> FIN_UNI_ID
					case "u":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_UNI_ID";
						lista_valores[numValores++]=iTemp.ToString();
						break;
						
						//<s>status</s> FIN_STATUS

					case "s":
						iTemp=int.Parse(n.InnerText);
						lista_campos[numValores]="FIN_STATUS";
						lista_valores[numValores++]=iTemp.ToString();
						break;		
			

						//<ag>confirmation police number</ag> FIN_POLICENUMBER
					case "ag":
						sTemp=n.InnerText;
						lista_campos[numValores]="FIN_POLICENUMBER";
						lista_valores[numValores++]="'"+sTemp+"'";
						break;
						//<cd>date and time of confirmation </cd> FIN_CONFIRM_DATE
					case "cd":

						lista_campos[numValores]="FIN_CONFIRM_DATE";
						lista_valores[numValores++]= "TO_DATE('"+n.InnerText+"','HH24MISSDDMMYY')";

						break;
					case "lt":

						lista_campos[numValores]="FIN_LATITUDE";
						lista_valores[numValores++]= n.InnerText;

						break;
					case "lg":

						lista_campos[numValores]="FIN_LONGITUD";
						lista_valores[numValores++]= n.InnerText;

						break;

					case "np":

						lista_campos[numValores]="FIN_NUM_PHOTOS";
						lista_valores[numValores++]= n.InnerText;

						break;				

				}
			}
		}

		#region IRecvMessage Members

		/// <summary>
		/// Inserts a new register in the FINES table, and if everything is successful sends an ACK_PROCESSED
		/// </summary>
		/// <returns>Message to send back to the sender</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			ILogger logger = null;
			bool bExistFine=false;
			bool bRet=false;
			
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();

				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{

					if(logger != null)
					{
						if (ExistFine(oraDBConn,ref bExistFine,logger))
						{
							if (bExistFine)
							{
								bRet=UpdateFine(oraDBConn,lista_campos,lista_valores,logger);
								if (!bRet)
								{
									if(logger != null)
										logger.AddLog("[Msg53:Process]: Error: Updating Fine",LoggerSeverities.Error);
									res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
								}
							}
							else
							{
								bRet=InsertFine(oraDBConn,lista_campos,lista_valores,logger);
								if (!bRet)
								{
									if(logger != null)
										logger.AddLog("[Msg53:Process]: Error: Inserting Fine",LoggerSeverities.Error);
									res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
								}
							}						
									
							if (bRet)
							{
								CFineManager oFineManager = new CFineManager();
								oFineManager.SetLogger(logger);
								oFineManager.SetDBConnection(oraDBConn);
								oFineManager.SetFineStatus(iFine);
								res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
							}

						}
						else
						{
							if(logger != null)
								logger.AddLog("[Msg53:Process]: Error finding if fine exists", LoggerSeverities.Error);
							res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
						}

					}
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg53:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			finally
			{

				if (oraCmd!=null)
				{
					oraCmd.Dispose();
					oraCmd = null;
				}


				if (oraDBConn!=null)
				{
					oraDBConn.Close();
					oraDBConn.Dispose();
					oraDBConn = null;
				}

			}
			
			return res;
		}


		public bool ExistFine(OracleConnection BDCon, ref bool bExist,ILogger logger)
		{

			bool bRet=false;
			bExist=false;
			OracleCommand BDCommand=null;

			try
			{
				BDCommand = BDCon.CreateCommand();

				BDCommand.CommandText = "SELECT FIN_ID FROM FINES WHERE FIN_ID = " + iFine.ToString();
				OracleDataReader myReader = BDCommand.ExecuteReader();
				while (myReader.Read())
				{
					bExist=true;
				}
				myReader.Close();
				BDCommand.Dispose();
				BDCommand=null;

				bRet=true;

			}
			catch(Exception e)
			{

				logger.AddLog("[Msg53:ExistFine]: Error: "+e.Message,LoggerSeverities.Error);

				if (BDCommand!=null)
				{
					BDCommand.Dispose();
					BDCommand=null;
				}

			}
			
			return bRet;
		}

		public bool UpdateFine(OracleConnection BDCon,string[] fields, string[] values,ILogger logger)
		{

			bool bRet=false;
			OracleCommand BDCommand=null;
			string command="";

			try
			{
				BDCommand = BDCon.CreateCommand();
				
				command="UPDATE FINES SET ";

				for(int i=0;i<fields.Length;i++)
				{
					command=command + fields[i] + " = " + values[i];

					if (i<(fields.Length-1))
						command=command+", ";
				}

				command=command+" WHERE FIN_ID = " + iFine.ToString();

				logger.AddLog("[Msg53:UpdateFine]: SQL: "+command,LoggerSeverities.Info);

				BDCommand.CommandText = command;
				BDCommand.ExecuteNonQuery();
				BDCommand.Dispose();
				BDCommand=null;

				bRet=true;

			}
			catch(Exception e)
			{
				logger.AddLog("[Msg53:UpdateFine]: Error: "+e.Message,LoggerSeverities.Error);

				if (BDCommand!=null)
				{
					BDCommand.Dispose();
					BDCommand=null;
				}

			}
			
			return bRet;
		}

		public bool InsertFine(OracleConnection BDCon,string[] fields, string[] values,ILogger logger)
		{

			bool bRet=false;
			OracleCommand BDCommand=null;
			string command="";
			string list_fields="(";
			string list_values="(";

			try
			{
				BDCommand = BDCon.CreateCommand();
				
				for(int i=0;i<fields.Length;i++)
				{
					list_fields=list_fields + fields[i];
					list_values=list_values + values[i];

					if (i<(fields.Length-1))
					{
						list_fields=list_fields+", ";
						list_values=list_values+", ";
					}
					else
					{
						list_fields=list_fields+") ";
						list_values=list_values+") ";

					}
				}

				command="INSERT INTO FINES"+list_fields+"VALUES"+list_values;

				logger.AddLog("[Msg53:InsertFine]: SQL: "+command,LoggerSeverities.Info);

				BDCommand.CommandText = command;
				BDCommand.ExecuteNonQuery();
				BDCommand.Dispose();
				BDCommand=null;

				bRet=true;

			}
			catch(Exception e)
			{
				logger.AddLog("[Msg53:InsertFine]: Error: "+e.Message,LoggerSeverities.Error);

				if (BDCommand!=null)
				{
					BDCommand.Dispose();
					BDCommand=null;
				}

			}
			
			return bRet;
		}



		#endregion
	}
}
