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

        public static void LogIIVIQuery(string wafer,string user, string msg)
        {
            SortLog(wafer, user, "IIVIQ", msg);
        }

        public static List<WebLog> GetIIVIQuery(string wafer)
        {
            var ret = new List<WebLog>();
            var sql = "select Name,MSG,UpdateTime from WebLog where MSGType='IIVIQ' and Machine = @wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var msg = Convert.ToString(line[1]);
                var tempvm = new WebLog(Convert.ToString(line[0]), msg, Convert.ToDateTime(line[2]).ToString("yyyy-MM-dd"));
                tempvm.IIVIWafer = wafer;
                ret.Add(tempvm);
            }
            return ret;
        }

        public static void LogWATDataWDog(string machine)
        {
            SortLog(machine, "SYSTEM", "WATDATAWDOG", "CHECK WAT DATA UNIFORMITY");
        }

        public static string GetLatestWATDataWDogTime(string machine)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@Machine", machine);

            var pretime = DateTime.Now.AddSeconds(-3600);
            var sql = "select top 1 UpdateTime from WebLog where MSGType='WATDATAWDOG' and Machine=@Machine order by UpdateTime desc";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);

            if (dbret.Count == 0)
            { return pretime.ToString("yyyy-MM-dd HH:mm:ss"); }

            var lasttime = UT.O2T(dbret[0][0]);
            if (lasttime < pretime)
            { return pretime.ToString("yyyy-MM-dd HH:mm:ss"); }
            else
            { return lasttime.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public static void Log(string filename,string msgtype,string msg)
        {
            var sql = "select Name from WebLog where MSGType='IGNDIESORT' and Name = @Name";
            var dict = new Dictionary<string, string>();
            dict.Add("@Name", filename);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql,dict);
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
            var msgdict = new Dictionary<string, bool>();
            foreach (var line in dbret)
            {
                var msg = Convert.ToString(line[1]);
                if (msgdict.ContainsKey(msg))
                { continue; }

                msgdict.Add(msg, true);
                ret.Add(new WebLog(Convert.ToString(line[0]),msg, Convert.ToDateTime(line[2]).ToString()));
            }

            return ret;
        }

        public static bool CheckEmailRecord(string fs, string emailtype)
        {
            var sql = "select Name from WebLog where MSGType=@MSGType and Name = @Name";
            var dict = new Dictionary<string, string>();
            dict.Add("@MSGType", emailtype);
            dict.Add("@Name", fs);

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            if (dbret.Count == 0)
            {
                SortLog("", fs, emailtype,"");
                return false;
            }
            return true;
        }

        public WebLog() {
            IIVIWafer = "";
            Name = "";
            MSG = "";
            UpdateTime = "";
        }
        public WebLog(string name,string msg,string updatetime)
        {
            IIVIWafer = "";
            Name = name;
            MSG = msg;
            UpdateTime = updatetime;
        }


        public string IIVIWafer { set; get; }
        public string Name { set; get; }
        public string MSG { set; get; }
        public string UpdateTime { set; get; }
    }
}