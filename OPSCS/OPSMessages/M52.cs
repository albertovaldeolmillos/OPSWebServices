using System;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using OPS.Components;
using OPS.Components.Data;
using Oracle.ManagedDataAccess.Client;
//using Oracle.DataAccess.Client;

namespace OPS.Comm.Becs.Messages
{
	/// <summary>
	/// M52 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg52 : MsgReceived, IRecvMessage
	{
		private int _unitId;
		private string _table;
		private int _version;
		public const int MAX_TEL_LEN=1000; 
		private const int PMSG_DPUNI_ID_PDA=4;


		/// <summary>
		/// Constructs a new M52 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg52(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "u": _unitId = Convert.ToInt32(n.InnerText); break;
					case "x": _table = n.InnerText; break;
					case "v": _version = Convert.ToInt32(n.InnerText); break;
				}
			}
		}

		#region DefinedRootTag(m52)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m52"; } }
		#endregion

		#region IRecvMessage Members

		/// <summary>
		/// Processes the m52 message.
		/// </summary>
		/// <returns>A string collection with the data to be returned</returns>
		public System.Collections.Specialized.StringCollection Process()
		{
			StringCollection res=null;
			OracleConnection oraDBConn=null;
			OracleCommand oraCmd=null;
			OracleDataReader dr= null;
			ILogger logger = null;
			bool bRes=false;
			string dataXML="";


			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				logger = DatabaseFactory.Logger;
				System.Data.IDbConnection DBCon=d.GetNewConnection();
				oraDBConn = (OracleConnection)DBCon;
				oraDBConn.Open();

				if (oraDBConn.State == System.Data.ConnectionState.Open)
				{

					oraCmd= new OracleCommand();
					oraCmd.Connection=(OracleConnection)oraDBConn;
				
					switch(_table)
					{
						case "GROUPS":
							bRes=GetGroupsChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "GROUPS_CHILDS":
							bRes=GetGroupsChildsChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "USERS":
							bRes=GetUsersChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "GROUPS_PDA":
							bRes=GetGroupsPDAChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "FINES_DEF":
							bRes=GetFinesDefChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "STREETS":
							bRes=GetStreetsChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "VEHMANUFACTURERS":
							bRes=GetVehManufacturersChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "VEHMODELS":
							bRes=GetVehModelsChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "VEHCOLORS":
							bRes=GetVehColorsChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "STREETS_STRETCHS":
							bRes=GetStreetsStretchsChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "WORKS":
							bRes=GetWorksChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "RESIDENTS":
							bRes=GetResidentsChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "UMDM_RESERVATIONS":
							bRes=GetUMDMReservationsChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "VIPS":
							bRes=GetVIPSChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "MSGS_PREDEF":
							bRes=GetMsgsPredefChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						/*case "MSGS_SENT":
							bRes=GetMsgsSentChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;
						case "MSGS_RECEIVED":
							bRes=GetMsgsReceivedChanges(ref dataXML,ref oraDBConn,ref oraCmd,ref dr);
							break;*/
						default:
							break;
					}
				}

				if (bRes)
				{
					res = new StringCollection();
					res.Add ((new AckMessage(_msgId, dataXML)).ToString());
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg52:Process]: Error in table "+_table,LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg52:Process]: Error: "+e.Message,LoggerSeverities.Error);
				res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
			}
			finally
			{

				if (dr!=null)
				{
					dr.Close();
					dr.Dispose();
					dr = null;
				}

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

		bool GetGroupsChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format("select MGRP_OP_TYPE, "+    
													  "		  MGRP_ID, "+                 
													  "		  MGRP_DGRP_ID, "+            
													  "		  MGRP_DESCSHORT, "+          
													  "		  MGRP_DESCLONG, "+           
													  "		  MGRP_RELATED, "+            
													  "		  MGRP_POSX, "+               
													  "		  MGRP_POSY, "+               
													  "		  MGRP_PATH, "+               
													  "		  MGRP_COLOUR, "+             
													  "		  MGRP_MOB_LIT_ID, "+         
													  "		  MGRP_VERSION "+          
													  "from   GROUPS_MOV "+
													  "where  MGRP_VERSION>{0} "+
													  "order by MGRP_VERSION asc",
													  _version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MGRP_OP_TYPE"));
					int		GRP_ID = dr.GetInt32(dr.GetOrdinal("MGRP_ID"));
					int		GRP_VERSION=dr.GetInt32(dr.GetOrdinal("MGRP_VERSION"));

					int		GRP_DGRP_ID;
					string	GRP_DESCSHORT;
					string	GRP_DESCLONG;
					int		GRP_RELATED;
					int		GRP_POSX;
					int		GRP_POSY;
					string	GRP_PATH;
					string	GRP_COLOUR;
					int		GRP_MOB_LIT_ID;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<GRP_ID>"+GRP_ID.ToString()+"</GRP_ID>";
					strXMLReg += "<GRP_VERSION>"+GRP_VERSION.ToString()+"</GRP_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_DGRP_ID")))
						{
							GRP_DGRP_ID=dr.GetInt32(dr.GetOrdinal("MGRP_DGRP_ID"));
							strXMLReg += "<GRP_DGRP_ID>"+GRP_DGRP_ID.ToString()+"</GRP_DGRP_ID>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_DESCSHORT")))
						{
							GRP_DESCSHORT=dr.GetString(dr.GetOrdinal("MGRP_DESCSHORT"));
							strXMLReg += "<GRP_DESCSHORT>"+GRP_DESCSHORT.ToString()+"</GRP_DESCSHORT>";

						}						
						

						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_DESCLONG")))
						{
							GRP_DESCLONG=dr.GetString(dr.GetOrdinal("MGRP_DESCLONG"));
							strXMLReg += "<GRP_DESCLONG>"+GRP_DESCLONG.ToString()+"</GRP_DESCLONG>";

						}

						
						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_RELATED")))
						{
							GRP_RELATED=dr.GetInt32(dr.GetOrdinal("MGRP_RELATED"));
							strXMLReg += "<GRP_RELATED>"+GRP_RELATED.ToString()+"</GRP_RELATED>";

						}
						
						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_POSX")))
						{
							GRP_POSX=dr.GetInt32(dr.GetOrdinal("MGRP_POSX"));
							strXMLReg += "<GRP_POSX>"+GRP_POSX.ToString()+"</GRP_POSX>";

						}
												
						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_POSY")))
						{
							GRP_POSY=dr.GetInt32(dr.GetOrdinal("MGRP_POSY"));
							strXMLReg += "<GRP_POSY>"+GRP_POSY.ToString()+"</GRP_POSY>";

						}
												
						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_PATH")))
						{
							GRP_PATH=dr.GetString(dr.GetOrdinal("MGRP_PATH"));
							strXMLReg += "<GRP_PATH>"+GRP_PATH.ToString()+"</GRP_PATH>";

						}
						
						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_COLOUR")))
						{
							GRP_COLOUR=dr.GetString(dr.GetOrdinal("MGRP_COLOUR"));
							strXMLReg += "<GRP_COLOUR>"+GRP_COLOUR.ToString()+"</GRP_COLOUR>";

						}
						
						if (!dr.IsDBNull(dr.GetOrdinal("MGRP_MOB_LIT_ID")))
						{
							GRP_MOB_LIT_ID=dr.GetInt32(dr.GetOrdinal("MGRP_MOB_LIT_ID"));
							strXMLReg += "<GRP_MOB_LIT_ID>"+GRP_MOB_LIT_ID.ToString()+"</GRP_MOB_LIT_ID>";

						}
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetGroupsChildsChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(  "select MCGRP_OP_TYPE, "+    
														"		  MCGRP_UNIQUE_ID, "+                 
														"		  MCGRP_ID, "+            
														"		  MCGRP_TYPE, "+          
														"		  MCGRP_CHILD, "+           
														"		  MCGRP_ORDER, "+            
														"		  MCGRP_VERSION "+               
														"from   GROUPS_CHILDS_MOV "+
														"where  MCGRP_VERSION>{0} "+
														"order by MCGRP_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MCGRP_OP_TYPE"));
					int		CGRP_UNIQUE_ID = dr.GetInt32(dr.GetOrdinal("MCGRP_UNIQUE_ID"));
					int		CGRP_VERSION=dr.GetInt32(dr.GetOrdinal("MCGRP_VERSION"));

					int		CGRP_ID;
					string	CGRP_TYPE;
					int		CGRP_CHILD;
					int		CGRP_ORDER;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<CGRP_UNIQUE_ID>"+CGRP_UNIQUE_ID.ToString()+"</CGRP_UNIQUE_ID>";
					strXMLReg += "<CGRP_VERSION>"+CGRP_VERSION.ToString()+"</CGRP_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MCGRP_ID")))
						{
							CGRP_ID=dr.GetInt32(dr.GetOrdinal("MCGRP_ID"));
							strXMLReg += "<CGRP_ID>"+CGRP_ID.ToString()+"</CGRP_ID>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MCGRP_TYPE")))
						{
							CGRP_TYPE=dr.GetString(dr.GetOrdinal("MCGRP_TYPE"));
							strXMLReg += "<CGRP_TYPE>"+CGRP_TYPE.ToString()+"</CGRP_TYPE>";

						}						
						

						if (!dr.IsDBNull(dr.GetOrdinal("MCGRP_CHILD")))
						{
							CGRP_CHILD=dr.GetInt32(dr.GetOrdinal("MCGRP_CHILD"));
							strXMLReg += "<CGRP_CHILD>"+CGRP_CHILD.ToString()+"</CGRP_CHILD>";

						}

						
						if (!dr.IsDBNull(dr.GetOrdinal("MCGRP_ORDER")))
						{
							CGRP_ORDER=dr.GetInt32(dr.GetOrdinal("MCGRP_ORDER"));
							strXMLReg += "<CGRP_ORDER>"+CGRP_ORDER.ToString()+"</CGRP_ORDER>";

						}
						
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetUsersChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;



			try
			{
				oraCmd.CommandText =	string.Format(  "select MUSR_OP_TYPE, "+    
														"		  MUSR_ID, "+                 
														"		  MUSR_NAME, "+            
														"		  MUSR_SURNAME1, "+          
														"		  MUSR_SURNAME2, "+           
														"		  MUSR_ROL_ID, "+            
														"		  MUSR_LOGIN, "+            
														"		  MUSR_PASSWORD, "+            
														"		  MUSR_LAN_ID, "+            
														"		  MUSR_STATUS, "+            
														"		  MUSR_CUS_ID, "+            
														"		  MUSR_VERSION "+               
														"from   USERS_MOV "+
														"where  MUSR_VERSION>{0} "+
														"order by MUSR_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MUSR_OP_TYPE"));
					int		USR_ID = dr.GetInt32(dr.GetOrdinal("MUSR_ID"));
					int		USR_VERSION=dr.GetInt32(dr.GetOrdinal("MUSR_VERSION"));

					string	USR_NAME;
					string	USR_SURNAME1;
					string	USR_SURNAME2;
					int		USR_ROL_ID;
					string	USR_LOGIN;
					string	USR_PASSWORD;
					int		USR_LAN_ID;
					int		USR_STATUS;
					int		USR_CUS_ID;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<USR_ID>"+USR_ID.ToString()+"</USR_ID>";
					strXMLReg += "<USR_VERSION>"+USR_VERSION.ToString()+"</USR_VERSION>";


					if (OP_TYPE!="D")
					{

						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_NAME")))
						{
							USR_NAME=dr.GetString(dr.GetOrdinal("MUSR_NAME"));
							strXMLReg += "<USR_NAME>"+USR_NAME.ToString()+"</USR_NAME>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_SURNAME1")))
						{
							USR_SURNAME1=dr.GetString(dr.GetOrdinal("MUSR_SURNAME1"));
							strXMLReg += "<USR_SURNAME1>"+USR_SURNAME1.ToString()+"</USR_SURNAME1>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_SURNAME2")))
						{
							USR_SURNAME2=dr.GetString(dr.GetOrdinal("MUSR_SURNAME2"));
							strXMLReg += "<USR_SURNAME2>"+USR_SURNAME2.ToString()+"</USR_SURNAME2>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_ROL_ID")))
						{
							USR_ROL_ID=dr.GetInt32(dr.GetOrdinal("MUSR_ROL_ID"));
							strXMLReg += "<USR_ROL_ID>"+USR_ROL_ID.ToString()+"</USR_ROL_ID>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_LOGIN")))
						{
							USR_LOGIN=dr.GetString(dr.GetOrdinal("MUSR_LOGIN"));
							strXMLReg += "<USR_LOGIN>"+USR_LOGIN.ToString()+"</USR_LOGIN>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_PASSWORD")))
						{
							USR_PASSWORD=dr.GetString(dr.GetOrdinal("MUSR_PASSWORD"));
							strXMLReg += "<USR_PASSWORD>"+USR_PASSWORD.ToString()+"</USR_PASSWORD>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_LAN_ID")))
						{
							USR_LAN_ID=dr.GetInt32(dr.GetOrdinal("MUSR_LAN_ID"));
							strXMLReg += "<USR_LAN_ID>"+USR_LAN_ID.ToString()+"</USR_LAN_ID>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_STATUS")))
						{
							USR_STATUS=dr.GetInt32(dr.GetOrdinal("MUSR_STATUS"));
							strXMLReg += "<USR_STATUS>"+USR_STATUS.ToString()+"</USR_STATUS>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("MUSR_CUS_ID")))
						{
							USR_CUS_ID=dr.GetInt32(dr.GetOrdinal("MUSR_CUS_ID"));
							strXMLReg += "<USR_CUS_ID>"+USR_CUS_ID.ToString()+"</USR_CUS_ID>";

						}

						
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetGroupsPDAChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(  "select MGPDA_OP_TYPE, "+    
														"		  MGPDA_ID, "+                 
														"		  MGPDA_GRP_ID, "+            
														"		  MGPDA_GRP_ID_CHILD, "+          
														"		  MGPDA_DESC, "+           
														"		  MGPDA_VERSION "+               
														"from   GROUPS_PDA_MOV "+
														"where  MGPDA_VERSION>{0} "+
														"order by MGPDA_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MGPDA_OP_TYPE"));
					int		GPDA_ID = dr.GetInt32(dr.GetOrdinal("MGPDA_ID"));
					int		GPDA_VERSION=dr.GetInt32(dr.GetOrdinal("MGPDA_VERSION"));

					int		GPDA_GRP_ID;
					int		GPDA_GRP_ID_CHILD;
					string	GPDA_DESC;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<GPDA_ID>"+GPDA_ID.ToString()+"</GPDA_ID>";
					strXMLReg += "<GPDA_VERSION>"+GPDA_VERSION.ToString()+"</GPDA_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MGPDA_GRP_ID")))
						{
							GPDA_GRP_ID=dr.GetInt32(dr.GetOrdinal("MGPDA_GRP_ID"));
							strXMLReg += "<GPDA_GRP_ID>"+GPDA_GRP_ID.ToString()+"</GPDA_GRP_ID>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MGPDA_GRP_ID_CHILD")))
						{
							GPDA_GRP_ID_CHILD=dr.GetInt32(dr.GetOrdinal("MGPDA_GRP_ID_CHILD"));
							strXMLReg += "<GPDA_GRP_ID_CHILD>"+GPDA_GRP_ID_CHILD.ToString()+"</GPDA_GRP_ID_CHILD>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MGPDA_DESC")))
						{
							GPDA_DESC=dr.GetString(dr.GetOrdinal("MGPDA_DESC"));
							strXMLReg += "<GPDA_DESC>"+GPDA_DESC.ToString()+"</GPDA_DESC>";

						}
						
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}
 

		bool GetFinesDefChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format("select MDFIN_OP_TYPE, "+    
													  "		  MDFIN_ID, "+                 
													  "		  MDFIN_CATEGORY, "+            
													  "		  MDFIN_DESCSHORT, "+          
													  "		  MDFIN_DESCLONG, "+           
													  "		  MDFIN_VALUE, "+            
													  "		  MDFIN_STATUS, "+               
													  "		  MDFIN_SIGN, "+               
													  "		  MDFIN_PAYINPDM, "+               
													  "		  MDFIN_NUMTICKETS, "+             
													  "		  MDFIN_POSTPAYABLE, "+         
													  "		  MDFIN_FINID_OFFSET, "+         
													  "		  MDFIN_VERSION "+          
													  "from   FINES_DEF_MOV "+
													  "where  MDFIN_VERSION>{0} "+
													  "order by MDFIN_VERSION asc",
													  _version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MDFIN_OP_TYPE"));
					int		DFIN_ID = dr.GetInt32(dr.GetOrdinal("MDFIN_ID"));
					int		DFIN_VERSION=dr.GetInt32(dr.GetOrdinal("MDFIN_VERSION"));

					string	DFIN_CATEGORY;
					string	DFIN_DESCSHORT;
					string	DFIN_DESCLONG;
					int		DFIN_VALUE;
					int		DFIN_STATUS;
					int		DFIN_SIGN;
					int		DFIN_PAYINPDM;
					int		DFIN_NUMTICKETS;
					int		DFIN_POSTPAYABLE;
					int     DFIN_FINID_OFFSET;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<DFIN_ID>"+DFIN_ID.ToString()+"</DFIN_ID>";
					strXMLReg += "<DFIN_VERSION>"+DFIN_VERSION.ToString()+"</DFIN_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_CATEGORY")))
						{
							DFIN_CATEGORY=dr.GetString(dr.GetOrdinal("MDFIN_CATEGORY"));
							strXMLReg += "<DFIN_CATEGORY>"+DFIN_CATEGORY.ToString()+"</DFIN_CATEGORY>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_DESCSHORT")))
						{
							DFIN_DESCSHORT=dr.GetString(dr.GetOrdinal("MDFIN_DESCSHORT"));
							strXMLReg += "<DFIN_DESCSHORT>"+DFIN_DESCSHORT.ToString()+"</DFIN_DESCSHORT>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_DESCLONG")))
						{
							DFIN_DESCLONG=dr.GetString(dr.GetOrdinal("MDFIN_DESCLONG"));
							strXMLReg += "<DFIN_DESCLONG>"+DFIN_DESCLONG.ToString()+"</DFIN_DESCLONG>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_VALUE")))
						{
							DFIN_VALUE=dr.GetInt32(dr.GetOrdinal("MDFIN_VALUE"));
							strXMLReg += "<DFIN_VALUE>"+DFIN_VALUE.ToString()+"</DFIN_VALUE>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_STATUS")))
						{
							DFIN_STATUS=dr.GetInt32(dr.GetOrdinal("MDFIN_STATUS"));
							strXMLReg += "<DFIN_STATUS>"+DFIN_STATUS.ToString()+"</DFIN_STATUS>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_SIGN")))
						{
							DFIN_SIGN=dr.GetInt32(dr.GetOrdinal("MDFIN_SIGN"));
							strXMLReg += "<DFIN_SIGN>"+DFIN_SIGN.ToString()+"</DFIN_SIGN>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_PAYINPDM")))
						{
							DFIN_PAYINPDM=dr.GetInt32(dr.GetOrdinal("MDFIN_PAYINPDM"));
							strXMLReg += "<DFIN_PAYINPDM>"+DFIN_PAYINPDM.ToString()+"</DFIN_PAYINPDM>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_NUMTICKETS")))
						{
							DFIN_NUMTICKETS=dr.GetInt32(dr.GetOrdinal("MDFIN_NUMTICKETS"));
							strXMLReg += "<DFIN_NUMTICKETS>"+DFIN_NUMTICKETS.ToString()+"</DFIN_NUMTICKETS>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_POSTPAYABLE")))
						{
							DFIN_POSTPAYABLE=dr.GetInt32(dr.GetOrdinal("MDFIN_POSTPAYABLE"));
							strXMLReg += "<DFIN_POSTPAYABLE>"+DFIN_POSTPAYABLE.ToString()+"</DFIN_POSTPAYABLE>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MDFIN_FINID_OFFSET")))
						{
							DFIN_FINID_OFFSET=dr.GetInt32(dr.GetOrdinal("MDFIN_FINID_OFFSET"));
							strXMLReg += "<DFIN_FINID_OFFSET>"+DFIN_FINID_OFFSET.ToString()+"</DFIN_FINID_OFFSET>";

						}					
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}

		bool GetStreetsChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select MSTR_OP_TYPE, "+    
														"		  MSTR_ID, "+                 
														"		  MSTR_DESC, "+            
														"		  MSTR_MIN, "+          
														"		  MSTR_MAX, "+           
														"		  MSTR_PROVINCIA, "+            
														"		  MSTR_MUNICIPIO, "+               
														"		  MSTR_TIPOVIA, "+               
														"		  MSTR_VERSION "+          
														"from   STREETS_MOV "+
														"where  MSTR_VERSION>{0} "+
														"order by MSTR_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MSTR_OP_TYPE"));
					int		STR_ID = dr.GetInt32(dr.GetOrdinal("MSTR_ID"));
					int		STR_VERSION=dr.GetInt32(dr.GetOrdinal("MSTR_VERSION"));

					string	STR_DESC;
					int		STR_MIN;
					int		STR_MAX;
					int		STR_PROVINCIA;
					int		STR_MUNICIPIO;
					string	STR_TIPOVIA;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<STR_ID>"+STR_ID.ToString()+"</STR_ID>";
					strXMLReg += "<STR_VERSION>"+STR_VERSION.ToString()+"</STR_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MSTR_DESC")))
						{
							STR_DESC=dr.GetString(dr.GetOrdinal("MSTR_DESC"));
							strXMLReg += "<STR_DESC>"+STR_DESC.ToString()+"</STR_DESC>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MSTR_MIN")))
						{
							STR_MIN=dr.GetInt32(dr.GetOrdinal("MSTR_MIN"));
							strXMLReg += "<STR_MIN>"+STR_MIN.ToString()+"</STR_MIN>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MSTR_MAX")))
						{
							STR_MAX=dr.GetInt32(dr.GetOrdinal("MSTR_MAX"));
							strXMLReg += "<STR_MAX>"+STR_MAX.ToString()+"</STR_MAX>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MSTR_PROVINCIA")))
						{
							STR_PROVINCIA=dr.GetInt32(dr.GetOrdinal("MSTR_PROVINCIA"));
							strXMLReg += "<STR_PROVINCIA>"+STR_PROVINCIA.ToString()+"</STR_PROVINCIA>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MSTR_MUNICIPIO")))
						{
							STR_MUNICIPIO=dr.GetInt32(dr.GetOrdinal("MSTR_MUNICIPIO"));
							strXMLReg += "<STR_MUNICIPIO>"+STR_MUNICIPIO.ToString()+"</STR_MUNICIPIO>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MSTR_TIPOVIA")))
						{
							STR_TIPOVIA=dr.GetString(dr.GetOrdinal("MSTR_TIPOVIA"));
							strXMLReg += "<STR_TIPOVIA>"+STR_TIPOVIA.ToString()+"</STR_TIPOVIA>";

						}
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetVehManufacturersChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select MVMAN_OP_TYPE, "+    
														"		  MVMAN_ID, "+                 
														"		  MVMAN_DESCSHORT, "+            
														"		  MVMAN_DESCLONG, "+          
														"		  MVMAN_VERSION "+          
														"from   VEHMANUFACTURERS_MOV "+
														"where  MVMAN_VERSION>{0} "+
														"order by MVMAN_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MVMAN_OP_TYPE"));
					int		VMAN_ID = dr.GetInt32(dr.GetOrdinal("MVMAN_ID"));
					int		VMAN_VERSION=dr.GetInt32(dr.GetOrdinal("MVMAN_VERSION"));

					string	VMAN_DESCSHORT;
					string	VMAN_DESCLONG;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<VMAN_ID>"+VMAN_ID.ToString()+"</VMAN_ID>";
					strXMLReg += "<VMAN_VERSION>"+VMAN_VERSION.ToString()+"</VMAN_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MVMAN_DESCSHORT")))
						{
							VMAN_DESCSHORT=dr.GetString(dr.GetOrdinal("MVMAN_DESCSHORT"));
							strXMLReg += "<VMAN_DESCSHORT>"+VMAN_DESCSHORT.ToString()+"</VMAN_DESCSHORT>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVMAN_DESCLONG")))
						{
							VMAN_DESCLONG=dr.GetString(dr.GetOrdinal("MVMAN_DESCLONG"));
							strXMLReg += "<VMAN_DESCLONG>"+VMAN_DESCLONG.ToString()+"</VMAN_DESCLONG>";

						}					
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetVehModelsChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select MVMOD_OP_TYPE, "+    
														"		  MVMOD_ID, "+                 
														"		  MVMOD_DESCSHORT, "+            
														"		  MVMOD_DESCLONG, "+          
														"		  MVMOD_VMAN_ID, "+          
														"		  MVMOD_VERSION "+          
														"from   VEHMODELS_MOV "+
														"where  MVMOD_VERSION>{0} "+
														"order by MVMOD_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MVMOD_OP_TYPE"));
					int		VMOD_ID = dr.GetInt32(dr.GetOrdinal("MVMOD_ID"));
					int		VMOD_VERSION=dr.GetInt32(dr.GetOrdinal("MVMOD_VERSION"));

					string	VMOD_DESCSHORT;
					string	VMOD_DESCLONG;
					int		VMOD_VMAN_ID;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<VMOD_ID>"+VMOD_ID.ToString()+"</VMOD_ID>";
					strXMLReg += "<VMOD_VERSION>"+VMOD_VERSION.ToString()+"</VMOD_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MVMOD_DESCSHORT")))
						{
							VMOD_DESCSHORT=dr.GetString(dr.GetOrdinal("MVMOD_DESCSHORT"));
							strXMLReg += "<VMOD_DESCSHORT>"+VMOD_DESCSHORT.ToString()+"</VMOD_DESCSHORT>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVMOD_DESCLONG")))
						{
							VMOD_DESCLONG=dr.GetString(dr.GetOrdinal("MVMOD_DESCLONG"));
							strXMLReg += "<VMOD_DESCLONG>"+VMOD_DESCLONG.ToString()+"</VMOD_DESCLONG>";

						}					
						if (!dr.IsDBNull(dr.GetOrdinal("MVMOD_VMAN_ID")))
						{
							VMOD_VMAN_ID=dr.GetInt32(dr.GetOrdinal("MVMOD_VMAN_ID"));
							strXMLReg += "<VMOD_VMAN_ID>"+VMOD_VMAN_ID.ToString()+"</VMOD_VMAN_ID>";

						}						
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetVehColorsChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select MVCOL_OP_TYPE, "+    
														"		  MVCOL_ID, "+                 
														"		  MVCOL_DESCSHORT, "+            
														"		  MVCOL_DESCLONG, "+          
														"		  MVCOL_VERSION "+          
														"from   VEHCOLORS_MOV "+
														"where  MVCOL_VERSION>{0} "+
														"order by MVCOL_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MVCOL_OP_TYPE"));
					int		VCOL_ID = dr.GetInt32(dr.GetOrdinal("MVCOL_ID"));
					int		VCOL_VERSION=dr.GetInt32(dr.GetOrdinal("MVCOL_VERSION"));

					string	VCOL_DESCSHORT;
					string	VCOL_DESCLONG;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<VCOL_ID>"+VCOL_ID.ToString()+"</VCOL_ID>";
					strXMLReg += "<VCOL_VERSION>"+VCOL_VERSION.ToString()+"</VCOL_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MVCOL_DESCSHORT")))
						{
							VCOL_DESCSHORT=dr.GetString(dr.GetOrdinal("MVCOL_DESCSHORT"));
							strXMLReg += "<VCOL_DESCSHORT>"+VCOL_DESCSHORT.ToString()+"</VCOL_DESCSHORT>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVCOL_DESCLONG")))
						{
							VCOL_DESCLONG=dr.GetString(dr.GetOrdinal("MVCOL_DESCLONG"));
							strXMLReg += "<VCOL_DESCLONG>"+VCOL_DESCLONG.ToString()+"</VCOL_DESCLONG>";

						}					
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetStreetsStretchsChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				// JTZ 01/07/2015 -- CHAPUZA:	Se ha hecho un join con la tabla STREET_STRETCHS para poder sacar el campo SS_EXT_ID
				//								para Hondarribia (PDAS del VC) sin tener que modificar la tabla y los triggers de 
				//								todas las localidades
				oraCmd.CommandText =	string.Format(	"select MSS_OP_TYPE, "+    
														"		  MSS_ID, "+                 
														"		  MSS_GRP_ID, "+            
														"		  MSS_STR_ID, "+          
														"		  MSS_STR_SS_ID, "+           
														"		  MSS_EVEN, "+            
														"		  MSS_STR_ID_DESDE, "+               
														"		  MSS_STR_ID_HASTA, "+
														"		  SS_EXT_ID, "+
														"		  MSS_VERSION "+          
														"from   STREETS_STRETCHS_MOV LEFT OUTER JOIN STREETS_STRETCHS ON MSS_ID = SS_ID "+
														"where  MSS_VERSION>{0} "+
														"order by MSS_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MSS_OP_TYPE"));
					int		SS_ID = dr.GetInt32(dr.GetOrdinal("MSS_ID"));
					int		SS_VERSION=dr.GetInt32(dr.GetOrdinal("MSS_VERSION"));

					int		SS_GRP_ID;
					int		SS_STR_ID;
					int		SS_STR_SS_ID;
					int		SS_EVEN;
					int		SS_STR_ID_DESDE;
					int		SS_STR_ID_HASTA;
					string	SS_DESC;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<SS_ID>"+SS_ID.ToString()+"</SS_ID>";
					strXMLReg += "<SS_VERSION>"+SS_VERSION.ToString()+"</SS_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MSS_GRP_ID")))
						{
							SS_GRP_ID=dr.GetInt32(dr.GetOrdinal("MSS_GRP_ID"));
							strXMLReg += "<SS_GRP_ID>"+SS_GRP_ID.ToString()+"</SS_GRP_ID>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MSS_STR_ID")))
						{
							SS_STR_ID=dr.GetInt32(dr.GetOrdinal("MSS_STR_ID"));
							strXMLReg += "<SS_STR_ID>"+SS_STR_ID.ToString()+"</SS_STR_ID>";

						}					
	
						if (!dr.IsDBNull(dr.GetOrdinal("MSS_STR_SS_ID")))
						{
							SS_STR_SS_ID=dr.GetInt32(dr.GetOrdinal("MSS_STR_SS_ID"));
							strXMLReg += "<SS_STR_SS_ID>"+SS_STR_SS_ID.ToString()+"</SS_STR_SS_ID>";

						}
				
						if (!dr.IsDBNull(dr.GetOrdinal("MSS_EVEN")))
						{
							SS_EVEN=dr.GetInt32(dr.GetOrdinal("MSS_EVEN"));
							strXMLReg += "<SS_EVEN>"+SS_EVEN.ToString()+"</SS_EVEN>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MSS_STR_ID_DESDE")))
						{
							SS_STR_ID_DESDE=dr.GetInt32(dr.GetOrdinal("MSS_STR_ID_DESDE"));
							strXMLReg += "<SS_STR_ID_DESDE>"+SS_STR_ID_DESDE.ToString()+"</SS_STR_ID_DESDE>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MSS_STR_ID_HASTA")))
						{
							SS_STR_ID_HASTA=dr.GetInt32(dr.GetOrdinal("MSS_STR_ID_HASTA"));
							strXMLReg += "<SS_STR_ID_HASTA>"+SS_STR_ID_HASTA.ToString()+"</SS_STR_ID_HASTA>";

						}
						
						if (!dr.IsDBNull(dr.GetOrdinal("SS_EXT_ID")))
						{
							SS_DESC=dr.GetString(dr.GetOrdinal("SS_EXT_ID"));
							strXMLReg += "<SS_DESC>"+SS_DESC.ToString()+"</SS_DESC>";

						}

					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetWorksChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select MWORK_OP_TYPE, "+    
														"		  MWORK_ID, "+                 
														"		  MWORK_SS_ID, "+            
														"		  MWORK_PDA_ID, "+          
														"		  MWORK_USR_ID, "+           
														"		  MWORK_UNI_ID, "+            
														"		  MWORK_NUM_PARK_SPACES, "+               
														"		  MWORK_REMARKS, "+               
														"		  MWORK_LICEN_NUMBER, "+               
														"		  MWORK_LICEN_CORP, "+               
														"		  TO_CHAR(MWORK_INI_DATE,'YYYYMMDDHH24MISS') MWORK_INI_DATE, "+               
														"		  TO_CHAR(MWORK_END_DATE,'YYYYMMDDHH24MISS') MWORK_END_DATE, "+               
														"		  MWORK_VERSION "+          
														"from   WORKS_MOV "+
														"where  MWORK_VERSION>{0} "+
														"order by MWORK_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MWORK_OP_TYPE"));
					string	WORK_PDA_ID = dr.GetString(dr.GetOrdinal("MWORK_PDA_ID"));
					int		WORK_VERSION=dr.GetInt32(dr.GetOrdinal("MWORK_VERSION"));

					int		WORK_ID;
					int		WORK_SS_ID;
					int		WORK_USR_ID;
					int		WORK_UNI_ID;
					int		WORK_NUM_PARK_SPACES;
					string  WORK_REMARKS;
					string	WORK_INI_DATE;
					string	WORK_END_DATE;
					string  WORK_LICEN_NUMBER;
					string	WORK_LICEN_CORP;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<WORK_PDA_ID>"+WORK_PDA_ID.ToString()+"</WORK_PDA_ID>";
					strXMLReg += "<WORK_VERSION>"+WORK_VERSION.ToString()+"</WORK_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_ID")))
						{
							WORK_ID=dr.GetInt32(dr.GetOrdinal("MWORK_ID"));
							strXMLReg += "<WORK_ID>"+WORK_ID.ToString()+"</WORK_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_SS_ID")))
						{
							WORK_SS_ID=dr.GetInt32(dr.GetOrdinal("MWORK_SS_ID"));
							strXMLReg += "<WORK_SS_ID>"+WORK_SS_ID.ToString()+"</WORK_SS_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_USR_ID")))
						{
							WORK_USR_ID=dr.GetInt32(dr.GetOrdinal("MWORK_USR_ID"));
							strXMLReg += "<WORK_USR_ID>"+WORK_USR_ID.ToString()+"</WORK_USR_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_UNI_ID")))
						{
							WORK_UNI_ID=dr.GetInt32(dr.GetOrdinal("MWORK_UNI_ID"));
							strXMLReg += "<WORK_UNI_ID>"+WORK_UNI_ID.ToString()+"</WORK_UNI_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_NUM_PARK_SPACES")))
						{
							WORK_NUM_PARK_SPACES=dr.GetInt32(dr.GetOrdinal("MWORK_NUM_PARK_SPACES"));
							strXMLReg += "<WORK_NUM_PARK_SPACES>"+WORK_NUM_PARK_SPACES.ToString()+"</WORK_NUM_PARK_SPACES>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_REMARKS")))
						{
							WORK_REMARKS=dr.GetString(dr.GetOrdinal("MWORK_REMARKS"));
							strXMLReg += "<WORK_REMARKS>"+WORK_REMARKS.ToString()+"</WORK_REMARKS>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_LICEN_NUMBER")))
						{
							WORK_LICEN_NUMBER=dr.GetString(dr.GetOrdinal("MWORK_LICEN_NUMBER"));
							strXMLReg += "<WORK_LICEN_NUMBER>"+WORK_LICEN_NUMBER.ToString()+"</WORK_LICEN_NUMBER>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_LICEN_CORP")))
						{
							WORK_LICEN_CORP=dr.GetString(dr.GetOrdinal("MWORK_LICEN_CORP"));
							strXMLReg += "<WORK_LICEN_CORP>"+WORK_LICEN_CORP.ToString()+"</WORK_LICEN_CORP>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_INI_DATE")))
						{
							WORK_INI_DATE=dr.GetString(dr.GetOrdinal("MWORK_INI_DATE"));
							strXMLReg += "<WORK_INI_DATE>"+WORK_INI_DATE.ToString()+"</WORK_INI_DATE>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MWORK_END_DATE")))
						{
							WORK_END_DATE=dr.GetString(dr.GetOrdinal("MWORK_END_DATE"));
							strXMLReg += "<WORK_END_DATE>"+WORK_END_DATE.ToString()+"</WORK_END_DATE>";

						}
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool GetResidentsChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select MRES_OP_TYPE, "+   
														"		  MRES_ID, "+                  
														"		  MRES_VEHICLEID, "+                 
														"		  MRES_GRP_ID, "+            
														"		  MRES_DART_ID, "+          
														"		  MRES_VERSION "+          
														"from   RESIDENTS_MOV "+
														"where  MRES_VERSION>{0} "+
														"order by MRES_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MRES_OP_TYPE"));
					int		RES_ID = dr.GetInt32(dr.GetOrdinal("MRES_ID"));
					int		RES_VERSION=dr.GetInt32(dr.GetOrdinal("MRES_VERSION"));

					string	RES_VEHICLEID;
					int		RES_GRP_ID;
					int		RES_DART_ID;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<RES_ID>"+RES_ID.ToString()+"</RES_ID>";
					strXMLReg += "<RES_VERSION>"+RES_VERSION.ToString()+"</RES_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MRES_VEHICLEID")))
						{
							RES_VEHICLEID=dr.GetString(dr.GetOrdinal("MRES_VEHICLEID"));
							strXMLReg += "<RES_VEHICLEID>"+RES_VEHICLEID.ToString()+"</RES_VEHICLEID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MRES_GRP_ID")))
						{
							RES_GRP_ID=dr.GetInt32(dr.GetOrdinal("MRES_GRP_ID"));
							strXMLReg += "<RES_GRP_ID>"+RES_GRP_ID.ToString()+"</RES_GRP_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MRES_DART_ID")))
						{
							RES_DART_ID=dr.GetInt32(dr.GetOrdinal("MRES_DART_ID"));
							strXMLReg += "<RES_DART_ID>"+RES_DART_ID.ToString()+"</RES_DART_ID>";

						}
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}

		bool GetUMDMReservationsChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select URM_OP_TYPE, "+   
					"		  URM_ID, "+                  
					"		  URM_US_ID, "+     
					"		  URM_UM_ID, "+     
					"		  URM_SLOT_NUM, "+ 
					"		  URM_SLOT_INI, "+  
					"		  URM_SLOT_END, "+  
					"		  TO_CHAR(URM_DATE_INI,'HH24MISSDDMMYY')  URM_DATE_INI, "+ 
					"		  TO_CHAR(URM_DATE_END ,'HH24MISSDDMMYY') URM_DATE_END, "+ 
					"		  URM_UTS_ID, "+    
					"		  URM_ENABLED, "+   
					"		  TO_CHAR(URM_UPDDATE  ,'HH24MISSDDMMYY') URM_MOVDATE, "+ 
					"		  URM_UU_ID, "+     
					"		  URM_VERSION "+     					
					"from   UMDM_RESERVATIONS_MOV "+
					"where  URM_VERSION>{0} and "+
					"		URM_US_ID = {1} "+
					"order by URM_VERSION asc",
					_version, _unitId);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("URM_OP_TYPE"));
					int		UR_ID = dr.GetInt32(dr.GetOrdinal("URM_ID"));
					int		UR_VERSION=dr.GetInt32(dr.GetOrdinal("URM_VERSION"));
					int 	UR_US_ID=dr.GetInt32(dr.GetOrdinal("URM_US_ID"));

					int 	UR_UM_ID;
					int		UR_SLOT_NUM;
					int		UR_SLOT_INI;
					int		UR_SLOT_END;
					string	UR_DATE_INI;
					string	UR_DATE_END;
					int		UR_UTS_ID;
					int		UR_ENABLED;
					string  UR_MOVDATE;
					int		UR_UU_ID;



					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<UR_ID>"+UR_ID.ToString()+"</UR_ID>";
					strXMLReg += "<UR_VERSION>"+UR_VERSION.ToString()+"</UR_VERSION>";
					strXMLReg += "<UR_US_ID>"+UR_US_ID.ToString()+"</UR_US_ID>";

					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("URM_UM_ID")))
						{
							UR_UM_ID=dr.GetInt32(dr.GetOrdinal("URM_UM_ID"));
							strXMLReg += "<UR_UM_ID>"+UR_UM_ID.ToString()+"</UR_UM_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("URM_SLOT_NUM")))
						{
							UR_SLOT_NUM=dr.GetInt32(dr.GetOrdinal("URM_SLOT_NUM"));
							strXMLReg += "<UR_SLOT_NUM>"+UR_SLOT_NUM.ToString()+"</UR_SLOT_NUM>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("URM_SLOT_INI")))
						{
							UR_SLOT_INI=dr.GetInt32(dr.GetOrdinal("URM_SLOT_INI"));
							strXMLReg += "<UR_SLOT_INI>"+UR_SLOT_INI.ToString()+"</UR_SLOT_INI>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("URM_SLOT_END")))
						{
							UR_SLOT_END=dr.GetInt32(dr.GetOrdinal("URM_SLOT_END"));
							strXMLReg += "<UR_SLOT_END>"+UR_SLOT_END.ToString()+"</UR_SLOT_END>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("URM_DATE_INI")))
						{
							UR_DATE_INI=dr.GetString(dr.GetOrdinal("URM_DATE_INI"));
							strXMLReg += "<UR_DATE_INI>"+UR_DATE_INI.ToString()+"</UR_DATE_INI>";

						}					

						if (!dr.IsDBNull(dr.GetOrdinal("URM_DATE_END")))
						{
							UR_DATE_END=dr.GetString(dr.GetOrdinal("URM_DATE_END"));
							strXMLReg += "<UR_DATE_END>"+UR_DATE_END.ToString()+"</UR_DATE_END>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("URM_UTS_ID")))
						{
							UR_UTS_ID=dr.GetInt32(dr.GetOrdinal("URM_UTS_ID"));
							strXMLReg += "<UR_UTS_ID>"+UR_UTS_ID.ToString()+"</UR_UTS_ID>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("URM_ENABLED")))
						{
							UR_ENABLED=((dr.GetString(dr.GetOrdinal("URM_ENABLED"))=="Y")?1:0);
							strXMLReg += "<UR_ENABLED>"+UR_ENABLED.ToString()+"</UR_ENABLED>";

						}


						if (!dr.IsDBNull(dr.GetOrdinal("URM_MOVDATE")))
						{
							UR_MOVDATE=dr.GetString(dr.GetOrdinal("URM_MOVDATE"));
							strXMLReg += "<UR_MOVDATE>"+UR_MOVDATE.ToString()+"</UR_MOVDATE>";

						}

						if (!dr.IsDBNull(dr.GetOrdinal("URM_UU_ID")))
						{
							UR_UU_ID=dr.GetInt32(dr.GetOrdinal("URM_UU_ID"));
							strXMLReg += "<UR_UU_ID>"+UR_UU_ID.ToString()+"</UR_UU_ID>";

						}

					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch (Exception e)
			{
				bRes=false;
			}

			return bRes;
			
		}

		bool GetVIPSChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{

				oraCmd.CommandText =	string.Format(	"select MVIP_OP_TYPE, "+   
														"		  MVIP_ID, "+                  
														"		  MVIP_VEHICLEID, "+                 
														"		  MVIP_GRP_ID, "+            
														"		  MVIP_DART_ID, "+  
														"		  MVIP_TEXT, "+  
														"		  MVIP_DAYOFWEEK, "+  
														"		  MVIP_INIHOUR, "+  
														"		  MVIP_INIMINUTE, "+  
														"		  MVIP_ENDHOUR, "+  
														"		  MVIP_ENDMINUTE, "+  
														"		  TO_CHAR(MVIP_INIDATE,'YYYYMMDDHH24MISS') MVIP_INIDATE, "+  
														"		  TO_CHAR(MVIP_ENDDATE,'YYYYMMDDHH24MISS') MVIP_ENDDATE, "+  
														"		  MVIP_VERSION "+          
														"from   VIPS_MOV "+
														"where  MVIP_VERSION>{0} "+
														"order by MVIP_VERSION asc",
														_version);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MVIP_OP_TYPE"));
					int		VIP_ID = dr.GetInt32(dr.GetOrdinal("MVIP_ID"));
					int		VIP_VERSION=dr.GetInt32(dr.GetOrdinal("MVIP_VERSION"));

					string	VIP_VEHICLEID;
					int		VIP_GRP_ID;
					int		VIP_DART_ID;
					string	VIP_TEXT;
					string	VIP_DAYOFWEEK;
					int		VIP_INIHOUR;
					int		VIP_INIMINUTE;
					int		VIP_ENDHOUR;
					int		VIP_ENDMINUTE;
					string	VIP_INIDATE;
					string	VIP_ENDDATE;


					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<VIP_ID>"+VIP_ID.ToString()+"</VIP_ID>";
					strXMLReg += "<VIP_VERSION>"+VIP_VERSION.ToString()+"</VIP_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_VEHICLEID")))
						{
							VIP_VEHICLEID=dr.GetString(dr.GetOrdinal("MVIP_VEHICLEID"));
							strXMLReg += "<VIP_VEHICLEID>"+VIP_VEHICLEID.ToString()+"</VIP_VEHICLEID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_GRP_ID")))
						{
							VIP_GRP_ID=dr.GetInt32(dr.GetOrdinal("MVIP_GRP_ID"));
							strXMLReg += "<VIP_GRP_ID>"+VIP_GRP_ID.ToString()+"</VIP_GRP_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_DART_ID")))
						{
							VIP_DART_ID=dr.GetInt32(dr.GetOrdinal("MVIP_DART_ID"));
							strXMLReg += "<VIP_DART_ID>"+VIP_DART_ID.ToString()+"</VIP_DART_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_TEXT")))
						{
							VIP_TEXT=dr.GetString(dr.GetOrdinal("MVIP_TEXT"));
							strXMLReg += "<VIP_TEXT>"+VIP_TEXT.ToString()+"</VIP_TEXT>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_DAYOFWEEK")))
						{
							VIP_DAYOFWEEK=dr.GetString(dr.GetOrdinal("MVIP_DAYOFWEEK"));
							strXMLReg += "<VIP_DAYOFWEEK>"+VIP_DAYOFWEEK.ToString()+"</VIP_DAYOFWEEK>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_INIHOUR")))
						{
							VIP_INIHOUR=dr.GetInt32(dr.GetOrdinal("MVIP_INIHOUR"));
							strXMLReg += "<VIP_INIHOUR>"+VIP_INIHOUR.ToString()+"</VIP_INIHOUR>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_INIMINUTE")))
						{
							VIP_INIMINUTE=dr.GetInt32(dr.GetOrdinal("MVIP_INIMINUTE"));
							strXMLReg += "<VIP_INIMINUTE>"+VIP_INIMINUTE.ToString()+"</VIP_INIMINUTE>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_ENDHOUR")))
						{
							VIP_ENDHOUR=dr.GetInt32(dr.GetOrdinal("MVIP_ENDHOUR"));
							strXMLReg += "<VIP_ENDHOUR>"+VIP_ENDHOUR.ToString()+"</VIP_ENDHOUR>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_ENDMINUTE")))
						{
							VIP_ENDMINUTE=dr.GetInt32(dr.GetOrdinal("MVIP_ENDMINUTE"));
							strXMLReg += "<VIP_ENDMINUTE>"+VIP_ENDMINUTE.ToString()+"</VIP_ENDMINUTE>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_INIDATE")))
						{
							VIP_INIDATE=dr.GetString(dr.GetOrdinal("MVIP_INIDATE"));
							strXMLReg += "<VIP_INIDATE>"+VIP_INIDATE.ToString()+"</VIP_INIDATE>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVIP_ENDDATE")))
						{
							VIP_ENDDATE=dr.GetString(dr.GetOrdinal("MVIP_ENDDATE"));
							strXMLReg += "<VIP_ENDDATE>"+VIP_ENDDATE.ToString()+"</VIP_ENDDATE>";

						}					
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}

				}

			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}

		bool GetMsgsPredefChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{

				oraCmd.CommandText =	string.Format(	"select MPMSG_OP_TYPE, "+   
					"		  MPMSG_ID, "+                  
					"		  MPMSG_DPUNI_ID, "+                 
					"		  MPMSG_TEXT, "+            
					"		  MPMSG_VERSION "+          
					"from   MSGS_PREDEF_MOV "+
					"where  MPMSG_VERSION>{0} and MPMSG_DPUNI_ID={1} "+  //PMSG_DPUNI_ID=4 -> PDAs
					"order by MPMSG_VERSION asc",
					_version,PMSG_DPUNI_ID_PDA);
				
				dr = oraCmd.ExecuteReader();	


				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MPMSG_OP_TYPE"));
					int		MPMSG_ID = dr.GetInt32(dr.GetOrdinal("MPMSG_ID"));
					int		MPMSG_VERSION=dr.GetInt32(dr.GetOrdinal("MPMSG_VERSION"));

					int		MPMSG_DPUNI_ID;
					string	MPMSG_TEXT;

					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<PMSG_ID>"+MPMSG_ID.ToString()+"</PMSG_ID>";
					strXMLReg += "<PMSG_VERSION>"+MPMSG_VERSION.ToString()+"</PMSG_VERSION>";


					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MPMSG_DPUNI_ID")))
						{
							MPMSG_DPUNI_ID=dr.GetInt32(dr.GetOrdinal("MPMSG_DPUNI_ID"));
							strXMLReg += "<PMSG_DPUNI_ID>"+MPMSG_DPUNI_ID.ToString()+"</PMSG_DPUNI_ID>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MPMSG_TEXT")))
						{
							MPMSG_TEXT=dr.GetString(dr.GetOrdinal("MPMSG_TEXT"));
							strXMLReg += "<PMSG_TEXT>"+MPMSG_TEXT.ToString()+"</PMSG_TEXT>";

						}
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}
				}
			}
			catch
			{
				bRes=false;
			}

			return bRes;
		}

		/*bool GetMsgsSentChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select MSMSG_OP_TYPE, "+   
					"		  MSMSG_ID, "+                  
					"		  MSMSG_USR_ID, "+                 
					"		  MSMSG_TEXT, "+            
					"		  TO_CHAR(MSMSG_DATE,'YYYYMMDDHH24MISS') MSMSG_DATE, "+
					"		  MSMSG_READ, "+
					"		  MSMSG_VERSION "+          
					"from   MSGS_SENT_MOV "+
					"where  MSMSG_VERSION>{0} "+
					"order by MSMSG_VERSION asc",
					_version);
				
				dr = oraCmd.ExecuteReader();	

				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MSMSG_OP_TYPE"));
					int		MSMSG_ID = dr.GetInt32(dr.GetOrdinal("MSMSG_ID"));
					int		MSMSG_VERSION=dr.GetInt32(dr.GetOrdinal("MSMSG_VERSION"));

					int		MSMSG_USR_ID;
					string	MSMSG_TEXT;
					string	MSMSG_DATE;
					int		MSMSG_READ;

					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<SMSG_ID>"+MSMSG_ID.ToString()+"</SMSG_ID>";
					strXMLReg += "<SMSG_VERSION>"+MSMSG_VERSION.ToString()+"</SMSG_VERSION>";

					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MSMSG_USR_ID")))
						{
							MSMSG_USR_ID=dr.GetInt32(dr.GetOrdinal("MSMSG_USR_ID"));
							strXMLReg += "<SMSG_USR_ID>"+MSMSG_USR_ID.ToString()+"</SMSG_USR_ID>";
						}
						if (!dr.IsDBNull(dr.GetOrdinal("MSMSG_TEXT")))
						{
							MSMSG_TEXT=dr.GetString(dr.GetOrdinal("MSMSG_TEXT"));
							strXMLReg += "<SMSG_TEXT>"+MSMSG_TEXT.ToString()+"</SMSG_TEXT>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MSMSG_DATE")))
						{
							MSMSG_DATE=dr.GetString(dr.GetOrdinal("MSMSG_DATE"));
							strXMLReg += "<SMSG_DATE>"+MSMSG_DATE.ToString()+"</SMSG_DATE>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MSMSG_READ")))
						{
							MSMSG_READ=dr.GetInt32(dr.GetOrdinal("MSMSG_READ"));
							strXMLReg += "<SMSG_READ>"+MSMSG_READ.ToString()+"</SMSG_READ>";
						}
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}
				}
			}
			catch
			{
				bRes=false;
			}

			return bRes;
		}

		bool GetMsgsReceivedChanges(ref string strXML, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref OracleDataReader dr)
		{
			bool bRes=true;
			strXML="";
			string strXMLReg;
			bool bEnd=false;

			try
			{
				oraCmd.CommandText =	string.Format(	"select MVMSG_OP_TYPE, "+   
					"		  MVMSG_ID, "+                  
					"		  MVMSG_USR_ID, "+                 
					"		  MVMSG_TEXT, "+            
					"		  TO_CHAR(MVMSG_DATE,'YYYYMMDDHH24MISS') MVMSG_DATE, "+
					"		  MVMSG_READ, "+
					"		  MVMSG_VERSION "+          
					"from   MSGS_RECEIVED_MOV "+
					"where  MVMSG_VERSION>{0} "+
					"order by MVMSG_VERSION asc",
					_version);
				
				dr = oraCmd.ExecuteReader();	

				while ((dr.Read())&&(!bEnd))
				{
					strXMLReg = "<r>";
					string	OP_TYPE = dr.GetString(dr.GetOrdinal("MVMSG_OP_TYPE"));
					int		MVMSG_ID = dr.GetInt32(dr.GetOrdinal("MVMSG_ID"));
					int		MVMSG_VERSION=dr.GetInt32(dr.GetOrdinal("MVMSG_VERSION"));

					int		MVMSG_USR_ID;
					string	MVMSG_TEXT;
					string	MVMSG_DATE;
					int		MVMSG_READ;

					strXMLReg += "<OP_TYPE>"+OP_TYPE.ToString()+"</OP_TYPE>";
					strXMLReg += "<VMSG_ID>"+MVMSG_ID.ToString()+"</VMSG_ID>";
					strXMLReg += "<VMSG_VERSION>"+MVMSG_VERSION.ToString()+"</VMSG_VERSION>";

					if (OP_TYPE!="D")
					{
						if (!dr.IsDBNull(dr.GetOrdinal("MVMSG_USR_ID")))
						{
							MVMSG_USR_ID=dr.GetInt32(dr.GetOrdinal("MVMSG_USR_ID"));
							strXMLReg += "<VMSG_USR_ID>"+MVMSG_USR_ID.ToString()+"</VMSG_USR_ID>";
						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVMSG_TEXT")))
						{
							MVMSG_TEXT=dr.GetString(dr.GetOrdinal("MVMSG_TEXT"));
							strXMLReg += "<VMSG_TEXT>"+MVMSG_TEXT.ToString()+"</VMSG_TEXT>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVMSG_DATE")))
						{
							MVMSG_DATE=dr.GetString(dr.GetOrdinal("MVMSG_DATE"));
							strXMLReg += "<VMSG_DATE>"+MVMSG_DATE.ToString()+"</VMSG_DATE>";

						}
						if (!dr.IsDBNull(dr.GetOrdinal("MVMSG_READ")))
						{
							MVMSG_READ=dr.GetInt32(dr.GetOrdinal("MVMSG_READ"));
							strXMLReg += "<VMSG_READ>"+MVMSG_READ.ToString()+"</VMSG_READ>";
						}
					}

					strXMLReg+="</r>";

					bEnd=(strXML.Length+strXMLReg.Length>MAX_TEL_LEN);
					if (!bEnd)
					{
						strXML += strXMLReg;
					}
				}
			}
			catch
			{
				bRes=false;
			}

			return bRes;
		}*/

		#endregion
	
	}	
}
