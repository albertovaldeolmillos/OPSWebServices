using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		private System.ServiceProcess.ServiceProcessInstaller fecsServiceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller fecsInstaller;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
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
			System.GC.SuppressFinalize(this);
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();			

			this.fecsServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.fecsInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// fecsServiceProcessInstaller
			// 
			this.fecsServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.fecsServiceProcessInstaller.Password = null;
			this.fecsServiceProcessInstaller.Username = null;
			// 
			// fecsInstaller
			// 
			this.fecsInstaller.DisplayName = (string)appSettings.GetValue("ServiceDisplayName",typeof(string));
			this.fecsInstaller.ServiceName = (string)appSettings.GetValue("ServiceName",typeof(string));

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

				this.fecsInstaller.ServicesDependedOn = serviceDependencies;


			}			

			fecsInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;

			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																					  this.fecsServiceProcessInstaller,
																					  this.fecsInstaller});

		}
		#endregion
	}
}
