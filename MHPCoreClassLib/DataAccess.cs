using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Data.OleDb;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Threading.Tasks;

namespace MHPCoreClassLib {


    /// <summary>
    /// Summary description for DataAccess.
    /// </summary>
    public class DataAccess : IDisposable {

        private SqlConnection conn = null;
        private string connectionstring = "";
        private int commandtimeout = 0;

        public DataAccess(string m_connectionstring) {
            try {
                connectionstring = m_connectionstring;
                conn = new SqlConnection(connectionstring);
            } catch (SqlException ex) {

            } catch (Exception ex) {

            }
        }

        public DataAccess(string m_connectionstring, int m_commandtimeout) {
            try {
                commandtimeout = m_commandtimeout;
                connectionstring = m_connectionstring;
                conn = new SqlConnection(connectionstring);
            } catch (SqlException ex) {

            } catch (Exception ex) {

            }
        }

        public ReturnClass UploadImageField(string filepath, string imagefieldparametername, string Sql) {
            ReturnClass outcome = new ReturnClass(true);
            SP_Parameters p = new SP_Parameters();
            SqlCommand cmd = null;
            Stream imgStream = null;
            FileInfo file = null;
            byte[] imgBinaryData = null;
            int RowsAffected = 0;
            int filesize = 0;
            int n = 0;

            if (!imagefieldparametername.StartsWith("@")) {
                imagefieldparametername = "@" + imagefieldparametername;
            }
            if (!File.Exists(filepath)) {
                outcome.SetFailureMessage("The file does not exist or is not accessible.");
            }

            if (outcome.Success) {
                try {
                    file = new FileInfo(filepath);
                    filesize = Convert.ToInt32(file.Length);
                } catch (Exception ex) {
                    outcome.Success = false;
                    outcome.Message = ex.Message;
                }
            }

            if (outcome.Success) {
                try {
                    imgStream = File.OpenRead(filepath);
                    imgBinaryData = new byte[filesize];
                    n = imgStream.Read(imgBinaryData, 0, filesize);
                } catch (Exception ex) {
                    outcome.Success = false;
                    outcome.Message = ex.Message;
                }
            }

            if (outcome.Success) {
                try {
                    cmd = new SqlCommand(Sql, conn);
                    if (commandtimeout > 0) {
                        cmd.CommandTimeout = commandtimeout;
                    }
                    p.Add(imagefieldparametername, SqlDbType.Image, filesize, ParameterDirection.Input, imgBinaryData);
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                } catch (Exception ex) {
                    outcome.Success = false;
                    outcome.Message = ex.Message;
                }
            }

            if (conn.State != ConnectionState.Open) conn.Open();
            if (outcome.Success) {
                try {
                    RowsAffected = cmd.ExecuteNonQuery();
                } catch (Exception ex) {
                    outcome.Success = false;
                    outcome.Message = ex.Message;
                }
            }
            try {
                imgStream.Close();
            } catch (Exception ex) {

            }
            return outcome;
        }

        #region Async Void Methods
        public async Task<ReturnClass> ExecSqlAsync(string strQuery) {
            ReturnClass outcome = await ExecSqlAsyncMethods(strQuery, null, false);
            return outcome;
        }

        public async Task<ReturnClass> ExecSqlParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await ExecSqlAsyncMethods(strQuery, p, false);
            return outcome;
        }

        public async Task<ReturnClass> ExecProcVoidAsync(string procname) {
            ReturnClass outcome = await ExecSqlAsyncMethods(procname, null, true);
            return outcome;
        }

        public async Task<ReturnClass> ExecProcVoidParamsAsync(string procname, SP_Parameters p) {
            ReturnClass outcome = await ExecSqlAsyncMethods(procname, p, true);
            return outcome;
        }

        private async Task<ReturnClass> ExecSqlAsyncMethods(string strQuery, SP_Parameters p, bool isProc) {
            ReturnClass outcome = new ReturnClass(true);
            int result = 0;
            string errormsg = "";
            bool isError = false;
            await conn.OpenAsync();
            using (var tran = conn.BeginTransaction()) {
                using (var command = new SqlCommand(strQuery, conn, tran)) {
                    command.CommandTimeout = commandtimeout;
                    if (isProc) {
                        command.CommandType = CommandType.StoredProcedure;
                    } else {
                        command.CommandType = CommandType.Text;
                    }

                    if (p != null) {
                        foreach (SqlParameter objparam1 in p) {
                            command.Parameters.Add(objparam1);
                        }
                    }
                    try {
                        result = await command.ExecuteNonQueryAsync();
                    } catch (Exception ex) {
                        errormsg = ex.ToString();
                        isError = true;
                        tran.Rollback();
                        throw;
                    }
                    tran.Commit();
                }
            }

            if (conn.State != ConnectionState.Closed) {
                conn.Close();
            }
            if (isError) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetDataTable error. Query is[" + strQuery + "] Error:[" + errormsg + "]";
            }
            outcome.Intvar = result;

