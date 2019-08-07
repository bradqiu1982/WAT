using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class FileLoadedData
    {
        public static void UpdateLoadedFile(string filename, string filetype)
        {
            var sql = "select AppV_A from FileLoadedData where AppV_A = @AppV_A and AppV_B=@AppV_B";
            var dict = new Dictionary<string, string>();
            dict.Add("@AppV_A", filename);
            dict.Add("@AppV_B", filetype);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count == 0)
            {
                sql = "insert into FileLoadedData(AppV_A,AppV_B,databackuptm) values(@AppV_A,@AppV_B,@databackuptm)";
                dict = new Dictionary<string, string>();
                dict.Add("@AppV_A", filename);
                dict.Add("@AppV_B", filetype);
                dict.Add("@databackuptm", DateTime.Now.ToString());
                DBUtility.ExeLocalSqlNoRes(sql, dict);
            }//end if
        }

        public static Dictionary<string, bool> LoadedFiles(string filetype)
        {
            var ret = new Dictionary<string, bool>();

            var sql = "select AppV_A from FileLoadedData where AppV_B=@AppV_B";
            var dict = new Dictionary<string, string>();
            dict.Add("@AppV_B", filetype);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.Add(Convert.ToString(line[0]), true);
            }
            return ret;
        }

        public static void RemoveLoadedFile(string filename, string filetype)
        {
            var sql = "";
            var dict = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(filetype))
            {
                sql = "delete from FileLoadedData where AppV_A = @AppV_A and AppV_B=@AppV_B";
                dict.Add("@AppV_A", filename);
                dict.Add("@AppV_B", filetype);
            }
            else
            {
                sql = "delete from FileLoadedData where AppV_A = @AppV_A";
                dict.Add("@AppV_A", filename);
            }
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

    }
}