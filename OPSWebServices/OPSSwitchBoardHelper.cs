using System;
using System.Data;
using System.Data.OracleClient;
using System.Configuration;
using System.Collections;
using System.Security.Cryptography;
using System.Xml;
using System.Web;

namespace OPSWebServices
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public sealed class COPSSwitchBoardHelper
	{

		const int DEFAULT_SECTOR_ID= 60001;
		const int DEFAULT_ARTICLE_DEF = 4;
		internal const string KEY_MESSAGE_TCP_0	= "75o73K3%0=53?73*h>7*32<5";
		internal const string KEY_MESSAGE_TCP_1	= "35s03!*3!8H3j33*53)73*lf";
		internal const string KEY_MESSAGE_TCP_2	= "7*32z5$8j07!3*35f5%73(30";
		internal const string KEY_MESSAGE_TCP_3	= "*5%57*3j3!*50,73*3(65k3%";
		internal const string KEY_MESSAGE_TCP_4	= "3!*50g73*5=57*3j$8j07!3*";
		internal const string KEY_MESSAGE_TCP_5	= "j07!(*h>7*32<5y8n%=!g5/&";
		internal const string KEY_MESSAGE_TCP_6	= "!8H37t3*5*3(65k3%57*3j3!";
		internal const string KEY_MESSAGE_TCP_7	= "253)73*lf5%73(30*32z5$8j";



		static public int GetSectorId(int iZone,int iSector)
		{
			int iSectorId=0;

			if ((iSector==1)||(iSector==2))
			{
				iSectorId=60001+2*(iZone-1)+(iSector-1);
			}
			else if ((iSector==3)&&(iZone==8))
			{
				iSector=60025;
			}

			return iSectorId;
		}


		static public int GetZoneSector(int iSectorId, ref int iZone,ref int iSector)
		{

			if ((iSectorId>=60001)&&(iSectorId<60025))
			{
				iZone=(iSectorId-60000+1)/2;
				iSector = ((iSectorId-60000+1)%2)+1;
			}
			else if (iSectorId==60025)
			{
				iZone=8;
				iSector=3;
			}
			else
			{
				iZone=-1;
				iSector=-1;
			}


			return iSectorId;
		}

		static public string GetSectorList(int iZone)
		{
			string stRdo;

			stRdo = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
			stRdo += "<r>";

			if ((iZone>=1)&&(iZone<=12))
			{
				stRdo += "<s>1</s>";
				stRdo += "<s>2</s>";


				if (iZone==8)
				{
					stRdo += "<s>3</s>";
				}			
			}			

			stRdo += "</r>";

			return stRdo;
		}

		static public bool GetResidentData( string strPlate, string strFechaOp, ref string stRdo,ref string stError)  
		{
			/*
			$this->m_fullURL  = $this->m_baseURL."/OPSSwitchBoardResident.aspx"."?"."Plate=".$Plate."&Date=".$this->m_calldt;
			# retrieve web page
			$this->m_opsxmlrdo=file_get_contents($this->m_fullURL);
			
			
				<?xml version="1.0" encoding="ISO-8859-1" ?> 
				<r>
					<res>1</res>   ( 0: NO, 1: SI )
					<ft>0</ft>		(fines today (multas hoy) 0:no 1:si)					
					<resblue>60001</resblue>  (grupo azul del residente)
					<oper>1</oper>  ( 0: NO, 1: ESRO, 2: ESRE )
					<m>2345AAA</m>  (matricula)
  					<ds>120002190809</ds>  (fecha de inicio)
					<de>122900180809</de>  (fecha de fin)
				</r>
				
			¿Es residente? ¿Tiene una operacion como residente?¿donde?
			select res_id, res_vehicleid, res_grp_id, res_dart_id from residents
	
			SELECT OPE_GRP_ID, g.grp_descshort , OPE_UNI_ID, OPE_INIDATE, OPE_ENDDATE
				FROM OPERATIONS t, GROUPS g, residents r
				where t.ope_dart_id = r.res_dart_id
				and t.ope_vehicleid = r.res_vehicleid
				and t.ope_grp_id = r.res_grp_id
				AND OPE_ENDDATE >= SYSDATE
				and t.ope_grp_id = g.grp_id
				--AND OPE_VEHICLEID = @PLATE
				AND ROWNUM <= 1
				order by OPE_MOVDATE desc
			
			*/
			bool bResult=false;
			OracleConnection	con		= null;
			OracleCommand cmd = null;
			OracleDataReader	drResGroups		= null;
			OracleDataReader	drOperations	= null;
			
			stError="";

			DateTime dtIniDate=DateTime.Now; 
			DateTime dtEndDate=DateTime.Now; 

			int nResidentOper = 0;
			int nResBlueGroup = 0;
			int nFinesToday=0;

			
			try
			{
				string sConn = ConfigurationSettings.AppSettings["ConnectionString"];
				if( sConn == null )
					throw new Exception("No ConnectionString configuration");

				con = new OracleConnection( sConn );
				
				cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.Connection.Open();	
		
				

				try
				{
					if( cmd == null )
						throw new Exception("Oracle command is null");
				
					// Conexion BBDD?
					if( cmd.Connection == null )
						throw new Exception("Oracle connection is null");

					

					cmd.CommandText = "select RES_GRP_ID from residents where RES_VEHICLEID='" + strPlate + "' order by RES_GRP_ID asc";

					// Recogemos la operacion
					if( drResGroups != null )
					{
						drResGroups.Close();
						drResGroups = null;
					}
					drResGroups = cmd.ExecuteReader();

						

					if (drResGroups.Read())
					{
						nResBlueGroup = Convert.ToInt32(drResGroups["RES_GRP_ID"].ToString());
					}


					if (nResBlueGroup > 0)
					{
						// Consulta de operaciones
						string sSQL ="select * "+
							"from (SELECT OPE_GRP_ID GRUPO, g.grp_descshort GRUPODESC, OPE_UNI_ID, " +
							"TO_CHAR(OPE_INIDATE,'HH24MISSDDMMYY') INIDATE, " +
							"TO_CHAR(OPE_ENDDATE,'HH24MISSDDMMYY') ENDDATE " +
							"FROM OPERATIONS t, GROUPS g, residents r "+
							"where t.ope_dart_id = r.res_dart_id "+
							"and t.ope_vehicleid = r.res_vehicleid "+
							"and t.ope_grp_id = r.res_grp_id "+
							"AND OPE_ENDDATE >= TO_DATE('"+strFechaOp+"','HH24MISSDDMMYY') "+
							"and t.ope_grp_id = g.grp_id "+
							"AND OPE_VEHICLEID = '" + strPlate+ "' "+
							"order by OPE_MOVDATE desc ) "+
							"where ROWNUM <= 1 ";


						cmd.CommandText = sSQL;

						// Recogemos la operacion
						if( drOperations != null )
						{
							drOperations.Close();
							drOperations = null;
						}
						drOperations = cmd.ExecuteReader();

						

						if (drOperations.Read())
						{
							//int nGrupo = Convert.ToInt32(drOperations["GRUPO"].ToString());
							int nGrupo=Convert.ToInt32(drOperations["GRUPO"].ToString());
							dtIniDate=StringToDtx(drOperations["INIDATE"].ToString());
							dtEndDate=StringToDtx(drOperations["ENDDATE"].ToString());

							if( (nGrupo%2)!=0 )
								nResidentOper = 1;
							else
								nResidentOper = 2;
							
						} 
					}
										

					cmd.CommandText="select count(*) from fines WHERE FIN_VEHICLEID='"+strPlate+"' AND TRUNC(FIN_DATE)=TRUNC(TO_DATE('"+strFechaOp+"','HH24MISSDDMMYY'))";

					
					if (cmd.ExecuteOracleScalar().ToString() != "0")
					{ 
						nFinesToday=1;
					}

					bResult = true;

				}
				catch(Exception e)
				{
					stRdo = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
					stRdo += "<rdo>"+HttpUtility.HtmlEncode(e.ToString())+"</rdo>";
					return true;
					

					bResult=false;
				}
				finally
				{
					if( drResGroups != null )
					{
						drResGroups.Close();
						drResGroups = null;
					}

					if( drOperations != null )
					{
						drOperations.Close();
						drOperations = null;
					}
				}
			}
			catch(Exception e)
			{
				stError=e.ToString();
				stRdo = "";
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
			/*
				<?xml version="1.0" encoding="ISO-8859-1" ?> 
				<r>
					<res>1</res>   ( 0: NO, 1: SI )
					<ft>0</ft>		(fines today (multas hoy) 0:no 1:si)
					<resblue>60003</resblue>  (
					<oper>1</oper>  ( 0: NO, 1: ESRO, 2: ESRE )
					<m>2345AAA</m>  (matricula)
  					<ds>120002190809</ds>  (fecha de inicio)
					<de>122900180809</de>  (fecha de fin)
				</r>
			*/

			stRdo = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
			stRdo += "<r>";
			
			if (bResult)
			{					
				if(nResBlueGroup > 0)
				{
					stRdo += "<res>1</res>";
					stRdo += "<ft>"+nFinesToday.ToString()+"</ft>";
					stRdo += "<resblue>"+ nResBlueGroup.ToString()+ "</resblue>";
					stRdo += "<oper>"+nResidentOper.ToString()+"</oper>";
					
					if( nResidentOper != 0)
					{
						stRdo += "<m>"+strPlate+"</m>";
						stRdo += "<ds>"+DtxToString(dtIniDate)+"</ds>";
						stRdo += "<de>"+DtxToString(dtEndDate)+"</de>";
					}
				}
				else
				{
					stRdo += "<res>0</res>";
					stRdo += "<ft>"+nFinesToday.ToString()+"</ft>";
				}
			}
			else
			{
				stRdo += "<res>0</res>";
				stRdo += "<ft>"+nFinesToday.ToString()+"</ft>";
			}

			stRdo += "</r>";

			return bResult;
		
		}

		static public bool GetAccountData( string strTelNumber,  ref string stRdo, ref string stError)
		{
			bool bResult=false;
			OracleConnection	con		= null;
			OracleCommand cmd = null;
			string strPlate="";
			string strPlate2="";
			stError="";

			string strCardNumber="";
			string strCardName="";
			DateTime dtExpirationDate=DateTime.Now; 
			int nUserId=-1;
			int nAccountActive=0;
			int nFunds=0;
			int nMinimumBalance=0;
			int nNoticeBalance=-1;

			try
			{

				string sConn = ConfigurationSettings.AppSettings["ConnectionString"];
				if( sConn == null )
					throw new Exception("No ConnectionString configuration");

				AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();

				try
				{
					nMinimumBalance= (int) appSettings.GetValue   ("MOBILE_PAY_MINIMUM_BALANCE",typeof(int));
				}
				catch
				{
					nMinimumBalance=0;
				}	

				try
				{
					nNoticeBalance= (int) appSettings.GetValue   ("MOBILE_PAY_NOTICE_BALANCE",typeof(int));
				}
				catch
				{
					nNoticeBalance=-1;
				}	

				con = new OracleConnection( sConn );
				
				cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.Connection.Open();	
		
				OracleDataReader	drPlates		= null;

				try
				{
					if( cmd == null )
						throw new Exception("Oracle command is null");
				
					// Conexion BBDD?
					if( cmd.Connection == null )
						throw new Exception("Oracle connection is null");

					// Consulta de Clientes
					string sSQL = "select * "+
						"from (select  MUP_PLATE,MU_NUM_CREDIT_CARD,TO_CHAR(MU_NUM_CC_EXPIRATION_DATE,'HH24MISSDDMMYY') MU_NUM_CC_EXPIRATION_DATE,MU_NAME_CARD,MU_ID,MU_ACTIVATE_ACCOUNT,MU_FUNDS "+
						"		from mobile_users_plates mup, mobile_users mu "+
						"		where mu.mu_id = mup.mup_mu_id "+
						"		and mu.mu_mobile_telephone = '"+strTelNumber+"' AND MU_VALID=1 AND MU_DELETED=0 AND MUP_VALID=1 AND MUP_DELETED=0 ORDER BY MUP_NUM_OPERATIONS DESC, MUP_PLATE) "+
						"where rownum <= 2";
				
					cmd.CommandText = sSQL;

					// Recogemos los Clientes
					if( drPlates != null )
					{
						drPlates.Close();
						drPlates = null;
					}
					drPlates = cmd.ExecuteReader();


					if (drPlates.Read())
					{
						nUserId = Convert.ToInt32(drPlates["MU_ID"].ToString());
						strPlate=drPlates["MUP_PLATE"].ToString();
						strCardNumber= Decrypt(nUserId,drPlates["MU_NUM_CREDIT_CARD"].ToString());
						strCardName=Decrypt(nUserId,drPlates["MU_NAME_CARD"].ToString());
						dtExpirationDate=StringToDtx(drPlates["MU_NUM_CC_EXPIRATION_DATE"].ToString());
						nAccountActive = (Convert.ToInt32(drPlates["MU_ACTIVATE_ACCOUNT"].ToString())>0) ? 1: 0; 
						nFunds=Convert.ToInt32(drPlates["MU_FUNDS"].ToString());
						bResult=true;
					} 

					if (drPlates.Read())
					{
						strPlate2=drPlates["MUP_PLATE"].ToString();
					}

				}
				catch(Exception e)
				{
					bResult=false;
				}
				finally
				{
					if( drPlates != null )
					{
						drPlates.Close();
						drPlates = null;
					}
				}


				
				

			}
			catch(Exception e)
			{
				stError=e.ToString();
				stRdo = "";
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
			/*
				<?xml version="1.0" encoding="ISO-8859-1" ?> 
				<r>
					<usr>0</usr>  ( -1: No existe usuario o no tiene matricula, 
									-2: Tarjeta caducada
									-3: Cuenta no activa
									-4: Saldo inferior al mínimo
									 0: Todo ok
									 1: tarjeta proxima a caducar 
									 2: Saldo superior al minimo pero inferior a un valor)
					<m>2345AAA</m>  (matricula)
					<m2>2345AAA</m2>  (matricula 2)
					<bal>1234</bal>  (saldo en centimos)
  					<tn>1234********5678</tn> (tarjeta de credito)
     				<td>03/12</td> 			(fecha caducidad tarjeta MM/YY)					
				</r>
			*/

			stRdo = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
			stRdo += "<r>";
			
			if (bResult)
			{					
				stRdo += "<m>"+strPlate+"</m>";

				if (strPlate2.Length>0)
				{
					stRdo += "<m2>"+strPlate2+"</m2>";				
				}
				
				if (strCardNumber.Length>=8)
				{
					string strCCBegin=strCardNumber.Substring(0,4);
					string strCCEnd=strCardNumber.Substring(strCardNumber.Length-4,4);
					stRdo += "<tn>"+ strCCBegin+strCCEnd.PadLeft(strCardNumber.Length-4,'*')+"</tn>";
				}
				else
				{
					stRdo += "<tn>"+strCardNumber+"</tn>";
				}
				

				if (nAccountActive==0)
				{
					stRdo += "<usr>-3</usr>";
				}
				/*else if((DtxToExpDateString(DateTime.Now).CompareTo(DtxToExpDateString(dtExpirationDate))>=0))
				{
					stRdo += "<usr>-2</usr>";
				}
				else if((DtxToExpDateString(DateTime.Now.AddMonths(+1)).CompareTo(DtxToExpDateString(dtExpirationDate))>=0))
				{
					stRdo += "<usr>1</usr>";
				}*/
				else if (nFunds<=nMinimumBalance)
				{
					stRdo += "<usr>-4</usr>";
				}
				else if ((nNoticeBalance>0)&&(nFunds<=nNoticeBalance))
				{
					stRdo += "<usr>2</usr>";
				}				
				else
					stRdo += "<usr>0</usr>";

				stRdo += "<td>"+dtExpirationDate.ToString("MM/yy")+"</td>";
				stRdo += "<bal>"+nFunds.ToString()+"</bal>";

			}
			else
			{
				//stRdo += "<m>"+strPlate+"</m>";
				stRdo += "<usr>-1</usr>";
			}

			stRdo += "</r>";

			return bResult;
		
		}

		static public bool ManageCall( string strTelNumber, string strFechaOp, bool bSaveOp,string strGroup, string strMaxTime, ref string stRdo,ref string stError)
		{
			int nSector=DEFAULT_SECTOR_ID;

			if (strGroup.Length>0)
			{
				nSector=Convert.ToInt32(strGroup);
			}


			int nMaxTime=Convert.ToInt32(strMaxTime);
			
			return ManageCall(strTelNumber, "", strFechaOp,bSaveOp,nSector,-1, nMaxTime, ref stRdo,ref stError);
		}

		static public bool ManageCall( string strTelNumber, string strPlate, string strFechaOp, bool bSaveOp,string strGroup, string strArticleDef, string strMaxTime, ref string stRdo,ref string stError)
		{
			int nSector=DEFAULT_SECTOR_ID;

			if (strGroup.Length>0)
			{
				nSector=Convert.ToInt32(strGroup);
			}


			int nMaxTime=Convert.ToInt32(strMaxTime);
			int nArticleDef=Convert.ToInt32(strArticleDef);
			
			return ManageCall(strTelNumber, strPlate, strFechaOp,bSaveOp,nSector, nArticleDef, nMaxTime, ref stRdo,ref stError);
		}


		static public bool ManageCall( string strTelNumber, string strPlatePar, string strFechaOp, bool bSaveOp,int nSector, int nArticleDefPar, int nMaxTime, ref string stRdo,ref string stError)
		{
			bool bResult=false;
			OracleConnection	con		= null;
			OracleCommand cmd = null;
			string strPlate=strPlatePar;
			stError="";

			int iTipoOperacion=0;
			string strParkingIniDate="";
			string strParkingEndDate="";
			string strParkingDevDate="";
			string strCardNumber="";
			string strCardName="";
			DateTime dtExpirationDate=DateTime.Now; 
			int nUserId=-1;
			int nUserBalance=0;
			double dQuantity=0;
			double dQuantityReturned=0;

			int nArticleDef=nArticleDefPar;
			int nVirtualUnit=-1;
			int iM1Result=-999;
			int nFinalBalance=0;
			
			try
			{
				string sConn = ConfigurationSettings.AppSettings["ConnectionString"];
				if( sConn == null )
					throw new Exception("No ConnectionString configuration");

				con = new OracleConnection( sConn );
				
				cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.Connection.Open();			

				if (GetUserData( strTelNumber, ref cmd, ref strPlate, ref strCardNumber, ref strCardName,ref dtExpirationDate,ref nUserId,ref nUserBalance ))
				{
					if ((strPlate.Length>0)&&
						(strCardNumber.Length>0)&&
						(DtxToExpDateString(DateTime.Now).CompareTo(DtxToExpDateString(dtExpirationDate))<=0))
					{

						//primero probamos una devolución
						bool bDevolucion=false;

						int nVirtualUnitSector=0;
						if (GetVirtualUnit(nSector,ref cmd,ref nVirtualUnit))
						//if (GetVirtualUnitAndTipoArtForPlate(strPlate,strFechaOp,ref cmd,ref nVirtualUnit,ref nVirtualUnitSector, ref nArticleDef))
						{
							nVirtualUnitSector=nSector;
							if (SendRefund(strPlate,strFechaOp,bSaveOp, nVirtualUnit, nArticleDef,nVirtualUnitSector,strCardNumber,dtExpirationDate,strCardName,nUserId,ref strParkingIniDate, ref strParkingDevDate, ref dQuantity, ref dQuantityReturned))
							{								
								iTipoOperacion=3;
								bDevolucion=true;
								nFinalBalance=nUserBalance+Convert.ToInt32(dQuantityReturned);
								bResult=GetMaxParkingTime(strPlate,strFechaOp,nVirtualUnit,nVirtualUnitSector, ref strParkingEndDate);
								if (!bResult)
								{
									strParkingEndDate=strFechaOp;
								}
							}
						}


						if (!bDevolucion)
						{
							if (GetVirtualUnit(nSector,ref cmd,ref nVirtualUnit))
							{
								int nMinimumBalance=0;

								AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();

								try
								{
									nMinimumBalance= (int) appSettings.GetValue   ("MOBILE_PAY_MINIMUM_BALANCE",typeof(int));
								}
								catch
								{
									nMinimumBalance=0;
								}	

								if (SendPark(strPlate,strFechaOp,bSaveOp, nVirtualUnit, nSector,strCardNumber,dtExpirationDate,strCardName,nUserId,nUserBalance,nMinimumBalance,nMaxTime, ref strParkingIniDate, ref strParkingEndDate, ref dQuantity, ref iTipoOperacion, ref iM1Result))
								{
									nFinalBalance=nUserBalance-Convert.ToInt32(dQuantity);
									if ((nFinalBalance>=nMinimumBalance)||(!bSaveOp))
									{
										bResult=true;
									}
									else
									{
										//saldo insuficiente para realizar la operación
										iTipoOperacion=-2;
									}
								}
							}
						}
					}
				}
				else
				{
					//el usuario no existe o no tiene ninguna matrícula
					iTipoOperacion=-1;
				}

			}
			catch(Exception e)
			{
				iTipoOperacion=0;
				stError=e.ToString();
				stRdo = "";
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

			stRdo = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
			stRdo += "<r>";
			stRdo += "<y>"+iTipoOperacion.ToString()+"</y>";
			if (bResult)
			{					
				stRdo += "<m>"+strPlate+"</m>";
				stRdo += "<ds>"+strParkingIniDate+"</ds>";
				stRdo += "<de>"+strParkingEndDate+"</de>";
				if (iTipoOperacion==3)
				{
					stRdo += "<dr>"+strParkingDevDate+"</dr>";
				}

				stRdo += "<q>"+dQuantity.ToString("##########0")+"</q>";
				stRdo += "<bal>"+nFinalBalance.ToString()+"</bal>";

				if (strCardNumber.Length>=8)
				{
					string strCCBegin=strCardNumber.Substring(0,4);
					string strCCEnd=strCardNumber.Substring(strCardNumber.Length-4,4);
					stRdo += "<tn>"+ strCCBegin+strCCEnd.PadLeft(strCardNumber.Length-4,'*')+"</tn>";
				}
				else
				{
					stRdo += "<tn>"+strCardNumber+"</tn>";
				}
				
				
				stRdo += "<td>"+dtExpirationDate.ToString("MM/yy")+"</td>";

			}
			else
			{
				stRdo += "<m>"+strPlate+"</m>";
				if (iM1Result!=-999)
				{
					stRdo += "<e>"+iM1Result+"</e>";
				}
			}

			stRdo += "</r>";

			return bResult;
		}


		static private bool GetUserData( string strTelNumber, ref OracleCommand cmd, ref string strPlate,ref string strCardNumber, ref string strCardName, ref DateTime dtExpDate, ref int nUserId, ref int nUserBalance )
		{
			bool bResult=false;
			
			OracleDataReader	drPlates		= null;
			nUserBalance=0;

			try
			{
				if( cmd == null )
					throw new Exception("Oracle command is null");
				
				// Conexion BBDD?
				if( cmd.Connection == null )
					throw new Exception("Oracle connection is null");

				// Consulta Numero de  Clientes
				
				/*	TGP @ END Numero de Clientes	*/

				// Consulta de Clientes
				string sSQL = "select * "+
							"from (select  MUP_PLATE,MU_NUM_CREDIT_CARD,TO_CHAR(MU_NUM_CC_EXPIRATION_DATE,'HH24MISSDDMMYY') MU_NUM_CC_EXPIRATION_DATE,MU_NAME_CARD,MU_ID,MU_FUNDS "+
							"		from mobile_users_plates mup, mobile_users mu "+
							"		where MU_VALID=1 AND MU_DELETED=0 AND MUP_VALID=1 AND MUP_DELETED=0 and mu.mu_id = mup.mup_mu_id "+
							"		and (mu.mu_mobile_telephone = '"+strTelNumber+"' or mu.mu_mobile_telephone2 = '"+strTelNumber+"' ) ";

				if (strPlate.Length>0)
				{
					sSQL+=  "		and mup.mup_plate = '"+strPlate+"' ";
				}

				sSQL+= ") where rownum <= 1";
				
				cmd.CommandText = sSQL;

				// Recogemos los Clientes
				if( drPlates != null )
				{
					drPlates.Close();
					drPlates = null;
				}
				drPlates = cmd.ExecuteReader();


				if (drPlates.Read())
				{
					nUserId = Convert.ToInt32(drPlates["MU_ID"].ToString());
					strPlate=drPlates["MUP_PLATE"].ToString();
					strCardNumber= Decrypt(nUserId,drPlates["MU_NUM_CREDIT_CARD"].ToString());
					strCardName=Decrypt(nUserId,drPlates["MU_NAME_CARD"].ToString());
					dtExpDate=StringToDtx(drPlates["MU_NUM_CC_EXPIRATION_DATE"].ToString());
					nUserBalance = Convert.ToInt32(drPlates["MU_FUNDS"].ToString());

					bResult=true;
				} 

			}
			catch(Exception e)
			{
				bResult=false;
			}
			finally
			{
				if( drPlates != null )
				{
					drPlates.Close();
					drPlates = null;
				}
			}


			return bResult;

		}
		

		static public bool GetLastSector( string strTelNumber,string strFechaOp, ref string strPlate,ref int iSectorId )
		{
			bool bResult=false;
			OracleConnection	con		= null;
			OracleCommand cmd = null;			
			OracleDataReader	dr		= null;
			int nUserId=-1;
			int iTelOpeId;
			int iReadSectorId;
			int iOpeId;
			iSectorId=-1;
			strPlate="";

			try
			{

				string sConn = ConfigurationSettings.AppSettings["ConnectionString"];
				if( sConn == null )
				throw new Exception("No ConnectionString configuration");

				con = new OracleConnection( sConn );
						
				cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.Connection.Open();	
				

				// Consulta de Clientes
				string sSQL = "select * "+
					"from (select  MUP_PLATE,MU_ID "+
					"		from mobile_users_plates mup, mobile_users mu "+
					"		where mu.mu_id = mup.mup_mu_id AND MU_VALID=1 AND MU_DELETED=0 AND MUP_VALID=1 AND MUP_DELETED=0 "+
					"		and mu.mu_mobile_telephone = '"+strTelNumber+"') "+
					"where rownum <= 1";
				
				cmd.CommandText = sSQL;

				dr = cmd.ExecuteReader();

				if (dr.Read())
				{
					nUserId = Convert.ToInt32(dr["MU_ID"].ToString());
					strPlate=dr["MUP_PLATE"].ToString();

					bResult=true;
				} 

				

				if (bResult)
				{
					dr.Close();
					bResult=false;
					
					sSQL = string.Format("select OPE_ID,OPE_GRP_ID from (SELECT OPE_ID,OPE_GRP_ID FROM OPERATIONS WHERE OPE_VEHICLEID='{0}'  AND OPE_MOVDATE<TO_DATE('{1}','HH24MISSDDMMYY') AND OPE_MOBI_USER_ID={2} ORDER BY OPE_MOVDATE DESC) where rownum <= 1", strPlate,strFechaOp,nUserId);
					cmd.CommandText=sSQL;

					dr = cmd.ExecuteReader();
					if (dr.HasRows)
					{
						dr.Read();
						iTelOpeId     = Convert.ToInt32(dr["OPE_ID"].ToString());
						iReadSectorId     = Convert.ToInt32(dr["OPE_GRP_ID"].ToString());
	
						dr.Close();
						sSQL = string.Format("select OPE_ID from (SELECT OPE_ID FROM OPERATIONS WHERE OPE_VEHICLEID='{0}'  AND OPE_MOVDATE<TO_DATE('{1}','HH24MISSDDMMYY') ORDER BY OPE_MOVDATE DESC) where rownum <= 1", strPlate,strFechaOp);
						cmd.CommandText=sSQL;

						dr = cmd.ExecuteReader();
						if (dr.HasRows)
						{
							dr.Read();
							iOpeId     = Convert.ToInt32(dr["OPE_ID"].ToString());

							if (iOpeId==iTelOpeId)
							{
								bResult=true;
								iSectorId = iReadSectorId;
							}

						}
						

					}

				}

			}
			catch(Exception e)
			{
				bResult=false;
			}
			finally
			{
				if( dr != null )
				{
					dr.Close();
					dr = null;
				}
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


			return bResult;

		}
		
		
		
		static private DateTime StringToDtx (string s)
		{
			int hour = Convert.ToInt32(s.Substring(0,2));
			int minute = Convert.ToInt32(s.Substring(2,2));
			int second = Convert.ToInt32(s.Substring(4,2));
			int day = Convert.ToInt32(s.Substring(6,2));
			int month = Convert.ToInt32(s.Substring(8,2));
			int year = Convert.ToInt32(s.Substring(10,2));
			DateTime dt = new DateTime(2000 + year, month, day, hour, minute, second);
			return dt;
		}

		/// <summary>
		/// Pass a DateTime to a string in format (hhmmssddmmyy)
		/// </summary>
		/// <param name="dt">DateTime to convert</param>
		/// <returns>string in OPS-dtx format</returns>
		static private string DtxToString (DateTime dt)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append (dt.Hour.ToString("D2"));
			sb.Append (dt.Minute.ToString("D2"));
			sb.Append (dt.Second.ToString("D2"));
			sb.Append (dt.Day.ToString("D2"));	
			sb.Append (dt.Month.ToString("D2"));
			int year = dt.Year - 2000;					// We use only 2 digits.
			sb.Append (year.ToString("D2"));
			return sb.ToString();

		}

		static private string DtxToExpDateString (DateTime dt)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
	
			int year = dt.Year - 2000;					// We use only 2 digits.
			sb.Append (year.ToString("D2"));
			sb.Append (dt.Month.ToString("D2"));			
			return sb.ToString();

		}

		static private bool GetVirtualUnitAndTipoArtForPlate(string strPlate, string strFechaOp, ref OracleCommand cmd, ref int nVirtualUnit, ref int nVirtualUniSector, ref int nTipoArt)
		{
			nVirtualUnit = 0;
			nVirtualUniSector = 0;
			bool bResult=false;
			OracleDataReader	dataReader		= null;
			try
			{

				if( cmd == null )
					throw new Exception("Oracle command is null");
				
				// Conexion BBDD?
				if( cmd.Connection == null )
					throw new Exception("Oracle connection is null");

				string strSQL="";
	
				if (nTipoArt>0)
				{
					strSQL = string.Format("SELECT GVU_UNI_ID, GVU_GRP_ID, OPE_DART_ID FROM OPERATIONS, GROUP_VIRTUAL_UNIT WHERE OPE_VEHICLEID='{0}' AND OPE_DART_ID={2} AND OPE_GRP_ID=GVU_GRP_ID AND OPE_MOVDATE<TO_DATE('{1}','HH24MISSDDMMYY') ORDER BY OPE_MOVDATE DESC", strPlate,strFechaOp, nTipoArt);
				}
				else
				{
					strSQL = string.Format("SELECT GVU_UNI_ID, GVU_GRP_ID, OPE_DART_ID FROM OPERATIONS, GROUP_VIRTUAL_UNIT WHERE OPE_VEHICLEID='{0}' AND OPE_GRP_ID=GVU_GRP_ID AND OPE_MOVDATE<TO_DATE('{1}','HH24MISSDDMMYY') ORDER BY OPE_MOVDATE DESC", strPlate,strFechaOp);
				}

				cmd.CommandText=strSQL;

				dataReader=cmd.ExecuteReader();
				if (dataReader.HasRows)
				{
					dataReader.Read();
					nVirtualUnit     = dataReader.GetInt32(0);
					nVirtualUniSector = dataReader.GetInt32(1);
					nTipoArt     = dataReader.GetInt32(2);
					bResult=true;
				}
				
			}
			catch (Exception e)
			{
				
			}
			finally
			{
				if( dataReader != null )
				{
					dataReader.Close();
					dataReader = null;
				}
			}

			return bResult;
		}

		static private bool GetVirtualUnit(int nSector, ref OracleCommand cmd, ref int nVirtualUnit)
		{
			nVirtualUnit = 0;
			bool bResult=false;
			OracleDataReader	dataReader		= null;
			try
			{
				if( cmd == null )
					throw new Exception("Oracle command is null");
				
				// Conexion BBDD?
				if( cmd.Connection == null )
					throw new Exception("Oracle connection is null");

				string strSQL = string.Format("SELECT NVL(GVU_UNI_ID,0) FROM GROUP_VIRTUAL_UNIT WHERE GVU_GRP_ID = {0}", nSector);
				cmd.CommandText=strSQL;

				dataReader=cmd.ExecuteReader();
				if (dataReader.HasRows)
				{
					dataReader.Read();
					nVirtualUnit     = dataReader.GetInt32(0);
					bResult=true;
				}
				
			}
			catch (Exception e)
			{
				
			}
			finally
			{
				if( dataReader != null )
				{
					dataReader.Close();
					dataReader = null;
				}
			}

			return bResult;
		}




		static private bool SendRefund(string strPlate,string strFechaOp, bool bSaveOp, int nVirtualUnit, int nArticleDef,int nSector,string strCard, DateTime dtExpDate, string strNameCard, int nUserId, ref string strDateIni, ref string strDateEnd, ref double dQuantity, ref double dQuantityReturned)
		{
			
			bool bResult=false;
			dQuantity=0;
			
			try
			{
				string strSeqId = DateTime.Now.ToString("ddHHmmss");			

				string strXML = string.Format(
					"<m1 id=\"{0}\"><m>{1}</m><d>{2}</d><u>{3}</u><o>{4}</o><pt>{5}</pt><ad>{6}</ad></m1>",
					strSeqId, strPlate, strFechaOp, nVirtualUnit, 3, 4, nArticleDef);		
		
				Messages wsMessages = new Messages();
				
				string strM1Res = wsMessages.Message(strXML);
				int nResult=0;

				if (ParseM1Message(strM1Res,ref nResult))
				{
					int nTipoOperacion=0;
					int nTipoArticulo=0;
					double nImporte;
					string strTime;
					double nAcumImporte;
					

					GetTipoOperacion(strM1Res, ref nTipoOperacion);
					GetTipoArticulo(strM1Res, ref nTipoArticulo);
					strDateIni= GetFechaIni(strM1Res);
					strDateEnd= GetFechaFin(strM1Res);
					nImporte=GetImporte(strM1Res);
					strTime=GetTime(strM1Res);
					nAcumImporte=GetAcumImporte(strM1Res);


					if (bSaveOp)
					{
						strXML = string.Format(
							"<m2 id=\"{0}\"><m>{1}</m><y>{2}</y><ad>{3}</ad><u>{4}</u>" +
							"<p>{5}</p><d>{6}</d><d1>{7}</d1><d2>{8}</d2><t>{9}</t><q>{10}</q><tn>{11}</tn><td>{12}</td><tm>{13}</tm><mui>{14}</mui><qr>{15}</qr><om>1</om><pvis>{16}</pvis></m2>",
							strSeqId, strPlate, 3, nTipoArticulo, nVirtualUnit, 4, strFechaOp, strDateIni, strDateEnd, strTime, nImporte, 
							strCard, DtxToString(dtExpDate), strNameCard, nUserId,nAcumImporte,5);
				
						string strM2Res = wsMessages.Message(strXML);

						bResult=ParseM2Message(strM1Res);
					}
					else
					{
						bResult=true;
					}

					if (bResult)
					{
						dQuantity=nAcumImporte;
						dQuantityReturned=nImporte;
					}

				}
				
			}
			catch (Exception e)
			{
				
			}
			finally
			{
				
			}

			return bResult;
		}


		static private bool SendPark(string strPlate,string strFechaOp, bool bSaveOp,int nVirtualUnit, int nSector,string strCard, DateTime dtExpDate, 
									 string strNameCard, int nUserId, int nUserBalance,int nMinimumBalance,int nMaxTime, ref string strDateIni, ref string strDateEnd, ref double dQuantity, 
									 ref int iTipoOperacion, ref int nResult)
		{
			
			bool bResult=false;
			nResult=0;
			
			try
			{
				string strSeqId = DateTime.Now.ToString("ddHHmmss");			

				int nArticleDef=0;
				try
				{
					nArticleDef= Int32.Parse(ConfigurationSettings.AppSettings["ArticleType"]);
				}
				catch
				{
					nArticleDef=DEFAULT_ARTICLE_DEF;
				}


				string strXML="";

				if (nMaxTime<=0)
				{
					strXML = string.Format(
						"<m1 id=\"{0}\"><m>{1}</m><d>{2}</d><u>{3}</u><o>{4}</o><g>{5}</g><pt>{6}</pt><cdl>1</cdl></m1>",
						strSeqId, strPlate, strFechaOp, nVirtualUnit, 1, nSector, 4 );	
				}
				else
				{
					strXML = string.Format(
						"<m1 id=\"{0}\"><m>{1}</m><d>{2}</d><u>{3}</u><o>{4}</o><g>{5}</g><pt>{6}</pt><t>{7}</t><cdl>1</cdl></m1>",
						strSeqId, strPlate, strFechaOp, nVirtualUnit, 1, nSector, 4, nMaxTime);	

				}
		
				Messages wsMessages = new Messages();
				
				string strM1Res = wsMessages.Message(strXML);
				

				if (ParseM1Message(strM1Res,ref nResult))
				{
					int nTipoArticulo=0;
					string strDateMin="";
					string strDateMax="";
					double nImporteMin=0;
					double nImporteMax=0;
					double nTimeMin=0;
					double nTimeMax=0;
					int nPostPago=0;
					

					GetTipoOperacion(strM1Res, ref iTipoOperacion);
					GetTipoArticulo(strM1Res, ref nTipoArticulo);
					strDateIni= GetFechaIni(strM1Res);
					GetDateLimits(strM1Res, ref strDateMin, ref strDateMax);
					GetImportes(strM1Res, ref nImporteMin, ref  nImporteMax);
					GetTimeLimits(strM1Res, ref  nTimeMin, ref  nTimeMax);
					GetPostPago(strM1Res, ref  nPostPago);
					strDateEnd=strDateMax;



					if ((nUserBalance-nImporteMax)<nMinimumBalance)
					{
						bSaveOp=false;
					}

					if (bSaveOp)
					{
						strXML = string.Format(
							"<m2 id=\"{0}\"><m>{1}</m><y>{2}</y><ad>{3}</ad><g>{4}</g><u>{5}</u>" +
							"<p>{6}</p><d>{7}</d><d1>{8}</d1><d2>{9}</d2><t>{10}</t><q>{11}</q><tn>{12}</tn><td>{13}</td><tm>{14}</tm><mui>{15}</mui><pp>{16}</pp><om>1</om><pvis>{17}</pvis></m2>",
							strSeqId, strPlate, iTipoOperacion, nTipoArticulo, nSector, nVirtualUnit, 4, strFechaOp, strDateIni, strDateEnd, nTimeMax, nImporteMax, 
							strCard, DtxToString(dtExpDate), strNameCard, nUserId, nPostPago,5);
					
						string strM2Res = wsMessages.Message(strXML);

						bResult=ParseM2Message(strM1Res);

						if (bResult)
						{
							UpdateNumOperationsPlate(nUserId,strPlate);
						}
					}
					else
					{
						bResult=true;
					}
					

					if (bResult)
					{
						dQuantity=nImporteMax;
					}

				}


				
			}
			catch (Exception e)
			{
				
			}
			finally
			{
				
			}

			return bResult;
		}




		static public bool GetDateTime(ref string strDateTime)
		{
			
			bool bResult=false;
			strDateTime="";
			
			try
			{
				string strSeqId = DateTime.Now.ToString("ddHHmmss");			

				string strXML = string.Format(
					"<m59 id=\"{0}\"></m59>",strSeqId);		
		
				Messages wsMessages = new Messages();
				
				string strM59Res = wsMessages.Message(strXML);
				int nResult=0;

				bResult=ParseM59Message(strM59Res,ref strDateTime);
				
			}
			catch (Exception e)
			{
				strDateTime="";
			}
			finally
			{
				
			}

			return bResult;
		}





		static private bool UpdateNumOperationsPlate(int nUserId, String strPlate)
		{

			bool bResult=false;
			OracleConnection	con		= null;
			OracleCommand cmd = null;
			
			try
			{
				string sConn = ConfigurationSettings.AppSettings["ConnectionString"];
				if( sConn == null )
					throw new Exception("No ConnectionString configuration");

				con = new OracleConnection( sConn );
				
				cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.Connection.Open();	
		
				try
				{
					if( cmd == null )
						throw new Exception("Oracle command is null");
				
					// Conexion BBDD?
					if( cmd.Connection == null )
						throw new Exception("Oracle connection is null");

					cmd.CommandText = String.Format("UPDATE MOBILE_USERS_PLATES SET MUP_NUM_OPERATIONS=MUP_NUM_OPERATIONS+1 WHERE MUP_MU_ID={0} AND MUP_PLATE='{1}'",nUserId, strPlate);

					int nResult = cmd.ExecuteNonQuery();		
					if (0 < nResult)
					{
						bResult=true;
					}
				
				}
				catch(Exception e)
				{
					bResult=false;
				}
				

			}
			catch(Exception e)
			{
				bResult=false;
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

			return bResult;


		}

		static private bool GetMaxParkingTime(string strPlate,string strFechaOp, int nVirtualUnit, int nSector, ref string strMaxDate)
		{
			
			bool bResult=false;
			
			try
			{
				string strSeqId = DateTime.Now.ToString("ddHHmmss");			


				string strXML = string.Format(
					"<m50 id=\"{0}\"><m>{1}</m><g>{2}</g><d>{3}</d><u>{4}</u></m50>",
					strSeqId, strPlate, nSector,strFechaOp, nVirtualUnit);	
		
				Messages wsMessages = new Messages();
				
				string strM50Res = wsMessages.Message(strXML);
				int nResult=0;

				bResult=ParseM50Message(strM50Res, ref nResult,ref strMaxDate);
				
			}
			catch (Exception e)
			{
				
			}
			finally
			{
				
			}

			return bResult;
		}


		static private bool ParseM59Message(String strMsg, ref string strDateTime)
		{
			bool bReturn = false;
			strDateTime="";

			//<ap id="8"><t>110708071112</t></ap>
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("ap");
				if (xmlList.Count == 1)
				{
					xmlList = doc.GetElementsByTagName("t");
					if (xmlList.Count == 1)
					{
						strDateTime = xmlList.Item(0).InnerText;
						if (strDateTime.Length>0)
						{			
							bReturn = true;
						}
					}				
				}
						
			}
			catch
			{
				strDateTime="";
			}


			return bReturn;
		}



		static private bool ParseM1Message(String strMsg, ref int nResult)
		{
			bool bReturn = false;
			nResult=-999;

			// Is it a XML document?
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("ap");
				if (xmlList.Count == 1)
				{
					xmlList = doc.GetElementsByTagName("Ar");
					if (xmlList.Count == 1)
					{
						nResult = Int32.Parse(xmlList.Item(0).InnerText);
						if (1 == nResult)
						{			
							bReturn = true;
						}
					}				
				}
						
			}
			catch
			{
				nResult=-999;
			}


			return bReturn;
		}

		static private bool ParseM50Message(String strMsg, ref int nResult,ref string strMaxDate)
		{
			bool bReturn = false;
			nResult=-999;
			strMaxDate="";

			// Is it a XML document?
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("ap");
				if (xmlList.Count == 1)
				{
					xmlList = doc.GetElementsByTagName("r");
					if (xmlList.Count == 1)
					{
						nResult = Int32.Parse(xmlList.Item(0).InnerText);
						if (1 == nResult)
						{			

							xmlList = doc.GetElementsByTagName("d");
							if (xmlList.Count == 1)
							{
								strMaxDate = xmlList.Item(0).InnerText;
								bReturn = true;
							}							
						}
					}				
				}
						
			}
			catch
			{
				nResult=-999;
			}


			return bReturn;
		}


		static private bool ParseM2Message(String strMsg)
		{
			bool bReturn = false;

			// Is it a XML document?
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("ap");
				if (xmlList.Count == 1)
				{
					bReturn = true;
				}	
			}
			catch
			{
			}

			return bReturn;
		}



		static private void GetImportes(String strMsg, ref double nImporteMin, ref double nImporteMax)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("Aq1");
				if (xmlList.Count == 1)
				{
					nImporteMin = Double.Parse(xmlList.Item(0).InnerText);
				}

				xmlList = doc.GetElementsByTagName("Aq2");
				if (xmlList.Count == 1)
				{
					nImporteMax = Double.Parse(xmlList.Item(0).InnerText);
				}
			}
			catch
			{
			}
		}

		static private void GetTimeLimits(String strMsg, ref double nTimeMin, ref double nTimeMax)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("At1");
				if (xmlList.Count == 1)
				{
					nTimeMin = Double.Parse(xmlList.Item(0).InnerText);
				}

				xmlList = doc.GetElementsByTagName("At2");
				if (xmlList.Count == 1)
				{
					nTimeMax = Double.Parse(xmlList.Item(0).InnerText);
				}
			}
			catch
			{
			}
		}

		static private void GetDateLimits(String strMsg, ref string strDateMin, ref string strDateMax)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("Ad1");
				if (xmlList.Count == 1)
				{
					strDateMin = xmlList.Item(0).InnerText;
				}

				xmlList = doc.GetElementsByTagName("Ad");
				if (xmlList.Count == 1)
				{
					strDateMax = xmlList.Item(0).InnerText;
				}
			}
			catch
			{
			}
		}


		static private void GetTipoOperacion(String strMsg, ref int nTipoOperacion)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("Ao");
				if (xmlList.Count == 1)
				{
					nTipoOperacion = (int) Double.Parse(xmlList.Item(0).InnerText);
				}

			}
			catch
			{
			}
		}


		static private void GetTipoArticulo(String strMsg, ref int nTipoArticulo)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("Aad");
				if (xmlList.Count == 1)
				{
					nTipoArticulo = (int) Double.Parse(xmlList.Item(0).InnerText);
				}

			}
			catch
			{
			}
		}

		static private void GetPostPago(String strMsg, ref int nPostPago)
		{
			XmlDocument doc = new XmlDocument();
			nPostPago=0;
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("App");
				if (xmlList.Count == 1)
				{
					nPostPago = (int) Double.Parse(xmlList.Item(0).InnerText);
				}

			}
			catch
			{
			}
		}		

		
		static private String GetFechaIni(String strMsg)
		{
			String strFechaIni = String.Empty;

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("Adi");
				if (xmlList.Count == 1)
				{
					strFechaIni = xmlList.Item(0).InnerText;
				}
			}
			catch
			{
			}

			return strFechaIni;
		}

		static private String GetFechaFin(String strMsg)
		{
			String strFechaFin = String.Empty;

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("Ad");
				if (xmlList.Count == 1)
				{
					strFechaFin = xmlList.Item(0).InnerText;
				}
			}
			catch
			{
			}
			return strFechaFin;
		}


		static private String GetTime(String strMsg)
		{
			String strTime = String.Empty;

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("At");
				if (xmlList.Count == 1)
				{
					strTime = xmlList.Item(0).InnerText;
				}
			}
			catch
			{
			}

			return strTime;
		}

		static private double GetImporte(String strMsg)
		{
			double nImporte = 0;

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("Aq");
				if (xmlList.Count == 1)
				{
					nImporte = double.Parse(xmlList.Item(0).InnerText);
				}
			}
			catch
			{
			}
			return nImporte;
		}

		static private double GetAcumImporte(String strMsg)
		{
			double nImporte = 0;

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.LoadXml(strMsg);
				XmlNodeList xmlList = doc.GetElementsByTagName("Aaq");
				if (xmlList.Count == 1)
				{
					nImporte = double.Parse(xmlList.Item(0).InnerText);
				}
			}
			catch
			{
			}
			return nImporte;
		}

		static private string Decrypt(int nUserId, string strEncrypted)
		{
			
			string strDecrypted = "";
			string strLocEncrypted="";

			try
			{
				string strKey = GetKeyToApply(nUserId.ToString());

				if ((strEncrypted.Substring(0,6)=="%&!()=") && (strEncrypted.Substring(strEncrypted.Length-6,6)=="%&!()="))
				{
					strLocEncrypted = strEncrypted.Substring(6,strEncrypted.Length-12);
				}

				byte [] byEncrypt = HexString_To_Bytes(strLocEncrypted);

				TripleDESCryptoServiceProvider TripleDesProvider=  new TripleDESCryptoServiceProvider();
				int sizeKey = System.Text.Encoding.Default.GetByteCount (strKey);
				byte [] byKey;
				byKey = new byte[sizeKey];	
				System.Text.Encoding.Default.GetBytes(strKey,0, strKey.Length,byKey, 0);
				TripleDesProvider.Mode=CipherMode.ECB;
				TripleDesProvider.Key=byKey;
				Array.Clear(TripleDesProvider.IV,0,TripleDesProvider.IV.Length);
						
				OPSTripleDesEncryptor OPSTripleDesEnc= new OPSTripleDesEncryptor(TripleDesProvider);
				byte [] byDecrypt;

				byDecrypt=OPSTripleDesEnc.Desencriptar(byEncrypt);

				strDecrypted = GetDataAsString(byDecrypt,0);

				char [] chTrim=  new char[1];
				chTrim[0] = '\0';

				strDecrypted = strDecrypted.TrimEnd(chTrim);

			}
			catch
			{								
			}

			return strDecrypted;
	
		}


		static private byte[] HexString_To_Bytes(string strInput)
		{
			// i variable used to hold position in string
			int i = 0;
			// x variable used to hold byte array element position
			int x = 0;
			// allocate byte array based on half of string length
			byte[] bytes = new byte[(strInput.Length) / 2];
			// loop through the string - 2 bytes at a time converting it to decimal equivalent and store in byte array
			while (strInput.Length > i + 1)
			{
				long lngDecimal = Convert.ToInt32(strInput.Substring(i, 2), 16);
				bytes[x] = Convert.ToByte(lngDecimal);
				i = i + 2;
				++x;
			}
			// return the finished byte array of decimal values
			return bytes;
		}

		///
		/// Convert byte_array to string
		///
		///
		static private string Bytes_To_HexString(byte[] bytes_Input)
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

		static private string GetDataAsString(byte[] data, int ignoreLastBytes)
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


		private static string GetKeyToApply(string key)
		{
			string strRes=KEY_MESSAGE_TCP_5;
			int iSum=0;
			int iMod;

			if (key.Length == 0)
			{
				strRes=KEY_MESSAGE_TCP_5;
			}
			else if (key.Length >= 24)
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




	}
}
