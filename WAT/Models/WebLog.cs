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

        public static void Log(string msgtype,string msg)
        {
            SortLog("", "", msgtype, msg);
        }

    }
}