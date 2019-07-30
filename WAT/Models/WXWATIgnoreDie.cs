using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace WAT.Models
{
    public class WXWATIgnoreDie
    {
        public static string GetUserName(string IP)
        {
            var machine = "";
            try
            {
                IPAddress myIP = IPAddress.Parse(IP);
                IPHostEntry GetIPHost = Dns.GetHostEntry(myIP);
                List<string> compName = GetIPHost.HostName.ToString().Split('.').ToList();
                machine = compName.First();
            }
            catch (Exception ex)
            { return IP; }

            var sql = "select username from machineusermap where machine = '<machine>' ";
            sql = sql.Replace("<machine>", machine);
            var dbret = DBUtility.ExeNPITraceSqlWithRes(sql);
            if (dbret.Count > 0)
            {
                return Convert.ToString(dbret[0][0]);
            }
            else
            { return machine; }
        }

        public static void UpdateIgnoreDie(string wafer, string x, string y, string reason, string username)
        {
            var sql = "delete from [EngrData].[dbo].[WXWATIgnoreDie] where Wafer=@Wafer and X=@X and Y=@Y";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wafer);
            dict.Add("@X", x);
            dict.Add("@Y", y);
            DBUtility.ExeLocalSqlNoRes(sql, dict);

            dict.Add("@Reason",reason);
            dict.Add("@UserName",username);

            sql = "insert into [EngrData].[dbo].[WXWATIgnoreDie](Wafer,X,Y,Reason,UserName) values(@Wafer,@X,@Y,@Reason,@UserName)";
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static Dictionary<string, WXWATIgnoreDie> RetrieveIgnoreDieDict(string wafer)
        {
            var ret = new Dictionary<string, WXWATIgnoreDie>();
            var dielist = RetrieveIgnoreDie(wafer);

            foreach (var item in dielist)
            {
                var key = item.X + ":" + item.Y;
                if (!ret.ContainsKey(key))
                { ret.Add(key, item); }
            }

            return ret;
        }

        public static List<WXWATIgnoreDie> RetrieveIgnoreDie(string wafer)
        {
            var ret = new List<WXWATIgnoreDie>();

            var sql = "select Wafer,X,Y,Reason,UserName from [EngrData].[dbo].[WXWATIgnoreDie] where Wafer = @Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", wafer);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var tempvm = new WXWATIgnoreDie();
                tempvm.Wafer = Convert.ToString(line[0]);
                tempvm.X = Convert.ToString(line[1]);
                tempvm.Y = Convert.ToString(line[2]);
                tempvm.Reason = Convert.ToString(line[3]);
                tempvm.UserName = Convert.ToString(line[4]);
                ret.Add(tempvm);
            }

            return ret;
        }


        public WXWATIgnoreDie()
        {
            Wafer = "";
            X = "";
            Y = "";
            Reason = "";
            UserName = "";
        }

        public string Wafer { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
        public string Reason { set; get; }
        public string UserName { set; get; }

    }
}