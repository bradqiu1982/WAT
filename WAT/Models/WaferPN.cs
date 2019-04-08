using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WaferPN
    {
        public static Dictionary<string, WaferPN> GetVcselDict()
        {
            var ret = new Dictionary<string, WaferPN>();

            var sql = "select Product,PArray,[Desc],MPN,FPN,Tech from WaferArray where Product = 'VCSEL'";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, null);
            foreach (var line in dbret)
            {
                var pd = Convert.ToString(line[0]);
                var pa = Convert.ToString(line[1]);
                var des = Convert.ToString(line[2]);
                var mpn = Convert.ToString(line[3]);
                var fpn = Convert.ToString(line[4]);
                var tec = Convert.ToString(line[5]);
                if (!ret.ContainsKey(mpn))
                {
                    ret.Add(mpn, new WaferPN(pd,pa ,des ,mpn,fpn,tec));
                }
                if (!ret.ContainsKey(fpn))
                {
                    ret.Add(fpn, new WaferPN(pd, pa, des, mpn, fpn, tec));
                }
            }
            return ret;
        }

        public WaferPN(string pd,string pa,string des,string mpn,string fpn,string tec)
        {
            Product = pd;
            PArray = pa;
            Desc = des;
            MPN = mpn;
            FPN = fpn;
            Tech = tec;
        }

        public WaferPN()
        {
            Product = "";
            PArray = "";
            Desc = "";
            MPN = "";
            FPN = "";
            Tech = "";
        }

        public string Product { set; get; }
        public string PArray { set; get; }
        public string Desc { set; get; }
        public string MPN { set; get; }
        public string FPN { set; get; }
        public string Tech { set; get; }

    }
}