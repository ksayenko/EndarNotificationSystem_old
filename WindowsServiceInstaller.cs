using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Windows.Forms;
using System.ServiceProcess;


namespace NotificationSystem
{
    [RunInstaller(true)]
    public partial class WindowsServiceInstaller : System.Configuration.Install.Installer
    {
        public WindowsServiceInstaller()
        {
            InitializeComponent();

            ServiceProcessInstaller serviceProcessInstaller =new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;

            serviceProcessInstaller.Username = "Kateryna.Sayenko@tetratech.com";          
            //serviceProcessInstaller.Password = "abc";

            //serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            serviceProcessInstaller.Password = null;
            serviceProcessInstaller.Username = null;


            //# Service Information
            serviceInstaller.DisplayName = Properties.EndarProcessorSettings.Default.ServiceName;// "EnDARDatabaseNotificationService";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            //# This must be identical to the WindowsService.ServiceBase name
            //# set in the constructor of WindowsService.cs
            serviceInstaller.ServiceName = Properties.EndarProcessorSettings.Default.ServiceName; //"EnDARDatabaseNotificationService";

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }

        private void serviceInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
        }
        public override void Install(System.Collections.IDictionary stateSaver)
        {
          
            base.Install(stateSaver);
            string path1 = Context.Parameters["assemblypath"];         

            //Properties.EndarProcessorSettings.Default.LogPath = path1;
            //Properties.EndarProcessorSettings.Default.Save();        

            Properties.EndarProcessorSettings.Default.Save();
          
            // Do something with path.
        } 
    }
}
