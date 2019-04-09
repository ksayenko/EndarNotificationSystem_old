using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace NotificationSystem
{
    public class SmtpAndMailOptions
    {
      
        private  string _sSmtpServer = "";
        private  int _iPort = 0;
        private  Boolean _isNetworkCredential = false;
        private Boolean _isEnableSSL = false;
        private  string _sUsername = "";
        private  string _sPassword = "";
        private  string _sDomain = "";
        private bool _isBodyHtml = true;
        private string _sDefaultFrom = "";
        private string _sDefaultCC = "";
        private int _iTimeoutCallBack = 20000;
        private string _sSMTPUsername = "";
        private string _sSMTPPassword = "";

         NetworkCredential _myCred = null;

         public NetworkCredential MyNetworkCredential
         {
             get { return _myCred; }
         }
            

        public  String SmtpServer
        {
            get { return _sSmtpServer; }
            set
            {
                _sSmtpServer = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPServer = _sSmtpServer;
            }

        }

        public Boolean IsBodyHtml
        {
            get { return _isBodyHtml; }
            set
            {
                _isBodyHtml = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.IsBodyHtml = _isBodyHtml;
            }
        }

        public  Boolean IsNetworkCredential
        {
            get { return _isNetworkCredential; }
            set
            {
                _isNetworkCredential = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.IsNetworkCredential = _isNetworkCredential;
            }
        }

        public Boolean IsEnableSSl
        {
            get { return _isEnableSSL; }
            set
            {
                _isNetworkCredential = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.IsEnableSSl = _isEnableSSL;
            }
        }

      
        public  int Port
        {
            get { return _iPort; }
            set
            {
                _iPort = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPPort = _iPort;
            }
        }

       
        public String SMTPUsername
        {
            get { return _sSMTPUsername; }
            set
            {
                _sSMTPUsername = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPUser = _sSMTPUsername;
            }
        }
        public String SMTPpassword
        {
            get { return _sSMTPPassword; }
            set
            {
                _sSMTPPassword = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPPassword = _sSMTPPassword;
            }
        }

        public String DefaultFrom
        {
            get { return _sDefaultFrom; }
            set
            {
                _sDefaultFrom = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.EmailFrom = _sDefaultFrom;
            }
        }

        public String DefaultCC
        {
            get { return _sDefaultCC; }
            set
            {
                _sDefaultCC = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.DefaultRecipient = _sDefaultCC;
            }
        }
        
        public String Domain
        {
            get { return _sDomain; }
            set
            {
                _sDomain = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.Domain = _sDomain;
            }
        }

        public int TimeoutCallBack
        {
            get { return _iTimeoutCallBack; }
            set
            {
                _iTimeoutCallBack = value;
                NotificationSystem.Properties.EndarProcessorSettings.Default.TimeoutCallBack = _iTimeoutCallBack;
            }
        }

        public  void ReadSMTPServerSettings()
        {
            if (NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPServer != null
                && NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPServer != "")
                _sSmtpServer = NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPServer;
            else
                _sSmtpServer = "smtp.tetratech.com";

            if (NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPPort > 0)
                _iPort = NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPPort;
            else
                Port = 25;

            if (NotificationSystem.Properties.EndarProcessorSettings.Default.TimeoutCallBack > 0)
                _iTimeoutCallBack = NotificationSystem.Properties.EndarProcessorSettings.Default.TimeoutCallBack;
            else
                TimeoutCallBack = 20000;

            if (NotificationSystem.Properties.EndarProcessorSettings.Default.IsEnableSSl != null)
                _isEnableSSL = NotificationSystem.Properties.EndarProcessorSettings.Default.IsEnableSSl;
            else
                IsEnableSSl = false;

            if (NotificationSystem.Properties.EndarProcessorSettings.Default.IsNetworkCredential != null)
                _isNetworkCredential = NotificationSystem.Properties.EndarProcessorSettings.Default.IsNetworkCredential;
            else
                IsNetworkCredential = false;

            if (NotificationSystem.Properties.EndarProcessorSettings.Default.Domain != null &&
                NotificationSystem.Properties.EndarProcessorSettings.Default.Domain != "")
                _sDomain = NotificationSystem.Properties.EndarProcessorSettings.Default.Domain;
            else
                _sDomain = "NA";

            //if (NotificationSystem.Properties.EndarProcessorSettings.Default.DatabasePassword != null &&
            //    NotificationSystem.Properties.EndarProcessorSettings.Default.DatabasePassword != "")
            //    _sPassword = NotificationSystem.Properties.EndarProcessorSettings.Default.DatabasePassword;
            //else
            //    _sPassword = "NA";

            if (NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPPassword != null &&
                NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPPassword != "")
                _sSMTPPassword = NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPPassword;
            else
                _sSMTPPassword = "NA";

            if (NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPUser != null &&
               NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPUser != "")
                _sSMTPUsername = NotificationSystem.Properties.EndarProcessorSettings.Default.SMTPUser;
            else
                _sSMTPUsername = "NA";

            //if (NotificationSystem.Properties.EndarProcessorSettings.Default.DatabaseUser != null &&
            //   NotificationSystem.Properties.EndarProcessorSettings.Default.DatabaseUser != "")
            //    _sUsername = NotificationSystem.Properties.EndarProcessorSettings.Default.DatabaseUser;
            //else
                _sUsername = "NA";

            if (NotificationSystem.Properties.EndarProcessorSettings.Default.EmailFrom != null &&
               NotificationSystem.Properties.EndarProcessorSettings.Default.EmailFrom != "")
                _sDefaultFrom = NotificationSystem.Properties.EndarProcessorSettings.Default.EmailFrom;
            else
                _sDefaultFrom = "default@endar.com"; 

        }

        public SmtpAndMailOptions()
        {
            ReadSMTPServerSettings();

            if (IsNetworkCredential)
            {
                NetworkCredential myCred = new NetworkCredential(SMTPUsername, SMTPpassword, Domain);
            }
        }
    }
}
