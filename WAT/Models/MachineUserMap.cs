using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace WAT.Models
{
    public class MachineUserMap
    {
        public static string DetermineCompName(string IP)
        {
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                return compName.First().ToUpper();
            }
            catch (Exception ex)
            { return string.Empty; }
        }

        public static void AddMachineUserMap(string ip, string username)
        {
            var machine = DetermineCompName(ip);

            var sql = "delete from machineusermap where machine = '<machine>'";
            sql = sql.Replace("<machine>", machine);
            DBUtility.ExeLocalSqlNoRes(sql);

            var tempname = username.Split(new string[] { "@" }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpper().Trim();

            sql = "insert into machineusermap(machine,username) values(@machine,@username)";
            var param = new Dictionary<string, string>();
            param.Add("@machine", machine.ToUpper().Trim());
            param.Add("@username", tempname.ToUpper().Trim());
            DBUtility.ExeLocalSqlNoRes(sql, param);
        }

        public static Dictionary<string, string> RetrieveUserMap(string machine = "", string username = "")
        {
            var ret = new Dictionary<string, string>();

            var sql = "select machine,username from machineusermap where 1 = 1";
            if (!string.IsNullOrEmpty(machine))
            {
                sql = sql + " and machine = '<machine>'";
                sql = sql.Replace("<machine>", machine);
            }
            if (!string.IsNullOrEmpty(username))
            {
                sql = sql + " and username = '<username>'";
                sql = sql.Replace("<username>", username);
            }

            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var tempvm = new MachineUserMap();
                tempvm.machine = Convert.ToString(line[0]);
                tempvm.username = Convert.ToString(line[1]);
                if (!ret.ContainsKey(tempvm.machine))
                {
                    ret.Add(tempvm.machine, tempvm.username);
                }
            }
            return ret;
        }

        public static List<string> GetUserMachineName(string ip)
        {
            var ret = new List<string>();
            var machine = DetermineCompName(ip);
            var mndict = RetrieveUserMap(machine);
            if (mndict.ContainsKey(machine))
            {
                var name = mndict[machine];
                ret.Add(machine);
                ret.Add(name);
            }

            return ret;
        }

        public static void LoginLog(string ip, string msg)
        {
            var machine = DetermineCompName(ip);

            var name = "";
            var mndict = RetrieveUserMap(machine);
            if (mndict.ContainsKey(machine))
            { name = mndict[machine]; }

            var sql = "insert into LoginLog(Machine,Name,MSG,UpdateTime) values(@Machine,@Name,@MSG,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Machine", machine);
            dict.Add("@Name", name);
            dict.Add("@MSG", msg);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }


        public static void LoginLog(string machine, string name, string msg)
        {
            var sql = "insert into LoginLog(Machine,Name,MSG,UpdateTime) values(@Machine,@Name,@MSG,@UpdateTime)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Machine", machine);
            dict.Add("@Name", name);
            dict.Add("@MSG", msg);
            dict.Add("@UpdateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }
        public string machine { set; get; }
        public string username { set; get; }
    }
}