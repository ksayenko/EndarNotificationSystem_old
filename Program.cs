using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NotificationSystem
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            
            try
            {
                LogHandler erh = null;
                System.ServiceProcess.ServiceBase[] ServicesToRun;
                // Change the following line to match.
                erh = new LogHandler("", "Program.Main()");
               
                NotificationService nsc = new NotificationService();
                erh = new LogHandler(nsc.ServiceName,"program main");
                erh.LogWarning("starting");
              
                ServicesToRun = new System.ServiceProcess.ServiceBase[] { nsc };  
              
                erh.WindowsServiceName = nsc.ServiceName;
                erh.LogWarning(nsc.ServiceName);
                System.ServiceProcess.ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                //LogHandler erh1 = new LogHandler("EnDARDatabaseNotificationService", "program main");                
                //erh1.LogWarning("error in program main" + ex.ToString());
                //erh1.LogError("Program.main()", ex);       
            }

        }

        public static void RestartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            LogHandler erh = null;
          
            try
            {
                int millisec1 = Environment.TickCount;
                erh = new LogHandler("", "RestartService");
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                LogHandler erh1 = new LogHandler("EnDARDatabaseNotificationService", "RestartService");
                erh1.LogWarning("error in RestartService" + ex.ToString());
                erh1.LogError("RestartService", ex);       
            }
        }
    }
}
