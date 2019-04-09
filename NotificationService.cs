using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SendMail;
using System.Windows.Forms;
using System.Timers;


namespace NotificationSystem
{
    /// <summary>
    /// NotificationServiceAndBatchesRun sends emails for edd uploads and others
    /// also it runs the batches which are stores in the config file 
    /// For this purpose we are using 2 timers.
    /// </summary>
    public partial class NotificationService : ServiceBase
    {
        private System.Timers.Timer nTimer = null;
        //private System.Timers.Timer nTimer_Batches = null;

        public ArrayList DATABASEUSER;
        public ArrayList DATASOURCE;
        public ArrayList DATABASEPWD;
        public ArrayList CATALOG;
        public string EMAILFROM;

        public string LOGPATH;
        public string EMAILPATH;
        
        public int TIMESPAN;
        public string SMTPSERVER;
        public string DEFAULTRECIPIENT;
        public int SENDTIME_SECONDS;       

        public int SMTPPORT;
        public bool ISBODYHTML;
        public bool ISNETWORKCREDENTIAL;
        public string DOMAIN;
        public int TIMEOUTCALLBACK;
        public bool ISENABLESSL;
        public string SMTPUSER;
        public string SMTPPWD;

        //system status
        public bool  IS_WRITE_TO_SYSTEM_TABLE;  
        public string SYSTEM_SECTION_NAME;
        public string SECTION_INFO1;
        public string SECTION_INFO2;
                    
        DataAccess da = null;
        LogHandler erh = null;
  
        public void SetDefaultValues()
        {
            DATABASEUSER = new ArrayList();
            DATABASEUSER.Add("Endar");

            DATASOURCE = new ArrayList();
            DATASOURCE.Add("Endar");

            DATABASEPWD = new ArrayList();
            DATABASEPWD.Add("Endar");

            LOGPATH = "";
            EMAILPATH = "";
            CATALOG = new ArrayList();
            CATALOG.Add("Endar");
            TIMESPAN = 10;
            SMTPSERVER = "";
            DEFAULTRECIPIENT = "";
            EMAILFROM = "endar@tetratech.com";
            SENDTIME_SECONDS = 60 * 30; //30 MINUTES
         
            SMTPPORT = 25;
            ISBODYHTML = true;
            ISNETWORKCREDENTIAL = false;
            DOMAIN = "";
            TIMEOUTCALLBACK = 1000;
            ISENABLESSL = false;
            SMTPUSER = "";
            SMTPPWD = "";

            IS_WRITE_TO_SYSTEM_TABLE = false;           
            SYSTEM_SECTION_NAME = "";
            SECTION_INFO1 = "";
         
            erh = new LogHandler("", "set default");          
        }


        public NotificationService()
        {
            InitializeComponent();
            SetDefaultValues();
            this.CanShutdown = true;
           
            this.ServiceName = Properties.EndarProcessorSettings.Default.ServiceName; //"EnDARDatabaseNotificationService";
            this.EventLog.Log = "Application";
            this.AutoLog = true;

            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
         
            
            ReadConfig();

            erh = new LogHandler(this.ServiceName, "NotificationServiceAndBatchesRun constructor");

            erh.WindowsServiceName = this.ServiceName;
            erh.LogWarning("In NotificationServiceAndBatchesRun constructor");                          

            try
            {
                nTimer = new System.Timers.Timer(TIMESPAN);
                nTimer.AutoReset = true;
                this.nTimer.Elapsed += new ElapsedEventHandler(nTimer_Tick);
             
                //nTimer_Batches = new System.Timers.Timer(TIMESPAN_FOR_BATCH);
                //nTimer_Batches.AutoReset = true;
                //this.nTimer_Batches.Elapsed += new ElapsedEventHandler(nTimer_Batches_Tick);
             
            }
            catch (Exception ex)
            {
                erh.LogWarning("Error in NotificationServiceAndBatchesRun constructor :" + ex.ToString());
            }
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();    
        }


