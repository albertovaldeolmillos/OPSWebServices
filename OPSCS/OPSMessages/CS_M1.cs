using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OPS.Comm;
using OPS.Components.Data;
using System.Threading;
using OPSCompute;
//using OPSCompute;

namespace CS_OPS_TesM1
{
	
	/// <summary>
	/// 
	/// </summary>
	public class CS_M1
	{
		static public long C_RES_OK = 1;
		static public long C_RES_NOK = -1;
		static private object lockObject=null;
		static private bool bInitialized=false;
		
		StringBuilder m_StrIn;  // Parameter IN
		StringBuilder m_StrOut; // Parameter OUT
		StringBuilder m_StrOutM50; // Parameter OUT
		static private StringBuilder m_StrRegParams = new StringBuilder("");
		bool m_bApplyHistory;
		bool m_bUseDefaultArticleDef;
		private string m_strComputeDllPath;

		[DllImport("kernel32.dll")]
		static extern bool SetDllDirectory(string lpPathName); 

		/// <summary>
		/// Constructor
		/// </summary>
		public CS_M1()
		{
			m_StrIn = new StringBuilder("");
			m_StrOut = new StringBuilder("");
			m_StrOutM50 = new StringBuilder("");
			m_bApplyHistory = true;
			m_bUseDefaultArticleDef=false;
			m_strComputeDllPath = "";
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns>
		/// C_RES_OK : Result OK
		/// C_RES_NOK: Result NOK
		/// </returns>
		public long Exectue()
		{
            String strOut = string.Empty;
            String strOutM50 = string.Empty;

            long lRslt = C_RES_OK;
			string szFunc = "CS_M1::Exectue ";
			ILogger logger = null;
			
			if (lockObject==null)
			{

				lockObject=new object();

				try
				{
					System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
					
					string strRegParamsPath=(string)appSettings.GetValue("M1RegParamsPath",typeof(string));
					if (m_strComputeDllPath.Length > 0)
						strRegParamsPath = m_strComputeDllPath;
					m_StrRegParams = new StringBuilder(strRegParamsPath);
					
					string delimStr = ";";
					char [] delimiter = delimStr.ToCharArray();
					string [] splitDLLDirectories = null;
					string strDLLDirectories=(string)appSettings.GetValue("M1DLLPath",typeof(string));
					splitDLLDirectories=strDLLDirectories.Split(delimiter);

					for (int i=0;i<splitDLLDirectories.Length;i++)
					{
						splitDLLDirectories[i]=splitDLLDirectories[i].Trim();
						if (splitDLLDirectories[i].Length>0)
						{
							SetDllDirectory(splitDLLDirectories[i]);
						}
					}					
				}
				catch
				{

				}

			}



			try
			{
				logger = DatabaseFactory.Logger;
				if(logger !=null)
					logger.AddLog(szFunc +  "Creating StringBuilder" ,LoggerSeverities.Debug);

				if (m_strComputeDllPath.Length > 0)
				{
					logger.AddLog(szFunc + "Using DLL path - " + m_strComputeDllPath, LoggerSeverities.Debug);
					m_StrRegParams.Clear();
					m_StrRegParams.Append(m_strComputeDllPath);
				}

				if (!bInitialized)
				{
					lock(lockObject)
					{
                        
						lRslt = OPSComputeM1.GetInstance().fnM1(m_StrIn.ToString(),m_StrRegParams.ToString(),ref strOut, ref strOutM50, 32768,m_bApplyHistory,m_bUseDefaultArticleDef);
					}					
				}
				else
				{
					lRslt = OPSComputeM1.GetInstance().fnM1(m_StrIn.ToString(),m_StrRegParams.ToString(), ref strOut, ref strOutM50,32768,m_bApplyHistory,m_bUseDefaultArticleDef);
				}
					
				if(lRslt != C_RES_OK)
				{
					StrOut = "";
					StrOutM50 = "";

				}
				else
				{
                    m_StrOut.Insert(0, strOut);
                    m_StrOutM50.Insert(0, strOutM50);
					bInitialized = true;
				}					
			
			}
			catch(Exception e)
			{
				Trace.Write(szFunc + e.Message);
				if(logger !=null)
					logger.AddLog(szFunc +  e.Message + lRslt.ToString() ,LoggerSeverities.Debug);

				lRslt = C_RES_NOK;
			}
			finally
			{
                strOut = null;
                strOutM50 = null;
            }

			return lRslt;
		}
		

		public string StrIn
		{
			get
			{
				return m_StrIn.ToString();
			}
			set
			{
				StringBuilder dummy = new StringBuilder(value);
				m_StrIn = dummy;
				dummy = null;
			}
		}
		public string StrOut
		{
			get
			{
				return m_StrOut.ToString();
			}
			set
			{
				StringBuilder dummy = new StringBuilder(value);
				m_StrOut = dummy;
				dummy = null;
			}
		}
		public string StrOutM50
		{
			get
			{
				return m_StrOutM50.ToString();
			}
			set
			{
				StringBuilder dummy = new StringBuilder(value);
				m_StrOutM50 = dummy;
				dummy = null;
			}
		}	

		public bool ApplyHistory
		{
			get
			{
				return m_bApplyHistory;
			}
			set
			{
				m_bApplyHistory=value;
			}
		}
		
		public bool UseDefaultArticleDef
		{
			get
			{
				return m_bUseDefaultArticleDef;
			}
			set
			{
				m_bUseDefaultArticleDef=value;
			}
		}

		public string StrComputeDllPath
		{
			get
			{
				return m_strComputeDllPath;
			}
			set
			{
				m_strComputeDllPath = value;
			}
		}
	}
}
