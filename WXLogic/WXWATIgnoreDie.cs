using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WXLogic
{
    class WXWATIgnoreDie
    {
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
