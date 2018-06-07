
using System;
using System.Web.UI;

namespace OPSWebServices
{
	/// <summary>
	/// Write to files ...
	/// </summary>
	public class HTTPWriter 
	{
		private string _contentType;
		private string _fileName;

		public HTTPWriter() 
		{
			_fileName = "";
			_contentType = "application/x-download";
		}

		public string ContentType
		{
			get 
			{
				return _contentType;
			} 
			set 
			{
				_contentType = value;
			}
		}

		public string FileName
		{
			get 
			{
				return _fileName;
			}
			set 
			{
				_fileName = value;
			}
		}

		public void Write(Page pPage, string contents)
		{
			WriteHeader(pPage);
			pPage.Response.Write(contents);
			WriteTail(pPage);
		}

		public void Write(Page pPage, byte[] contents)
		{
			WriteHeader(pPage);
			pPage.Response.BinaryWrite(contents);
			WriteTail(pPage);			
		}

		public void WriteHeader(Page pPage) 
		{
			pPage.Response.Clear();
			pPage.Response.Buffer= true;
			pPage.Response.ContentType = _contentType; //contentType;
			pPage.Response.Charset = "";
			//pPage.EnableViewState = false;
			if (_fileName != "")
				pPage.Response.AppendHeader("Content-Disposition", "attachment; filename=" + _fileName);
			
		}

		public void WriteTail(Page pPage)
		{
			pPage.Response.End();
		}
	}

}