        private void ReadConfig()
        {
            try
            {               
                Properties.EndarProcessorSettings.Default.Reload();

                DATABASEUSER = new ArrayList();
                foreach (string s1 in Properties.EndarProcessorSettings.Default.DatabaseUser)
                {
                    DATABASEUSER.Add(s1);
                    //erh.LogWarning("User " + s1);
                }

                LOGPATH = Properties.EndarProcessorSettings.Default.LogPath;
                EMAILPATH = LOGPATH + "\\EMAILS";
                TIMESPAN = Properties.EndarProcessorSettings.Default.TimeSpan;

                CATALOG = new ArrayList();
                foreach (string s1 in Properties.EndarProcessorSettings.Default.InitialCatalog)
                    CATALOG.Add(s1);
                DATABASEPWD = new ArrayList();
                foreach (string s1 in Properties.EndarProcessorSettings.Default.DatabasePassword)
                    DATABASEPWD.Add(s1);

                DATASOURCE = new ArrayList();
                foreach (string s1 in Properties.EndarProcessorSettings.Default.Data_Source)
                    DATASOURCE.Add(s1);

                SMTPSERVER = Properties.EndarProcessorSettings.Default.SMTPServer;
                DEFAULTRECIPIENT = Properties.EndarProcessorSettings.Default.DefaultRecipient;
                SENDTIME_SECONDS = Properties.EndarProcessorSettings.Default.Send_Time_Seconds;

                EMAILFROM = Properties.EndarProcessorSettings.Default.EmailFrom;
              
                SMTPPORT = Properties.EndarProcessorSettings.Default.SMTPPort;
                SMTPUSER = Properties.EndarProcessorSettings.Default.SMTPUser;
                SMTPPWD = Properties.EndarProcessorSettings.Default.SMTPPassword;

                ISBODYHTML = Properties.EndarProcessorSettings.Default.IsBodyHtml;

                //write to system_status_table
                IS_WRITE_TO_SYSTEM_TABLE = Properties.EndarProcessorSettings.Default.Write_to_System_Table;
                SYSTEM_SECTION_NAME = Properties.EndarProcessorSettings.Default.SystemSectionName;
                SECTION_INFO1 = Properties.EndarProcessorSettings.Default.SystemSectionInfo1;
                SECTION_INFO2 = Properties.EndarProcessorSettings.Default.SystemSectionInfo2;

                bool bUseInstallPathForLogging = Properties.EndarProcessorSettings.Default.UseInstallPathForLogging;
                if (bUseInstallPathForLogging)
                {
                    LOGPATH = Application.CommonAppDataPath + Path.DirectorySeparatorChar + "Logs";
                    Properties.EndarProcessorSettings.Default.LogPath = LOGPATH;
                    //Properties.EndarProcessorSettings.Default.Save();   
                    EMAILPATH = LOGPATH + "\\EMAILS";
                }
                erh.LogWarning("Stop Reading the config file; Log to " + LOGPATH + " UseInstallPathForLogging " + bUseInstallPathForLogging.ToString());
                //Properties.EndarProcessorSettings.Default.Save();
            }
            catch (Exception ex)
            {
                erh.LogWarning("error in ReadConfig: " + ex.ToString());
                SetDefaultValues();

                IS_WRITE_TO_SYSTEM_TABLE = true;
                SYSTEM_SECTION_NAME = "EndarNotification";
                SECTION_INFO1 = "LastRun";
                SECTION_INFO2 = "LastEmailSend";
            }

        }

        private string ReadFromArrayList(ArrayList al, int index)
        {
            string str = "<<none>>";
            try
            {
                if (al.Count > index)
                    str = al[index].ToString();
            }
            catch { }

            return str;

        }

        protected override void OnStart(string[] args)
        {
          
            erh.LogWarning("Endar notification service started. The time span is " + TIMESPAN.ToString(), true);
            if (IS_WRITE_TO_SYSTEM_TABLE)
            {
                int n = CATALOG.Count;
                for (int i = 0; i < n; i++)
                {
                    string ds = ReadFromArrayList(DATASOURCE, i);
                    string c = ReadFromArrayList(CATALOG, i);
                    string u = ReadFromArrayList(DATABASEUSER, i);
                    string p = ReadFromArrayList(DATABASEPWD, i);

                    da = new DataAccess(ds, c, u, p);
                    da.Update_System_Status_LastServiceRun(SYSTEM_SECTION_NAME, SECTION_INFO1, "Endar notification service started ");
                }
            }
           
            this.nTimer.Interval = TIMESPAN;
            //this.nTimer_Batches.Interval = TIMESPAN_FOR_BATCH;
            nTimer.AutoReset = true;
            nTimer.Enabled = true;
            //nTimer_Batches.AutoReset = true;
            //nTimer_Batches.Enabled = true;
            try
            {
                nTimer.Start();
                RunNotification();
            }
            catch (Exception ex)
            {
                erh.LogWarning("ERROR in OnStart "+ ex.Message + Environment.NewLine + ex.ToString()+ Environment.NewLine );
            } 


        }

   
        private void nTimer_Tick(object sender, ElapsedEventArgs e)
        {
            RunNotification();

            if (null != nTimer)
            {
                nTimer.Start(); // re - enable the timer
                nTimer.Enabled = true;
                erh.LogWarning("Timer re-enabled ");
            }
        }

