using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAT.Models
{
    public class WATApertureSize
    {
        private static string GetAPConst2162FromAllen(string WaferNum)
        {
            var ConstList = new List<double>();

            var dict = new Dictionary<string, string>();
            dict.Add("@WaferNum", WaferNum);
            var sql = "SELECT [ApCalc2162],[Ith] FROM [EngrData].[dbo].[VR_Ox_Pts_Data] where WaferID = @WaferNum";
            var dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            if (dbret.Count == 0)
            {
                sql = @"select distinct m.Value as APSIZE_Meas,v.Ith from EngrData.dbo.Ox_meas_view m 
                        left join AllenDataSQL.AllenData.dbo.Legacy_Oxide_Coordinates_View  x on m.product = x.product and x.Fieldname = m.Location
                        left join EngrData.dbo.Wuxi_WAT_VR_Report v on v.WaferID = m.container and v.Xcoord= x.X_coord and v.Ycoord= x.Y_coord
                        where m.container = @WaferNum and v.Ith is not null";
                dbret = DBUtility.ExeAllenSqlWithRes(sql, dict);
            }

            foreach (var line in dbret)
            {
                var apm = UT.O2D(line[0]);
                var ith = UT.O2D(line[1]);
                if (apm != 0 && ith != 0)
                {
                    var c = apm - 7996.8 * ith;
                    ConstList.Add(c);
                }
            }

            if (ConstList.Count > 0)
            {
                return MathNet.Numerics.Statistics.Statistics.Median(ConstList).ToString();
            }

            return string.Empty;
        }

        private static string GetAPConst2162FromLocal(string WaferNum)
        {
            var sql = "select ApConst from  WAT.dbo.WaferAPConst where Wafer= @Wafer";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", WaferNum);
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return UT.O2S(line[0]);
            }
            return string.Empty;
        }

        private static void StoreAPConst2162ToLocal(string WaferNum, string apconst)
        {
            var sql = "insert into WAT.dbo.WaferAPConst(Wafer,ApConst) values(@Wafer,@ApConst)";
            var dict = new Dictionary<string, string>();
            dict.Add("@Wafer", WaferNum);
            dict.Add("@ApConst", apconst);
            DBUtility.ExeLocalSqlNoRes(sql, dict);
        }

        public static string PrepareAPConst2162(string WaferNum)
        {
            if (WaferNum.Length != 9 || !WaferNum.Contains("-"))
            { return string.Empty; }
            var apconst = "";
            apconst = GetAPConst2162FromLocal(WaferNum);
            if (!string.IsNullOrEmpty(apconst))
            { return apconst; }

            apconst = GetAPConst2162FromAllen(WaferNum);
            if (!string.IsNullOrEmpty(apconst))
            { StoreAPConst2162ToLocal(WaferNum, apconst); }
            return apconst;
        }

    }
}