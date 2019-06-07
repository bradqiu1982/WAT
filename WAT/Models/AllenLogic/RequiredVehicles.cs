using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class RequiredVehicles
    {
        public static List<RequiredVehicles> GetData(string PN)
        {
            var sql = @"select Bin_Product, Eval_ProductName  from [EngrData].[insite].[Eval_Specs_Bin_PassFail]
		                where Bin_Product in (SELECT Bin_Product
		                FROM [EngrData].[insite].[Eval_Specs_Bin_PassFail] with(nolock) 
		                WHERE [Eval_ProductName] = @productname GROUP BY Bin_Product)
		                GROUP BY Bin_Product,Eval_ProductName";
            var dict = new Dictionary<string, string>();
            dict.Add("@productname", PN);

            var ret = new List<RequiredVehicles>();
            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.Add(new RequiredVehicles(UT.O2S(line[0]), UT.O2S(line[1])));
            }
            return ret;
        }

        public RequiredVehicles(string bpn,string epn)
        {
            Bin_PN = bpn;
            Eval_PN = epn;
        }

        public string Bin_PN {set;get;}
        public string Eval_PN { set; get; }
    }
}