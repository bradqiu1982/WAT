using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace WAT.Models
{
    public class WebLog
    {
        private static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        private static void SortLog(string Machine, string Name, string MSGType, string MSG)
        {
            var sql = "insert into WebLog(Machine,Name,MSGType,MSG,UpdateTime) values(@Machine,@Name,@MSGType,@MSG,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Machine", Machine);
            dict.Add("@Name", Name);
            dict.Add("@MSGType", MSGType);
            dict.Add("@MSG", MSG);
            dict.Add("@UpdateTime",DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static void LogVisitor(string ip, string msg)
        {
            var machine = DetermineCompName(ip);
            var username = "";
            var sql = "select username from machineusermap where machine = '<machine>' ";
            sql = sql.Replace("<machine>", machine);
            var dbret = DBUtility.ExeNPITraceSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                username = Convert.ToString(dbret[0][0]);
            }

            SortLog(machine, username, "VISIT", msg);
        }

        public static void Log(string filename,string msgtype,string msg)
        {
            var sql = "select Name from WebLog where MSGType='IGNDIESORT' and Name = '<Name>'";
            sql = sql.Replace("<Name>", filename);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            if (dbret.Count == 0)
            {
                SortLog("", filename, msgtype, msg);
            }
        }

        public static void UpdateIgnoreDieSort(string filename)
        {
            SortLog("", filename, "IGNDIESORT", "");
        }

        public static List<WebLog> GetFailedConvertFiles()
        {
            var ret = new List<WebLog>();

            var sql = "select Name,MSG,UpdateTime from WebLog where MSGType='DIESORT' and Name not in (select distinct Name from WebLog where MSGType='IGNDIESORT') order by Name,UpdateTime desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                ret.Add(new WebLog(Convert.ToString(line[0]), Convert.ToString(line[1]), Convert.ToDateTime(line[2]).ToString()));
            }

            return ret;
        }


        public WebLog() {
            Name = "";
            MSG = "";
            UpdateTime = "";
        }
        public WebLog(string name,string msg,string updatetime)
        {
            Name = name;
            MSG = msg;
            UpdateTime = updatetime;
        }

        public string Name { set; get; }
        public string MSG { set; get; }
        public string UpdateTime { set; get; }
    }
}