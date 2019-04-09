using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Data;
using System.Data.SqlClient;
using System.IO;


namespace NotificationSystem
{
    public class NotifyMailMessage
    {
        //ID	0
        //ID_IN for combined 1 -- new
        //USER_ID	2
        //EMAIL	3
        //MESSAGE_DATE	4
        //MESSAGE_CATEGORY	5
        //MESSAGE_SUBJECT	6
        //MESSAGE_BODY	7
        //FROM_ADDRESS	8
        //WASTE_FACILITY_ID	9
        //SENT_DATE	10
        //PROCESS_RESULT	11
        //BATCH_ID	12
        //TRANSMIT_PRIORITY	13
        //FILE_ATTACHED 14 
        // TABLE_FILE_FROM 15
        //I_TITLE_EMAIL_REQUEST_DATE_TIME 16


        const string TITLE_MESSAGE_ID = "ID";
        const int I_TITLE_MESSAGE_ID = 0;

        const string TITLE_MESSAGE_ID_IN = "ID_IN";
        const int I_TITLE_MESSAGE_ID_IN = 1;

        const string TITLE_USER_ID = "USER_ID";
        const int I_TITLE_USER_ID = 2;

        const string TITLE_EMAIL = "EMAIL";
        const int I_TITLE_EMAIL = 3;

        const string TITLE_MESSAGE_DATE = "MESSAGE_DATE";
        const int I_TITLE_MESSAGE_DATE = 4;

        const string TITLE_MESSAGE_CATEGORY = "MESSAGE_CATEGORY";
        const int I_TITLE_MESSAGE_CATEGORY = 5;

        const string TITLE_MESSAGE_SUBJECT = "MESSAGE_SUBJECT";
        const int I_TITLE_MESSAGE_SUBJECT = 6;

        const string TITLE_MESSAGE_BODY = "MESSAGE_BODY";
        const int I_TITLE_MESSAGE_BODY = 7;

        const string TITLE_FROM_ADDRESS = "FROM_ADDRESS";
        const int I_TITLE_FROM_ADDRESS = 8;

        const string TITLE_SITE_ID = "SITE_ID";
        const int I_TITLE_SITE_ID = 9;

        const string TITLE_SENT_DATE = "SENT_DATE";
        const int I_TITLE_SENT_DATE = 10;

        const string TITLE_PROCESS_RESULT = "PROCESS_RESULT";
        const int I_TITLE_PROCESS_RESULT = 11;

        const string TITLE_BATCH_ID = "BATCH_ID";
        const int I_TITLE_BATCH_ID = 12;

        const string TITLE_TRANSMIT_PRIORITY = "TRANSMIT_PRIORITY";
        const int I_TITLE_TRANSMIT_PRIORITY = 13;

        const string TITLE_FILE_ATTACHED = "FILE_ATTACHED";
        const int I_TITLE_FILE_ATTACHED = 14;

        const string TITLE_TABLE_FILE_FROM = "TABLE_FILE_FROM";
        const int I_TITLE_TABLE_FILE_FROM = 15;

        const string TITLE_EMAIL_REQUEST_DATE_TIME = "FUNCTION_RAN_DATE_TIME";
        const int I_TITLE_EMAIL_REQUEST_DATE_TIME = 16;


        //r.SENT_DATE, r.PROCESS_RESULT, m.BATCH_ID 

        string _sFrom;
        string _sTo;
        string[] _sOtherTo;
        string[] _sCopyTo;
        string _subject;
        string _sBody;
        string[] _sPathToAttachments;
        Attachment[] _Attachments;
        string _messageId;
        string _transmit_priority;
        bool _file_attached;
        string _table_attached_from;
        string _emailpath;

        int _LastTimeThisMessageHasBeenSent = -1;

        public string From
        {
            get { return _sFrom; }
            set { _sFrom = value; }
        }

        public string To
        {
            get { return _sTo; }
            set { _sTo = value; }
        }

        public string Body
        {
            get { return _sBody; }
            set { _sBody = value; }
        }

        public string Subject
        {
            get { return _subject; }
            set { _subject = value; }
        }

        public string MessageId
        {
            get { return _messageId; }
            set { _messageId = value; }
        }

        public string[] OtherTo
        {
            get { return _sOtherTo; }
        }

        public string[] CopyTo
        {
            get { return _sCopyTo; }
        }


        public string[] PathToAttachments
        {
            get { return _sPathToAttachments; }
        }

        public Attachment[] Attachments
        {
            get { return _Attachments; }
        }


