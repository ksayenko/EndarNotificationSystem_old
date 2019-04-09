using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace NotificationSystem
{

    public class DataAccess : IDisposable
    {
        SqlConnection dataConnection;
        SqlDataAdapter dataAdapter;

        LogHandler mylog;
        string constring;

        public bool IsLogging;

        //private xmlDataset As DataSet
        //private Const cCONFIGFILE As string = "Settings.xml"


        public DataAccess(string Data_Source, string InitialCatalog, string DatabaseUser, string DatabasePassword)
        {
            mylog = new LogHandler(Properties.EndarProcessorSettings.Default.ServiceName, "DataAccess costructor");           
            constring = "Data Source=" + Data_Source + ";";
            constring += "Initial Catalog=" + InitialCatalog + ";";
            constring += "User=" + DatabaseUser + ";";
            constring += "Password=" + DatabasePassword + ";";  
            IsLogging = true;

            mylog.LogWarning("Connection string: " + constring + " ");
            dataAdapter = new SqlDataAdapter();

            try
            {
                dataConnection = new SqlConnection(constring);
            }
            catch (Exception ex)
            {
                mylog.LogWarning("ERROR in DataAccess. Connection string " + constring + " " + ex.Message + Environment.NewLine + ex.ToString());
            }

           
        }

        public void WriteToDataBase_MessageSend(string messageid)
        {
        }

        public DataTable ResponsesList()
        {

            string sql = "SELECT ID, EDD_NAME,	UPLOADER_NAME,	FILE_NAME,	BATCH_ID,	EMAIL_TO,	EMAIL_TO_CC,	UPLOAD_STATUS,	RESPONSE_TEXT, RESPONSE_DATE ";
            sql += " FROM UPLOAD_RESPONSE where RESPONSE_SENT=0";

            return Select(sql, "ResponsesList");
        }


        public DataTable Select(string sql)
        {
            return Select(sql, "generic");
        }

        public DataTable GetDataTableForAttachment(string datatablename, string batch_id)
        {
            string sql = "Select * from "+datatablename+" where _batch_id ='" + batch_id + "'";
            return Select(sql, datatablename);
        }

        public DataTable Select(string sql, string datatablename)
        {
            DataTable dt = null;
            int nTry = 0;
            int nMaxTry = 5;
            bool bSuccess = false;

            try
            {
                while (!bSuccess && nTry < nMaxTry)
                {
                    try
                    {
                        Connect();

                        SqlCommand sqlcom = new SqlCommand(sql, dataConnection);
                        dataAdapter = new SqlDataAdapter();
                        dataAdapter.SelectCommand = sqlcom;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            dt = ds.Tables[0];
                            dt.TableName = datatablename;
                            bSuccess = true;
                        }
                        else
                        {
                            nTry++;
                            mylog.LogWarning("Something was wrong, trying one more time. End of " + nTry.ToString() + " try out of " + nMaxTry.ToString());
                        }

                        //mylog.LogWarning("Successful try in Select " + nTry.ToString());

                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        mylog.LogWarning("ERROR (SqlException) in Select ");
                        mylog.LogWarning(sql); ;
                        mylog.LogWarning(ex.ToString()); ;
                        mylog.LogError(ex);
                        nTry++;
                        mylog.LogWarning("End of " + nTry.ToString() + " try out of " + nMaxTry.ToString());
                        dataAdapter = new SqlDataAdapter();
                    }
                    catch (System.Data.OleDb.OleDbException ex)
                    {
                        mylog.LogWarning("ERROR  (OleDbException) in Select ");
                        mylog.LogWarning(sql); ;
                        mylog.LogError(ex);
                        nTry++;
                        mylog.LogWarning("End of " + nTry.ToString() + " try out of " + nMaxTry.ToString());
                        dataAdapter = new SqlDataAdapter();
                    }
                    catch (Exception ex)
                    {
                        mylog.LogWarning("ERROR in Select " + sql + " " + ex.Message + Environment.NewLine + ex.ToString());
                        nTry++;
                        mylog.LogWarning("End of " + nTry.ToString() + " try out of " + nMaxTry.ToString());
                        dataAdapter = new SqlDataAdapter();
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                mylog.LogWarning("ERROR in Select (SqlException) " + sql + " " + ex.Message + Environment.NewLine + ex.ToString());
            }
            catch (Exception ex)
            {
                mylog.LogWarning("ERROR in Select " + sql + " " + ex.Message + Environment.NewLine + ex.ToString());
            }

            CloseConnection();

            return dt;
        }

        public SqlDataAdapter DataAdapter
        {
            get
            {
                if (dataAdapter == null)
                    dataAdapter = new SqlDataAdapter();
                return dataAdapter;
            }
            set { dataAdapter = value; }
        }


        public void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (dataAdapter != null)
                        dataAdapter.Dispose();

                    if (dataConnection != null)
                    {
                        dataConnection.Close();
                        dataConnection.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                mylog.LogWarning("ERROR in Dispose " + ex.Message + " \\n" + ex.ToString());
            }
        }

        public void Execute(string sSql)
        {
            int nTry = 0;
            int nMaxTry = 5;
            bool bSuccess = false;

            SqlCommand queryCommand;
            try
            {
                while (!bSuccess && nTry < nMaxTry)
                {
                    try
                    {
                        Connect();
                        queryCommand = new SqlCommand();
                        queryCommand.CommandType = CommandType.Text;
                        queryCommand.CommandText = sSql;
                        queryCommand.Connection = dataConnection;
                        queryCommand.ExecuteNonQuery();

                        queryCommand.Dispose();
                        bSuccess = true;
         
                    }
                    catch (System.Data.SqlClient.SqlException ex)
                    {
                        mylog.LogWarning("ERROR in DataAccess.Execute (System.Data.SqlClient.SqlException ex) " + ex.Message + " " + ex.ToString() + " " + " SQLStatement: " + sSql);
                        nTry++;
                        mylog.LogWarning("End of " + nTry.ToString() + " try out of " + nMaxTry.ToString());
                        queryCommand = null;
                        dataAdapter = new SqlDataAdapter();
                    }
                    catch (Exception ex)
                    {
                        mylog.LogWarning("ERROR in DataAccess.Execute " + ex.Message + " " + ex.ToString() + " " + " SQLStatement: " + sSql);
                        nTry++;
                        mylog.LogWarning("End of " + nTry.ToString() + " try out of " + nMaxTry.ToString());
                        queryCommand = null;
                        dataAdapter = new SqlDataAdapter();
                    }

                }
            }
            catch (Exception ex)
            {
                mylog.LogWarning("ERROR in DataAccess.Execute " + ex.Message + " " + ex.ToString() + " " + " SQLStatement: " + sSql);
            }

            CloseConnection();
        }

        private void CloseConnection()
        {
            try
            {
                dataAdapter = null;
                if (dataConnection != null)
                {
                    if (dataConnection.State == ConnectionState.Open)
                        dataConnection.Close();
                    dataConnection = null;
                }
            }
            catch (Exception ex)
            {
                mylog.LogWarning("ERROR in CloseConnection " + ex.Message + " " + ex.ToString());
            }
        }       

        public void Connect()
        {
            try
            {
                if (dataConnection == null)
                    dataConnection = new SqlConnection(constring);

                if (dataConnection.State == ConnectionState.Closed)
                    dataConnection.Open();

                if (dataConnection.State == ConnectionState.Broken)
                {
                    dataConnection = null;
                    dataConnection = new SqlConnection(constring);
                }
            }
            catch (Exception ex)
            {
                mylog.LogWarning("ERROR in Connect " + ex.Message + " " + ex.ToString());
            }
        }

        public void Update_Notif_Message_Recipient(string messageid, DateTime sent, Boolean bSent)
        {
            string sql = "Update NOTIF_MESSAGE_RECIPIENT set PROCESS_RESULT = ";
            string sql2 = ", SENT_DATE ='";

            if (messageid != "")
            {
                if (bSent)
                {
                    sql += " 1 ";

                    if (sent > DateTime.MinValue)
                        sql += sql2 + sent.ToString() + "'";
                    else
                        sql += sql2 + DateTime.Now.ToString() + "'";
                }

                else

                    sql += " 0 , sent_date = null";

                sql += " where id in ( " + messageid +")";

                if (IsLogging)
                    mylog.LogWarning("Execute Update_Notif_Message_Recipient: " + sql);

                this.Execute(sql);

            }
        }

        public void Update_Notif_Message_Recipient_SentDateOnly(string messageid, DateTime sent)
        {
            if (messageid != "")
            {
               string sql = "Update NOTIF_MESSAGE_RECIPIENT set SENT_DATE = '"+ DateTime.Now.ToString() + "'  where id in ( " + messageid + ")";

                if (IsLogging)
                    mylog.LogWarning("Execute Update_Notif_Message_Recipient_SentDateOnly : " + sql);

                this.Execute(sql);

            }
        }

        public DataTable GetAllMessagesToSend()
        {
            string sql = "select ID, ID_IN, USER_ID ,EMAIL, MESSAGE_DATE, MESSAGE_CATEGORY, MESSAGE_SUBJECT , ";
            sql += " MESSAGE_BODY, FROM_ADDRESS, SITE_ID, SENT_DATE, PROCESS_RESULT, BATCH_ID, TRANSMIT_PRIORITY,FILE_ATTACHED, TABLE_FILE_FROM,FUNCTION_RAN_DATE_TIME ";
            sql+=" from dbo.ufn_Get_Notifications_To_Send()";
            //sql = "select r.ID, r.USER_ID, r.EMAIL,m.MESSAGE_DATE, ";
            //sql += " m.MESSAGE_CATEGORY, m.MESSAGE_SUBJECT, m.MESSAGE_BODY, m.FROM_ADDRESS, ";
            //sql += " m.SITE_ID, ";
            //sql += " r.SENT_DATE, r.PROCESS_RESULT,m.BATCH_ID, m.TRANSMIT_PRIORITY ";
            //sql += " from NOTIF_MESSAGE_RECIPIENT r ";
            //sql += " left join USERS u on r.USER_ID = u.USER_ID ";
            //sql += " inner join NOTIF_MESSAGE m on r.MESSAGE_ID = m.ID ";
            //sql += " where (u.ISACTIVE = 'true' or u.ISACTIVE is null) ";
            //sql += " and (r.SENT_DATE is null or PROCESS_RESULT is null or PROCESS_RESULT = 0 and R.USER_ID IS NOT NULL)";
            //sql += " AND R.USER_ID IS NOT NULL";

            return Select(sql, "AllMessages");
        }

        public DateTime Datetime_Now_PST()
        {
            DateTime dtUTC = DateTime.UtcNow;
            var zone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime pacificNow = TimeZoneInfo.ConvertTimeFromUtc(dtUTC, zone);
            return pacificNow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="system_section_keyword"></param>
        /// <param name="system_section_info_keyword"></param>
        /// <param name="messageid"></param>
        /// <param name="sent"></param>
        /// <param name="bSent"></param>
        /// <returns></returns>
        public bool Update_System_Status_LastEmailSend(string system_section_keyword, string system_section_info_keyword,
            string messageid, DateTime sent, Boolean bSent)
        {

            if (system_section_keyword == "" || system_section_info_keyword == "")
                return false;

            mylog.LogWarning("Update_System_Status_LastEmailSend");

            DateTime dtlastUpdate_PST = Datetime_Now_PST();
            DateTimeOffset dtlastUpdate_OFS = DateTimeOffset.Now;
            string emailSend = "";

            string strWhere = "SYSTEM_SECTION_NAME  = '" + system_section_keyword + "' and SECTION_INFO = '" + system_section_info_keyword + "'";
            string sqlSelect = "SELECT COUNT(*) FROM SYSTEM_STATUS WHERE " + strWhere;
            if (IsLogging)
                mylog.LogWarning("select " + sqlSelect);
            DataTable dt = this.Select(sqlSelect);

            if (messageid != "")
            {
                if (bSent)
                {
                    emailSend = " email has been sent ";

                    if (sent > DateTime.MinValue)
                        emailSend += "on " + sent.ToString();
                }

                else
                    emailSend = " email did not go through. ";

                emailSend += " messageid in ( " + messageid +" )";
            }

            string sqlUpdate = "Update SYSTEM_STATUS set LAST_DATETIME_UPDATED = '" + dtlastUpdate_PST.ToString() + "' " +
                ", LAST_DATETIME_UPDATED_OFFSET_GLOBAL = '" + dtlastUpdate_OFS.ToString() + "' " +
                 ", COMMENT = '" + emailSend + "' " +
                " where " + strWhere;

            string sqlInsert = "INSERT INTO SYSTEM_STATUS(SYSTEM_SECTION_NAME,SECTION_INFO,LAST_DATETIME_UPDATED,LAST_DATETIME_UPDATED_OFFSET_GLOBAL,COMMENT) VALUES(" +
                "'" + system_section_keyword + "','" +
                    system_section_info_keyword + "','" +
                    dtlastUpdate_PST.ToString() + "','" +
                    dtlastUpdate_OFS.ToString() + "','" +
                   emailSend + "')";

            if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() != "0")
            {            
                if (IsLogging)
                    mylog.LogWarning("Update " + sqlUpdate);
                this.Execute(sqlUpdate);
            }
            else
            {
                if (IsLogging)
                    mylog.LogWarning("Insert " + sqlInsert);
                this.Execute(sqlInsert);
            }

            return true;
        }

        public bool Update_System_Status_LastServiceRun(string system_section_keyword, string system_section_info_keyword)
        {
            return Update_System_Status_LastServiceRun(system_section_keyword, system_section_info_keyword, "");
        }

        public bool Update_System_Status_LastServiceRun(string system_section_keyword, string system_section_info_keyword, string comment)
        {

            if (system_section_keyword == "" || system_section_info_keyword == "")
                return false;

            DateTime dtlastUpdate_PST = Datetime_Now_PST();
            DateTimeOffset dtlastUpdate_OFS = DateTimeOffset.Now;

            string strWhere = "SYSTEM_SECTION_NAME  = '" + system_section_keyword + "' and SECTION_INFO = '" + system_section_info_keyword + "'";
            string sqlSelect = "SELECT COUNT(*) FROM SYSTEM_STATUS WHERE " + strWhere;
          
            DataTable dt = this.Select(sqlSelect);

            string sqlUpdate = "Update SYSTEM_STATUS set LAST_DATETIME_UPDATED = '" + dtlastUpdate_PST.ToString() + "' " +
                ", LAST_DATETIME_UPDATED_OFFSET_GLOBAL = '" + dtlastUpdate_OFS.ToString() + "' " +
                 ", Comment = '" + comment + "' " +
               " where " + strWhere;

            string sqlInsert = "INSERT INTO SYSTEM_STATUS(SYSTEM_SECTION_NAME,SECTION_INFO,LAST_DATETIME_UPDATED,LAST_DATETIME_UPDATED_OFFSET_GLOBAL,COMMENT) VALUES(" +
                "'" + system_section_keyword + "','" +
                    system_section_info_keyword + "','" +
                    dtlastUpdate_PST.ToString() + "','" +
                    dtlastUpdate_OFS.ToString() + "','" + comment + "')";

            if (dt.Rows.Count > 0 && dt.Rows[0][0].ToString() != "0")
            {
                if (IsLogging)
                    mylog.LogWarning("Update " + sqlUpdate);
                this.Execute(sqlUpdate);
            }
            else
            {
                if (IsLogging)
                    mylog.LogWarning("Insert " + sqlInsert);
                this.Execute(sqlInsert);
            }
            dt = null; ;

            return true;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
   

