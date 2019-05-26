using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATBITemp
    {
        public static int GetBITemp(string pn,string rp)
        {
            var sql = @"SELECT condition_value FROM [EngrData].[dbo].[Eval_Conditions_New] where evalpartnumber = @productname AND cast(right(spec,2) as int) = @rp AND condition = 'Temperature'";
            var dict = new Dictionary<string, string>();
            dict.Add("@productname", pn);
            dict.Add("@rp", rp);

            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return UT.O2I(line[0]);
            }
            return -1;
        }
    }
}