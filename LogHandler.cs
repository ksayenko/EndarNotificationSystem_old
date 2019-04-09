using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;


namespace NotificationSystem
{
    public class LogHandler
    {
        private string logFile =  "logNS.txt";
        string path;
        DirectoryInfo dir;
        private string _serviceName = "CustomWindowsService";
        private string _moduleName = "na";
        private readonly string sNA = "NA";
        private readonly string sDefaultWS= "CustomWindowsService";

        public string WindowsServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
        public string ModuleName
        {
            get { return _moduleName; }
            set { _moduleName = value; }
        }

        public LogHandler():this("","")
        {        
        }


        public LogHandler(string sn, string mn)
        {
            if (sn != null || sn.Length > 1)
            {
                _serviceName = sn;
            }
            else
                _serviceName = sDefaultWS;

            if (mn != null || mn.Length > 1)
            {
                _moduleName = mn;
            }
            else
                _moduleName = sNA;
              
            path = Properties.EndarProcessorSettings.Default.LogPath.Trim();

            bool bUseInstallPathForLogging = true;
            bUseInstallPathForLogging=Properties.EndarProcessorSettings.Default.UseInstallPathForLogging;
           
            if (bUseInstallPathForLogging)
            {
                path = Application.CommonAppDataPath + Path.DirectorySeparatorChar + "Logs";                             
                Properties.EndarProcessorSettings.Default.LogPath = path;   
           
            }

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

           dir  = new DirectoryInfo(path);
           if (_moduleName.Length > 3)
           {
               this.LogWarning("Log object created in " + _moduleName);              
           }
              
        
        }
        public void LogWarning(string strLogText)
        {
            LogWarning(strLogText, false);
        }

        public void LogWarning(string strLogText, bool bWriteToApplicationLog)
        {

            try
            {
                {

                    if (strLogText.Trim().Length == 0)
                    {
                        return;
                    }
                    else
                    {

                        // Create a writer && open the file:
                        StreamWriter log = null;
                        string fullpath = dir.ToString() + "\\" + logFile;

                        if (!File.Exists(fullpath))
                        {

                            log = new StreamWriter(fullpath, true);
                        }
                        else
                        {
                            //get file date
                            //if( today { use, else rename current && make new
                            DateTime fDate = File.GetLastWriteTime(fullpath);
                            if (fDate.Day == DateTime.Now.Day)
                                log = new StreamWriter(fullpath, true);
                            else
                            {
                                File.Move(fullpath, dir.ToString() + "\\" + fDate.Date.ToString("MMddyyyy") + "_log.txt");
                                log = new StreamWriter(fullpath, true);
                            }
                        }


                        // Write to the file:
                        log.Write(DateTime.Now);
                        log.Write("," + strLogText);
                        log.Write("\n\r");
                        log.Write(Environment.NewLine);

                        if (bWriteToApplicationLog)
                            EventLog.WriteEntry(_serviceName, strLogText, EventLogEntryType.Information);
                        

                        // Close the stream:
                        log.Close();
                   
                    }
                }
            }
            catch (Exception e)
            {
                if (path != null)
                    EventLog.WriteEntry( _serviceName, "ERROR In LogWarning: " + e.ToString() + path, EventLogEntryType.Error);
                else
                    EventLog.WriteEntry( _serviceName, "ERROR In LogWarning: " + e.ToString(), EventLogEntryType.Error);
            }

        }

        public void LogError(string modulename, Exception ex)
        {
            try
            {
                {
                    // create an event log source called EndarNotificationSystem if( it doesn//t exist
                    if (!EventLog.SourceExists(_serviceName))
                    {
                        EventLog.CreateEventSource(_serviceName, "Application");
                    }

                    // create an eventlog object to h&&le writing errors to the event log
                    EventLog log = new EventLog("Application");
                    log.Source = _serviceName;

                    // build the event log error message
                    StringBuilder errorMessage = new StringBuilder("ERROR in the module " + modulename + " : ");
                    if (ex != null)
                    {
                        errorMessage.Append(ex.GetType().Name + Environment.NewLine);

                        // continue appending details to the exception log message
                        errorMessage.Append(Environment.NewLine + Environment.NewLine + "Message: ");
                        errorMessage.Append(ex.Message);
                        errorMessage.Append(Environment.NewLine + Environment.NewLine + "InnerException: ");
                        errorMessage.Append(ex.InnerException.Message);
                        errorMessage.Append(Environment.NewLine + Environment.NewLine + "Stack Trace: ");
                        errorMessage.Append(ex.StackTrace);
                    }

                    if (log != null && errorMessage != null)
                        // write the error to the event log
                        log.WriteEntry(errorMessage.ToString(), EventLogEntryType.Error);

                }
            }
            catch (Exception e)
            {
                EventLog.WriteEntry(_serviceName, "ERROR In LogError: " + e.ToString(), EventLogEntryType.Error);
            }


        }


        public void LogError(Exception ex)
        {
            LogError("n/a", ex);
        }

        public void LogError(System.Data.OleDb.OleDbException ex)
        {
            try
            {
                {

                    // create an event log source called EndarNotificationSystem if( it doesn//t exist
                    if (!EventLog.SourceExists(_serviceName))
                    {
                        EventLog.CreateEventSource(_serviceName, "Application");
                    }

                    // create an eventlog object to h&&le writing errors to the event log
                    EventLog log = new EventLog("Application");
                    log.Source = _serviceName;

                    // build the event log error message
                    StringBuilder errorMessage = new StringBuilder("ERROR: ");
                    errorMessage.Append(ex.GetType().Name + Environment.NewLine);

                    // continue appending details to the exception log message
                    errorMessage.Append(Environment.NewLine + Environment.NewLine + "Message: ");
                    errorMessage.Append(ex.Message);


                    for (int i = 0; i <= ex.Errors.Count - 1; i++)
                    {

                        errorMessage.Append(Environment.NewLine + Environment.NewLine + "ERROR " + i + ": ");
                        errorMessage.Append("Index #" + i + "\n" +
                              "Message: " + ex.Errors[i].Message + "\n" +
                              "NativeError: " + ex.Errors[i].NativeError + "\n" +
                              "Source: " + ex.Errors[i].Source + "\n" +
                              "SQLState: " + ex.Errors[i].SQLState + "\n");
                    }

                    errorMessage.Append(Environment.NewLine + Environment.NewLine + "Stack Trace: ");
                    errorMessage.Append(ex.StackTrace);

                    // write the error to the event log
                    log.WriteEntry(errorMessage.ToString(), EventLogEntryType.Error);
                }

            }
            catch (Exception e)
            {
                EventLog.WriteEntry(_serviceName, "ERROR In LogError: " + e.ToString(), EventLogEntryType.Error);
            }


        }


     
    }
}
