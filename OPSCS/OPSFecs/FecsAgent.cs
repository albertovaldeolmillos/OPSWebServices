using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceProcess;

namespace OPS.Comm.Fecs
{
	public class FecsAgent : System.ServiceProcess.ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private FecsEngine _engine = null;

		public FecsAgent()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();
		}

		/// <summary> 
		/// Required method for Designer support - do not modify	
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "FECS Agent Service";
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
			System.GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			// The service is starting, so create a new engine and starts it
			_engine = new FecsEngine();
			_engine.Start();
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			if (_engine != null)
			{
				_engine.Stop();
				_engine = null;
			}
		}
	}
}
