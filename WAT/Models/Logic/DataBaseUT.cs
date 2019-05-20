using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using Oracle.DataAccess.Client;
using System.Web.Caching;
using System.Web.Mvc;
using System.Text;

namespace WAT.Models
{
    public class DataBaseUT
    {
        private static void logthdinfo(string info)
        {
            try
            {
                var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql_log");
                if (!Directory.Exists(dir))
                { Directory.CreateDirectory(dir); }

                var filename = Path.Combine(dir, "sqlexception-WAT-" + DateTime.Now.ToString("yyyy-MM-dd"));
                if (File.Exists(filename))
                {
                    var content = System.IO.File.ReadAllText(filename);
                    content = content + "\r\n" + DateTime.Now.ToString() + " : " + info;
                    System.IO.File.WriteAllText(filename, content);
                }
                else
                {
                    System.IO.File.WriteAllText(filename, DateTime.Now.ToString() + " : " + info);
                }
            }
            catch (Exception ex)
            { }

        }


        public static void CloseConnector(SqlConnection conn)
        {
            if (conn == null)
                return;

            try
            {
                conn.Dispose();
            }
            catch (SqlException ex)
            {
                logthdinfo("close conn exception: " + ex.Message + "\r\n");
                //System.Windows.MessageBox.Show(ex.ToString());
            }
            catch (Exception ex)
            {
                logthdinfo("close conn exception: " + ex.Message + "\r\n");
            }
        }

        private static SqlConnection GetAllenConnector()
        {
            var conn = new SqlConnection();
            try
            {
                conn.ConnectionString = @"Server=TEX-CSSQL.texas.ads.finisar.com;User ID=jmpuser;Password=UhnWNcgHo;Database=EngrData;Connection Timeout=120;";
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                logthdinfo("fail to connect to the mes report pdms database:" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
            catch (Exception ex)
            {
                logthdinfo("fail to connect to the mes report pdms database" + ex.Message);
                //System.Windows.MessageBox.Show(ex.ToString());
                return null;
            }
        }

        public static List<List<object>> ExeAllenSqlWithRes(string sql, Dictionary<string, string> parameters = null)
        {
            var ret = new List<List<object>>();
            var conn = GetAllenConnector();
            if (conn == null)
                return ret;
            SqlDataReader sqlreader = null;
            SqlCommand command = null;

            try
            {
                command = conn.CreateCommand();
                command.CommandTimeout = 180;
                command.CommandText = sql;
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        SqlParameter parameter = new SqlParameter();
                        parameter.ParameterName = param.Key;
                        parameter.SqlDbType = SqlDbType.NVarChar;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }
                }
                sqlreader = command.ExecuteReader();
                if (sqlreader.HasRows)
                {
                    while (sqlreader.Read())
                    {
                        Object[] values = new Object[sqlreader.FieldCount];
                        sqlreader.GetValues(values);
                        ret.Add(values.ToList<object>());
                    }

                }

                sqlreader.Close();
                CloseConnector(conn);
                return ret;
            }
            catch (SqlException ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                try
                {
                    if (sqlreader != null)
                    {
                        sqlreader.Close();
                    }
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e)
                { }

                CloseConnector(conn);

                ret.Clear();
                return ret;
            }
            catch (Exception ex)
            {
                logthdinfo("execute exception: " + sql + "\r\n" + ex.Message + "\r\n");

                try
                {
                    if (sqlreader != null)
                    {
                        sqlreader.Close();
                    }
                    if (command != null)
                    {
                        command.Dispose();
                    }
                }
                catch (Exception e)
                { }

                CloseConnector(conn);

                ret.Clear();
                return ret;
            }
        }
    }
}