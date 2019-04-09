using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.Security.Permissions;
using System.Windows.Forms;

using NotificationSystem;

namespace SendMail
{
    public class ClassSendMail
    {

        private Boolean _isLogging = false;

        private DataAccess _da;

        private NotificationService _ns;

        public DataAccess MyDataAccess
        {
            get { return _da; }
        }

        public ClassSendMail()
        {
            _ns = null;
        }

        public ClassSendMail(NotificationService ns)
        {
            _ns = ns;
        }

        public Boolean IsLogging
        {
            get { return _isLogging; }
            set { _isLogging = value; }
        }

        [HostProtection(ExternalThreading = true)]
        public void SendMailWithAttachments(string sFrom,
                                                  string sTo,
                                                  string[] sOtherTo,
                                                  string[] sCopyTo,
                                                  string subject,
                                                  string sBody,
                                                  Attachment[] maAttachment,
                                                  string messageId, string priority, bool isLogging, DataAccess da)
        {

            try
            {
                SmtpAndMailOptions smtp_opt = new SmtpAndMailOptions();
                LogHandler logHandler = new LogHandler();
                _da = da;

                sTo = sTo.Trim();

                if (sFrom == "" || sFrom.ToLower().Contains("default"))
                {
                    sFrom = smtp_opt.DefaultFrom;
                    if (sFrom == "")
                    {
                        logHandler.LogWarning("Exception in SendMailWithAttachments: No From information");
                        return;
                    }
                }

                if (!IsValidEmail(sFrom))
                {
                    return;
                }

                if (sTo == "")
                {
                    //if (IsLogging)
                    logHandler.LogWarning("Exception in SendMailWithAttachments: No receipient (To) information");
                }

                if (!IsValidEmail(sTo))
                {
                    //if (IsLogging)
                    logHandler.LogWarning("Exception in SendMailWithAttachments: Invalid recepient address (To) information:" + sTo);
                    return;
                }

                if (smtp_opt.SmtpServer == "")
                {
                    //if (IsLogging)
                    logHandler.LogWarning("Exception in SendMailWithAttachments: No relay server specified (SMTP Server");
                    return;
                }

                System.Net.Mail.MailMessage newMessage = new System.Net.Mail.MailMessage(sFrom.Trim(), sTo.Trim());

                newMessage.Subject = subject;
                newMessage.IsBodyHtml = smtp_opt.IsBodyHtml;
                if (priority.ToLower().Contains("im"))
                    newMessage.Priority = MailPriority.High;
                else
                    newMessage.Priority = MailPriority.Normal;

                string body = "";

                if (smtp_opt.IsBodyHtml)
                {
                    body = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">";
                    body += "<HTML><HEAD><META http-equiv=Content-Type content=\"text/html; charset=iso-8859-1\">";
                    body += "</HEAD>";
                    body += "<BODY><DIV><FONT face=Arial size=3>";
                    body += "</br>";
                }
                body += sBody;
                if (smtp_opt.IsBodyHtml)
                {

                    body += "</br>";
                    body += "</FONT></DIV></BODY></HTML></end>";
                }
                newMessage.Body = body;



                if (sOtherTo != null && sOtherTo.Length > 0)
                {
                    for (int i = 0; i <= sOtherTo.Length - 1; i++)
                    {
                        if (sOtherTo[i].Trim() != "")
                        {
                            if (IsValidEmail(sOtherTo[i]))
                            {
                                newMessage.To.Add(sOtherTo[i]);
                            }
                            else
                            {
                                if (IsLogging)
                                    logHandler.LogWarning("Exception in SendMailWithAttachments: Invalid recepient address (OtherTo: " + sOtherTo[i] + "). Email will still be sent");
                            }
                        }
                    }
                }
                if ((sCopyTo == null || sCopyTo.Length == 0) && smtp_opt.DefaultCC.Trim() != "")
                {
                    sCopyTo = new string[1];
                    sCopyTo[0] = smtp_opt.DefaultCC.Trim();

                }

                if (sCopyTo != null && sCopyTo.Length > 0)
                {
                    for (int i = 0; i <= sCopyTo.Length - 1; i++)
                    {
                        if (sCopyTo[i].Trim() != "")
                        {
                            if (IsValidEmail(sCopyTo[i]))
                            {
                                newMessage.To.Add(sCopyTo[i]);
                            }
                            else
                            {
                                if (IsLogging)
                                    logHandler.LogWarning("Exception in SendMailWithAttachments: Invalid recepient address (CopyTo: " + sCopyTo[i] + "). Email will still be sent");
                            }
                        }
                    }
                }

                if (maAttachment != null)
                {
                    for (int i = 0; i <= maAttachment.Length - 1; i++)
                    {
                        if (maAttachment[i] != null)
                        {
                            newMessage.Attachments.Add(maAttachment[i]);
                        }
                    }
                }
                try
                {
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();//(sSMTPserver, port);
                    smtp.Timeout = 200000; // default is 100 seconds 100,000 miliseconds
                    smtp.Host = smtp_opt.SmtpServer;
                    smtp.Port = smtp_opt.Port;
                    smtp.EnableSsl = smtp_opt.IsEnableSSl;
                    if (smtp_opt.SMTPUsername == "NA")
                    {
                        smtp.Credentials = CredentialCache.DefaultNetworkCredentials;
                        if (smtp_opt.IsNetworkCredential)
                            smtp.Credentials = smtp_opt.MyNetworkCredential;
                    }
                    else
                    {
                        smtp.UseDefaultCredentials = true;
                        smtp.Credentials = new System.Net.NetworkCredential(smtp_opt.SMTPUsername, smtp_opt.SMTPpassword);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    }


                    if (IsLogging)
                        logHandler.LogWarning("Ready to send message: " + messageId);

                    smtp.SendCompleted += new SendCompletedEventHandler(SendCompletedCallBack);
                    try
                    {
                        smtp.SendAsync(newMessage, messageId);
                        DateTime dtPST = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time");
                        da.Update_Notif_Message_Recipient_SentDateOnly(messageId, dtPST);
                    }

                    catch (Exception ex)
                    {
                        logHandler.LogError("SendAsync ", ex);
                    }
                    finally
                    {
                        //mailSent = true;
                    }

                }
                catch (System.NullReferenceException ex)
                {
                    //if (IsLogging)

                    logHandler.LogWarning("ERROR in SendMailWithAttachments NullReferenceException " + ex.Message + "\n " + ex.ToString());
                }
                catch (System.Net.Mail.SmtpException ehttp)
                {
                    if (isLogging)

                        logHandler.LogWarning("ERROR in SendMailWithAttachments HttpException " + ehttp.Message + "\n" + ehttp.ToString());
                }
                catch (IndexOutOfRangeException ex)
                {
                    //if (IsLogging)

                    logHandler.LogWarning("ERROR in SendMailWithAttachments IndexOutOfRangeException " + ex.Message + "\n" + ex.ToString());
                }
                catch (ArgumentException ex)
                {

                    logHandler.LogWarning("ERROR in SendMailWithAttachments ArgumentException " + ex.Message + "\n" + ex.ToString());
                }
                catch (System.Exception ex)
                {
                    //if (IsLogging)

                    logHandler.LogWarning("ERROR in SendMailWithAttachments " + ex.Message + "  " + ex.ToString() + " " + ex.InnerException.ToString());
                }
            }
            catch (System.Exception e)
            {
                //if (IsLogging)
                LogHandler logHandler = new LogHandler();
                logHandler.LogWarning("ERROR in SendMailWithAttachments: " + e.Message + " " + e.ToString() +
                   "message : " + sFrom + " " + sTo + " " + sBody + " " + subject + " " + maAttachment.Length);
            }

        }