        public string Transmit_Priority
        {
            get { return _transmit_priority; }
            set { _transmit_priority = value; }
        }

        public int LastTimeThisMessageHasBeenSent // in seconds, -1 meant never been sent
        {
            get { return _LastTimeThisMessageHasBeenSent; }
         
        }

        public NotifyMailMessage(DataTable datatable, int row, string Data_Source, string InitialCatalog, string DatabaseUser, string DatabasePassword)
            : this(datatable, row, "",Data_Source, InitialCatalog, DatabaseUser, DatabasePassword) { }

        public NotifyMailMessage(DataTable datatable, int row, string emailpath,string Data_Source, string InitialCatalog, string DatabaseUser, string DatabasePassword)
        {
            _emailpath = emailpath;
            if (datatable != null &&
                datatable.Rows.Count > 0 &&
                datatable.Rows.Count > row)
                createMailMessage(datatable.Rows[row], Data_Source, InitialCatalog, DatabaseUser, DatabasePassword);
            else
            {
                makeEmptyMessage();
            }
        }

        public NotifyMailMessage(DataRow dr, string emailpath, string Data_Source, string InitialCatalog, string DatabaseUser, string DatabasePassword) 
        {
            _emailpath = emailpath;
            createMailMessage(dr, Data_Source, InitialCatalog, DatabaseUser, DatabasePassword);
        }
        

        private void makeEmptyMessage()
        {
            _sFrom = "";
            _sTo = "";
            _sOtherTo = null;
            _sCopyTo = null;
            _subject = "";
            _sBody = "";
            _sPathToAttachments = null;
            _Attachments = null;
            _messageId = "";
            _transmit_priority = "";
            _table_attached_from = "";
            _file_attached = false;
            _LastTimeThisMessageHasBeenSent = -1;

        }


        public void createMailMessage(DataRow dr, string Data_Source, string InitialCatalog, string DatabaseUser, string DatabasePassword)
        {
            LogHandler l = new LogHandler();
            l.LogWarning("IN createMailMessage");

            makeEmptyMessage();
            string userid = "tt\\kateryna.sayenko";
            string email = "kateryna.sayenko@tetratech.com";
            DateTime message_date = DateTime.Now;

            DateTime sent_date = DateTime.Now;
            DateTime retrieve_email_request_date = DateTime.Now;

            string message_category = "";
            string message_subject = "";
            string message_body = "";
            string site_id = "";
            string batchId = "";
            string temp = "";
            string table_attached_from = "";
            bool file_attached = false;        

            bool iserror = false;
            try
            {

                //ID
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_MESSAGE_ID_IN);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_MESSAGE_ID_IN].ToString();
                    }
                    catch { temp = ""; }

                if (temp == null || temp == "")
                {
                    iserror = true;
                    return;
                }
                else
                {
                    _messageId = temp;
                }

