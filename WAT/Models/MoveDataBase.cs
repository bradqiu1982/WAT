using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class MoveDataBase
    {
        public static bool MoveDB(string tab,string sourcedb,string targetdb)
        {
            SqlConnection sourcecon = null;
            SqlConnection targetcon = null;

            try
            {
                sourcecon = DBUtility.GetConnector(sourcedb);

                targetcon = DBUtility.GetConnector(targetdb);
                var tempsql = "delete from " + tab;
                DBUtility.ExeSqlNoRes(targetcon, tempsql);

                for (int idx = 0; ;)
                {
                    var endidx = idx + 10000;

                    //load data to memory
                    var sql = "select * from(select ROW_NUMBER() OVER(order by(select null)) as mycount, * from " + tab + ") s1 where s1.mycount > " + idx.ToString() + " and s1.mycount <= " + endidx.ToString();
                    var dt = DBUtility.ExecuteSqlReturnTable(sourcecon, sql);
                    if (dt.Rows.Count == 0)
                    {
                        break;
                    }

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(targetcon))
                        {
                            bulkCopy.DestinationTableName = tab;
                            bulkCopy.BulkCopyTimeout = 120;

                            try
                            {
                                for (int i = 1; i < dt.Columns.Count; i++)
                                {
                                    bulkCopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                                }
                                bulkCopy.WriteToServer(dt);
                                dt.Clear();
                            }
                            catch (Exception ex) { return false; }
                        }//end using
                    }//end if

                    idx = idx + 10000;
                }//end for
            }
            catch (Exception ex)
            {
                if (targetcon != null)
                {
                    DBUtility.CloseConnector(targetcon);
                    targetcon = null;
                }

                if (sourcecon != null)
                {
                    DBUtility.CloseConnector(sourcecon);
                    sourcecon = null;
                }
                return false;
            }

            if (targetcon != null)
            {
                DBUtility.CloseConnector(targetcon);
            }

            if (sourcecon != null)
            {
                DBUtility.CloseConnector(sourcecon);
            }

            return true;
        }
    }

}