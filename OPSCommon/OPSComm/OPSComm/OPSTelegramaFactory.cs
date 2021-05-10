using System;

namespace OPS.Comm
{
	/// <summary>
	/// Summary description for OPSTelegramaFactory.
	/// </summary>
	public class OPSTelegramaFactory
	{

		public const int FRAMETYPE_NOENCRYPT=1;
		public const int FRAMETYPE_ENCRYPT=2;

		public OPSTelegramaFactory()
		{
		}

		public static OPSTelegrama CreateOPSTelegrama(int id,string data)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			int iFrameType=(int)appSettings.GetValue("FrameType",typeof(int));
			OPSTelegrama result=null;

			switch (iFrameType)
			{
				case FRAMETYPE_NOENCRYPT:
					result=new OPSTelegramaFrame1(id,data);
					break;
				case FRAMETYPE_ENCRYPT:
					result=new OPSTelegramaFrame2(id,data);
					break;
				default:
					result=new OPSTelegramaFrame1(id,data);
					break;
			}

			return result;


		}

		public static OPSTelegrama CreateOPSTelegrama(byte[] data)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			int iFrameType=(int)appSettings.GetValue("FrameType",typeof(int));
			OPSTelegrama result=null;

			switch (iFrameType)
			{
				case FRAMETYPE_NOENCRYPT:
					result=new OPSTelegramaFrame1(data);
					break;
				case FRAMETYPE_ENCRYPT:
					result=new OPSTelegramaFrame2(data);
					break;
				default:
					result=new OPSTelegramaFrame1(data);
					break;
			}

			return result;


		}

        public static OPSTelegrama CreateOPSTelegrama(byte[] data,int dataLength)
        {
            System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
            int iFrameType = (int)appSettings.GetValue("FrameType", typeof(int));
            OPSTelegrama result = null;
			int nEncryptionOption = 0;

			try
			{	
				int nTempValue = (int)appSettings.GetValue("ThreadTime", typeof(int));

				if ( nTempValue == 500 )
					nEncryptionOption = 1;
				else if ( nTempValue == 1000 )
					nEncryptionOption = 2;
				else
					nEncryptionOption = 0;
			}
			catch
			{
				nEncryptionOption = 0;
			}

            switch (iFrameType)
            {
                case FRAMETYPE_NOENCRYPT:
                    result = new OPSTelegramaFrame1(data,dataLength);
                    break;
                case FRAMETYPE_ENCRYPT:
                    result = new OPSTelegramaFrame2(data,dataLength, nEncryptionOption);
                    break;
                default:
                    result = new OPSTelegramaFrame1(data, dataLength);
                    break;
            }

            return result;


        }

		public static OPSTelegrama CreateOPSTelegrama(byte[] data,int dataLength, bool bUseNewKey)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			int iFrameType = (int)appSettings.GetValue("FrameType", typeof(int));
			OPSTelegrama result = null;

			switch (iFrameType)
			{
				case FRAMETYPE_NOENCRYPT:
					result = new OPSTelegramaFrame1(data,dataLength);
					break;
				case FRAMETYPE_ENCRYPT:
					result = new OPSTelegramaFrame2(data,dataLength, bUseNewKey);
					break;
				default:
					result = new OPSTelegramaFrame1(data, dataLength);
					break;
			}

			return result;


		}

		public static OPSTelegrama CreateOPSTelegrama(int id,string data, bool bUseNewKey)
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			int iFrameType=(int)appSettings.GetValue("FrameType",typeof(int));
			OPSTelegrama result=null;

			switch (iFrameType)
			{
				case FRAMETYPE_NOENCRYPT:
					result=new OPSTelegramaFrame1(id,data);
					break;
				case FRAMETYPE_ENCRYPT:
					result=new OPSTelegramaFrame2(id,data, bUseNewKey);
					break;
				default:
					result=new OPSTelegramaFrame1(id,data);
					break;
			}

			return result;
		}
	}
}
