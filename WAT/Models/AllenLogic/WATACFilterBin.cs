using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATACFilterBin
    {
        public static Dictionary<string, bool> GetFilterBin(string PN)
        {
            var ret = new Dictionary<string, bool>();

            var sql = @"select filterbin from [EngrData].[dbo].[Newdatacom_ACFilterBins] where actestlisting = @PN and filteron is not null";
            var dict = new Dictionary<string, string>();
            dict.Add("@PN", PN);
            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                var bin = UT.O2S(line[0]);
                if (!ret.ContainsKey(bin))
                { ret.Add(bin, true); }
            }
            return ret;
        }

    }
}