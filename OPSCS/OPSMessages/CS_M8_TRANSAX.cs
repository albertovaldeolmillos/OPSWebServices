using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OPS.Comm;
using OPS.Components.Data;
using System.Threading;

namespace OPS.Comm.Becs.Messages
{

	/*struct mbTRXRequestInfo
	 {
		 char mbTRXTerminalUser[26];
		 char mbTRXTerminalPass[26];
		 char mbTRXTerminalStore[10];
		 char mbTRXTerminalStation[10];
		 char mbTRXRequestId[42];
		 char mbTRXRequestInvoice[18];
		 char mbTRXRequestAmount[26];
		 char mbTRXRequestTrack2[82];
		 char mbTRXPrimaryAddress[32];
		 int  mbTRXPrimaryPort;
		 char mbTRXSecondaryAddress[32];
		 int  mbTRXSecondaryPort;
	 };*/

	[StructLayout(LayoutKind.Sequential, Pack=1 )]
	public struct TRXRequestInfo 
	{ 
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
		public string mbTRXTerminalUser;//[26];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
		public string mbTRXTerminalPass;//[26];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
		public string mbTRXTerminalStore;//[10];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
		public string mbTRXTerminalStation;//[10];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
		public string mbTRXRequestId;//[42];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
		public string mbTRXRequestInvoice;//[18];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
		public string mbTRXRequestAmount;//[26];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 82)]
		public string mbTRXRequestTrack2;//[82];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string mbTRXPrimaryAddress;//[32];
		public int  mbTRXPrimaryPort; //4
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string mbTRXSecondaryAddress;//[32];
		public int  mbTRXSecondaryPort;//4
	}


	/*struct mbTRXResponseInfo  {
	int TRXResponseOperStatus;
	char mbTRXResponseId[42];
	char mbTRXResponseTransStatus[200];
	char mbTRXResponseISORespCode[8];
	char mbTRXResponseApproval[18];
	char mbTRXResponseBatch[8];
	char mbTRXResponseInvoice[18];
	char mbTRXResponseCardName[42];
	char mbTRXResponseMaskedPAN[46];
	char mbTRXResponseAmount[26];
	};*/


	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public struct TRXResponseInfo 
	{ 
		public int TRXResponseOperStatus;//4
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
		public string mbTRXResponseId;//[42];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)]
		public string mbTRXResponseTransStatus;//[200];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
		public string mbTRXResponseISORespCode;//[8];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
		public string mbTRXResponseApproval;//[18];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
		public string mbTRXResponseBatch;//[8];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
		public string mbTRXResponseInvoice;//[18];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 42)]
		public string mbTRXResponseCardName;//[42];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 46)]
		public string mbTRXResponseMaskedPAN;//[46];
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
		public string mbTRXResponseAmount;//[26];

	}


	/// <summary>
	/// Summary description for CS_M8_TRANSAX.
	/// </summary>
	public class CS_M8_TRANSAX
	{

		static private  int C_RES_OK = 0;
		static private int C_RES_NOK = -1;
		static private object lockObject=null;
		static bool	_TRXTrace=false;
		static private string 		_TRXPrimaryAddress="216.130.225.11";
		static private int			_TRXPrimaryPort=60051;
		static private string		_TRXSecondaryAddress="216.130.225.11";
		static private int			_TRXSecondaryPort=60051;

		public string 		_TRXTerminalUser="";
		public string 		_TRXTerminalPass="";
		public string 		_TRXTerminalStore="";
		public string 		_TRXTerminalStation="";
		public string 		_TRXRequestId="";
		public string 		_TRXRequestInvoice="";
		public string 		_TRXRequestAmount="";
		public string 		_TRXRequestTrack2="";

		public int			_TRXResponseOperStatus=-1;
		public string 		_TRXResponseId="";
		public string 		_TRXResponseTransStatus="";
		public string 		_TRXResponseISORespCode="";
		public string 		_TRXResponseApproval="";
		public string 		_TRXResponseBatch="";
		public string 		_TRXResponseInvoice="";
		public string 		_TRXResponseCardName="";
		public string 		_TRXResponseMaskedPAN="";
		public string 		_TRXResponseAmount="";



		[DllImport("kernel32.dll")]
		static extern bool SetDllDirectory(string lpPathName); 