        public bool IsValidEmail(string email)
        {
            try
            {
                email = email.ToLower();
                //regular expression pattern for valid email
                //addresses, allows for the following domains:
                //com,edu,info,gov,int,mil,net,org,biz,name,museum,coop,aero,pro,tv
                string pattern = "^[-a-zA-Z0-9][-.a-zA-Z0-9]*@[-.a-zA-Z0-9]+(\\.[-.a-zA-Z0-9]+)*\\.";
                pattern += "(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$";
                //Regular expression object
                System.Text.RegularExpressions.Regex check = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);
                //boolean variable to return to calling method
                bool valid = false;

                //make sure an email address was provided
                if (String.IsNullOrEmpty(email))
                {
                    valid = false;
                }
                else
                {
                    //use IsMatch to validate the address
                    valid = check.IsMatch(email);
                }
                //return the value to the calling method
                return valid;
            }
            catch (Exception ex)
            {
                LogHandler logHandler = new LogHandler();
                logHandler.LogWarning("ERROR in IsValidEmail: " + ex.Message + " " + ex.ToString());
            }
            return false;
        }


        private void SendCompletedCallBack(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            try
            {
                LogHandler logHandler = new LogHandler();
                string strMailMessage = "";

                strMailMessage = "";
                string messageId = e.UserState.ToString();
                bool bSent;

                bSent = false;

                System.Net.Mail.SmtpClient smtp = (System.Net.Mail.SmtpClient)sender;

                messageId = messageId.ToLower();

                if (e.Cancelled)
                {
                    strMailMessage = "CANCELED";
                    if (IsLogging)
                        logHandler.LogWarning("Message Cancelled : " + e.Cancelled + " UPLOAD_RESPONSE.ID= " + messageId.ToString());
                    bSent = false;

                    if (MyDataAccess != null)
                        MyDataAccess.Update_Notif_Message_Recipient(messageId, DateTime.MinValue, bSent);
                }
                if (e.Error != null)
                {
                    strMailMessage = "ERROR";
                    bSent = false;
                    if (IsLogging)
                        logHandler.LogWarning("Message  " + " messageId= " + messageId.ToString() + " hasn't been sent; ");
                    if (e.Error.Message != null)
                        logHandler.LogWarning("SMTP ERROR Message : " + e.Error.Message + e.Error.StackTrace);
                    if (e.Error.InnerException != null)
                        logHandler.LogWarning("SMTP ERROR InnerException: " + e.Error.InnerException.ToString());

                    if (MyDataAccess != null)
                        MyDataAccess.Update_Notif_Message_Recipient(messageId, DateTime.MinValue, bSent);

                   //restart service needed
                    if (_ns != null && _ns.CanStop)
                    {
                        _ns.ExitCode = 55;//need restart
                        this._ns.Stop();
                        logHandler.LogWarning("Stopping service");
                        Environment.Exit(55);
                    }

                }
                else
                {
                    strMailMessage = "SENT";
                    if (IsLogging)
                        logHandler.LogWarning("Message  " + " messageId =" + messageId.ToString() + " has been sent; ");
                    bSent = true;

                    if (MyDataAccess != null)
                        MyDataAccess.Update_Notif_Message_Recipient(messageId, DateTime.Now, bSent);
                }


            }
            catch (System.NullReferenceException ex)
            {
                LogHandler logHandler = new LogHandler();
                logHandler.LogWarning("ERROR in SendCompletedCallBack NullReferenceException " + ex.Message + "\n " + ex.ToString());
            }
            catch (Exception ex)
            {
                LogHandler logHandler = new LogHandler();
                logHandler.LogWarning("ERROR in SendCompletedCallBack " + ex.Message + "\n" + ex.InnerException.ToString());
            }
        }


        public string ReadLine(int timeoutms)
        {
            try
            {
                ReadLineDelegate d = Console.ReadLine;
                IAsyncResult result = d.BeginInvoke(null, null);
                result.AsyncWaitHandle.WaitOne(timeoutms);//timeout e.g. 15000 for 15 secs
                if (result.IsCompleted)
                {
                    string resultstr = d.EndInvoke(result);
                    //Console.WriteLine("Read: " + resultstr);
                    return resultstr;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                LogHandler logHandler = new LogHandler();
                logHandler.LogWarning("ERROR in ReadLine " + ex.Message + "\n" + ex.InnerException.ToString());
                return "";
            }
        }
        
        delegate string ReadLineDelegate();
    }

}