            return outcome;
        }

        #endregion

        #region Async DataSet Methods


        public async Task<DTReturnClass> GetDataTableAsync(string queryString) {
            return await GetDataTableAsyncMethods(queryString, null, false);
        }

        public async Task<DTReturnClass> GetDataTableParamsAsync(string queryString, SP_Parameters p) {
            return await GetDataTableAsyncMethods(queryString, p, false);
        }

        public async Task<DTReturnClass> GetDataTableProcAsync(string procname) {
            return await GetDataTableAsyncMethods(procname, null, true);
        }

        public async Task<DTReturnClass> GetDataTableProcParamsAsync(string procname, SP_Parameters p) {
            return await GetDataTableAsyncMethods(procname, p, true);
        }

        private async Task<DTReturnClass> GetDataTableAsyncMethods(string strQuery, SP_Parameters p, bool isProc) {
            DTReturnClass outcome = new DTReturnClass(true);
            DataTable dt = new DataTable();

            string errormsg = "";
            bool isError = false;

            using (var command = new SqlCommand(strQuery, conn)) {
                command.CommandTimeout = commandtimeout;
                if (isProc) {
                    command.CommandType = CommandType.StoredProcedure;
                } else {
                    command.CommandType = CommandType.Text;
                }

                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        command.Parameters.Add(objparam1);
                    }
                }

                await conn.OpenAsync();

                try {
                    using (var reader = await command.ExecuteReaderAsync()) {
                        if (await reader.ReadAsync()) {
                            dt.Load(reader, LoadOption.OverwriteChanges);
                        }
                    }
                } catch (Exception ex) {
                    errormsg = ex.ToString();
                    isError = true;
                }
            }

            if (isError) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetDataTable error. Query is[" + strQuery + "] Error:[" + errormsg + "]";
            } else {
                outcome.Datatable = dt;
            }

            return outcome;
        }
        /* better than DataSet.Load apparently
        private void ConvertDataReaderToTableManually()
    {
        SqlConnection conn = null;
        try
        {
            string connString = ConfigurationManager.ConnectionStrings["NorthwindConn"].ConnectionString;
            conn = new SqlConnection(connString);
            string query = "SELECT * FROM Customers";
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            DataTable dtSchema = dr.GetSchemaTable();
            DataTable dt = new DataTable();
            // You can also use an ArrayList instead of List<>
            List<DataColumn> listCols = new List<DataColumn>();

            if (dtSchema != null)
            {
                foreach (DataRow drow in dtSchema.Rows)
                {
                    string columnName = System.Convert.ToString(drow["ColumnName"]);
                    DataColumn column = new DataColumn(columnName, (Type)(drow["DataType"]));
                    column.Unique = (bool)drow["IsUnique"];
                    column.AllowDBNull = (bool)drow["AllowDBNull"];
                    column.AutoIncrement = (bool)drow["IsAutoIncrement"];
                    listCols.Add(column);
                    dt.Columns.Add(column);
                }
            }

            // Read rows from DataReader and populate the DataTable
            while (dr.Read())
            {
                DataRow dataRow = dt.NewRow();
                for (int i = 0; i < listCols.Count; i++)
                {
                    dataRow[((DataColumn)listCols[i])] = dr[i];
                }
                dt.Rows.Add(dataRow);
            }
            GridView2.DataSource = dt;
            GridView2.DataBind();
        }
        catch (SqlException ex)
        {
            // handle error
        }
        catch (Exception ex)
        {
            // handle error
        }
        finally
        {
            conn.Close();
        }

    }
        */

        #endregion

        #region Async Scalar Methods

        public async Task<ReturnClass> GetStringScalarAsync(string strQuery) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, null, false, ScalarType.String);
            return outcome;
        }
        public async Task<ReturnClass> GetStringScalarParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, p, false, ScalarType.String);
            return outcome;
        }
        public async Task<ReturnClass> GetIntScalarAsync(string strQuery) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, null, false, ScalarType.Int);
            return outcome;
        }
        public async Task<ReturnClass> GetIntScalarParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, p, false, ScalarType.Int);
            return outcome;
        }
        public async Task<ReturnClass> GetLongScalarAsync(string strQuery) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, null, false, ScalarType.Long);
            return outcome;
        }
        public async Task<ReturnClass> GetLongScalarParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, p, false, ScalarType.Long);
            return outcome;
        }
        public async Task<ReturnClass> GetDoubleScalarAsync(string strQuery) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, null, false, ScalarType.Double);
            return outcome;
        }
        public async Task<ReturnClass> GetDoubleScalarParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, p, false, ScalarType.Double);
            return outcome;
        }

        public async Task<ReturnClass> GetStringScalarProcAsync(string strQuery) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, null, true, ScalarType.String);
            return outcome;
        }
        public async Task<ReturnClass> GetStringScalarProcParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, p, true, ScalarType.String);
            return outcome;
        }
        public async Task<ReturnClass> GetIntScalarProcAsync(string strQuery) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, null, true, ScalarType.Int);
            return outcome;
        }
        public async Task<ReturnClass> GetIntScalarProcParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, p, true, ScalarType.Int);
            return outcome;
        }
        public async Task<ReturnClass> GetLongScalarProcAsync(string strQuery) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, null, true, ScalarType.Long);
            return outcome;
        }
        public async Task<ReturnClass> GetLongScalarProcParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, p, true, ScalarType.Long);
            return outcome;
        }
        public async Task<ReturnClass> GetDoubleScalarProcAsync(string strQuery) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, null, true, ScalarType.Double);
            return outcome;
        }
        public async Task<ReturnClass> GetDoubleScalarProcParamsAsync(string strQuery, SP_Parameters p) {
            ReturnClass outcome = await GetScalarAsyncMethods(strQuery, p, true, ScalarType.Double);
            return outcome;
        }

        private enum ScalarType {
            None = 0,
            String = 1,
            Int = 2,
            Long = 3,
            Double = 4
        }
        private async Task<ReturnClass> GetScalarAsyncMethods(string strQuery, SP_Parameters p, bool isProc, ScalarType sctype) {
            ReturnClass outcome = new ReturnClass(true);

            string errormsg = "";
            bool isError = false;

            using (var command = new SqlCommand(strQuery, conn)) {
                command.CommandTimeout = commandtimeout;
                if (isProc) {
                    command.CommandType = CommandType.StoredProcedure;
                } else {
                    command.CommandType = CommandType.Text;
                }

                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        command.Parameters.Add(objparam1);
                    }
                }

                await conn.OpenAsync();

                try {
                    using (var reader = await command.ExecuteReaderAsync()) {
                        if (await reader.ReadAsync()) {
                            switch (sctype) {
                                case ScalarType.String:
                                    outcome.Message = reader[0].ToString();
                                    break;
                                case ScalarType.Int:
                                    outcome.Intvar = (int)reader[0];
                                    break;
                                case ScalarType.Long:
                                    outcome.Longvar = (long)reader[0];
                                    break;
                                case ScalarType.Double:
                                    outcome.Doublevar = (double)reader[0];
                                    break;
                            }
                        }
                    }
                } catch (Exception ex) {
                    errormsg = ex.ToString();
                    isError = true;
                }
            }

            if (isError) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetDataTable error. Query is[" + strQuery + "] Error:[" + errormsg + "]";
            }

            return outcome;
        }
        #endregion

        #region Sync void methods
        public ReturnClass ExecSql(string strQuery) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;
            int results = 0;
            try {
                cmd = new SqlCommand(strQuery, conn);
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                if (conn.State != ConnectionState.Open) conn.Open();
                results = cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An update query Failed. Please see logs for exact error";
                outcome.Techmessage = "ExecSql error. Query is[" + strQuery + "] Error:[" + ex.Message + "]";
                results = -1;
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            outcome.Intvar = results;
            return outcome;
        }

        public ReturnClass ExecSqlParams(string q, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;
            int results = 0;
            try {
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.Connection = conn;
                cmd.CommandText = q;
                if (conn.State != ConnectionState.Open) conn.Open();
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                results = cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An update query Failed. Please see logs for exact error";
                outcome.Techmessage = "ExecSqlParams error. Query is[" + q + "] Error:[" + ex.ToString() + "]";
                results = -1;
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            outcome.Intvar = results;
            return outcome;
        }

        public ReturnClass ExecProcVoid(string procname) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;
            int i = 0;
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procname;
                cmd.Connection = conn;
                i = cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An ExecProcVoid query Failed. Please see webmaster for exact error";
                outcome.Techmessage = "ExecProcVoid error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            return outcome;
        }

        public ReturnClass ExecProcVoidParams(string procname, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;
            int i = 0;
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procname;
                cmd.Connection = conn;
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                i = cmd.ExecuteNonQuery();
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An ExecProcVoidParams query Failed. Please see webmaster for exact error";
                outcome.Techmessage = "ExecProcVoidParams error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            return outcome;
        }


        #endregion

        #region Sync Scalar return methods

        public ReturnClass GetStringScalar(string QueryString) {
            ReturnClass outcome = new ReturnClass(true);
            DataTable returns = null;
            SqlDataAdapter da = null;
            string outy = "";
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                returns = new DataTable();
                da = new SqlDataAdapter(QueryString, conn);
                da.Fill(returns);
                da.Dispose();
                if (returns != null) {
                    for (int i = 0; i < returns.Rows.Count; i++) {
                        outy = returns.Rows[i][0].ToString();
                    }
                }
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An GetStringScalar query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetStringScalar error. Query is[" + QueryString + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            if (outcome.Success) {
                outcome.Message = outy;
            }
            return outcome;
        }

        public ReturnClass GetStringScalarParams(string procname, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;
            // DO NOT SPECIFY OUTPUT PARAMS WITHIN p. 
            // All procs must have output param named @out
            string outy = "";
            try {
                cmd = new SqlCommand(procname, conn);
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }

                if (conn.State != ConnectionState.Open) conn.Open();
                cmd.ExecuteNonQuery();
                outy = cmd.Parameters[cmd.Parameters.Count - 1].Value.ToString();

            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An GetStringScalarParams query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetStringScalarParams error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            if (outcome.Success) {
                outcome.Message = outy;
            }
            return outcome;
        }

        public ReturnClass GetIntScalar(string queryString) {
            ReturnClass outcome = new ReturnClass(true);
            DataTable returns = null;
            SqlDataAdapter da = null;
            int outy = 0;
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                returns = new DataTable();
                da = new SqlDataAdapter(queryString, conn);
                da.Fill(returns);
                da.Dispose();
                if (conn.State != ConnectionState.Closed) conn.Close();
                if (returns != null) {
                    for (int i = 0; i < returns.Rows.Count; i++) {
                        outy = Convert.ToInt32(returns.Rows[i][0]);
                    }
                }
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An query failed. Please see logs for exact error";
                outcome.Techmessage = "GetIntScalar error. Query is[" + queryString + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            outcome.Intvar = outy;
            return outcome;
        }

        public ReturnClass GetIntScalarParams(string Sql, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            int returnvalue = 0;
            DataTable dt = new DataTable();
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try {
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.Connection = conn;
                cmd.CommandText = Sql;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
                if (dt != null) {
                    for (int i = 0; i < dt.Rows.Count; i++) {
                        returnvalue = Convert.ToInt32(dt.Rows[i][0]);
                    }
                }
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetIntScalarParams error. Query is[" + Sql + "] Error:[" + ex.ToString() + "]";

            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            if (outcome.Success) {
                outcome.Intvar = returnvalue;
            }
            return outcome;
        }

        public ReturnClass GetLongScalar(string queryString) {
            ReturnClass outcome = new ReturnClass(true);
            DataTable returns = null;
            SqlDataAdapter da = null;
            long returnvalue = 0;
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                returns = new DataTable();
                da = new SqlDataAdapter(queryString, conn);
                da.Fill(returns);
                da.Dispose();
                if (conn.State != ConnectionState.Closed) conn.Close();
                if (returns != null) {
                    for (int i = 0; i < returns.Rows.Count; i++) {
                        returnvalue = Convert.ToInt64(returns.Rows[i][0]);
                    }
                }
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An query failed. Please see logs for exact error";
                outcome.Techmessage = "GetLongScalar error. Query is[" + queryString + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            if (outcome.Success) {
                outcome.Longvar = returnvalue;
            }
            return outcome;
        }

        public ReturnClass GetLongScalarParams(string Sql, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            long returnvalue = 0;
            DataTable dt = new DataTable();
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try {
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.Connection = conn;
                cmd.CommandText = Sql;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
                if (dt != null) {
                    for (int i = 0; i < dt.Rows.Count; i++) {
                        returnvalue = Convert.ToInt64(dt.Rows[i][0]);
                    }
                }
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetLongScalarParams error. Query is[" + Sql + "] Error:[" + ex.ToString() + "]";

            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            if (outcome.Success) {
                outcome.Longvar = returnvalue;
            }
            return outcome;
        }

        public ReturnClass GetDoubleScalarParams(string Sql, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            double returnvalue = -1;
            DataTable dt = new DataTable();
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try {
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.Connection = conn;
                cmd.CommandText = Sql;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
                if (dt != null) {
                    for (int i = 0; i < dt.Rows.Count; i++) {
                        returnvalue = Convert.ToDouble(dt.Rows[i][0]);
                    }
                }
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetIntScalarParams error. Query is[" + Sql + "] Error:[" + ex.ToString() + "]";

            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            if (outcome.Success) {
                outcome.Doublevar = returnvalue;
            }
            return outcome;
        }

        public ReturnClass GetIntScalarProcParams(string procname, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;

            // DO NOT SPECIFY OUTPUT PARAMS WITHIN p. 
            // All procs must have output param named @out
            int outy = -1;
            try {
                cmd = new SqlCommand(procname, conn);
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd.ExecuteNonQuery();
                outy = Convert.ToInt32(cmd.Parameters[cmd.Parameters.Count - 1].Value);
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An GetIntScalarParams query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetIntScalarParams error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            outcome.Intvar = outy;
            return outcome;
        }

        public ReturnClass GetLongScalarProcParams(string procname, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;

            // DO NOT SPECIFY OUTPUT PARAMS WITHIN p. 
            // All procs must have output param named @out
            long outy = -1;
            try {
                cmd = new SqlCommand(procname, conn);
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd.ExecuteNonQuery();
                outy = Convert.ToInt64(cmd.Parameters[cmd.Parameters.Count - 1].Value);
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An GetLongScalarProcParams query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetLongScalarProcParams error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            outcome.Longvar = outy;
            return outcome;
        }

        public ReturnClass GetDoubleScalarProcParams(string procname, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;

            // DO NOT SPECIFY OUTPUT PARAMS WITHIN p. 
            // All procs must have output param named @out
            double outy = -1;
            try {
                cmd = new SqlCommand(procname, conn);
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                //OutputParam = cmd.Parameters.Add("@" + outparamname, SqlDbType.Money);
                //OutputParam.Direction = ParameterDirection.Output;
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd.ExecuteNonQuery();
                outy = Convert.ToDouble(cmd.Parameters[cmd.Parameters.Count - 1].Value);
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An GetDoubleScalarParams query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetDoubleScalarParams error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            outcome.Doublevar = outy;
            return outcome;
        }

        public ReturnClass ExecProcIntResultParams(string procname, SP_Parameters p) {
            ReturnClass outcome = new ReturnClass(true);
            SqlCommand cmd = null;
            int outy = 0;
            int i = 0;
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procname;
                cmd.Connection = conn;
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                i = cmd.ExecuteNonQuery();
                outy = (int)cmd.Parameters[cmd.Parameters.Count - 1].Value;
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An ExecProcIntResultParams query Failed. Please see webmaster for exact error";
                outcome.Techmessage = "ExecProcIntResultParams error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            outcome.Intvar = outy;
            return outcome;
        }


        #endregion

        #region Sync DataSet methods


        public DTReturnClass GetDataTable(string queryString) {
            DTReturnClass outcome = new DTReturnClass(true);
            DataTable dt = new DataTable();
            SqlDataAdapter da = null;
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                da = new SqlDataAdapter(queryString, conn);
                da.Fill(dt);
                da.Dispose();
                outcome.Datatable = dt;
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetDataTable error. Query is[" + queryString + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (conn.State != ConnectionState.Closed) conn.Close();
            }
            return outcome;
        }

        public DTReturnClass GetDataTableParams(string queryString, SP_Parameters p) {
            DTReturnClass outcome = new DTReturnClass(true);
            DataTable dt = new DataTable();
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try {
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.Connection = conn;
                cmd.CommandText = queryString;
                if (conn.State != ConnectionState.Open) conn.Open();
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                if (conn.State != ConnectionState.Closed) conn.Close();
                outcome.Datatable = dt;
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetDataTableParams error. Query is[" + queryString + "] Error:[" + ex.ToString() + "]";

            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            return outcome;
        }

        public DTReturnClass GetDataTableProc(string procname) {
            DTReturnClass outcome = new DTReturnClass(true);
            DataTable dt = new DataTable();
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try {
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procname;
                if (conn.State != ConnectionState.Open) conn.Open();
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                if (conn.State != ConnectionState.Closed) conn.Close();
                outcome.Datatable = dt;
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetDataTableParams error. proc is[" + procname + "] Error:[" + ex.ToString() + "]";

            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            return outcome;
        }

        public DTReturnClass GetDataTableProcParams(string procname, SP_Parameters p) {
            DTReturnClass outcome = new DTReturnClass(true);
            DataTable dt = new DataTable();
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try {
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procname;
                if (conn.State != ConnectionState.Open) conn.Open();
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                if (conn.State != ConnectionState.Closed) conn.Close();
                outcome.Datatable = dt;
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "A query Failed. Please see logs for exact error";
                outcome.Techmessage = "GetDataTableParams error. proc is[" + procname + "] Error:[" + ex.ToString() + "]";

            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            return outcome;
        }


        public DTReturnClass ExecProcRS(string procname) {
            DTReturnClass outcome = new DTReturnClass(true);
            DataTable dt = new DataTable();
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procname;
                cmd.Connection = conn;

                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                outcome.Datatable = dt;

            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An ExecProcRS query Failed. Please see webmaster for exact error";
                outcome.Techmessage = "ExecProcRS error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            return outcome;
        }

        public DTReturnClass ExecProcRSParams(string procname, SP_Parameters p) {
            DTReturnClass outcome = new DTReturnClass(true);
            DataTable dt = new DataTable();
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try {
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd = new SqlCommand();
                if (commandtimeout > 0) {
                    cmd.CommandTimeout = commandtimeout;
                }
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procname;
                cmd.Connection = conn;
                if (p != null) {
                    foreach (SqlParameter objparam1 in p) {
                        cmd.Parameters.Add(objparam1);
                    }
                }
                da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                da.Dispose();
                if (conn.State != ConnectionState.Closed) conn.Close();
                outcome.Datatable = dt;
            } catch (Exception ex) {
                outcome.Success = false;
                outcome.Message = "An query Failed. Please see logs for exact error";
                outcome.Techmessage = "ExecProcRSParams error. Procedure is[" + procname + "] Error:[" + ex.ToString() + "]";
            } finally {
                if (cmd != null) {
                    cmd.Dispose();
                }
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                }
            }
            return outcome;
        }


        #endregion

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources
                if (conn.State != ConnectionState.Closed) {
                    conn.Close();
                    //conn.Dispose();
                }
            }
            // free native resources
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


    }

    //public class SP_Parameters : IEnumerable {
    //    private ArrayList Items;
    //    private SqlParameter sqlparam;
    //    public SP_Parameters() {
    //        Items = new ArrayList();
    //    }
    //    public void Add(SqlParameter para) {
    //        Items.Add(para);
    //    }
    //    public void Add(string parametername, SqlDbType dbType, int paramsize, ParameterDirection direction, object paramvalue) {
    //        sqlparam = new SqlParameter(parametername, dbType);
    //        sqlparam.Direction = direction;
    //        if (paramsize != 0) sqlparam.Size = paramsize;
    //        if (paramvalue != null) sqlparam.Value = paramvalue;
    //        Items.Add(sqlparam);
    //    }

    //    public IEnumerator GetEnumerator() {
    //        return this.Items.GetEnumerator();
    //    }
    //}

    public class SP_Parameters : IEnumerable {
        private List<SqlParameter> Items = null;
        private SqlParameter sqlparam;

        public SP_Parameters() {
            Items = new List<SqlParameter>();
        }
        public void Add(SqlParameter para) {
            Items.Add(para);
        }
        public void Add(string parametername, SqlDbType dbType, int paramsize, ParameterDirection direction, object paramvalue) {
            sqlparam = new SqlParameter(parametername, dbType);
            sqlparam.Direction = direction;
            if (paramsize != 0) sqlparam.Size = paramsize;
            if (paramvalue != null) sqlparam.Value = paramvalue;
            Items.Add(sqlparam);
        }

        public IEnumerator GetEnumerator() {
            return this.Items.GetEnumerator();
        }
    }

}