        private void RunNotification()
        {
            erh.LogWarning("****************** Timer Ticks ************************");
              int n = CATALOG.Count;
              for (int ki = 0; ki < n; ki++)
              {
                  string ds = ReadFromArrayList(DATASOURCE, ki);
                  string c = ReadFromArrayList(CATALOG, ki);
                  string u = ReadFromArrayList(DATABASEUSER, ki);
                  string p = ReadFromArrayList(DATABASEPWD, ki);

                  erh.LogWarning("connection " + (ki+1).ToString() + " " + ds);

                  da = new DataAccess(ds, c, u, p);


                  if (IS_WRITE_TO_SYSTEM_TABLE)
                  {
                      da.Update_System_Status_LastServiceRun(SYSTEM_SECTION_NAME, SECTION_INFO1);
                  }
                  //erh.LogWarning("database user " + Properties.EndarProcessorSettings.Default.DatabaseUser);



                  DataTable dsend = da.GetAllMessagesToSend();
                  if (dsend == null)
                      erh.LogWarning("can't get data from the database");
                  else if (dsend != null && dsend.Rows.Count == 0)
                      erh.LogWarning("No emails to send");
                  else
                  {

                      erh.LogWarning("Sending " + dsend.Rows.Count.ToString() + " emails.");

                      if (dsend != null)
                      {
                          ClassSendMail mail = new ClassSendMail(this);
                          SmtpAndMailOptions smtp = new SmtpAndMailOptions();

                          for (int i = 0; i < dsend.Rows.Count; i++)
                          {
                              NotifyMailMessage mes = new NotifyMailMessage(dsend.Rows[i], EMAILPATH, ds, c, u, p);

                              if (mes.LastTimeThisMessageHasBeenSent < 0 || mes.LastTimeThisMessageHasBeenSent > SENDTIME_SECONDS) // if we last time send it less then 30 minutes ago
                              {

                                  mail.SendMailWithAttachments(mes.From,
                                      mes.To,
                                      mes.OtherTo,
                                      mes.CopyTo,
                                      mes.Subject,
                                      mes.Body,
                                      mes.Attachments,
                                      mes.MessageId,
                                      mes.Transmit_Priority,
                                      true, da);

                                  if (IS_WRITE_TO_SYSTEM_TABLE)
                                  {
                                      da.Update_System_Status_LastEmailSend(SYSTEM_SECTION_NAME, SECTION_INFO2, mes.MessageId, DateTime.Now, true);
                                  }
                              }
                              else
                              {
                                  erh.LogWarning(" Skipping : LastTimeThisMessageHasBeenSent = " + mes.LastTimeThisMessageHasBeenSent.ToString() + ", sec for message id " + mes.MessageId + " Wait Time is = " + SENDTIME_SECONDS.ToString());
                              }
                              string s = mail.ReadLine(smtp.TimeoutCallBack);
                          }

                      }
                  }
              }
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.

            erh.LogWarning("Endar notification service stopped ", true); 
            try
            {
                // Service stopped. Also stop the timer.
                nTimer.Enabled = false;
                nTimer = null;

                if (IS_WRITE_TO_SYSTEM_TABLE)
                    da.Update_System_Status_LastServiceRun(SYSTEM_SECTION_NAME,SECTION_INFO1,"Endar notification service stoped ");

            
              
            }
            catch (Exception ex)
            {
                erh.LogWarning("Endar notification service stopping " + ex.ToString(), true); 
            }

            
        }
        protected override void OnContinue()
        {
            erh.LogWarning("Endar notification service is working now ", true); 
            if (IS_WRITE_TO_SYSTEM_TABLE)
                da.Update_System_Status_LastServiceRun(SYSTEM_SECTION_NAME, SECTION_INFO1, "Endar notification is working  ");
        }
    }
}
