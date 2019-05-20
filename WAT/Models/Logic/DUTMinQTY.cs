using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class DUTMinQTY
    {

        public static DUTMinQTY GetCount(string PN,string DCDName)
        {
            var ret = new DUTMinQTY();
            var sql = @"SELECT  min([min_DUT_Count]) FROM [EngrData].[insite].[Eval_Specs_Bin_PassFail]
                        WHERE [Eval_ProductName] = @PN AND [DCDefName] = @DCDName";
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", PN);
            dict.Add("@DCDName", DCDName);

            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.MinQTY = UT.O2I(line[0]);
            }
            return ret;
        }

        public DUTMinQTY()
        {
            MinQTY = 0;
        }

        public int MinQTY { set; get; }
    }
}