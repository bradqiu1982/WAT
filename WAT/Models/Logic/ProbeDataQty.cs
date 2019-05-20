using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class ProbeDataQty
    {
        public static ProbeDataQty GetCount(string container)
        {
            var ret = new ProbeDataQty();
            var sql = @"select  count(*) from [EngrData].[dbo].[Eval_XY_Coordinates] xy with(nolock)
                          inner join [EngrData].[dbo].[VR_Eval_Pts_Data_Basic] rp  with(nolock) on rp.waferid=left(xy.container,9) AND rp.xcoord=xy.xcoord AND rp.ycoord=xy.ycoord
                          where container = @container";
            var dict = new Dictionary<string, string>();
            dict.Add("@container", container);

            var dbret = DataBaseUT.ExeAllenSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                ret.ProbeCount = UT.O2I(line[0]);
            }

            return ret;
        }

        public ProbeDataQty()
        {
            ProbeCount = 0;
        }

        public int ProbeCount { set; get; }
    }
}