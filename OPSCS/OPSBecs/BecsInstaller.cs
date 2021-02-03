using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		private System.ServiceProcess.ServiceInstaller becsInstaller;
		private System.ServiceProcess.ServiceProcessInstaller becsServiceProcessInstaller;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{

			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();

			this.becsServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.becsInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// becsServiceProcessInstaller
			// 
			this.becsServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.becsServiceProcessInstaller.Password = null;
			this.becsServiceProcessInstaller.Username = null;
			// 
			// becsInstaller
			// 
//			this.becsInstaller.DisplayName = "OPS Back End Communications Server Bilbao";
//			this.becsInstaller.ServiceName = "OPSBecsBilbao";
			this.becsInstaller.DisplayName = (string)appSettings.GetValue("ServiceDisplayName",typeof(string));
			this.becsInstaller.ServiceName = (string)appSettings.GetValue("ServiceName",typeof(string));

			string delimStr = ";";
			char [] delimiter = delimStr.ToCharArray();
			string [] splitDependencies = null;
			string strDependencies=(string)appSettings.GetValue("ServiceDependencies",typeof(string));
			splitDependencies=strDependencies.Split(delimiter);
			int nValues=0;

			for (int i=0;i<splitDependencies.Length;i++)
			{
				splitDependencies[i]=splitDependencies[i].Trim();
				if (splitDependencies[i].Length>0)
				{
					nValues++;
				}
			}

			if (nValues>0)
			{
				string [] serviceDependencies= new string[nValues];
				int j=0;
				for (int i=0;i<splitDependencies.Length;i++)
				{
					if (splitDependencies[i].Length>0)
					{
						serviceDependencies[j++]=splitDependencies[i];
					}
				}

				this.becsInstaller.ServicesDependedOn = serviceDependencies;


			}
			
			becsInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			
			
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																					  this.becsServiceProcessInstaller,
																					  this.becsInstaller});

		}
		#endregion
	}
}
