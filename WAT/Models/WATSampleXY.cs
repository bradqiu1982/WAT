using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Xml;

namespace WAT.Models
{
    public class WATSampleXY
    {
        private static string GetArrayFromDieSort(string wafer)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("@wafer", wafer);
            var sql = "SELECT top 1 [PArray] FROM [WAT].[dbo].[WaferSampleData] where WAFER = @wafer";
            var dbret = DBUtility.ExeLocalSqlWithRes(sql, dict);
            foreach (var line in dbret)
            {
                return Convert.ToString(line[0]).ToUpper().Replace("1X", "");
            }
            return string.Empty;
        }


        WATSampleXY()
        {
            CouponID = "";
            ChannelInfo = "";
            X = "";
            Y = "";
        }

        public string CouponID { set; get; }
        public string ChannelInfo { set; get; }
        public string X { set; get; }
        public string Y { set; get; }
    }
}