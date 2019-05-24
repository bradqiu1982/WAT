using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WaferShippable
    {
        public static int WFShippable(string wafer)
        {
            var sql = @"SELECT shipable FROM [EngrData].[dbo].[Wafer_Shipment] where wafer = @wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return UT.O2I(line[0]);
            }
            return 0;
        }
    }
}