		[DllImport(@"TRXPayGateDemo.dll", EntryPoint="TRXAuthorize", CharSet=CharSet.Ansi )]
		public static extern int TRXAuthorize(TRXRequestInfo oRequestInfo, ref TRXResponseInfo oResponseInfo,bool m_bLogFile);	
		/// <summary>
		/// Constructor
		/// </summary>
		public CS_M8_TRANSAX()
		{
			int iRslt = C_RES_NOK;
			_TRXTerminalUser="";
			_TRXTerminalPass="";
			_TRXTerminalStore="";
			_TRXTerminalStation="";
			_TRXRequestId="";
			_TRXRequestInvoice="";
			_TRXRequestAmount="";
			_TRXRequestTrack2="";

			_TRXResponseOperStatus=-1;
			_TRXResponseId="";
			_TRXResponseTransStatus="";
			_TRXResponseISORespCode="";
			_TRXResponseApproval="";
			_TRXResponseBatch="";
			_TRXResponseInvoice="";
			_TRXResponseCardName="";
			_TRXResponseMaskedPAN="";
			_TRXResponseAmount="";

		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns>
		/// C_RES_OK : Result OK
		/// C_RES_NOK: Result NOK
		/// </returns>
		public int  TRXAuthorize()
		{
			int iRslt = C_RES_NOK;
			string szFunc = "CS_M8_TRANSAX::TRXAuthorize ";
			ILogger logger = null;
			TRXRequestInfo oRequestInfo;
			TRXResponseInfo oResponseInfo;
			

			if (lockObject==null)
			{

/*
 * 
 * 	<!-- TRANSAX -->
	<!-- DLL Paths needed for Transax libreries -->
	<add key = "TransaxDLLPath" value = "C:\Inetpub\OPSServices\Common\WIN32\TRXPayGateDemo" />
	<!-- Transax Primary Server -->
	<add key = "TrxPrimaryServer" value = "216.130.225.11" />
	<!-- Transax Primary Server Port -->
	<add key = "TrxPrimaryServerPort" value = "60051" />
	<!-- Transax Secundary Server -->
	<add key = "TrxSecundaryServer" value = "216.130.225.11" />
	<!-- Transax Secundary Server Port -->
	<add key = "TrxSecundaryServerPort" value = "60051" />
	<!-- Transax Trace Parameter -->
	<add key = "TrxTrace" value = "1" />	
 * */

				lockObject=new object();

				try
				{
					System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
								
					
					_TRXPrimaryAddress=(string)appSettings.GetValue("TrxPrimaryServer",typeof(string));
					_TRXPrimaryPort=(int)appSettings.GetValue("TrxPrimaryServerPort",typeof(int));
					_TRXSecondaryAddress=(string)appSettings.GetValue("TrxSecundaryServer",typeof(string));
					_TRXSecondaryPort=(int)appSettings.GetValue("TrxSecundaryServerPort",typeof(int));
					_TRXTrace=(((int)appSettings.GetValue("TrxTrace",typeof(int)))==1);


					string delimStr = ";";
					char [] delimiter = delimStr.ToCharArray();
					string [] splitDLLDirectories = null;
					string strDLLDirectories=(string)appSettings.GetValue("TransaxDLLPath",typeof(string));
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
				
				oRequestInfo =new TRXRequestInfo();
				oResponseInfo=new TRXResponseInfo();


				oRequestInfo.mbTRXPrimaryAddress=_TRXPrimaryAddress;
				oRequestInfo.mbTRXPrimaryPort=_TRXPrimaryPort;
				oRequestInfo.mbTRXSecondaryAddress=_TRXSecondaryAddress;
				oRequestInfo.mbTRXSecondaryPort=_TRXSecondaryPort;
				oRequestInfo.mbTRXTerminalUser=_TRXTerminalUser;
				oRequestInfo.mbTRXTerminalPass=_TRXTerminalPass;
				oRequestInfo.mbTRXTerminalStore=_TRXTerminalStore;
				oRequestInfo.mbTRXTerminalStation=_TRXTerminalStation;
				oRequestInfo.mbTRXRequestId=_TRXRequestId;
				oRequestInfo.mbTRXRequestInvoice=_TRXRequestInvoice;
				oRequestInfo.mbTRXRequestAmount=_TRXRequestAmount;
				oRequestInfo.mbTRXRequestTrack2=_TRXRequestTrack2;


				
				lock(lockObject)
				{
					iRslt = TRXAuthorize(oRequestInfo, ref oResponseInfo, _TRXTrace);
				}					
					
				_TRXResponseOperStatus=oResponseInfo.TRXResponseOperStatus;
				_TRXResponseId=oResponseInfo.mbTRXResponseId;
				_TRXResponseTransStatus=oResponseInfo.mbTRXResponseTransStatus;
				_TRXResponseISORespCode=oResponseInfo.mbTRXResponseISORespCode;
				_TRXResponseApproval=oResponseInfo.mbTRXResponseApproval;
				_TRXResponseBatch=oResponseInfo.mbTRXResponseBatch;
				_TRXResponseInvoice=oResponseInfo.mbTRXResponseInvoice;
				_TRXResponseCardName=oResponseInfo.mbTRXResponseCardName;
				_TRXResponseMaskedPAN=oResponseInfo.mbTRXResponseMaskedPAN;
				_TRXResponseAmount=oResponseInfo.mbTRXResponseAmount;
					
			
			}
			catch(Exception e)
			{
				Trace.Write(szFunc + e.Message);
				if(logger !=null)
					logger.AddLog(szFunc +  e.Message + iRslt.ToString() ,LoggerSeverities.Debug);

				iRslt = C_RES_NOK;
			}
			finally
			{
			}

			return iRslt;
		}
		
	}
}