                //USER_ID
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_USER_ID);
                }
                catch { temp = ""; }

                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_USER_ID].ToString();
                    }
                    catch { temp = ""; }

                if (temp == null || temp == "")
                {
                    iserror = true;
                    return;
                }
                else
                {
                    userid = temp;
                }
                //EMAIL
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_EMAIL);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "kateryna.sayenko@tetratech.com";
                        temp = dr[I_TITLE_EMAIL].ToString();
                    }
                    catch { temp = ""; }

                if (temp == null || temp == "")
                {
                    iserror = true;
                    return;
                }
                else
                {
                    email = temp;
                }
                //MESSAGE_DATE
                DateTime dttemp = DateTime.MinValue;
                try
                {
                    dttemp = dr.Field<DateTime>(TITLE_MESSAGE_DATE);
                }
                catch { dttemp = DateTime.MinValue; }
                if (dttemp == DateTime.MinValue)
                    try
                    {
                        dttemp = (DateTime)dr[I_TITLE_MESSAGE_DATE];
                    }
                    catch { dttemp = DateTime.MinValue; }

                if (dttemp == DateTime.MinValue)
                {
                    iserror = true;
                    return;
                }
                else
                {
                    message_date = dttemp;
                }
                //MESSAGE_CATEGORY
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_MESSAGE_CATEGORY);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_MESSAGE_CATEGORY].ToString();
                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    message_category = temp + "( to " + InitialCatalog + ")";
                }
                //MESSAGE_SUBJECT
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_MESSAGE_SUBJECT);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_MESSAGE_SUBJECT].ToString();
                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    message_subject = temp;
                }
                //MESSAGE_BODY
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_MESSAGE_BODY);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_MESSAGE_BODY].ToString();
                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    message_body = temp;
                }
                //FROM_ADDRESS
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_FROM_ADDRESS);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_FROM_ADDRESS].ToString();
                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    _sFrom = temp;
                }
                //FACILITY_ID
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_SITE_ID);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_SITE_ID].ToString();
                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    site_id = temp;
                }
                //_batchId            
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_BATCH_ID);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_BATCH_ID].ToString();
                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    batchId = temp;
                }
                //TRANSMIT_PRIORITY
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_TRANSMIT_PRIORITY);
                }
                catch { temp = ""; }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_TRANSMIT_PRIORITY].ToString();
                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    _transmit_priority = temp;
                }

                //FILE_ATTACHED
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_FILE_ATTACHED).ToString();
                }
                catch (Exception ex)
                {
                    l.LogWarning(TITLE_FILE_ATTACHED + ex.ToString());
                    temp = "";
                }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_FILE_ATTACHED].ToString();

                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    file_attached = false;

                    if (temp.ToLower() == "true" || temp == "0")
                        file_attached = true;

                    _file_attached = file_attached;
                }

                //
                //_table_attached_from
                try
                {
                    temp = "";
                    temp = dr.Field<string>(TITLE_TABLE_FILE_FROM);
                }
                catch (Exception ex)
                {
                    l.LogWarning(TITLE_TABLE_FILE_FROM + ex.ToString());
                    temp = "";
                }
                if (temp == null || temp == "")
                    try
                    {
                        temp = "";
                        temp = dr[I_TITLE_TABLE_FILE_FROM].ToString();
                    }
                    catch { temp = ""; }

                if (temp != "")
                {
                    table_attached_from = temp;
                }

                //Send_Time_Seconds
                 dttemp = DateTime.MinValue;
                try
                {
                    dttemp = dr.Field<DateTime>(TITLE_EMAIL_REQUEST_DATE_TIME);
                }
                catch { dttemp = DateTime.MinValue; }
                if (dttemp == DateTime.MinValue)
                    try
                    {
                        dttemp = (DateTime)dr[I_TITLE_EMAIL_REQUEST_DATE_TIME];
                    }
                    catch { dttemp = DateTime.Now; }

                retrieve_email_request_date = dttemp;


                //Send_date
                dttemp = DateTime.MinValue;
                try
                {
                    dttemp = dr.Field<DateTime>(TITLE_SENT_DATE);
                }
                catch { dttemp = DateTime.MinValue; }
                if (dttemp == DateTime.MinValue)
                    try
                    {
                        dttemp = (DateTime)dr[I_TITLE_SENT_DATE];
                    }
                    catch { dttemp = DateTime.MinValue; }

                sent_date = dttemp;


            }
            catch { }

            if (iserror)
            {
                makeEmptyMessage();
            }
            else
            {
                _sTo = email;
                _subject = "";
                if (message_category != "")
                {
                    _subject += message_category;
                    _sBody += "<b>" + message_category + "</b></br></br>";
                }
                if (message_subject != "")
                    _subject += " : " + message_subject;
                if (site_id != "")
                    _subject += " for " + site_id;
                _sBody += "<b>" + message_subject + " </br></br>";
                _sBody += "On " + message_date.ToString() + "</br>";
                if (site_id != "")
                    _sBody += " for " + site_id;
                if (batchId != "")
                {
                    _sBody += "</br></br> EDD Batch ID:" + batchId;
                }
                _sBody += "</br></br></b>";
                message_body = message_body.Replace("The message is", "The message is :</br>");
                message_body = message_body.Replace("File name ", "</br>File name ");
                message_body = message_body.Replace("1. ", "</br>1. ");
                message_body = message_body.Replace("2. ", "</br>2. ");
                message_body = message_body.Replace("3. ", "</br>3. ");
                message_body = message_body.Replace("4. ", "</br>4. ");
                message_body = message_body.Replace("5. ", "</br>5. ");
                _sBody += message_body;              
               
                if (file_attached && table_attached_from != "")
                {
                    string fileout = "";                    

                    if (WriteFileToTheDisk(table_attached_from, batchId, Data_Source,InitialCatalog,DatabaseUser,DatabasePassword, out fileout))
                        if (fileout != "")
                        {
                            l.LogWarning("file attached : " + fileout + " for meesage id : " + MessageId);
                            Attachment a = new Attachment(fileout);
                            _Attachments = new Attachment[1];
                            _Attachments[0] = a;
                        }
                }

                //l.LogWarning("sent_date " + sent_date.ToString() + " retrieve_email_request_date " + retrieve_email_request_date.ToString() + " MessageId" + MessageId);

                if (sent_date == DateTime.MinValue || retrieve_email_request_date == DateTime.MinValue)
                    _LastTimeThisMessageHasBeenSent = -1;
                else
                {             
                    TimeSpan span = (retrieve_email_request_date - sent_date);
                    _LastTimeThisMessageHasBeenSent = Math.Abs((int)span.TotalSeconds);
                }

                //l.LogWarning("_LastTimeThisMessageHasBeenSent = " + _LastTimeThisMessageHasBeenSent.ToString() +" message id "+ _messageId);
            }
        }

        bool WriteFileToTheDisk(string tablename, string batch, string Data_Source, string InitialCatalog, string DatabaseUser, string DatabasePassword, out string filelocation)
        {
            bool returnvalue = true;
            filelocation = "";
            LogHandler l = new LogHandler();
            StreamWriter sw = null;
        
            if (tablename != "" && _emailpath !="")
            {
                try
                {

                    DataAccess da = new DataAccess(Data_Source, InitialCatalog, DatabaseUser, DatabasePassword);
                    DataTable dt = da.GetDataTableForAttachment(tablename, batch);                    

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        string filename = tablename + GetExtensionDate(da.Datetime_Now_PST()) + ".csv";
                        if (!Directory.Exists(_emailpath))
                            Directory.CreateDirectory(_emailpath);

                        filelocation = _emailpath + "\\" + filename;

                        sw = new StreamWriter(filelocation, true);

                        string header = "";
                        string columnheader = "";
                        string line = "";
                        string value = "";

                        int cols = dt.Columns.Count;
                        int rows = dt.Rows.Count;

                        l.LogWarning("ROWS TO WRITE " + rows.ToString());

                        for (int i = 0; i < cols; i++)
                        {
                            columnheader = dt.Columns[i].ColumnName;
                            if (IsExcludeColumn(columnheader))
                                continue;
                            header += "\""+columnheader + "\",";
                        }
                        header = header.Remove(header.Length - 1);
                        sw.WriteLine(header);

                        for (int i = 0; i < rows; i++)
                        {
                            DataRow dr = dt.Rows[i];
                            line = "";

                            for (int j = 0; j < cols; j++)
                            {
                                columnheader = dt.Columns[j].ColumnName;

                                if (IsExcludeColumn(columnheader))
                                    continue;
                                else
                                {
                                    value = dr[j].ToString();

                                    line += "\"" + value + "\"" + ",";
                                }
                            }

                            line = line.Remove(line.Length - 1);
                            sw.WriteLine(line);
                        }
                        sw.Close();
                        sw.Dispose();
                        sw = null;
                    
                    }
                }
                catch (Exception ex)
                {
                    LogHandler l1 = new LogHandler();
                    l1.LogWarning("WRITE FILE TO DISK ERROR: "+ ex.ToString());
                    if (sw !=null)
                        sw.Close();
                }                    
            }  
            return returnvalue;
        }

        private bool IsExcludeColumn(string str)
        {
            bool rv = false;
            str = str.ToUpper();

            rv =str.Contains("_BATCH_ID") ||
            str.Contains("__FILE_PROCESSED") ||
            str.Contains("_COMMAND") ||
            str.Contains("__SITE_ID") ||
            str.Contains("__SAMPLE_DATE") ||
            str.Contains("__WEATHER") ||
            str.Contains("__CAS_NUMBER") ||
            str.Contains("ROW_INDEX");

            return rv;
        }



        string GetExtensionDate(DateTime dPacific)
        {
            string thedate = "_";

            thedate = dPacific.Year.ToString();

            if (dPacific.Month < 10)
                thedate += "0";
            thedate += dPacific.Month.ToString();

            if (dPacific.Day < 10)
                thedate += "0";
            thedate += dPacific.Day.ToString();

            thedate += "_";
            if (dPacific.Hour < 10)
                thedate += "0";
            thedate += dPacific.Hour.ToString();
           
            if (dPacific.Minute < 10)
                thedate += "0";
            thedate += dPacific.Minute.ToString();
            if (dPacific.Second < 10)
                thedate += "0";
            thedate += dPacific.Second.ToString();
          
            thedate += "_"+(dPacific.Millisecond / 10).ToString();

            return thedate;
        }
    }
}
