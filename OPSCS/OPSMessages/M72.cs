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
	/// m72 - Update request of the data for a table.
	/// </summary>
	internal sealed class Msg72 : MsgReceived, IRecvMessage
	{


		public const int DEF_OPERSTATE_BYCING_CLOSED=0;
		public const int DEF_OPERSTATE_BYCING_ASSIGNED=1;
		public const int DEF_OPERSTATE_BYCING_OPEN=2;
		public const int DEF_OPERSTATE_BYCING_CANCELLED=3;


		public const int DEF_OPERTYPE_BYCING_FIRST_GET=0;
		public const int DEF_OPERTYPE_BYCING_AMPLIATION=1;


		public const int DEF_CARD_OPERSTATE_BYCING_CLOSED=0;
		public const int DEF_CARD_OPERSTATE_BYCING_OPEN=1;
		public const int DEF_CARD_OPERSTATE_BYCING_CANCEL=2;

		public const int DEF_INCIDENCE_STATUS_GENERATED=0;
		public const int DEF_INCIDENCE_STATUS_SOLVING=1;
		public const int DEF_INCIDENCE_STATUS_SOLVED=2;

		public const int DEF_INFRACTION_STATUS_GENERATED=0;
		public const int DEF_INFRACTION_STATUS_USER_SANCTIONED=1;
		public const int DEF_INFRACTION_STATUS_USER_SANCTION_FINISH=2;
		public const int DEF_INFRACTION_STATUS_CANCELLED=3;

		public const int DEF_USER_STATUS_ACTIVE=0;
		public const int DEF_USER_STATUS_BLOCKED=1;
		public const int DEF_USER_STATUS_SANCTIONED=2;
		public const int DEF_USER_STATUS_ACTIVATION_PENDING=3;

		public const int DEF_MIN_DAYS_FOR_BLOCKING_USER=40000;


		private int _operation=-1;
		private int	_type=-1;
		private int _status=-1;
		private int _user=-1;
		private int _sourcePlat=-1;
		private int _destinationPlat=-1;
		private int	_unit=-1;
		private string	_movDate="";
		private string	_iniDate="";
		private string	_iniDate0="";
		private string	_endDate="";
		private int _npr=-1;
		private int _withHolding=-1;
		private int _ticketNumber1=-1;
		private int _ticketNumber2=-1;
		private int _quantity=-1;
		private int _time=-1;
		private int _online=0;
		private long		_lChipCardId	= -1;



/*
 * 
 * <m72 id="17" ret="1">
 *					<o>2</o>
 *					<ot>0</ot>
 *					<os>0</os>
 *					<us>1</us>
 *					<ps>8</ps>
 *					<pd>8</pd>
 *					<u>103</u>
 *					<d>152631170310</d>
 *					<d0>152319170310</d0>
 *					<d1>152319170310</d1>
 *					<d2>152627170310</d2>
 *					<npr>0</npr>
 *					<wh>0</wh>
 *					<tcn1>1</tcn1>
 *					<tcn2>2</tcn2>
 * </m72>
 * 
 * 
 * 
 */



		/// <summary>
		/// Constructs a new m72 with the data of the message.
		/// </summary>
		/// <param name="msgXml">XML Document with the message</param>
		public Msg72(XmlDocument msgXml) : base(msgXml) {}

		/// <summary>
		/// Parses the XML and stores data in private member variables
		/// </summary>
		protected override void DoParseMessage()
		{
			foreach (XmlNode n in _root.ChildNodes)
			{
				switch (n.Name)
				{
					case "o": _operation = Convert.ToInt32(n.InnerText); break;
					case "ot": _type = Convert.ToInt32(n.InnerText); break;
					case "os": _status = Convert.ToInt32(n.InnerText); break;
					case "us": _user = Convert.ToInt32(n.InnerText); break;
					case "ps": _sourcePlat = Convert.ToInt32(n.InnerText); break;
					case "pd": _destinationPlat = Convert.ToInt32(n.InnerText); break;
					case "u": _unit = Convert.ToInt32(n.InnerText); break;
					case "d": _movDate = n.InnerText; break;
					case "d0": _iniDate0 = n.InnerText; break;
					case "d1": _iniDate =n.InnerText; break;
					case "d2": _endDate = n.InnerText; break;
					case "npr": _npr = Convert.ToInt32(n.InnerText); break;		
					case "wh": _withHolding = Convert.ToInt32(n.InnerText); break;		
					case "tcn1": _ticketNumber1 = Convert.ToInt32(n.InnerText); break;		
					case "tcn2": _ticketNumber2 = Convert.ToInt32(n.InnerText); break;		
					case "q": _quantity = Convert.ToInt32(n.InnerText); break;	
					case "t": _time = Convert.ToInt32(n.InnerText); break;	
					case "om": _online = Convert.ToInt32(n.InnerText); break;	
					case "chi": _lChipCardId = Convert.ToInt32(n.InnerText); break;	

			
					

				}
			}
		}

		#region DefinedRootTag(m72)
		/// <summary>
		/// Returns the root tag that each type of message must have.
		/// That method MUST be defined on each class, althought is not defined in any interface or base class
		/// </summary>
		public static string DefinedRootTag { get { return "m72"; } }
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
			ILogger logger = null;
			bool bRes=false;
			bool bExist=false;


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
					oraCmd.CommandText="";
					
					/*
					 * CERRADA O
					 * ABIERTA O
					 * CANCELADA
					 */
					if ((_status==DEF_OPERSTATE_BYCING_CLOSED)||(_status==DEF_OPERSTATE_BYCING_OPEN)||(_status==DEF_OPERSTATE_BYCING_CANCELLED))
					{
						int iOpId=-1;

						if (ExistOperation(ref bExist,ref oraDBConn,ref oraCmd, ref iOpId))
						{


							if (bExist)
							{	
								

								oraCmd.CommandText =	string.Format("update BYCYCLES_OPERATIONS  set "+
																			"BYCOP_EX_TRIGGER=1, "+
																			"BYCOP_TYPE={0}, "+
																			"BYCOP_STATUS={1}, "+
																			"BYCOP_SRC_PLAT_ID={2}, "+
																			"BYCOP_DST_PLAT_ID={3}, "+
																			"BYCOP_MOVDATE=to_date('{4}', 'HH24MISSDDMMYY'), "+
																			"BYCOP_INIDATE=to_date('{5}', 'HH24MISSDDMMYY'), "+
																			"BYCOP_ENDDATE={6}, "+
																			"BYCOP_NUM_NO_POSIBLE_RETURNS={7}, "+
																			"{9} "+
																			"{10} "+
																			"{13} "+
																			"{14} "+
																			"{15} "+
																			"BYCOP_ONLINEOP={16}, "+
																			"BYCOP_WITHHOLDING={8} "+
																			"where  BYCOP_BUSER_ID = {11} "+
																			"  and  TO_CHAR(BYCOP_INIDATE0,'HH24MISSDDMMYY') = '{12}'",
																			_type,_status,_sourcePlat,
																			(_destinationPlat!=-1)?_destinationPlat.ToString():"NULL",
																			_movDate,_iniDate,
																			(_endDate.Length>0)?"to_date('"+_endDate+"', 'HH24MISSDDMMYY')":"NULL",
																			_npr,
																			_withHolding,
																			(_ticketNumber1!=-1)?"BYCOP_TICKETNUM1="+_ticketNumber1.ToString()+",":"",
																			(_ticketNumber2!=-1)?"BYCOP_TICKETNUM2="+_ticketNumber2.ToString()+",":"",
																			_user,_iniDate0,
																			(_lChipCardId!=-1)?"BYCOP_CHIPCARD_ID="+_lChipCardId.ToString()+",":"",
																			(_quantity!=-1)?"BYCOP_VALUE="+_quantity.ToString()+",":"",
																			(_time!=-1)?"BYCOP_DURATION="+_time.ToString()+",":"",							
																			_online);

								oraCmd.ExecuteNonQuery();


								if ((_status==DEF_OPERSTATE_BYCING_CLOSED)&&(_quantity!=-1)&&(_time!=-1)&&(_endDate.Length>0))
								{
									
									bool bMustSanction=false;
									int iSanctionType=-1;
									int iTimeExceeed=-1; 
									int iMoneySanction=-1;
									int iDaysSanctioned=-1;

									if (MustSanction(ref bMustSanction,ref oraDBConn,ref oraCmd, 
										ref iSanctionType, ref iTimeExceeed, ref iMoneySanction, ref iDaysSanctioned))
									{


										if (bMustSanction)
										{
											if (InsertInfraction(ref oraDBConn,ref oraCmd, 
												iSanctionType, iTimeExceeed, 
												iMoneySanction, iDaysSanctioned,iOpId))
											{
												if (iDaysSanctioned<DEF_MIN_DAYS_FOR_BLOCKING_USER)
												{
													if (SanctionUser(ref oraDBConn,ref oraCmd,iDaysSanctioned))
													{
														bRes=true;
													}
	
												}
												else
												{
													//Usuario Bloqueado
													if (BlockUser(ref oraDBConn,ref oraCmd))
													{
														bRes=true;
													}

												}
											}
										
										}
										else
										{
											bRes=true;
										}
									}
								}
								else
								{
									bRes=true;
								}															 
							}
						}
					}
					else if (_status==DEF_OPERSTATE_BYCING_ASSIGNED) /*ASIGNADA*/
					{
						int iOpId=-1;
						
						if (ExistOperation(ref bExist,ref oraDBConn,ref oraCmd, ref iOpId))
						{
							if (!bExist)
							{
								oraCmd.CommandText =	string.Format("INSERT INTO BYCYCLES_OPERATIONS ("+
									"BYCOP_TYPE, "+
									"BYCOP_STATUS, "+
									"BYCOP_SRC_PLAT_ID, "+
									"BYCOP_BUSER_ID, "+
									"BYCOP_MOVDATE, "+
									"BYCOP_INIDATE, "+
									"BYCOP_INIDATE0, "+
									"BYCOP_NUM_NO_POSIBLE_RETURNS, "+
									"BYCOP_WITHHOLDING, "+
									"BYCOP_TICKETNUM1, "+
									"BYCOP_CHIPCARD_ID, "+
									"BYCOP_ONLINEOP,BYCOP_EX_TRIGGER) VALUES "+
									"({0},{1},{2},{3},to_date('{4}', 'HH24MISSDDMMYY'), "+
									"to_date('{5}', 'HH24MISSDDMMYY'), "+
									"to_date('{6}', 'HH24MISSDDMMYY'),{7},{8},{9},{10},{11},1) ",
									_type,_status,_sourcePlat,_user,
									_movDate,_iniDate,_iniDate0,_npr,
									_withHolding,
									(_ticketNumber1!=-1)?_ticketNumber1.ToString():"NULL",
									(_lChipCardId!=-1)?_lChipCardId.ToString():"NULL",
									_online);


								oraCmd.ExecuteNonQuery();
								bRes=true;
							}
							else
							{
								bRes=true;
							}
						}

					}
					
				}

				if (bRes)
				{
					res = ReturnAck(AckMessage.AckTypes.ACK_PROCESSED);
				}
				else
				{
					if(logger != null)
						logger.AddLog("[Msg72:Process]: Error.",LoggerSeverities.Error);
					res = ReturnNack(NackMessage.NackTypes.NACK_ERROR_BECS);
				}
			}
			catch(Exception e)
			{
				if(logger != null)
					logger.AddLog("[Msg72:Process]: Error: "+e.Message,LoggerSeverities.Error);
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

		bool ExistOperation(ref bool bExist, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,ref int iOpId)
		{
			bool bRes=true;
			bExist=false;
			OracleDataReader dr= null;
			iOpId=-1;

			try
			{
				oraCmd.CommandText =	string.Format("select bycop_id "+    
													  "from   BYCYCLES_OPERATIONS "+
													  "where  BYCOP_BUSER_ID = {0} "+
													  "  and  TO_CHAR(BYCOP_INIDATE0,'HH24MISSDDMMYY') = '{1}'",
													  _user, _iniDate0 );
				


				dr=oraCmd.ExecuteReader();


				if (dr.Read())
				{
					iOpId=dr.GetInt32(dr.GetOrdinal("bycop_id"));
					bExist=true;

				}

			}
			catch
			{
				bRes=false;
			}
			finally
			{
				if (dr!=null)
				{
					dr.Close();
					dr.Dispose();
					dr = null;
				}

			}
			return bRes;
			
		}


		bool MustSanction(ref bool bMustSanction, ref OracleConnection oraDBConn,ref OracleCommand oraCmd,
						  ref int iSanctionType, ref int iTimeExceeed, ref int iMoneySanction, ref int iDaysSanctioned )
		{
			bool bRes=true;
			bMustSanction=false;
			OracleDataReader dr= null;


			try
			{
				oraCmd.CommandText =	string.Format("select bycinftq_id,"+
					"		{0} - bycinftq_max_time bycinftq_time_exceeded,"+
					" 		bycinftq_value,"+
					"		bycinftq_disable_time"+
					"	from bicycles_infraction_type_quant"+
					"	where to_date('{1}', 'HH24MISSDDMMYY') >= bycinftq_dateini"+
					"	and to_date('{1}', 'HH24MISSDDMMYY') < bycinftq_dateend"+
					"	and {0} >= bycinftq_ini_time_exceeded"+
					"	and {0} <= bycinftq_end_time_exceeded",					
					_time, _endDate );
				

				dr=oraCmd.ExecuteReader();


				if (dr.Read())
				{
					iSanctionType=dr.GetInt32(dr.GetOrdinal("bycinftq_id"));
					iTimeExceeed=dr.GetInt32(dr.GetOrdinal("bycinftq_time_exceeded"));
					iMoneySanction=dr.GetInt32(dr.GetOrdinal("bycinftq_value"));
					iDaysSanctioned=dr.GetInt32(dr.GetOrdinal("bycinftq_disable_time"));
					bMustSanction=true;

				}


			}
			catch
			{
				bRes=false;
				bMustSanction=false;
			}
			finally
			{
				if (dr!=null)
				{
					dr.Close();
					dr.Dispose();
					dr = null;
				}

			}

			return bRes;
			
		}


		bool InsertInfraction(ref OracleConnection oraDBConn,ref OracleCommand oraCmd,
			int iSanctionType, int iTimeExceeed, int iMoneySanction, int iDaysSanctioned,int iOpId )
		{
			bool bRes=false;


			try
			{
				oraCmd.CommandText =	string.Format("insert into bicycles_infractions "+
													   "(bycinf_type_id,"+
													    "bycinf_buser_id,"+
														"bycinf_date,"+
														"bycinf_status,"+
														"bycinf_value,"+
														"bycinf_disable_time,"+
														"bycinf_time_exceeded,"+
														"bycinf_bycop_id,"+
														"bycinf_ex_trigger) "+
														"values "+
														"({0},{1}, to_date('{2}', 'HH24MISSDDMMYY'),{3},{4},{5},{6},{7},1)",					
														iSanctionType, _user, _endDate,
														DEF_INFRACTION_STATUS_USER_SANCTIONED,iMoneySanction,iDaysSanctioned,
														iTimeExceeed,iOpId);
				
				if (oraCmd.ExecuteNonQuery()==1)
				{
					bRes=true;					
				}



			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}



		bool SanctionUser(ref OracleConnection oraDBConn,ref OracleCommand oraCmd,int iDaysSanctioned )
		{
			bool bRes=false;


			try
			{
				oraCmd.CommandText =	string.Format("update bicycles_users set "+
														"buser_status={0}, "+
														"buser_disabled_until= to_date('{1}', 'HH24MISSDDMMYY')+{2}, "+
														"buser_ex_trigger=1 "+
													  "where buser_id={3}",
														DEF_USER_STATUS_SANCTIONED, _endDate,iDaysSanctioned,_user);
				
				if (oraCmd.ExecuteNonQuery()==1)
				{
					bRes=true;					
				}



			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}


		bool BlockUser(ref OracleConnection oraDBConn,ref OracleCommand oraCmd )
		{
			bool bRes=false;


			try
			{
				oraCmd.CommandText =	string.Format("update bicycles_users set "+
														"buser_status={0}, "+
														"buser_ex_trigger=1 "+
														"where buser_id={1}",
														DEF_USER_STATUS_BLOCKED,_user);
				
				if (oraCmd.ExecuteNonQuery()==1)
				{
					bRes=true;					
				}



			}
			catch
			{
				bRes=false;
			}

			return bRes;
			
		}




		#endregion
	
	}	
}
